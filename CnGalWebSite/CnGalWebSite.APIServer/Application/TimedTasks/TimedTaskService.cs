using BootstrapBlazor.Components;
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
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.TimedTasks;
using CnGalWebSite.Helper.Extensions;
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
        private readonly IRepository<TimedTask, int> _timedTaskRepository;
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

        private static readonly ConcurrentDictionary<Type, Func<IEnumerable<TimedTask>, string, SortOrder, IEnumerable<TimedTask>>> SortLambdaCacheApplicationUser = new();

        public TimedTaskService(IRepository<TimedTask, int> timedTaskRepository, IStoreInfoService storeInfoService, IBackUpArchiveService backUpArchiveService, ITableService tableService, ILogger<TimedTaskService> logger,
        IPerfectionService perfectionService, ISearchHelper searchHelper, ISteamInforService steamService,
        INewsService newsService, IExamineService examineService, ILotteryService lotteryService, IFileService fileService, IEntryService entryService,
        IRecommendService recommendService)
        {
            _timedTaskRepository = timedTaskRepository;
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

        public async Task RunTimedTask(TimedTask item)
        {
            if (item.IsRuning == true || item.IsPause == true)
            {
                return;
            }
            try
            {
                int maxNum = 0;
                //更新执行状态
                _timedTaskRepository.Clear();
                item = await _timedTaskRepository.FirstOrDefaultAsync(s => s.Id == item.Id);
                if (item != null)
                {
                    item.IsRuning = true;
                    await _timedTaskRepository.UpdateAsync(item);
                }
                //根据不同类型任务进行调用
                switch (item.Type)
                {
                    case TimedTaskType.UpdateGameSteamInfor:
                        if(!int.TryParse(item.Parameter, out maxNum))
                        {
                            maxNum = 20;
                        }
                        await _storeInfoService.BatchUpdate(maxNum);
                        break;
                    case TimedTaskType.UpdateUserSteamInfor:
                        if (!int.TryParse(item.Parameter, out maxNum))
                        {
                            maxNum = 10;
                        }
                        await _steamService.BatchUpdateUserSteamInfo(maxNum);
                        break;
                    case TimedTaskType.BackupEntry:
                        if (!int.TryParse(item.Parameter, out maxNum))
                        {
                            maxNum = 10;
                        }
                        await _backUpArchiveService.BackUpAllEntries(maxNum);
                        break;
                    case TimedTaskType.BackupArticle:
                        if (!int.TryParse(item.Parameter, out maxNum))
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
                        await _searchHelper.UpdateDataToSearchService(item.LastExecutedTime ?? DateTime.MinValue);
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
                        if (!int.TryParse(item.Parameter, out maxNum))
                        {
                            maxNum = 2;
                        }
                        await _fileService.TransferMainImagesToPublic(maxNum);
                        break;
                    case TimedTaskType.UpdateRoleBrithdays:
                        await _entryService.UpdateRoleBrithday();
                        break;
                    case TimedTaskType.PostAllBookingNotice:
                        if (!int.TryParse(item.Parameter, out maxNum))
                        {
                            maxNum = 2;
                        }
                        await _entryService.PostAllBookingNotice(maxNum);
                        break;
                    case TimedTaskType.UpdateRecommend:
                        await _recommendService.Update();
                        break;
                }
                //记录执行时间
                _timedTaskRepository.Clear();
                item = await _timedTaskRepository.FirstOrDefaultAsync(s => s.Id == item.Id);
                if (item != null)
                {
                    item.IsLastFail = false;
                    item.IsRuning = false;
                    item.LastExecutedTime = DateTime.Now.ToCstTime();
                    await _timedTaskRepository.UpdateAsync(item);
                }
                _logger.LogInformation("成功执行定时任务：{name}", item.Name ?? item.Type.GetDisplayName());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "执行定时任务失败：{name}", item.Name ?? item.Type.GetDisplayName());

                _timedTaskRepository.Clear();
                item = await _timedTaskRepository.FirstOrDefaultAsync(s => s.Id == item.Id);
                if (item != null)
                {
                    item.IsLastFail = false;
                    item.IsRuning = false;
                    item.IsLastFail = true;
                    item.LastExecutedTime = DateTime.Now.ToCstTime();
                    await _timedTaskRepository.UpdateAsync(item);
                }
            }
            finally
            {

            }
        }

    }
}
