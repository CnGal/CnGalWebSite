using Microsoft.EntityFrameworkCore;
using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Perfections;
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


        public PerfectionService(IRepository<Entry, int> entryRepository, IRepository<PerfectionCheck, long> perfectionCheckRepository,
            IRepository<Perfection, long> perfectionRepository, IRepository<Article, long> articleRepository, IRepository<PerfectionOverview, long> perfectionOverviewRepository)
        {
            _entryRepository = entryRepository;
            _perfectionCheckRepository = perfectionCheckRepository;
            _perfectionRepository = perfectionRepository;
            _articleRepository = articleRepository;
            _perfectionOverviewRepository = perfectionOverviewRepository;
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

                groups = await _entryRepository.GetAll().AsNoTracking()
                    .Include(s => s.Perfection)
                    .Where(s => s.Type == EntryType.Game && s.IsHidden != true && string.IsNullOrWhiteSpace(s.Name) == false
                          && (level == PerfectionLevel.ToBeImproved ? (s.Perfection.Grade < 60) : (level == PerfectionLevel.Good ? (s.Perfection.Grade >= 60 && s.Perfection.Grade < 80) : (s.Perfection.Grade >= 80))))
                    .Select(s => s.Id).Skip(temp).Take(length_1).ToListAsync();
            }
            else
            {
                groups = await _entryRepository.GetAll().AsNoTracking()
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

                groups = await _perfectionCheckRepository.GetAll().AsNoTracking()
                    .Include(s => s.Perfection).ThenInclude(s => s.Entry)
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
                groups = await _perfectionCheckRepository.GetAll().AsNoTracking()
                    .Include(s => s.Perfection).ThenInclude(s => s.Entry)
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
            var entries = await _entryRepository.GetAll().Where(s => s.Type == EntryType.Game).Select(s => s.Id).ToListAsync();
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
                var entry = await _entryRepository.GetAll().Where(s => s.Type == EntryType.Game).AsNoTracking()
                    .Include(s => s.Information).ThenInclude(s => s.Additional)
                    .Include(s => s.Relevances)
                    .Include(s => s.Pictures)
                    .Include(s => s.Tags)
                    .Include(s => s.Perfection).ThenInclude(s => s.Checks)
                    .FirstOrDefaultAsync(s => s.Id == entryId);
                if (entry == null)
                {
                    return;
                }
                //找到词条当前完善度对象
                var perfection = entry.Perfection;
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
                results.AddRange(await CheckEntryStaff(entry));
                //检查steamId
                results.Add(CheckEntrySteamId(entry));
                //检查制作组
                results.AddRange(await CheckEntryProductionGroup(entry));
                //检查发行商
                results.AddRange(await CheckEntryPublisher(entry));
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
                results.AddRange(await CheckEntryRelevances(entry));
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
                await _perfectionRepository.GetRangeUpdateTable().Where(s => s.Id == perfection.Id).Set(s => s.Grade, b => grade).ExecuteAsync();

                _perfectionRepository.Clear();
                _perfectionCheckRepository.Clear();
                _entryRepository.Clear();

            }
            catch (Exception)
            {
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

        public async Task<List<PerfectionCheck>> CheckEntryRelevances(Entry entry)
        {
            var results = new List<PerfectionCheck>();
            //获取所有staff
            var relevances = entry.Relevances.ToList();

            if (relevances.Count == 0)
            {
                return new List<PerfectionCheck>{
                    new PerfectionCheck
                    {
                        CheckType = PerfectionCheckType.RelevanceArticles,
                        DefectType = PerfectionDefectType.NotFilledIn,
                        Grade = 0,
                    },
                     new PerfectionCheck
                    {
                        CheckType = PerfectionCheckType.RelevanceEntries,
                        DefectType = PerfectionDefectType.NotFilledIn,
                        Grade = 0,
                    }
                };
            }

            //只判断关联词条 不判断链接
            var score = (double)20 / relevances.Count(s => s.Modifier == "游戏" || s.Modifier == "制作组" || s.Modifier == "角色" || s.Modifier == "文章" || s.Modifier == "动态");

            //判断词条 不考虑Staff
            var entries = relevances.Where(s => s.Modifier == "游戏" || s.Modifier == "制作组" || s.Modifier == "角色").Select(s => new { s.DisplayName, s.Modifier }).ToList();
            var entrynames = entries.Select(s => s.DisplayName);
            //拿到存在和不存在的名单
            var existentialEntries = await _entryRepository.GetAll().Where(s => entrynames.Contains(s.Name)).Select(s => new { s.Name, s.Type }).ToListAsync();

            var existentialEntryCount = 0;

            foreach (var item in entries)
            {
                var temp = existentialEntries.FirstOrDefault(s => s.Name.ToLower() == item.DisplayName.ToLower());
                if (temp == null)
                {
                    results.Add(new PerfectionCheck
                    {
                        CheckType = PerfectionCheckType.RelevanceEntries,
                        DefectType = PerfectionDefectType.NonExistent,
                        Infor = item.DisplayName,
                        Grade = score / 2,
                    });
                }
                else if ((temp.Type == EntryType.Game && item.Modifier != "游戏") || (temp.Type == EntryType.ProductionGroup && item.Modifier != "制作组") || (temp.Type == EntryType.Role && item.Modifier != "角色"))
                {
                    results.Add(new PerfectionCheck
                    {
                        CheckType = PerfectionCheckType.RelevanceEntries,
                        DefectType = PerfectionDefectType.TypeError,
                        Infor = item.DisplayName,
                        Grade = score / 2,
                    });
                }
                else
                {
                    existentialEntryCount++;
                }
            }

            //添加总计存在的词条 分数
            if (existentialEntryCount > 0)
            {
                results.Add(new PerfectionCheck
                {
                    CheckType = PerfectionCheckType.RelevanceEntries,
                    DefectType = PerfectionDefectType.None,
                    Grade = score * existentialEntryCount,
                });
            }

            //判断文章 
            var articles = relevances.Where(s => s.Modifier == "文章" || s.Modifier == "动态").Select(s => s.DisplayName).ToList();
            //拿到存在和不存在的名单
            var existentialArticles = await _articleRepository.GetAll().Where(s => articles.Contains(s.Name)).Select(s => s.Name).ToListAsync();

            var existentialArticleCount = 0;

            foreach (var item in articles)
            {
                if (existentialArticles.Any(s => s.ToLower() == item.ToLower()))
                {
                    existentialArticleCount++;

                }
                else
                {
                    results.Add(new PerfectionCheck
                    {
                        CheckType = PerfectionCheckType.RelevanceArticles,
                        DefectType = PerfectionDefectType.NonExistent,
                        Infor = item,
                        Grade = score / 2,
                    });
                }
            }

            //添加总计存在的词条 分数
            if (existentialArticleCount > 0)
            {
                results.Add(new PerfectionCheck
                {
                    CheckType = PerfectionCheckType.RelevanceArticles,
                    DefectType = PerfectionDefectType.None,
                    Grade = score * existentialArticleCount,
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
            var officialWebsite = entry.Information.FirstOrDefault(s => (s.Modifier == "基本信息" || s.Modifier == "相关网站") && s.DisplayName == "官网");
            if (officialWebsite != null && string.IsNullOrWhiteSpace(officialWebsite.DisplayValue) == false)
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
            var gamePlatforms = entry.Information.FirstOrDefault(s => s.Modifier == "基本信息" && s.DisplayName == "游戏平台");
            if (gamePlatforms != null && string.IsNullOrWhiteSpace(gamePlatforms.DisplayValue) == false)
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
            var issueTime = entry.Information.FirstOrDefault(s => s.Modifier == "基本信息" && (s.DisplayName == "发行时间" || s.DisplayName == "发行时间备注") && string.IsNullOrWhiteSpace(s.DisplayValue) == false);
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

        public async Task<List<PerfectionCheck>> CheckEntryPublisher(Entry entry)
        {
            try
            {
                var results = new List<PerfectionCheck>();

                var publisher = entry.Information.FirstOrDefault(s => s.Modifier == "基本信息" && s.DisplayName == "发行商");
                if (publisher == null || string.IsNullOrWhiteSpace(publisher.DisplayValue))
                {
                    return new List<PerfectionCheck>{
                        new PerfectionCheck
                        {
                            CheckType = PerfectionCheckType.Publisher,
                            DefectType = PerfectionDefectType.NotFilledIn,
                            Grade = 0,
                        }
                    };
                }
                var publishers = publisher.DisplayValue.Replace("，", ",").Replace("、", ",").Split(",").ToList();
                publishers.RemoveAll(s => string.IsNullOrWhiteSpace(s));

                if (publishers.Count == 0)
                {
                    return new List<PerfectionCheck>{
                        new PerfectionCheck
                        {
                            CheckType = PerfectionCheckType.Publisher,
                            DefectType = PerfectionDefectType.NotFilledIn,
                            Grade = 0,
                        }
                    };
                }


                ToolHelper.Purge(ref publishers);

                //拿到存在和不存在的名单
                var existentialStaffs = await _entryRepository.GetAll().Where(s => publishers.Contains(s.Name)).Select(s => new { s.Name, s.Type }).ToListAsync();

                var existentialCount = 0;

                var score = (double)2 / publishers.Count;
                foreach (var item in publishers)
                {
                    var temp = existentialStaffs.FirstOrDefault(s => s.Name.ToLower() == item.ToLower());
                    if (temp == null)
                    {
                        results.Add(new PerfectionCheck
                        {
                            CheckType = PerfectionCheckType.Publisher,
                            DefectType = PerfectionDefectType.NonExistent,
                            Infor = item,
                            Grade = score / 2,
                        });

                    }
                    else if (temp.Type != EntryType.Staff && temp.Type != EntryType.ProductionGroup)
                    {
                        results.Add(new PerfectionCheck
                        {
                            CheckType = PerfectionCheckType.Publisher,
                            DefectType = PerfectionDefectType.TypeError,
                            Infor = item,
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
                        CheckType = PerfectionCheckType.Publisher,
                        DefectType = PerfectionDefectType.None,
                        Grade = score * existentialCount,
                    });
                }

                return results;

            }
            catch (Exception)
            {
                return null;
            }

        }

        public async Task<List<PerfectionCheck>> CheckEntryProductionGroup(Entry entry)
        {
            var results = new List<PerfectionCheck>();

            var publisher = entry.Information.FirstOrDefault(s => s.Modifier == "基本信息" && s.DisplayName == "制作组");

            if (publisher == null || string.IsNullOrWhiteSpace(publisher.DisplayValue))
            {
                return new List<PerfectionCheck>{
                        new PerfectionCheck
                        {
                            CheckType = PerfectionCheckType.ProductionGroup,
                            DefectType = PerfectionDefectType.NotFilledIn,
                            Grade = 0,
                        }
                    };
            }

            var publishers = publisher.DisplayValue.Replace("，", ",").Replace("、", ",").Split(",").ToList();
            publishers.RemoveAll(s => string.IsNullOrWhiteSpace(s));

            if (publishers.Count == 0)
            {
                return new List<PerfectionCheck>{
                    new PerfectionCheck
                    {
                        CheckType = PerfectionCheckType.ProductionGroup,
                        DefectType = PerfectionDefectType.NotFilledIn,
                        Grade = 0,
                    }
                };
            }


            ToolHelper.Purge(ref publishers);

            //拿到存在和不存在的名单
            var existentialStaffs = await _entryRepository.GetAll().Where(s => publishers.Contains(s.Name)).Select(s => new { s.Name, s.Type }).ToListAsync();

            var existentialCount = 0;

            var score = (double)5 / publishers.Count;
            foreach (var item in publishers)
            {
                var temp = existentialStaffs.FirstOrDefault(s => s.Name.ToLower() == item.ToLower());
                if (temp == null)
                {
                    results.Add(new PerfectionCheck
                    {
                        CheckType = PerfectionCheckType.ProductionGroup,
                        DefectType = PerfectionDefectType.NonExistent,
                        Infor = item,
                        Grade = score / 2,
                    });

                }
                else if (temp.Type != EntryType.Staff && temp.Type != EntryType.ProductionGroup)
                {
                    results.Add(new PerfectionCheck
                    {
                        CheckType = PerfectionCheckType.ProductionGroup,
                        DefectType = PerfectionDefectType.TypeError,
                        Infor = item,
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
                    CheckType = PerfectionCheckType.ProductionGroup,
                    DefectType = PerfectionDefectType.None,
                    Grade = score * existentialCount,
                });
            }

            return results;
        }

        public PerfectionCheck CheckEntrySteamId(Entry entry)
        {
            var steamId = entry.Information.FirstOrDefault(s => s.Modifier == "基本信息" && s.DisplayName == "Steam平台Id");
            if (steamId != null && string.IsNullOrWhiteSpace(steamId.DisplayValue) == false)
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

        public async Task<List<PerfectionCheck>> CheckEntryStaff(Entry entry)
        {
            var results = new List<PerfectionCheck>();
            //获取所有staff
            var staffs = entry.Information.ToList().Where(s => s.Modifier == "STAFF").Select(s => s.DisplayValue).ToList();


            if (staffs.Count == 0)
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

            ToolHelper.Purge(ref staffs);

            //拿到存在和不存在的名单
            var existentialStaffs = await _entryRepository.GetAll().Where(s => staffs.Contains(s.Name)).Select(s => new { s.Name, s.Type }).ToListAsync();

            var existentialCount = 0;

            var score = (double)20 / staffs.Count;
            foreach (var item in staffs)
            {
                var temp = existentialStaffs.FirstOrDefault(s => s.Name.ToLower() == item.ToLower());
                if (temp == null)
                {
                    results.Add(new PerfectionCheck
                    {
                        CheckType = PerfectionCheckType.Staff,
                        DefectType = PerfectionDefectType.NonExistent,
                        Infor = item,
                        Grade = score / 2,
                    });

                }
                else if (temp.Type != EntryType.Staff && temp.Type != EntryType.ProductionGroup)
                {
                    results.Add(new PerfectionCheck
                    {
                        CheckType = PerfectionCheckType.Staff,
                        DefectType = PerfectionDefectType.TypeError,
                        Infor = item,
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
                    await _perfectionCheckRepository.GetRangeUpdateTable()
                        .Where(s => s.Infor == item.Key)
                        .Set(s => s.Count, b => item.Count)
                        .ExecuteAsync();
                }

                //计算每个词条完善度总分超过全站的百分比
                var grades = await _perfectionRepository.GetAll().Select(s => new { s.Id, s.Grade }).ToListAsync();
                var count = grades.Count;
                foreach (var item in grades)
                {
                    var temp = 100 * (double)grades.Count(s => s.Grade < item.Grade) / count;
                    await _perfectionRepository.GetRangeUpdateTable().Where(s => s.Id == item.Id).Set(s => s.VictoryPercentage, b => temp).ExecuteAsync();

                }

                //计算每个已完成的明细检查 全站的已完成百分比
                var checkCounts = await _perfectionCheckRepository.GetAll()
                     .Where(s => s.DefectType == PerfectionDefectType.None)
                    .Select(s => new { s.CheckType })
                    .GroupBy(s => s.CheckType)
                    .Select(s => new { s.Key, Count = s.Count() })
                    .ToListAsync();

                foreach (var item in checkCounts)
                {
                    await _perfectionCheckRepository.GetRangeUpdateTable()
                        .Where(s => s.CheckType == item.Key)
                        .Set(s => s.VictoryPercentage, b => 100 * (double)item.Count / count)
                        .ExecuteAsync();
                }

            }
            catch (Exception)
            {
                return;
            }
        }

        public async Task<PerfectionLevelOverviewModel> GetPerfectionLevelOverview()
        {
            var grades = await _perfectionRepository.GetAll().OrderBy(s => s.Grade).Select(s => s.Grade).ToListAsync();

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
                perfection.LastUpdateTime = nowTime;
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
