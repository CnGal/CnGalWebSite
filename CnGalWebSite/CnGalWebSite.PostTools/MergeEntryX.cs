using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Articles;
using CnGalWebSite.DataModel.ViewModel;
using CnGalWebSite.DataModel.ViewModel.Base;
using CnGalWebSite.Helper.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Json;
using BlazorComponent;

namespace CnGalWebSite.PostTools
{
    public class MergeEntryX
    {
        private readonly SettingX _setting;
        private readonly HttpClient _httpClient;
        private readonly ImageX _image;
        private readonly HostDataX _hostData;
        private readonly HtmlX _html;
        private readonly HistoryX _history;

        public MergeEntryX(HttpClient httpClient, SettingX setting, ImageX image, HostDataX hostData, HtmlX html, HistoryX history)
        {
            _setting = setting;
            _httpClient = httpClient;
            _image = image;
            _hostData = hostData;
            _history = history;
            _html = html;
        }


        public async Task ProcMergeEntry()
        {
            //尝试读取数据文件
            var isFirst = true;
            List<string> links = new List<string>();

            Console.WriteLine("-> 读取词条");
            while (links.Count == 0)
            {
                try
                {
                    using var fs1 = new FileStream(_setting.MergeEntriesFileName, FileMode.Open, FileAccess.Read);
                    using var sr1 = new StreamReader(fs1);
                    while (sr1.EndOfStream == false)
                    {
                        var str = await sr1.ReadLineAsync();
                        if (string.IsNullOrWhiteSpace(str) == false)
                        {
                            links.Add(str);
                        }
                    }
                }
                catch (Exception)
                {
                    File.Create(_setting.MergeEntriesFileName).Close();
                }

                if (isFirst)
                {
                    Console.WriteLine($"请在{_setting.MergeEntriesFileName}文件中输入要合并的词条，每行只填写一项，会自动忽略前后空格，目前不支持合并游戏");
                    Console.WriteLine($"例如：");
                    Console.WriteLine($"cngal -> CnGal");
                    Console.WriteLine($"cngal资料站 -> CnGal");
                    Console.WriteLine($"按下回车【Enter】确认，按下【Esc】返回");
                }
                if (links.Any() == false)
                {
                    if (isFirst == false)
                    {
                        Console.WriteLine($"请在{_setting.MergeEntriesFileName}文件中输入要导入的链接");
                    }
                    var key = Console.ReadKey();
                    if (key.Key == ConsoleKey.Escape)
                    {
                        return;
                    }
                }

                isFirst = false;
            }
            Console.WriteLine($"-> 已读取{links.Count}个词条");

            //循环读取链接并生成审核模型
            Console.WriteLine("-> 开始处理词条");

            foreach (var item in links)
            {
                Console.WriteLine($"正在合并第 {links.IndexOf(item) + 1} 个词条");

                var tempstr = item.Split("->");
                if (tempstr.Length != 2)
                {
                    OutputHelper.Write(OutputLevel.Dager, $"格式错误：{item}");
                    continue;
                }

                var tempMergeEntry = new MergeEntryModel
                {
                    SubName = tempstr[0].Trim(),
                    HostName = tempstr[1].Trim(),
                };

                Console.WriteLine($"-> 将 {tempMergeEntry.SubName} 合并到 {tempMergeEntry.HostName}");

                if (string.IsNullOrWhiteSpace(tempMergeEntry.HostName) || string.IsNullOrWhiteSpace(tempMergeEntry.SubName))
                {
                    OutputHelper.Write(OutputLevel.Dager, $"词条名称不能为空");
                    continue;
                }

                var mergeEntry = _history.mergeEntries.FirstOrDefault(s => s.HostName == tempMergeEntry.HostName && s.SubName == tempMergeEntry.SubName);
                if (mergeEntry != null && mergeEntry.PostTime != null)
                {
                    OutputHelper.Write(OutputLevel.Warning, $"-> {item} 已于 {mergeEntry.PostTime} 提交审核[HostId:{mergeEntry.HostId}][SubId:{mergeEntry.SubId}]");
                    continue;
                }
                else
                {
                    mergeEntry = tempMergeEntry;
                }

                Console.WriteLine("获取词条");
                try
                {
                    await GetMergeEntryId(mergeEntry);
                }
                catch (Exception ex)
                {
                    OutputHelper.PressError(ex, "获取词条失败", "请确保词条存在");
                    continue;
                }

                Console.WriteLine("生成审核记录");
                try
                {
                    await GenerateMergeEntry(mergeEntry);
                }
                catch (Exception ex)
                {
                    OutputHelper.PressError(ex, "生成审核记录失败");
                    continue;
                }

                _history.mergeEntries.Add(mergeEntry);
                _history.Save();

                Console.WriteLine("发送审核记录");
                try
                {
                    await PostExamine(mergeEntry);
                }
                catch (Exception ex)
                {
                    OutputHelper.PressError(ex, "发送审核记录失败");
                    continue;
                }

                Console.WriteLine("隐藏从词条");
                try
                {
                    await HideSubEntry(mergeEntry);
                }
                catch (Exception ex)
                {
                    OutputHelper.PressError(ex, "递交隐藏从词条申请失败");
                    continue;
                }

                mergeEntry.PostTime = DateTime.Now.ToCstTime();
                _history.Save();

                OutputHelper.Write(OutputLevel.Infor, $"-> 合并完成第 {links.IndexOf(item) + 1} 个词条");

            }

            //成功
            OutputHelper.Repeat();
            Console.WriteLine($"总计处理{links.Count}个词条，感谢您使用本投稿工具");
            OutputHelper.Repeat();
            Console.WriteLine("按任意键返回上级菜单");
            Console.ReadKey();
        }

