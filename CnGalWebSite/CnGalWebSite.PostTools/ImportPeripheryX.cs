using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel;
using CnGalWebSite.DataModel.ViewModel.Peripheries;
using CnGalWebSite.Helper.Helper;
using Newtonsoft.Json;
using System.Net.Http.Json;

namespace CnGalWebSite.PostTools
{
    internal class ImportPeripheryX
    {
        private readonly SettingX _setting;
        private readonly HttpClient _httpClient;
        private readonly ImageX _image;
        private readonly HostDataX _hostData;
        private readonly HtmlX _html;
        private readonly HistoryX _history;

        public ImportPeripheryX(HttpClient httpClient, SettingX setting, ImageX image, HostDataX hostData, HtmlX html, HistoryX history)
        {
            _setting = setting;
            _httpClient = httpClient;
            _image = image;
            _hostData = hostData;
            _history = history;
            _html = html;
        }


        public async Task ProcImportPeriphery()
        {
            //尝试读取数据文件
            var isFirst = true;
            var peripheries = new List<ImportPeripheryModel>();

            Console.WriteLine("-> 读取周边");
            while (peripheries.Count == 0)
            {
                try
                {
                    using var file = File.OpenText(_setting.ImportPeripheriesFileName);
                    var serializer = new JsonSerializer();
                    peripheries = (List<ImportPeripheryModel>)serializer.Deserialize(file, typeof(List<ImportPeripheryModel>));
                }
                catch (Exception)
                {
                    File.Create(_setting.MergeEntriesFileName).Close();
                }

                if (isFirst)
                {
                    Console.WriteLine($"请在{_setting.ImportPeripheriesFileName}文件中输入要导入的数据，具体格式请查看源码");
                    Console.WriteLine($"按下回车【Enter】确认，按下【Esc】返回");
                }
                if (peripheries.Any() == false)
                {
                    if (isFirst == false)
                    {
                        Console.WriteLine($"请在{_setting.MergeEntriesFileName}文件中输入要导入的数据，具体格式请查看源码");
                    }
                    var key = Console.ReadKey();
                    if (key.Key == ConsoleKey.Escape)
                    {
                        return;
                    }
                }

                isFirst = false;
            }
            Console.WriteLine($"-> 已读取{peripheries.Count}个周边");

            //循环读取链接并生成审核模型
            Console.WriteLine("-> 开始处理周边");

            foreach (var item in peripheries)
            {
                Console.WriteLine($"正在导入第 {peripheries.IndexOf(item) + 1} 个周边");

                var importPeriphery = _history.importPeripheries.FirstOrDefault(s => s.Name == item.Name);
                if (importPeriphery != null && importPeriphery.PostTime != null)
                {
                    OutputHelper.Write(OutputLevel.Warning, $"-> {item} 已于 {importPeriphery.PostTime} 提交审核[Id:{importPeriphery.Id}]");
                    continue;
                }
                else
                {
                    importPeriphery = item;
                }

                Console.WriteLine("生成审核记录");
                try
                {
                    await GenerateImportPeriphery(importPeriphery);
                }
                catch (Exception ex)
                {
                    OutputHelper.PressError(ex, "生成审核记录失败");
                    continue;
                }

                _history.importPeripheries.Add(importPeriphery);
                //importPeriphery.PostTime = DateTime.Now.ToCstTime();

                _history.Save();

                OutputHelper.Write(OutputLevel.Infor, $"-> 导入完成第 {peripheries.IndexOf(item) + 1} 个周边");

            }

            //成功
            OutputHelper.Repeat();
            Console.WriteLine($"总计处理{peripheries.Count}个周边，感谢您使用本投稿工具");
            OutputHelper.Repeat();
            Console.WriteLine("按任意键返回上级菜单");
            _ = Console.ReadKey();
        }

