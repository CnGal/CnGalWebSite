
using CnGalWebSite.APIServer.Application.BackUpArchives;
using CnGalWebSite.APIServer.Application.Entries;
using CnGalWebSite.APIServer.Application.Files;
using CnGalWebSite.APIServer.Application.Lotteries;
using CnGalWebSite.APIServer.Application.News;
using CnGalWebSite.APIServer.Application.Perfections;
using CnGalWebSite.APIServer.Application.Recommends;
using CnGalWebSite.APIServer.Application.Search;
using CnGalWebSite.APIServer.Application.SteamInfors;
using CnGalWebSite.APIServer.Application.Stores;
using CnGalWebSite.APIServer.Application.Tables;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.APIServer.ExamineX;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.EventBus.Models;
using CnGalWebSite.Helper.Extensions;
using CnGalWebSite.TimedTask.Models.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.TimedTasks
{
    public class TimedTaskService : ITimedTaskService
    {
        private readonly IStoreInfoService _storeInfoService;
        private readonly ITableService _tableService;
        private readonly IBackUpArchiveService _backUpArchiveService;
        private readonly IPerfectionService _perfectionService;
        private readonly ISearchHelper _searchHelper;
        private readonly INewsService _newsService;
        private readonly IExamineService _examineService;
        private readonly ILotteryService _lotteryService;
        private readonly IFileService _fileService;
        private readonly IEntryService _entryService;
        private readonly ISteamInforService _steamService;
        private readonly IRecommendService _recommendService;
        private readonly ILogger<TimedTaskService> _logger;

        public TimedTaskService(IStoreInfoService storeInfoService, IBackUpArchiveService backUpArchiveService, ITableService tableService, ILogger<TimedTaskService> logger,
        IPerfectionService perfectionService, ISearchHelper searchHelper, ISteamInforService steamService,
        INewsService newsService, IExamineService examineService, ILotteryService lotteryService, IFileService fileService, IEntryService entryService,
        IRecommendService recommendService)
        {
            _storeInfoService = storeInfoService;
            _backUpArchiveService = backUpArchiveService;
            _tableService = tableService;
            _perfectionService = perfectionService;
            _searchHelper = searchHelper;
            _newsService = newsService;
            _examineService = examineService;
            _lotteryService = lotteryService;
            _fileService = fileService;
            _logger = logger;
            _entryService = entryService;
            _steamService = steamService;
            _recommendService = recommendService;
        }

        public async Task RunTimedTask(RunTimedTaskModel Model)
        {
            _logger.LogInformation("开始执行定时任务：{name}", Model.Note ?? ((TimedTaskType)Model.Type).GetDisplayName());
            try
            {
                int maxNum = 0;
                //根据不同类型任务进行调用
                switch ((TimedTaskType)Model.Type)
                {
                    case TimedTaskType.UpdateGameSteamInfor:
                        if (!int.TryParse(Model.Parameter, out maxNum))
                        {
                            maxNum = 20;
                        }
                        await _storeInfoService.BatchUpdate(maxNum);
                        break;
                    case TimedTaskType.UpdateUserSteamInfor:
                        if (!int.TryParse(Model.Parameter, out maxNum))
                        {
                            maxNum = 10;
                        }
                        await _steamService.BatchUpdateUserSteamInfo(maxNum);
                        break;
                    case TimedTaskType.BackupEntry:
                        if (!int.TryParse(Model.Parameter, out maxNum))
                        {
                            maxNum = 10;
                        }
                        await _backUpArchiveService.BackUpAllEntries(maxNum);
                        break;
                    case TimedTaskType.BackupArticle:
                        if (!int.TryParse(Model.Parameter, out maxNum))
                        {
                            maxNum = 10;
                        }
                        await _backUpArchiveService.BackUpAllArticles(maxNum);
                        break;
                    case TimedTaskType.UpdateDataSummary:
                        await _tableService.UpdateAllInforListAsync();
                        break;
                    case TimedTaskType.UpdateSitemap:
                        await _backUpArchiveService.UpdateSitemap();
                        break;
                    case TimedTaskType.UpdatePerfectionCountAndVictoryPercentage:
                        await _perfectionService.UpdatePerfectionCountAndVictoryPercentage();
                        break;
                    case TimedTaskType.UpdatePerfectionOverview:
                        await _perfectionService.UpdatePerfectionOverview();
                        break;
                    case TimedTaskType.UpdatePerfection:
                        await _perfectionService.UpdateAllEntryPerfectionsAsync();
                        break;
                    case TimedTaskType.UpdateDataToElasticsearch:
                        await _searchHelper.UpdateDataToSearchService(Model.LastExecutedTime ?? DateTime.MinValue);
                        break;
                    case TimedTaskType.UpdateGameNews:
                        await _newsService.UpdateNewestGameNews();
                        break;
                    case TimedTaskType.UpdateWeiboUserInfor:
                        await _newsService.UpdateWeiboUserInforCache();
                        break;
                    case TimedTaskType.ExaminesCompletion:
                        await _examineService.ExaminesCompletion();
                        break;
                    case TimedTaskType.DrawLottery:
                        await _lotteryService.DrawAllLottery();
                        break;
                    case TimedTaskType.TransferAllMainImages:
                        if (!int.TryParse(Model.Parameter, out maxNum))
                        {
                            maxNum = 2;
                        }
                        await _fileService.TransferMainImagesToPublic(maxNum);
                        break;
                    case TimedTaskType.UpdateRoleBrithdays:
                        await _entryService.UpdateRoleBrithday();
                        break;
                    case TimedTaskType.PostAllBookingNotice:
                        if (!int.TryParse(Model.Parameter, out maxNum))
                        {
                            maxNum = 2;
                        }
                        await _entryService.PostAllBookingNotice(maxNum);
                        break;
                    case TimedTaskType.UpdateRecommend:
                        await _recommendService.Update();
                        break;
                }

                _logger.LogInformation("成功执行定时任务：{name}", Model.Note ?? ((TimedTaskType)Model.Type).GetDisplayName());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "执行定时任务失败：{name}", Model.Note ?? ((TimedTaskType)Model.Type).GetDisplayName());
            }
        }

    }
}