        private async Task GenerateMergeEntry(MergeEntryModel model)
        {
            //清空
            model.Examines.Clear();

            //获取词条
            var hostEntry = await _httpClient.GetFromJsonAsync<EntryIndexViewModel>(ToolHelper.WebApiPath + "api/entries/GetEntryView/" + model.HostId);
            var subEntry = await _httpClient.GetFromJsonAsync<EntryIndexViewModel>(ToolHelper.WebApiPath + "api/entries/GetEntryView/" + model.SubId);

            if (subEntry == null|| hostEntry == null)
            {
                throw new Exception("无法获取目标词条");
            }
            if (subEntry.Type == EntryType.Game|| hostEntry.Type == EntryType.Game)
            {
                throw new Exception("目前不支持合并游戏");
            }

            //获取从词条关联的各个游戏
            var reEntryIds = subEntry.Roles.Select(x => x.Id).ToList();
            reEntryIds.AddRange(subEntry.ProductionGroups.Select(x => x.Id));//这里是显示名称 极少部分词条可能会有问题
            reEntryIds.AddRange(subEntry.EntryRelevances.Select(x => x.Id));
            reEntryIds.AddRange(subEntry.StaffGames.Select(x => x.Id));
            //获取主词条关联的各个游戏
            reEntryIds.AddRange(hostEntry.Roles.Select(x => x.Id));
            reEntryIds.AddRange(hostEntry.ProductionGroups.Select(x => x.Id));//这里是显示名称 极少部分词条可能会有问题
            reEntryIds.AddRange(hostEntry.EntryRelevances.Select(x => x.Id));
            reEntryIds.AddRange(hostEntry.StaffGames.Select(x => x.Id));

            //替换关联信息
            foreach (var item in reEntryIds)
            {
                var examineModel = await _httpClient.GetFromJsonAsync<EditRelevancesViewModel>(ToolHelper.WebApiPath + "api/entries/editrelevances/" + item);
                ReplaceEntryName(examineModel.Games, model.SubName, model.HostName);
                ReplaceEntryName(examineModel.staffs, model.SubName, model.HostName);
                ReplaceEntryName(examineModel.Roles, model.SubName, model.HostName);
                ReplaceEntryName(examineModel.Groups, model.SubName, model.HostName);

                model.Examines.Add(examineModel);
            }

            //游戏
            var reGameIds = subEntry.EntryRelevances.Where(s => s.Type == EntryType.Game).Select(x => x.Id).ToList();
            reGameIds.AddRange(subEntry.StaffGames.Select(x => x.Id));
            reGameIds.AddRange(hostEntry.EntryRelevances.Where(s => s.Type == EntryType.Game).Select(x => x.Id));
            reGameIds.AddRange(hostEntry.StaffGames.Select(x => x.Id));

            //替换Staff，开发商和发行商
            foreach (var item in reGameIds)
            {
                var examineModel = await _httpClient.GetFromJsonAsync<EditAddInforViewModel>(ToolHelper.WebApiPath + "api/entries/EditAddInfor/" + item);
                var tempStaffs = examineModel.Staffs.Where(s => s.Name == model.SubName);
                foreach (var temp in tempStaffs)
                {
                    temp.Name = model.HostName;
                }
                if (string.IsNullOrWhiteSpace(examineModel.Publisher) == false)
                {
                    examineModel.Publisher= examineModel.Publisher.Replace(model.SubName, model.HostName);

                }
                if (string.IsNullOrWhiteSpace(examineModel.ProductionGroup) == false)
                {
                    examineModel.ProductionGroup= examineModel.ProductionGroup.Replace(model.SubName, model.HostName);

                }
                if (string.IsNullOrWhiteSpace(examineModel.CV) == false)
                {
                    examineModel.CV = examineModel.CV.Replace(model.SubName, model.HostName);

                }
                model.Examines.Add(examineModel);

            }


            //获取从词条关联的各个文章
            //获取从词条关联的各个游戏
            var reArticleIds = subEntry.ArticleRelevances.Select(x => x.Id).ToList();
            reArticleIds.AddRange(subEntry.ArticleRelevances.Select(x => x.Id));

            //替换关联信息
            foreach (var item in reArticleIds)
            {
                var examineModel = await _httpClient.GetFromJsonAsync<EditArticleRelevancesViewModel>(ToolHelper.WebApiPath + "api/articles/editarticlerelevances/" + item);
                ReplaceEntryName(examineModel.Games, model.SubName, model.HostName);
                ReplaceEntryName(examineModel.Staffs, model.SubName, model.HostName);
                ReplaceEntryName(examineModel.Roles, model.SubName, model.HostName);
                ReplaceEntryName(examineModel.Groups, model.SubName, model.HostName);

                model.Examines.Add(examineModel);
            }
        }

