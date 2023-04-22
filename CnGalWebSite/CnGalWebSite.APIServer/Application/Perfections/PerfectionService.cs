using BootstrapBlazor.Components;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Perfections;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.Perfections
{
    public class PerfectionService : IPerfectionService
    {
        private readonly IRepository<Entry, int> _entryRepository;
        private readonly IRepository<Article, long> _articleRepository;
        private readonly IRepository<PerfectionCheck, long> _perfectionCheckRepository;
        private readonly IRepository<Perfection, long> _perfectionRepository;
        private readonly IRepository<PerfectionOverview, long> _perfectionOverviewRepository;
        private readonly ILogger<PerfectionService> _logger;


        public PerfectionService(IRepository<Entry, int> entryRepository, IRepository<PerfectionCheck, long> perfectionCheckRepository, ILogger<PerfectionService> logger,
        IRepository<Perfection, long> perfectionRepository, IRepository<Article, long> articleRepository, IRepository<PerfectionOverview, long> perfectionOverviewRepository)
        {
            _entryRepository = entryRepository;
            _perfectionCheckRepository = perfectionCheckRepository;
            _perfectionRepository = perfectionRepository;
            _articleRepository = articleRepository;
            _perfectionOverviewRepository = perfectionOverviewRepository;
            _logger = logger;
        }

        public async Task<QueryData<ListPerfectionAloneModel>> GetPaginatedResult(CnGalWebSite.DataModel.ViewModel.Search.QueryPageOptions options, ListPerfectionAloneModel searchModel)
        {
            var items = _perfectionRepository.GetAll()
                .Include(s => s.Entry)
                .Where(s => s.Entry.IsHidden == false && string.IsNullOrWhiteSpace(s.Entry.Name) == false && s.Entry.Type == EntryType.Game)
                .AsNoTracking();

            // 处理 SearchText 模糊搜索
            if (!string.IsNullOrWhiteSpace(options.SearchText))
            {
                items = items.Where(item => (item.Entry != null && item.Entry.Name.Contains(options.SearchText)));
            }


            // 排序
            var isSorted = false;
            if (!string.IsNullOrWhiteSpace(options.SortName))
            {

                items = items.OrderBy(s => s.Id).Sort(options.SortName, (SortOrder)options.SortOrder);
                isSorted = true;
            }
            // 设置记录总数
            var total = items.Count();

            // 内存分页
            var itemsReal = await items.Skip((options.PageIndex - 1) * options.PageItems).Take(options.PageItems).ToListAsync();

            //复制数据
            var resultItems = new List<ListPerfectionAloneModel>();
            foreach (var item in itemsReal)
            {
                resultItems.Add(new ListPerfectionAloneModel
                {
                    Id = item.EntryId,
                    Name = item.Entry.Name,

                    Grade = item.Grade,
                    VictoryPercentage = item.VictoryPercentage,
                });
            }

            return new QueryData<ListPerfectionAloneModel>()
            {
                Items = resultItems,
                TotalCount = total,
                IsSorted = isSorted,
                // IsFiltered = isFiltered
            };
        }

        public async Task<QueryData<ListPerfectionCheckAloneModel>> GetPaginatedResult(CnGalWebSite.DataModel.ViewModel.Search.QueryPageOptions options, ListPerfectionCheckAloneModel searchModel)
        {
            var items = _perfectionCheckRepository.GetAll()
                .Include(s => s.Perfection).ThenInclude(s => s.Entry)
                .Where(s => s.Perfection.Entry.IsHidden == false && string.IsNullOrWhiteSpace(s.Perfection.Entry.Name) == false && s.Perfection.Entry.Type == EntryType.Game)
                .AsNoTracking();

            // 处理 SearchText 模糊搜索
            if (!string.IsNullOrWhiteSpace(options.SearchText))
            {
                items = items.Where(item => (item.Perfection.Entry != null && item.Perfection.Entry.Name.Contains(options.SearchText)));
            }


            // 排序
            var isSorted = false;
            if (!string.IsNullOrWhiteSpace(options.SortName))
            {

                items = items.OrderBy(s => s.Id).Sort(options.SortName, (SortOrder)options.SortOrder);
                isSorted = true;
            }
            // 设置记录总数
            var total = items.Count();

            // 内存分页
            var itemsReal = await items.Skip((options.PageIndex - 1) * options.PageItems).Take(options.PageItems).ToListAsync();

            //复制数据
            var resultItems = new List<ListPerfectionCheckAloneModel>();
            foreach (var item in itemsReal)
            {
                resultItems.Add(new ListPerfectionCheckAloneModel
                {
                    Id = item.Perfection.EntryId,
                    Name = item.Perfection.Entry.Name,
                    Type = item.CheckType,
                    Grade = item.Grade,
                    VictoryPercentage = item.VictoryPercentage,
                });
            }

            return new QueryData<ListPerfectionCheckAloneModel>()
            {
                Items = resultItems,
                TotalCount = total,
                IsSorted = isSorted,
                // IsFiltered = isFiltered
            };
        }


        public async Task<List<PerfectionCheckViewModel>> GetEntryPerfectionCheckList(long perfectionId)
        {
            var checks = await _perfectionCheckRepository.GetAll().Include(s => s.Perfection).Where(s => s.PerfectionId == perfectionId).ToListAsync();

            var model = new List<PerfectionCheckViewModel>();

            foreach (var check in checks)
            {
                model.Add(new PerfectionCheckViewModel
                {
                    CheckType = check.CheckType,
                    DefectType = check.DefectType,
                    Count = check.Count,
                    Grade = check.Grade,
                    Id = check.Id,
                    Infor = check.Infor,
                    VictoryPercentage = check.VictoryPercentage,
                    Level = ToolHelper.GetEntryPerfectionCheckLevel(check.CheckType, check.DefectType),
                });
            }

            return model.OrderByDescending(s => s.Level).ThenBy(s => s.DefectType).ToList();
        }

        public async Task<PerfectionInforTipViewModel> GetEntryPerfection(int entryId)
        {
            //先获取词条
            var entry = await _entryRepository.GetAll().AsNoTracking().Where(s => s.Id == entryId).Select(s => new
            {
                s.LastEditTime,
                s.Id,
                s.Name,
                s.Examines.Count,
                s.Type
            }).FirstOrDefaultAsync();
            if (entry == null)
            {
                return null;
            }

            //不是游戏词条 不获取完善度
            if (entry.Type != EntryType.Game)
            {
                return new PerfectionInforTipViewModel
                {
                    LastEditTime = entry.LastEditTime,
                    EntryId = entry.Id,
                    EntryName = entry.Name,
                    EditCount = entry.Count
                };
            }


            var model = await _perfectionRepository.GetAll().Where(s => s.EntryId == entryId).Select(s => new PerfectionInforTipViewModel
            {
                DefectCount = s.Checks.Where(s => s.DefectType != PerfectionDefectType.None).Count(),
                Grade = s.Grade,
                Id = s.Id,
                Level = ToolHelper.GetEntryPerfectionLevel(s.Grade),
                VictoryPercentage = s.VictoryPercentage,
            }).FirstOrDefaultAsync();

            if (model == null)
            {
                await UpdateEntryPerfectionResultAsync(entryId);
                model = await _perfectionRepository.GetAll().Where(s => s.EntryId == entryId).Select(s => new PerfectionInforTipViewModel
                {
                    DefectCount = s.Checks.Where(s => s.DefectType != PerfectionDefectType.None).Count(),
                    Grade = s.Grade,
                    Id = s.Id,
                    Level = ToolHelper.GetEntryPerfectionLevel(s.Grade),
                    VictoryPercentage = s.VictoryPercentage,
                }).FirstOrDefaultAsync();

                if (model == null)
                {
                    return null;
                }
            }

            model.LastEditTime = entry.LastEditTime;
            model.EntryId = entry.Id;
            model.EntryName = entry.Name;
            model.EditCount = entry.Count;

            return model;
        }

        public async Task<List<PerfectionInforTipViewModel>> GetPerfectionLevelRadomListAsync(PerfectionLevel level)
        {
            var model = new List<PerfectionInforTipViewModel>();
            var length = await _entryRepository.GetAll().AsNoTracking()
                .Where(s => s.Type == EntryType.Game && s.IsHidden != true && string.IsNullOrWhiteSpace(s.Name) == false
                      && (level == PerfectionLevel.ToBeImproved ? (s.Perfection.Grade < 60) : (level == PerfectionLevel.Good ? (s.Perfection.Grade >= 60 && s.Perfection.Grade < 80) : (s.Perfection.Grade >= 80))))
                .CountAsync();
            var length_1 = 6;
            List<int> groups;
            if (length > length_1)
            {
                var random = new Random();
                var temp = random.Next(0, length - length_1);

                groups = await _entryRepository.GetAll().AsNoTracking().OrderBy(s=>s.Id)
                    .Include(s => s.Perfection)
                    .Where(s => s.Type == EntryType.Game && s.IsHidden != true && string.IsNullOrWhiteSpace(s.Name) == false
                          && (level == PerfectionLevel.ToBeImproved ? (s.Perfection.Grade < 60) : (level == PerfectionLevel.Good ? (s.Perfection.Grade >= 60 && s.Perfection.Grade < 80) : (s.Perfection.Grade >= 80))))
                    .Select(s => s.Id).Skip(temp).Take(length_1).ToListAsync();
            }
            else
            {
                groups = await _entryRepository.GetAll().AsNoTracking().OrderBy(s => s.Id)
                    .Include(s => s.Perfection)
                    .Where(s => s.Type == EntryType.Game && s.IsHidden != true && string.IsNullOrWhiteSpace(s.Name) == false
                          && (level == PerfectionLevel.ToBeImproved ? (s.Perfection.Grade < 60) : (level == PerfectionLevel.Good ? (s.Perfection.Grade >= 60 && s.Perfection.Grade < 80) : (s.Perfection.Grade >= 80))))
                    .Select(s => s.Id).Take(length_1).ToListAsync();
            }
            foreach (var item in groups)
            {
                model.Add(await GetEntryPerfection(item));
            }
            return model;
        }

        public async Task<List<PerfectionCheckViewModel>> GetPerfectionCheckLevelRadomListAsync(PerfectionCheckLevel level)
        {
            var model = new List<PerfectionInforTipViewModel>();
            var length = await _perfectionCheckRepository.GetAll().AsNoTracking()
                .Include(s => s.Perfection).ThenInclude(s => s.Entry)
                .Where(s => s.Perfection.Entry.IsHidden == false && string.IsNullOrWhiteSpace(s.Perfection.Entry.Name) == false)
                .Where
                (s =>
                    (
                        (s.DefectType == PerfectionDefectType.None) ?
                        (
                             (level == PerfectionCheckLevel.None)
                         ) :
                        (
                            (level == PerfectionCheckLevel.High) ?
                            (
                                 (s.CheckType == PerfectionCheckType.BriefIntroduction || s.CheckType == PerfectionCheckType.MainImage
                                   || s.CheckType == PerfectionCheckType.IssueTime || s.CheckType == PerfectionCheckType.MainPage)
                             )
                            :
                            (
                                (level == PerfectionCheckLevel.Middle) ?
                                (
                                        (s.CheckType == PerfectionCheckType.Staff || s.CheckType == PerfectionCheckType.SteamId
                                    || s.CheckType == PerfectionCheckType.ProductionGroup)
                                 )
                                 :
                                 (
                                     (s.CheckType != PerfectionCheckType.BriefIntroduction && s.CheckType != PerfectionCheckType.MainImage
                                    && s.CheckType != PerfectionCheckType.IssueTime && s.CheckType != PerfectionCheckType.MainPage
                                    && s.CheckType != PerfectionCheckType.Staff && s.CheckType != PerfectionCheckType.SteamId
                                    && s.CheckType != PerfectionCheckType.ProductionGroup)

                                  )
                             )
                         )
                     )
                )
                .CountAsync();

            var length_1 = 6;
            List<PerfectionCheckViewModel> groups;
            if (length > length_1)
            {
                var random = new Random();
                var temp = random.Next(0, length - length_1);

                groups = await _perfectionCheckRepository.GetAll().AsNoTracking().OrderBy(s => s.Id)
                    .Include(s => s.Perfection).ThenInclude(s => s.Entry)
                    .Where(s => s.Perfection.Entry.IsHidden == false && string.IsNullOrWhiteSpace(s.Perfection.Entry.Name) == false)
                    .Where
                    (s =>
                        (
                            (s.DefectType == PerfectionDefectType.None) ?
                            (
                                 (level == PerfectionCheckLevel.None)
                             ) :
                            (
                                (level == PerfectionCheckLevel.High) ?
                                (
                                     (s.CheckType == PerfectionCheckType.BriefIntroduction || s.CheckType == PerfectionCheckType.MainImage
                                       || s.CheckType == PerfectionCheckType.IssueTime || s.CheckType == PerfectionCheckType.MainPage)
                                 )
                                :
                                (
                                    (level == PerfectionCheckLevel.Middle) ?
                                    (
                                            (s.CheckType == PerfectionCheckType.Staff || s.CheckType == PerfectionCheckType.SteamId
                                        || s.CheckType == PerfectionCheckType.ProductionGroup)
                                     )
                                     :
                                     (
                                         (s.CheckType != PerfectionCheckType.BriefIntroduction && s.CheckType != PerfectionCheckType.MainImage
                                        && s.CheckType != PerfectionCheckType.IssueTime && s.CheckType != PerfectionCheckType.MainPage
                                        && s.CheckType != PerfectionCheckType.Staff && s.CheckType != PerfectionCheckType.SteamId
                                        && s.CheckType != PerfectionCheckType.ProductionGroup)

                                      )
                                 )
                             )
                         )
                    )
                    .Skip(temp).Take(length_1)
                    .Select(check => new PerfectionCheckViewModel
                    {
                        CheckType = check.CheckType,
                        DefectType = check.DefectType,
                        Count = check.Count,
                        Grade = check.Grade,
                        Id = check.Id,
                        Infor = check.Infor,
                        VictoryPercentage = check.VictoryPercentage,
                        Level = level,
                        EntryId = check.Perfection.Entry.Id,
                        EntryName = check.Perfection.Entry.DisplayName
                    })
                    .ToListAsync();
            }
            else
            {
                groups = await _perfectionCheckRepository.GetAll().AsNoTracking().OrderBy(s => s.Id)
                    .Include(s => s.Perfection).ThenInclude(s => s.Entry)
                    .Where(s => s.Perfection.Entry.IsHidden == false && string.IsNullOrWhiteSpace(s.Perfection.Entry.Name) == false)
                    .Where
                    (s =>
                        (
                            (s.DefectType == PerfectionDefectType.None) ?
                            (
                                 (level == PerfectionCheckLevel.None)
                             ) :
                            (
                                (level == PerfectionCheckLevel.High) ?
                                (
                                     (s.CheckType == PerfectionCheckType.BriefIntroduction || s.CheckType == PerfectionCheckType.MainImage
                                       || s.CheckType == PerfectionCheckType.IssueTime || s.CheckType == PerfectionCheckType.MainPage)
                                 )
                                :
                                (
                                    (level == PerfectionCheckLevel.Middle) ?
                                    (
                                            (s.CheckType == PerfectionCheckType.Staff || s.CheckType == PerfectionCheckType.SteamId
                                        || s.CheckType == PerfectionCheckType.ProductionGroup)
                                     )
                                     :
                                     (
                                         (s.CheckType != PerfectionCheckType.BriefIntroduction && s.CheckType != PerfectionCheckType.MainImage
                                        && s.CheckType != PerfectionCheckType.IssueTime && s.CheckType != PerfectionCheckType.MainPage
                                        && s.CheckType != PerfectionCheckType.Staff && s.CheckType != PerfectionCheckType.SteamId
                                        && s.CheckType != PerfectionCheckType.ProductionGroup)

                                      )
                                 )
                             )
                         )
                    )
                    .Take(length_1)
                    .Select(check => new PerfectionCheckViewModel
                    {
                        CheckType = check.CheckType,
                        DefectType = check.DefectType,
                        Count = check.Count,
                        Grade = check.Grade,
                        Id = check.Id,
                        Infor = check.Infor,
                        VictoryPercentage = check.VictoryPercentage,
                        Level = level,
                        EntryId = check.Perfection.Entry.Id,
                        EntryName = check.Perfection.Entry.DisplayName
                    })
                    .ToListAsync();
            }

            return groups;
        }


        public async Task UpdateAllEntryPerfectionsAsync()
        {
            //删除不符合条件词条的完善度检查
            await _perfectionCheckRepository.GetAll().Where(s => s.Perfection.Entry.Type != EntryType.Game || string.IsNullOrWhiteSpace(s.Perfection.Entry.Name) || s.Perfection.Entry.IsHidden == true).ExecuteDeleteAsync();
            await _perfectionRepository.GetAll().Where(s => s.Entry.Type != EntryType.Game || string.IsNullOrWhiteSpace(s.Entry.Name) || s.Entry.IsHidden == true).ExecuteDeleteAsync();

            var entries = await _entryRepository.GetAll().AsNoTracking().Where(s => s.Type == EntryType.Game && s.IsHidden == false && string.IsNullOrWhiteSpace(s.Name) == false).Select(s => s.Id).ToListAsync();
            foreach (var item in entries)
            {
                await UpdateEntryPerfectionResultAsync(item);
            }
        }

        public async Task UpdateEntryPerfectionResultAsync(int entryId)
        {
            try
            {
                //查找词条
                var entry = await _entryRepository.GetAll().AsNoTracking()
                    .Include(s => s.Information).ThenInclude(s => s.Additional)
                    .Include(s => s.EntryRelationFromEntryNavigation).ThenInclude(s => s.ToEntryNavigation)
                    .Include(s => s.EntryStaffFromEntryNavigation).ThenInclude(s => s.ToEntryNavigation)
                    .Include(s => s.Articles)
                    .Include(s => s.Pictures)
                    .Include(s => s.Tags)
                    .Include(s => s.Outlinks)
                    .Include(s => s.Releases)
                    .FirstOrDefaultAsync(s => s.Id == entryId);
                if (entry == null)
                {
                    return;
                }
                //找到词条当前完善度对象
                var perfection = await _perfectionRepository.GetAll().Include(s => s.Checks).FirstOrDefaultAsync(s => s.EntryId == entryId);
                if (perfection == null)
                {
                    perfection = new Perfection
                    {
                        EntryId = entry.Id,
                        Checks = new List<PerfectionCheck>()
                    };
                    perfection = await _perfectionRepository.InsertAsync(perfection);
                }


                //获取之前的结果列表
                var checks = new List<PerfectionCheck>();
                foreach (var item in perfection.Checks)
                {
                    checks.Add(item);
                }
                //运行新的检查
                var results = new List<PerfectionCheck>
                {
                    //检查简介
                    CheckEntryBriefIntroduction(entry),
                    //检查主图
                    CheckEntryMainImage(entry)
                };
                //检查Staff
                results.AddRange( CheckEntryStaff(entry));
                //检查steamId
                results.Add(CheckEntrySteamId(entry));
                //检查制作组
                results.AddRange( CheckEntryProductionGroup(entry, PerfectionCheckType.ProductionGroup,5));
                //检查发行商
                results.AddRange(CheckEntryProductionGroup(entry, PerfectionCheckType.Publisher,2));
                //检查发行时间
                results.Add(CheckEntryIssueTime(entry));
                //检查游戏平台
                results.Add(CheckEntryGamePlatforms(entry));
                //检查官网
                results.Add(CheckEntryOfficialWebsite(entry));
                //检查QQ群
                results.Add(CheckEntryQQgroup(entry));
                //检查主页
                results.Add(CheckEntryMainPage(entry));
                //检查图片
                results.Add(CheckEntryImages(entry));
                //检查关联信息
                results.AddRange(CheckEntryRelevances(entry));
                //检查标签
                results.Add(CheckEntryTags(entry));

                //前后进行对比 对更新的项目进行修改
                var noCommonItemUnfilled = true;
                foreach (var item in results)
                {
                    var temp = checks.FirstOrDefault(s => s.CheckType == item.CheckType && s.Infor == item.Infor);

                    if (temp == null)
                    {
                        item.PerfectionId = perfection.Id;
                        await _perfectionCheckRepository.InsertAsync(item);
                    }
                    else
                    {
                        if (item.Grade != temp.Grade || item.DefectType != temp.DefectType)
                        {
                            temp.Grade = item.Grade;
                            temp.Infor = item.Infor;
                            temp.DefectType = item.DefectType;
                            await _perfectionCheckRepository.UpdateAsync(temp);
                        }
                    }
                    //检查是否只有特殊项目处于未填写状态
                    if (noCommonItemUnfilled && item.DefectType != PerfectionDefectType.None && item.CheckType != PerfectionCheckType.IssueTime
                        && item.CheckType != PerfectionCheckType.Publisher && item.CheckType != PerfectionCheckType.QQgroup && item.CheckType != PerfectionCheckType.SteamId
                        && item.CheckType != PerfectionCheckType.OfficialWebsite)
                    {
                        noCommonItemUnfilled = false;
                    }
                }

                foreach (var item in checks)
                {
                    var temp = results.FirstOrDefault(s => s.CheckType == item.CheckType && s.Infor == item.Infor);

                    if (temp == null)
                    {
                        await _perfectionCheckRepository.DeleteAsync(item);
                    }
                }

                //如果只有特殊项目未填写则 直接100
                double grade = 0;
                if (noCommonItemUnfilled)
                {
                    grade = 100;
                }
                else
                {
                    grade = results.Select(s => s.Grade).ToArray().Sum();

                }

                _perfectionRepository.Clear();
                await _perfectionRepository.GetAll().Where(s => s.Id == perfection.Id).ExecuteUpdateAsync(s=>s.SetProperty(s => s.Grade, b => grade));


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "计算词条(Id:{id})完善度失败", entryId);
                return;
            }

        }

        #region 词条完善度检查细分方法

        public PerfectionCheck CheckEntryTags(Entry entry)
        {
            if (entry.Tags.Count != 0)
            {
                return new PerfectionCheck
                {
                    CheckType = PerfectionCheckType.Tags,
                    DefectType = PerfectionDefectType.None,
                    Grade = 10,
                };
            }
            else
            {
                return new PerfectionCheck
                {
                    CheckType = PerfectionCheckType.Tags,
                    DefectType = PerfectionDefectType.NotFilledIn,
                    Grade = 0,
                };
            }
        }

        public List<PerfectionCheck> CheckEntryRelevances(Entry entry)
        {
            var results = new List<PerfectionCheck>();
            //获取所有staff
            var relevances = entry.EntryRelationFromEntryNavigation.ToList();

            if (relevances.Count == 0)
            {
                results.Add(new PerfectionCheck
                {
                    CheckType = PerfectionCheckType.RelevanceEntries,
                    DefectType = PerfectionDefectType.NotFilledIn,
                    Grade = 0,
                });
            }
            else
            {
                results.Add(new PerfectionCheck
                {
                    CheckType = PerfectionCheckType.RelevanceEntries,
                    DefectType = PerfectionDefectType.None,
                    Grade = 10,
                });
            }

            if (entry.Articles.Count == 0)
            {
                results.Add(new PerfectionCheck
                {
                    CheckType = PerfectionCheckType.RelevanceArticles,
                    DefectType = PerfectionDefectType.NotFilledIn,
                    Grade = 0,
                });
            }
            else
            {
                results.Add(new PerfectionCheck
                {
                    CheckType = PerfectionCheckType.RelevanceArticles,
                    DefectType = PerfectionDefectType.None,
                    Grade = 10,
                });
            }



            return results;
        }

        public PerfectionCheck CheckEntryImages(Entry entry)
        {
            if (entry.Pictures.Count != 0)
            {
                return new PerfectionCheck
                {
                    CheckType = PerfectionCheckType.Images,
                    DefectType = PerfectionDefectType.None,
                    Grade = 10,
                };
            }
            else
            {
                return new PerfectionCheck
                {
                    CheckType = PerfectionCheckType.Images,
                    DefectType = PerfectionDefectType.NotFilledIn,
                    Grade = 0,
                };
            }
        }

        public PerfectionCheck CheckEntryMainPage(Entry entry)
        {
            if (entry.MainPage == null)
            {
                return new PerfectionCheck
                {
                    CheckType = PerfectionCheckType.MainPage,
                    DefectType = PerfectionDefectType.NotFilledIn,
                    Grade = 0
                };
            }

            if (entry.MainPage.Length >= 100)
            {
                return new PerfectionCheck
                {
                    CheckType = PerfectionCheckType.MainPage,
                    DefectType = PerfectionDefectType.None,
                    Grade = 10,
                };
            }
            else
            {
                return new PerfectionCheck
                {
                    CheckType = PerfectionCheckType.MainPage,
                    DefectType = PerfectionDefectType.InsufficientLength,
                    Grade = 10 * ((double)entry.MainPage.Length / 100),
                };
            }
        }

        public PerfectionCheck CheckEntryQQgroup(Entry entry)
        {
            var QQgroup = entry.Information.FirstOrDefault(s => s.Modifier == "基本信息" && s.DisplayName == "QQ群");
            if (QQgroup != null && string.IsNullOrWhiteSpace(QQgroup.DisplayValue) == false)
            {
                return new PerfectionCheck
                {
                    CheckType = PerfectionCheckType.QQgroup,
                    DefectType = PerfectionDefectType.None,
                    Grade = 1,
                };
            }
            else
            {
                return new PerfectionCheck
                {
                    CheckType = PerfectionCheckType.QQgroup,
                    DefectType = PerfectionDefectType.NotFilledIn,
                    Grade = 0,
                };
            }
        }

        public PerfectionCheck CheckEntryOfficialWebsite(Entry entry)
        {
            var officialWebsite = entry.Outlinks.FirstOrDefault();
            if (officialWebsite != null && string.IsNullOrWhiteSpace(officialWebsite.Link) == false)
            {
                return new PerfectionCheck
                {
                    CheckType = PerfectionCheckType.OfficialWebsite,
                    DefectType = PerfectionDefectType.None,
                    Grade = 1,
                };
            }
            else
            {
                return new PerfectionCheck
                {
                    CheckType = PerfectionCheckType.OfficialWebsite,
                    DefectType = PerfectionDefectType.NotFilledIn,
                    Grade = 0,
                };
            }
        }

        public PerfectionCheck CheckEntryGamePlatforms(Entry entry)
        {
            var gamePlatforms = entry.Releases.FirstOrDefault(s => s.GamePlatformTypes.Any());
            if (gamePlatforms != null)
            {
                return new PerfectionCheck
                {
                    CheckType = PerfectionCheckType.GamePlatforms,
                    DefectType = PerfectionDefectType.None,
                    Grade = 1,
                };
            }
            else
            {
                return new PerfectionCheck
                {
                    CheckType = PerfectionCheckType.GamePlatforms,
                    DefectType = PerfectionDefectType.NotFilledIn,
                    Grade = 0,
                };
            }
        }

        public PerfectionCheck CheckEntryIssueTime(Entry entry)
        {
            var issueTime = entry.Releases.FirstOrDefault(s=>s.Time!=null||string.IsNullOrWhiteSpace(s.TimeNote)==false);
            if (issueTime != null)
            {
                return new PerfectionCheck
                {
                    CheckType = PerfectionCheckType.IssueTime,
                    DefectType = PerfectionDefectType.None,
                    Grade = 5,
                };
            }
            else
            {
                return new PerfectionCheck
                {
                    CheckType = PerfectionCheckType.IssueTime,
                    DefectType = PerfectionDefectType.NotFilledIn,
                    Grade = 0,
                };
            }
        }

        public List<PerfectionCheck> CheckEntryProductionGroup(Entry entry, PerfectionCheckType type,double totalScore)
        {
            var results = new List<PerfectionCheck>();

            var publishers = entry.EntryStaffFromEntryNavigation.Where(s => s.PositionGeneral == (type == PerfectionCheckType.ProductionGroup ? PositionGeneralType.ProductionGroup : PositionGeneralType.Publisher));

            if (publishers.Any()==false)
            {
                return new List<PerfectionCheck>{
                        new PerfectionCheck
                        {
                            CheckType = type,
                            DefectType = PerfectionDefectType.NotFilledIn,
                            Grade = 0,
                        }
                    };
            }

            //拿到存在和不存在的名单
            var existentialStaffs = publishers.Where(s => s.ToEntryNavigation != null);

            var existentialCount = 0;

            var score = totalScore / publishers.Count();
            foreach (var item in publishers)
            {
                var displayName = string.IsNullOrWhiteSpace(item.CustomName) ? (item.ToEntryNavigation?.DisplayName ?? item.Name) : item.CustomName;

                if (item.ToEntryNavigation == null)
                {
                    results.Add(new PerfectionCheck
                    {
                        CheckType = type,
                        DefectType = PerfectionDefectType.NonExistent,
                        Infor = displayName,
                        Grade = score / 2,
                    });

                }
                else if (item.ToEntryNavigation.Type != EntryType.Staff && item.ToEntryNavigation.Type != EntryType.ProductionGroup)
                {
                    results.Add(new PerfectionCheck
                    {
                        CheckType = type,
                        DefectType = PerfectionDefectType.TypeError,
                        Infor = displayName,
                        Grade = score / 2,
                    });
                }
                else
                {
                    existentialCount++;
                }
            }
            if (existentialCount > 0)
            {
                results.Add(new PerfectionCheck
                {
                    CheckType = type,
                    DefectType = PerfectionDefectType.None,
                    Grade = score * existentialCount,
                });
            }

            return results;
        }

        public PerfectionCheck CheckEntrySteamId(Entry entry)
        {
            var steamId = entry.Releases.FirstOrDefault(s => string.IsNullOrWhiteSpace(s.Link));
            if (steamId != null)
            {
                return new PerfectionCheck
                {
                    CheckType = PerfectionCheckType.SteamId,
                    DefectType = PerfectionDefectType.None,
                    Grade = 5,
                };
            }
            else
            {
                return new PerfectionCheck
                {
                    CheckType = PerfectionCheckType.SteamId,
                    DefectType = PerfectionDefectType.NotFilledIn,
                    Grade = 0,
                };
            }
        }

        public List<PerfectionCheck> CheckEntryStaff(Entry entry)
        {
            var results = new List<PerfectionCheck>();
            //获取所有staff
            var staffs = entry.EntryStaffFromEntryNavigation.Where(s => s.PositionGeneral != PositionGeneralType.Publisher && s.PositionGeneral != PositionGeneralType.ProductionGroup);


            if (staffs.Any()==false)
            {
                return new List<PerfectionCheck>{
                    new PerfectionCheck
                    {
                        CheckType = PerfectionCheckType.Staff,
                        DefectType = PerfectionDefectType.NotFilledIn,
                        Grade = 0,
                    }
                };
            }

            var existentialCount = 0;

            var score = (double)20 / staffs.Count();
            foreach (var item in staffs)
            {
                if (item.ToEntryNavigation == null)
                {
                    results.Add(new PerfectionCheck
                    {
                        CheckType = PerfectionCheckType.Staff,
                        DefectType = PerfectionDefectType.NonExistent,
                        Infor = item.Name,
                        Grade = score / 2,
                    });

                }
                else if (item.ToEntryNavigation.Type != EntryType.Staff && item.ToEntryNavigation.Type != EntryType.ProductionGroup)
                {
                    results.Add(new PerfectionCheck
                    {
                        CheckType = PerfectionCheckType.Staff,
                        DefectType = PerfectionDefectType.TypeError,
                        Infor = item.ToEntryNavigation.Name,
                        Grade = score / 2,
                    });
                }
                else
                {
                    existentialCount++;
                }
            }
            if (existentialCount > 0)
            {
                results.Add(new PerfectionCheck
                {
                    CheckType = PerfectionCheckType.Staff,
                    DefectType = PerfectionDefectType.None,
                    Grade = score * existentialCount,
                });
            }

            return results;
        }

        public PerfectionCheck CheckEntryMainImage(Entry entry)
        {
            if (entry.MainPicture == null)
            {
                return new PerfectionCheck
                {
                    CheckType = PerfectionCheckType.MainImage,
                    DefectType = PerfectionDefectType.NotFilledIn,
                    Grade = 0
                };
            }

            if (string.IsNullOrWhiteSpace(entry.MainPicture) == false)
            {
                return new PerfectionCheck
                {
                    CheckType = PerfectionCheckType.MainImage,
                    DefectType = PerfectionDefectType.None,
                    Grade = 5,
                };
            }
            else
            {
                return new PerfectionCheck
                {
                    CheckType = PerfectionCheckType.MainImage,
                    DefectType = PerfectionDefectType.NotFilledIn,
                    Grade = 0,
                };
            }
        }

        public PerfectionCheck CheckEntryBriefIntroduction(Entry entry)
        {
            if (string.IsNullOrWhiteSpace(entry.BriefIntroduction))
            {
                return new PerfectionCheck
                {
                    CheckType = PerfectionCheckType.BriefIntroduction,
                    DefectType = PerfectionDefectType.NotFilledIn,
                    Grade = 0
                };
            }

            if (entry.BriefIntroduction.Length >= 30)
            {
                return new PerfectionCheck
                {
                    CheckType = PerfectionCheckType.BriefIntroduction,
                    DefectType = PerfectionDefectType.None,
                    Grade = 5,
                };
            }
            else
            {
                return new PerfectionCheck
                {
                    CheckType = PerfectionCheckType.BriefIntroduction,
                    DefectType = PerfectionDefectType.InsufficientLength,
                    Grade = 5 * ((double)entry.BriefIntroduction.Length / 30),
                };
            }
        }

        #endregion

        public async Task UpdatePerfectionCountAndVictoryPercentage()
        {
            try
            {
                //计算关联到同一个不存在的词条的数目
                var counnts = await _perfectionCheckRepository.GetAll()
                    .Where(s => s.DefectType == PerfectionDefectType.NonExistent && string.IsNullOrWhiteSpace(s.Infor) == false)
                    .Select(s => new { s.Infor })
                    .GroupBy(s => s.Infor)
                    .Select(s => new { s.Key, Count = s.Count() })
                    .ToListAsync();

                //遍历更新数据
                foreach (var item in counnts)
                {
                    await _perfectionCheckRepository.GetAll()
                        .Where(s => s.Infor == item.Key)
                        .ExecuteUpdateAsync(s => s.SetProperty(s => s.Count, b => item.Count));
                }

                //计算每个词条完善度总分超过全站的百分比
                var grades = await _perfectionRepository.GetAll().AsNoTracking()
                .Include(s => s.Entry)
                .Where(s => s.Entry.IsHidden == false && string.IsNullOrWhiteSpace(s.Entry.Name) == false)
                .Select(s => new { s.Id, s.Grade }).ToListAsync();
                var count = grades.Count;
                foreach (var item in grades)
                {
                    var temp = 100 * (double)grades.Count(s => s.Grade < item.Grade) / count;
                    await _perfectionRepository.GetAll().Where(s => s.Id == item.Id).ExecuteUpdateAsync(s=>s.SetProperty(s => s.VictoryPercentage, b => temp));

                }

                //计算每个已完成的明细检查 全站的已完成百分比
                var checkCounts = await _perfectionCheckRepository.GetAll().AsNoTracking()
                    .Include(s => s.Perfection).ThenInclude(s => s.Entry)
                    .Where(s => s.Perfection.Entry.IsHidden == false && string.IsNullOrWhiteSpace(s.Perfection.Entry.Name) == false)
                    .Where(s => s.DefectType == PerfectionDefectType.None)
                    .Select(s => new { s.CheckType })
                    .GroupBy(s => s.CheckType)
                    .Select(s => new { s.Key, Count = s.Count() })
                    .ToListAsync();

                foreach (var item in checkCounts)
                {
                    await _perfectionCheckRepository.GetAll()
                        .Where(s => s.CheckType == item.Key)
                        .ExecuteUpdateAsync(s => s.SetProperty(s => s.VictoryPercentage, b => 100 * (double)item.Count / count));
                }

            }
            catch (Exception)
            {
                return;
            }
        }

        public async Task<PerfectionLevelOverviewModel> GetPerfectionLevelOverview()
        {
            var grades = await _perfectionRepository.GetAll().AsNoTracking()
                .Include(s => s.Entry)
                .Where(s => s.Entry.IsHidden == false && string.IsNullOrWhiteSpace(s.Entry.Name) == false && s.Entry.Type == EntryType.Game)
                .OrderBy(s => s.Grade).Select(s => s.Grade).ToListAsync();

            var model = new PerfectionLevelOverviewModel();

            var count = grades.Count;
            model.ToBeImprovedCount = grades.Count(s => s < 60);
            model.GoodCount = grades.Count(s => s >= 60 && s < 80);
            model.ExcellentCount = grades.Count(s => s >= 80);

            //获取平均值
            model.AverageValue = grades.Average();
            model.Median = (count % 2 == 0) ? ((grades[count / 2 - 1] + grades[count / 2]) / 2) : grades[count / 2];
            model.Mode = grades.Select(s => (int)s).GroupBy(s => s).Select(s => new { s.Key, Count = s.Count() }).OrderByDescending(s => s.Count).Select(s => s.Key).First();
            model.StandardDeviation = Math.Sqrt(grades.Sum(x => Math.Pow(x - model.AverageValue, 2)) / grades.Count);

            return model;

        }

        public async Task UpdatePerfectionOverview()
        {
            var nowTime = DateTime.Now.ToCstTime();

            var model = await GetPerfectionLevelOverview();

            //判断当天是否已经更新
            var perfection = await _perfectionOverviewRepository.GetAll().FirstOrDefaultAsync(s => s.LastUpdateTime.Date == nowTime.Date);
            if (perfection != null)
            {
                perfection.StandardDeviation = model.StandardDeviation;
                perfection.Mode = model.Mode;
                perfection.ToBeImprovedCount = model.ToBeImprovedCount;
                perfection.ExcellentCount = model.ExcellentCount;
                perfection.AverageValue = model.AverageValue;
                perfection.GoodCount = model.GoodCount;
                perfection.Median = model.Median;

                await _perfectionOverviewRepository.UpdateAsync(perfection);
                return;
            }


            await _perfectionOverviewRepository.InsertAsync(new PerfectionOverview
            {
                StandardDeviation = model.StandardDeviation,
                AverageValue = model.AverageValue,
                ExcellentCount = model.ExcellentCount,
                GoodCount = model.GoodCount,
                LastUpdateTime = nowTime,
                Median = model.Median,
                Mode = model.Mode,
                ToBeImprovedCount = model.ToBeImprovedCount,
            });
        }
    }
}