        private async Task GenerateImportPeriphery(ImportPeripheryModel data)
        {
            var model = new CreatePeripheryViewModel
            {
                Main = new EditPeripheryMainViewModel
                {
                    SaleLink = data.SaleLink,
                    Size = data.Size,
                    SmallBackgroundPicture = data.SmallBackgroundPicture,
                    SongCount = data.SongCount,
                    Author = data.Author,
                    IsAvailableItem = data.IsAvailableItem,
                    BackgroundPicture = data.BackgroundPicture,
                    Brand = data.Brand,
                    BriefIntroduction = data.BriefIntroduction,
                    Category = data.Category,
                    DisplayName = data.DisplayName ?? data.Name,
                    IndividualParts = data.IndividualParts,
                    IsReprint = data.IsReprint,
                    Material = data.Material,
                    PageCount = data.PageCount,
                    Name = data.Name,
                    Price = data.Price,
                    Thumbnail = data.Thumbnail,
                    Type = data.Type,
                },
                Peripheries = new EditPeripheryRelatedPeripheriesViewModel
                {
                    Peripheries = data.Peripheries.Select(s => new RelevancesModel
                    {
                        DisplayName = s
                    }).ToList()
                }
            };

            //裁剪图片
            await _image.CutImage(data);
            model.Main.MainPicture = data.Image;

            //关联词条
            var game = _hostData.games.FirstOrDefault(s => data.Name.Contains(s));
            if (string.IsNullOrWhiteSpace(game) == false)
            {
                model.Entries.Games.Add(new RelevancesModel { DisplayName = game });
            }

            var result = await _httpClient.PostAsJsonAsync(ToolHelper.WebApiPath + "api/peripheries/CreatePeriphery", model);

            var jsonContent = result.Content.ReadAsStringAsync().Result;
            var obj = System.Text.Json.JsonSerializer.Deserialize<Result>(jsonContent, ToolHelper.options);
            //判断结果
            if (obj.Successful == false)
            {
                throw new Exception(obj.Error);
            }

        }


    }

    public class ImportPeripheryModel : ImageModel
    {
        public long Id { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 显示名称
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// 简介
        /// </summary>
        public string BriefIntroduction { get; set; }
        /// <summary>
        /// 背景图
        /// </summary>
        public string BackgroundPicture { get; set; }
        /// <summary>
        /// 小背景图
        /// </summary>
        public string SmallBackgroundPicture { get; set; }
        /// <summary>
        /// 缩略图
        /// </summary>
        public string Thumbnail { get; set; }

        /// <summary>
        /// 分类
        /// </summary>
        public PeripheryType Type { get; set; }
        #region 通用属性
        /// <summary>
        /// 尺寸
        /// </summary>
        public string Size { get; set; }
        /// <summary>
        /// 类别
        /// </summary>
        public string Category { get; set; }
        /// <summary>
        /// 品牌
        /// </summary>
        public string Brand { get; set; }
        /// <summary>
        /// 作者
        /// </summary>
        public string Author { get; set; }
        /// <summary>
        /// 材质
        /// </summary>
        public string Material { get; set; }
        /// <summary>
        /// 单独部件数量
        /// </summary>
        public string IndividualParts { get; set; }
        /// <summary>
        /// 贩售链接
        /// </summary>
        public string SaleLink { get; set; }

        /// <summary>
        /// 价格
        /// </summary>
        public string Price { get; set; }
        /// <summary>
        /// 是否再版
        /// </summary>
        public bool IsReprint { get; set; }
        /// <summary>
        /// 是否为可用物品
        /// </summary>
        public bool IsAvailableItem { get; set; }
        #endregion
        #region 设定集
        /// <summary>
        /// 页数
        /// </summary>
        public int PageCount { get; set; }
        #endregion
        #region 设定集
        /// <summary>
        /// 歌曲数
        /// </summary>
        public int SongCount { get; set; }
        #endregion
        /// <summary>
        /// 优先级
        /// </summary>
        public int Priority { get; set; } = 0;

        /// <summary>
        /// 最后编辑时间
        /// </summary>
        public DateTime LastEditTime { get; set; }

        /// <summary>
        /// 阅读数
        /// </summary>
        public int ReaderCount { get; set; }

        /// <summary>
        /// 收集到该周边的用户数
        /// </summary>
        public int CollectedCount { get; set; }

        /// <summary>
        /// 评论数
        /// </summary>
        public int CommentCount { get; set; }

        /// <summary>
        /// 是否隐藏
        /// </summary>
        public bool IsHidden { get; set; } = false;

        /// <summary>
        /// 是否可以评论
        /// </summary>
        public bool? CanComment { get; set; } = true;


        /// <summary>
        /// 关联词条
        /// </summary>
        public List<string> Entries { get; set; } = new List<string>();
        /// <summary>
        /// 图片列表
        /// </summary>
        public List<string> Pictures { get; set; } = new List<string>();
        /// <summary>
        /// 关联周边列表
        /// </summary>
        public List<string> Peripheries { get; set; } = new List<string>();

        public DateTime? PostTime { get; set; }
    }

}