        private void ReplaceEntryName(List<RelevancesModel> list, string oldName, string newName)
        {
            var temp = list.FirstOrDefault(s => s.DisplayName == oldName);
            if (temp != null)
            {
                temp.DisPlayValue = newName;
            }
        }

        private async Task GetMergeEntryId(MergeEntryModel model)
        {
            //获取词条
            model.HostId = await _httpClient.GetFromJsonAsync<int>(ToolHelper.WebApiPath + "api/entries/GetId/" + ToolHelper.Base64EncodeName(model.HostName));
            model.SubId = await _httpClient.GetFromJsonAsync<int>(ToolHelper.WebApiPath + "api/entries/GetId/" + ToolHelper.Base64EncodeName(model.SubName));


        }

        private async Task PostExamine(MergeEntryModel model)
        {
            foreach (var item in model.Examines)
            {
                var result = item.GetType().Name switch
                {
                    "EditArticleRelevancesViewModel" => await _httpClient.PostAsJsonAsync(ToolHelper.WebApiPath + "api/entries/editarticlerelevances", item as EditArticleRelevancesViewModel),
                    "EditAddInforViewModel" => await _httpClient.PostAsJsonAsync(ToolHelper.WebApiPath + "api/entries/EditAddInfor", item as EditAddInforViewModel),
                    "EditRelevancesViewModel" => await _httpClient.PostAsJsonAsync(ToolHelper.WebApiPath + "api/entries/editrelevances", item as EditRelevancesViewModel),
                    _ => null
                };

                var jsonContent = result.Content.ReadAsStringAsync().Result;
                var obj = System.Text.Json.JsonSerializer.Deserialize<Result>(jsonContent, ToolHelper.options);
                //判断结果
                if (obj.Successful == false)
                {
                    throw new Exception(obj.Error);
                }
            }
        }

        private async Task HideSubEntry(MergeEntryModel model)
        {
            try
            {

                var result = await _httpClient.PostAsJsonAsync<HiddenArticleModel>(ToolHelper.WebApiPath + "api/entries/HiddenEntry", new HiddenArticleModel { Ids = new long[] { model.SubId }, IsHidden = true });
                var jsonContent = result.Content.ReadAsStringAsync().Result;
                var obj = System.Text.Json.JsonSerializer.Deserialize<Result>(jsonContent, ToolHelper.options);
                //判断结果
                if (obj.Successful == false)
                {
                    throw new Exception(obj.Error);
                }

            }
            catch
            {
                var examineModel = await _httpClient.GetFromJsonAsync<EditMainViewModel>(ToolHelper.WebApiPath + "api/entries/EditMain/" + model.SubId);
                examineModel.Name = examineModel.DisplayName = "已删除_" + examineModel.Name;


                var result = await _httpClient.PostAsJsonAsync(ToolHelper.WebApiPath + "api/entries/EditMain", examineModel);
                var jsonContent = result.Content.ReadAsStringAsync().Result;
                var obj = System.Text.Json.JsonSerializer.Deserialize<Result>(jsonContent, ToolHelper.options);
                //判断结果
                if (obj.Successful == false)
                {
                    throw new Exception(obj.Error);
                }

                OutputHelper.Write(OutputLevel.Warning, $"隐藏从词条失败，可能当前未以管理员角色登入，已尝试递交隐藏请求");

            }
        }
    }

    public class MergeEntryModel
    {
        public int HostId { get; set; }
        public string HostName { get; set; }

        public int SubId { get; set; }
        public string SubName { get; set; }

        public List<BaseEditModel> Examines = new List<BaseEditModel>();

        public DateTime? PostTime { get; set; }
    }
}
