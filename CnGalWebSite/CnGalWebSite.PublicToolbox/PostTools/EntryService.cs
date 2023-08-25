using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Articles;
using CnGalWebSite.DataModel.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using CnGalWebSite.PublicToolbox.DataRepositories;
using CnGalWebSite.PublicToolbox.Models;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.Helper.Helper;
using Microsoft.Extensions.Logging;
using System.Security.Policy;
using CnGalWebSite.DataModel.ViewModel.Videos;
using CnGalWebSite.Core.Services;

namespace CnGalWebSite.PublicToolbox.PostTools
{
    public class EntryService:IEntryService
    {
        private readonly IHttpService _httpService;
        private readonly IRepository<MergeEntryModel> _mergeEntryRepository;
        private readonly ILogger<EntryService> _logger;
        public event Action<KeyValuePair<OutputLevel, string>> ProgressUpdate;


        public EntryService(IHttpService httpService, IRepository<MergeEntryModel> mergeEntryRepository, ILogger<EntryService> logger)
        {
            _logger = logger;
            _httpService = httpService;
            _mergeEntryRepository = mergeEntryRepository;
        }


        public async Task ProcMergeEntry(MergeEntryModel model)
        {

            OnProgressUpdate(model, OutputLevel.Infor, $"开始将<{model.SubName}>合并到<{model.HostName}>");

            if (string.IsNullOrWhiteSpace(model.HostName) || string.IsNullOrWhiteSpace(model.SubName))
            {
                OnProgressUpdate(model, OutputLevel.Dager, $"词条名称不能为空");
                return;
            }

            var mergeEntry = _mergeEntryRepository.GetAll().FirstOrDefault(s => s.HostName == model.HostName && s.SubName == model.SubName);
            if (mergeEntry != null && mergeEntry.PostTime != null)
            {
                OnProgressUpdate(model, OutputLevel.Dager, $"{model.SubName}({mergeEntry.SubId}) -> {model.HostName}({mergeEntry.HostId}) 已于 {mergeEntry.PostTime} 提交审核");
                return;
            }
            else
            {
                mergeEntry = model;
            }

            model.CompleteTaskCount++;


            OnProgressUpdate(model, OutputLevel.Infor, "获取词条");
            try
            {
                await GetMergeEntryId(mergeEntry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{SubName} -> {HostName} 获取词条失败", model.SubName, model.HostName);
                OnProgressUpdate(model, OutputLevel.Dager, "获取词条失败");
                return;
            }

            model.CompleteTaskCount++;


            OnProgressUpdate(model, OutputLevel.Infor, "生成审核记录");
            try
            {
                await GenerateMergeEntry(mergeEntry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{SubName}({SubId}) -> {HostName}({HostId}) 生成审核记录失败", model.SubName, model.SubId, model.HostName, model.HostId);
                OnProgressUpdate(model, OutputLevel.Dager, "生成审核记录失败");

                return;
            }

            await _mergeEntryRepository.InsertAsync(mergeEntry);

            model.CompleteTaskCount++;

            OnProgressUpdate(model, OutputLevel.Infor, "发送审核记录");
            try
            {
                await PostExamine(mergeEntry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{SubName}({SubId}) -> {HostName}({HostId}) 发送审核记录失败", model.SubName, model.SubId, model.HostName, model.HostId);
                OnProgressUpdate(model, OutputLevel.Dager, "发送审核记录失败");
                return;
            }

            model.CompleteTaskCount++;


            OnProgressUpdate(model, OutputLevel.Infor, "隐藏从词条");
            try
            {
                await HideSubEntry(mergeEntry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{SubName}({SubId}) -> {HostName}({HostId}) 递交隐藏从词条申请失败", model.SubName, model.SubId, model.HostName, model.HostId);
                OnProgressUpdate(model, OutputLevel.Dager, "递交隐藏从词条申请失败");
                return;
            }

            model.CompleteTaskCount++;


            mergeEntry.PostTime = DateTime.Now.ToCstTime();
            await _mergeEntryRepository.SaveAsync();

            model.CompleteTaskCount++;


            OnProgressUpdate(model, OutputLevel.Infor, $"成功将 {model.SubName} 合并到 {model.HostName}");

        }

        protected void OnProgressUpdate(MergeEntryModel model,OutputLevel level, string infor)
        {
            if(level == OutputLevel.Dager)
            {
                model.Error = infor;
            }
            ProgressUpdate?.Invoke(new KeyValuePair<OutputLevel, string>(level, infor));
        }

        private async Task GenerateMergeEntry(MergeEntryModel model)
        {
            //清空
            model.Examines.Clear();

            //获取词条
            var hostEntry = await _httpService.GetAsync<EntryIndexViewModel>(ToolHelper.WebApiPath + "api/entries/GetEntryView/" + model.HostId);
            var subEntry = await _httpService.GetAsync<EntryIndexViewModel>(ToolHelper.WebApiPath + "api/entries/GetEntryView/" + model.SubId);

            if (subEntry == null || hostEntry == null)
            {
                throw new Exception("无法获取目标词条");
            }
            if (subEntry.Type == EntryType.Game || hostEntry.Type == EntryType.Game)
            {
                throw new Exception("目前不支持合并游戏");
            }

            model.CompleteTaskCount++;

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

            //游戏
            var reGameIds = subEntry.EntryRelevances.Where(s => s.Type == EntryType.Game||s.Type== EntryType.Role).Select(x => x.Id).ToList();
            reGameIds.AddRange(subEntry.StaffGames.Select(x => x.Id));
            reGameIds.AddRange(subEntry.Roles.Select(x => x.Id));
            reGameIds.AddRange(hostEntry.EntryRelevances.Where(s => s.Type == EntryType.Game || s.Type == EntryType.Role).Select(x => x.Id));
            reGameIds.AddRange(hostEntry.StaffGames.Select(x => x.Id));
            reGameIds.AddRange(hostEntry.Roles.Select(x => x.Id));

            //替换关联信息
            foreach (var item in reEntryIds)
            {
                var examineModel = await _httpService.GetAsync<EditRelevancesViewModel>(ToolHelper.WebApiPath + "api/entries/editrelevances/" + item);
                ReplaceEntryName(examineModel.Games, model.SubName, model.HostName);
                ReplaceEntryName(examineModel.staffs, model.SubName, model.HostName);
                ReplaceEntryName(examineModel.Roles, model.SubName, model.HostName);
                ReplaceEntryName(examineModel.Groups, model.SubName, model.HostName);

                model.Examines.Add(examineModel);
            }

            model.CompleteTaskCount++;


            //替换Staff，开发商和发行商
            foreach (var item in reGameIds)
            {
                var examineModel = await _httpService.GetAsync<EditAddInforViewModel>(ToolHelper.WebApiPath + "api/entries/EditAddInfor/" + item);
                var tempStaffs = examineModel.Staffs.Where(s => s.Name == model.SubName);
                foreach (var temp in tempStaffs)
                {
                    temp.Name = model.HostName;
                }

                examineModel.Publisher = ReplaceEntryName(examineModel.Publisher, model.SubName, model.HostName);
                examineModel.ProductionGroup = ReplaceEntryName(examineModel.ProductionGroup, model.SubName, model.HostName);
                examineModel.CV = ReplaceEntryName(examineModel.CV, model.SubName, model.HostName);

                model.Examines.Add(examineModel);

            }

            model.CompleteTaskCount++;


            //获取从词条关联的各个文章
            var reArticleIds = subEntry.ArticleRelevances.Select(x => x.Id).ToList();
            reArticleIds.AddRange(subEntry.NewsOfEntry.Select(x => x.ArticleId));

            //替换关联信息
            foreach (var item in reArticleIds)
            {
                var examineModel = await _httpService.GetAsync<EditArticleRelevancesViewModel>(ToolHelper.WebApiPath + "api/articles/editrelevances/" + item);
                ReplaceEntryName(examineModel.Games, model.SubName, model.HostName);
                ReplaceEntryName(examineModel.Staffs, model.SubName, model.HostName);
                ReplaceEntryName(examineModel.Roles, model.SubName, model.HostName);
                ReplaceEntryName(examineModel.Groups, model.SubName, model.HostName);

                model.Examines.Add(examineModel);
            }

            //获取从词条关联的各个视频
            var reVideoIds = subEntry.VideoRelevances.Select(x => x.Id).ToList();

            //替换关联信息
            foreach (var item in reVideoIds)
            {
                var examineModel = await _httpService.GetAsync<EditVideoRelevancesViewModel>(ToolHelper.WebApiPath + "api/videos/editrelevances/" + item);
                ReplaceEntryName(examineModel.Games, model.SubName, model.HostName);
                ReplaceEntryName(examineModel.staffs, model.SubName, model.HostName);
                ReplaceEntryName(examineModel.Roles, model.SubName, model.HostName);
                ReplaceEntryName(examineModel.Groups, model.SubName, model.HostName);

                model.Examines.Add(examineModel);
            }

            model.CompleteTaskCount++;

        }

        private void ReplaceEntryName(List<RelevancesModel> list, string oldName, string newName)
        {
            var temp = list.FirstOrDefault(s => s.DisplayName == oldName);
            if (temp != null)
            {
                temp.DisplayName = newName;
            }
        }

        private string ReplaceEntryName(string text, string oldName, string newName)
        {
            StringBuilder result = new StringBuilder();
            if(string.IsNullOrWhiteSpace(text))
            {
                return text;
            }

            var texts= text.Replace("，", ",").Replace("、", ",").Split(',').ToList();
            texts.RemoveAll(s => string.IsNullOrWhiteSpace(s));

            foreach (var item in texts)
            {
                if(result.Length > 0)
                    {
                        result.Append(',');
                    }
                if (item == oldName)
                {
                   
                    result.Append(newName);
                }
                else
                {
                    result.Append(item);
                }
            }

            return result.ToString();
          
        }

        private async Task GetMergeEntryId(MergeEntryModel model)
        {
            //获取词条
            model.HostId = await _httpService.GetAsync<int>(ToolHelper.WebApiPath + "api/entries/GetId/" + ToolHelper.Base64EncodeName(model.HostName));
            model.SubId = await _httpService.GetAsync<int>(ToolHelper.WebApiPath + "api/entries/GetId/" + ToolHelper.Base64EncodeName(model.SubName));

            if(model.HostId==0||model.SubId==0)
            {
                throw new Exception("词条不存在");
            }
        }

        private async Task PostExamine(MergeEntryModel model)
        {
            foreach (var item in model.Examines)
            {
                var result = item.GetType().Name switch
                {
                    "EditArticleRelevancesViewModel" => await _httpService.PostAsync<EditArticleRelevancesViewModel, Result>(ToolHelper.WebApiPath + "api/articles/editrelevances", item as EditArticleRelevancesViewModel),
                    "EditAddInforViewModel" => await _httpService.PostAsync<EditAddInforViewModel, Result>(ToolHelper.WebApiPath + "api/entries/EditAddInfor", item as EditAddInforViewModel),
                    "EditRelevancesViewModel" => await _httpService.PostAsync<EditRelevancesViewModel, Result>(ToolHelper.WebApiPath + "api/entries/editrelevances", item as EditRelevancesViewModel),
                    "EditVideoRelevancesViewModel" => await _httpService.PostAsync<EditVideoRelevancesViewModel, Result>(ToolHelper.WebApiPath + "api/videos/editrelevances", item as EditVideoRelevancesViewModel),
                    _ => null
                };

                //判断结果
                if (result.Successful == false)
                {
                    throw new Exception(result.Error);
                }
            }
        }

        private async Task HideSubEntry(MergeEntryModel model)
        {
            try
            {

                var result = await _httpService.PostAsync<HiddenArticleModel, Result>(ToolHelper.WebApiPath + "api/entries/HiddenEntry", new HiddenArticleModel { Ids = new long[] { model.SubId }, IsHidden = true });
                //判断结果
                if (result.Successful == false)
                {
                    throw new Exception(result.Error);
                }

            }
            catch
            {
                var examineModel = await _httpService.GetAsync<EditMainViewModel>(ToolHelper.WebApiPath + "api/entries/EditMain/" + model.SubId);
                examineModel.Name = examineModel.DisplayName = "已删除_" + examineModel.Name;


                var result = await _httpService.PostAsync<EditMainViewModel, Result>(ToolHelper.WebApiPath + "api/entries/EditMain", examineModel);
                //判断结果
                if (result.Successful == false)
                {
                    throw new Exception(result.Error);
                }

                _logger.LogWarning("{SubName}({SubId}) -> {HostName}({HostId}) 隐藏从词条失败，可能当前未以管理员角色登入，已尝试递交隐藏请求", model.SubName, model.SubId, model.HostName, model.HostId);
                OnProgressUpdate(model, OutputLevel.Dager, "已提交编辑记录，等待审核通过");
            }
        }
    }
}
