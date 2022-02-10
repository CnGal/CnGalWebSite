using BootstrapBlazor.Components;
using CnGalWebSite.APIServer.Application.Articles;
using CnGalWebSite.APIServer.Application.Disambigs;
using CnGalWebSite.APIServer.Application.Entries;
using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.Application.Perfections;
using CnGalWebSite.APIServer.Application.Peripheries;
using CnGalWebSite.APIServer.Application.Ranks;
using CnGalWebSite.APIServer.Application.Tags;
using CnGalWebSite.APIServer.Application.Users;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.DataModel.Application.Dtos;
using CnGalWebSite.DataModel.ExamineModel;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.ImportModel;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Disambig;
using Markdig;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using TencentCloud.Cme.V20191029.Models;
using Markdown = Markdig.Markdown;
using Tag = CnGalWebSite.DataModel.Model.Tag;

namespace CnGalWebSite.APIServer.ExamineX
{
    public class ExamineService : IExamineService
    {
        private readonly IRepository<Examine, int> _examineRepository;
        private readonly IAppHelper _appHelper;
        private readonly IRepository<Disambig, int> _disambigRepository;
        private readonly IRepository<DataModel.Model.Tag, int> _tagRepository;
        private readonly IRepository<Article, long> _articleRepository;
        private readonly IRepository<Comment, long> _commentRepository;
        private readonly IRepository<Entry, int> _entryRepository;
        private readonly IRepository<Periphery, long> _peripheryRepository;
        private readonly IRepository<ApplicationUser, string> _userRepository;
        private readonly IEntryService _entryService;
        private readonly IPeripheryService _peripheryService;
        private readonly IArticleService _articleService;
        private readonly ITagService _tagService;
        private readonly IDisambigService _disambigService;
        private readonly IUserService _userService;
        private readonly IRankService _rankService;
        private readonly IPerfectionService _perfectionService;
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;

        private static readonly ConcurrentDictionary<Type, Func<IEnumerable<Examine>, string, SortOrder, IEnumerable<Examine>>> SortLambdaCacheEntry = new();

        public ExamineService(IRepository<Examine, int> examineRepository, IAppHelper appHelper, IRepository<Entry, int> entryRepository, IRankService rankService, IPerfectionService perfectionService,
        IArticleService articleService, ITagService tagService, IDisambigService disambigService, IUserService userService, IRepository<ApplicationUser, string> userRepository,
        IRepository<Article, long> articleRepository, IRepository<DataModel.Model.Tag, int> tagRepository, IEntryService entryService, IPeripheryService peripheryService,
        IRepository<Comment, long> commentRepository, IRepository<Disambig, int> disambigRepository, IRepository<Periphery, long> peripheryRepository,
        IConfiguration configuration, UserManager<ApplicationUser> userManager)
        {
            _examineRepository = examineRepository;
            _appHelper = appHelper;
            _articleRepository = articleRepository;
            _tagRepository = tagRepository;
            _commentRepository = commentRepository;
            _disambigRepository = disambigRepository;
            _entryRepository = entryRepository;
            _entryService = entryService;
            _articleService = articleService;
            _tagService = tagService;
            _disambigService = disambigService;
            _userService = userService;
            _userRepository = userRepository;
            _rankService = rankService;
            _perfectionService = perfectionService;
            _peripheryRepository = peripheryRepository;
            _peripheryService = peripheryService;
            _configuration = configuration;
            _userManager = userManager;
        }

        public async Task<PagedResultDto<ExaminedNormalListModel>> GetPaginatedResult(GetExamineInput input, int entryId = 0, string userId = "")
        {
            IQueryable<Examine> query = null;
            if (entryId > 0)
            {
                query = _examineRepository.GetAll().AsNoTracking().Where(s => s.EntryId == entryId && s.IsPassed == true);
            }
            else if (string.IsNullOrWhiteSpace(userId) == false)
            {
                query = _examineRepository.GetAll().AsNoTracking().Where(s => s.ApplicationUserId == userId);

            }
            else
            {
                query = _examineRepository.GetAll().AsNoTracking();
            }

            //判断是否是条件筛选
            if (!string.IsNullOrWhiteSpace(input.ScreeningConditions))
            {
                switch (input.ScreeningConditions)
                {
                    case "待审核":
                        query = query.Where(s => s.IsPassed == null);
                        break;
                    case "已通过":
                        query = query.Where(s => s.IsPassed == true);
                        break;
                    case "未通过":
                        query = query.Where(s => s.IsPassed == false);
                        break;

                }
            }
            //判断输入的查询名称是否为空
            if (!string.IsNullOrWhiteSpace(input.FilterText))
            {
                //尝试将查询翻译成操作
                var operation = Operation.None;
                switch (input.FilterText)
                {
                    case "修改用户主页":
                        operation = Operation.UserMainPage;
                        break;
                    case "编辑词条主要信息":
                        operation = Operation.EstablishMain;
                        break;
                    case "编辑词条附加信息":
                        operation = Operation.EstablishAddInfor;
                        break;
                    case "编辑词条主页":
                        operation = Operation.EstablishMainPage;
                        break;
                    case "编辑词条图片":
                        operation = Operation.EstablishImages;
                        break;
                    case "编辑词条相关链接":
                        operation = Operation.EstablishRelevances;
                        break;
                    case "编辑词条标签":
                        operation = Operation.EstablishTags;
                        break;
                    case "编辑文章主要信息":
                        operation = Operation.EditArticleMain;
                        break;
                    case "编辑文章关联词条":
                        operation = Operation.EditArticleRelevanes;
                        break;
                    case "编辑文章内容":
                        operation = Operation.EditArticleMainPage;
                        break;
                }
                query = query.Where(s => s.ApplicationUser.UserName.Contains(input.FilterText)
                  || s.Context.Contains(input.FilterText)
                  || (s.Entry != null && s.Entry.Name.Contains(input.FilterText))
                  || (s.Article != null && s.Article.Name.Contains(input.FilterText))
                  || s.Operation == operation);
            }
            //统计查询数据的总条数
            var count = query.Count();
            //根据需求进行排序，然后进行分页逻辑的计算
            if (input.IsVisual)
            {
                query = query.OrderBy(input.Sorting).Skip(input.CurrentPage).Take(input.MaxResultCount);
            }
            else
            {
                query = query.OrderBy(input.Sorting).Skip((input.CurrentPage - 1) * input.MaxResultCount).Take(input.MaxResultCount);

            }

            //将结果转换为List集合 加载到内存中
            List<ExaminedNormalListModel> models = null;
            if (count != 0)
            {
                models = await GetExaminesToNormalListAsync(query, false);
            }
            else
            {
                models = new List<ExaminedNormalListModel>();
            }
            var dtos = new PagedResultDto<ExaminedNormalListModel>
            {
                TotalCount = count,
                CurrentPage = input.CurrentPage,
                MaxResultCount = input.MaxResultCount,
                Data = models,
                FilterText = input.FilterText,
                Sorting = input.Sorting,
                ScreeningConditions = input.ScreeningConditions
            };
            return dtos;
        }

        public async Task<QueryData<ListExamineAloneModel>> GetPaginatedResult(CnGalWebSite.DataModel.ViewModel.Search.QueryPageOptions options, ListExamineAloneModel searchModel)
        {
            IQueryable<Examine> items = _examineRepository.GetAll().Include(s => s.ApplicationUser).AsNoTracking();
            // 处理高级搜索
            if (!string.IsNullOrWhiteSpace(searchModel.EntryId?.ToString()))
            {
                items = items.Where(item => item.EntryId.ToString().Contains(searchModel.EntryId.ToString(), StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(searchModel.ArticleId?.ToString()))
            {
                items = items.Where(item => item.ArticleId.ToString().Contains(searchModel.ArticleId.ToString(), StringComparison.OrdinalIgnoreCase) );
            }

            if (!string.IsNullOrWhiteSpace(searchModel.TagId?.ToString()))
            {
                items = items.Where(item => item.TagId.ToString().Contains(searchModel.TagId.ToString(), StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(searchModel.ApplicationUserId?.ToString()))
            {
                items = items.Where(item => item.ApplicationUserId.ToString().Contains(searchModel.ApplicationUserId.ToString(), StringComparison.OrdinalIgnoreCase) );
            }
            if (!string.IsNullOrWhiteSpace(searchModel.Comments))
            {
                items = items.Where(item => item.Comments.Contains(searchModel.Comments, StringComparison.OrdinalIgnoreCase));
            }
            if (searchModel.Operation != null)
            {
                items = items.Where(item => item.Operation == searchModel.Operation);
            }



            // 处理 SearchText 模糊搜索
            if (!string.IsNullOrWhiteSpace(options.SearchText))
            {
                items = items.Where(item => (item.EntryId.ToString().Contains(options.SearchText) )
                             || (item.EntryId.ToString().Contains(options.SearchText))
                             || (item.ArticleId.ToString().Contains(options.SearchText) )
                             || (item.TagId.ToString().Contains(options.SearchText) )
                             || (item.ApplicationUserId.ToString().Contains(options.SearchText)));
            }

            // 排序
            var isSorted = false;
            if (!string.IsNullOrWhiteSpace(options.SortName))
            {

                items=items.Sort(options.SortName, (SortOrder)options.SortOrder);
                isSorted = true;
            }

            // 设置记录总数
            var total = items.Count();

            // 内存分页
            var itemsReal =await items.Skip((options.PageIndex - 1) * options.PageItems).Take(options.PageItems).ToListAsync();

            //复制数据
            var resultItems = new List<ListExamineAloneModel>();
            foreach (var item in itemsReal)
            {
                resultItems.Add(new ListExamineAloneModel
                {
                    Id = item.Id,
                    Operation = item.Operation,
                    IsPassed = item.IsPassed,
                    PassedTime = item.PassedTime,
                    ApplyTime = item.ApplyTime,
                    Comments = item.Comments,
                    ApplicationUserId = item.ApplicationUserId,
                    UserName = item.ApplicationUser.UserName,
                    EntryId = item.EntryId,
                    TagId = item.TagId,
                    CommentId = item.CommentId,
                    PassedAdminName = item.PassedAdminName,
                    ArticleId = item.ArticleId,
                    ContributionValue = item.ContributionValue
                });
            }

            return new QueryData<ListExamineAloneModel>()
            {
                Items = resultItems,
                TotalCount = total,
                IsSorted = isSorted,
                // IsFiltered = isFiltered
            };
        }



        public async Task<bool> GetExamineView(Models.ExaminedViewModel model, Examine examine)
        {
            return examine.Operation switch
            {
                Operation.UserMainPage => GetUserMainPageExamineView(model, examine),
                Operation.EditUserMain => await GetEditUserMainExamineView(model, examine),
                Operation.EstablishMain => await GetEstablishMainExamineView(model, examine),
                Operation.EstablishAddInfor => await GetEstablishAddInforExamineView(model, examine),
                Operation.EstablishImages => await GetEstablishImagesExamineView(model, examine),
                Operation.EstablishRelevances => await GetEstablishRelevancesExamineView(model, examine),
                Operation.EstablishTags => await GetEstablishTagsExamineView(model, examine),
                Operation.EstablishMainPage => await GetEstablishMainPageExamineView(model, examine),
                Operation.EditArticleMain => await GetEditArticleMainExamineView(model, examine),
                Operation.EditArticleRelevanes => await GetEditArticleRelevanesExamineView(model, examine),
                Operation.EditArticleMainPage => await GetEditArticleMainPageExamineView(model, examine),
                Operation.EditTag => await GetEditTagExamineView(model, examine),
                Operation.PubulishComment => await GetPubulishCommentExamineView(model, examine),
                Operation.DisambigMain => await GetDisambigMainExamineView(model, examine),
                Operation.DisambigRelevances => await GetDisambigRelevancesExamineView(model, examine),
                Operation.EditPeripheryMain => await GetEditPeripheryMainExamineView(model, examine),
                Operation.EditPeripheryImages => await GetEditPeripheryImagesExamineView(model, examine),
                Operation.EditPeripheryRelatedEntries => await GetEditPeripheryRelatedEntriesExamineView(model, examine),
                Operation.EditPeripheryRelatedPeripheries => await GetEditPeripheryRelatedPeripheriesExamineView(model, examine),
                Operation.EditTagMain => await GetEditTagMainExamineView(model, examine),
                Operation.EditTagChildTags => await GetEditTagChildTagsExamineView(model, examine),
                Operation.EditTagChildEntries => await GetEditTagChildEntriesExamineView(model, examine),
                _ => false,
            };
        }

        public async Task<List<ExaminedNormalListModel>> GetExaminesToNormalListAsync(IQueryable<Examine> examines, bool isShowRanks)
        {
            var temp = await examines.AsNoTracking()
                          .Include(s => s.ApplicationUser)
                          .Include(s => s.Entry)
                          .Include(s => s.Tag)
                          .Include(s => s.Article)
                          .Include(s => s.Periphery)
                          .Select(n => new
                          {
                              n.Id,
                              n.ApplyTime,
                              n.ApplicationUser.PhotoPath,
                              n.PassedTime,
                              n.EntryId,
                              n.ArticleId,
                              n.TagId,
                              n.CommentId,
                              n.DisambigId,
                              n.PeripheryId,
                              TagName = n.Tag == null ? "" : n.Tag.Name,
                              EntryName = n.Entry == null ? "" : n.Entry.Name,
                              ArticleName = n.Article == null ? "" : n.Article.Name,
                              DisambigName = n.Disambig == null ? "" : n.Disambig.Name,
                              PeripheryName = n.Periphery == null ? "" : n.Periphery.Name,
                              UserId = n.ApplicationUserId,
                              n.ApplicationUser.UserName,
                              n.Operation,
                              n.IsPassed,
                          })
                          .ToListAsync();
            var result = new List<ExaminedNormalListModel>();
            foreach (var item in temp)
            {
                var tempModel = new ExaminedNormalListModel
                {
                    Id = item.Id,
                    ApplyTime = item.ApplyTime,
                    PassedTime = item.PassedTime,
                    UserId = item.UserId,
                    UserName = item.UserName,
                    Operation = item.Operation,
                    IsPassed = item.IsPassed,
                    UserImage = _appHelper.GetImagePath(item.PhotoPath, "user.png"),
                    Ranks = isShowRanks ? (await _rankService.GetUserRanks(item.UserId)).Where(s => s.Name != "编辑者").ToList() : new List<DataModel.ViewModel.Ranks.RankViewModel>()
                };
                tempModel.RelatedId = item.EntryId != null ? item.EntryId.ToString() : (item.ArticleId != null ? item.ArticleId.ToString() : (item.TagId != null ? item.TagId.ToString() : (item.CommentId != null ? item.CommentId.ToString() : (item.DisambigId != null ? item.DisambigId.ToString() : (item.PeripheryId != null ? item.PeripheryId.ToString() : "")))));
                tempModel.Type = item.EntryId != null ? ExaminedNormalListModelType.Entry : (item.ArticleId != null ? ExaminedNormalListModelType.Article : (item.TagId != null ? ExaminedNormalListModelType.Tag : (item.CommentId != null ? ExaminedNormalListModelType.Comment : (item.DisambigId != null ? ExaminedNormalListModelType.Disambig : (item.PeripheryId != null ? ExaminedNormalListModelType.Periphery : ExaminedNormalListModelType.User)))));
                tempModel.RelatedName = item.EntryId != null ? item.EntryName : (item.ArticleId != null ? item.ArticleName : (item.TagId != null ? item.TagName : (item.DisambigId != null ? item.DisambigName : (item.PeripheryId != null ? item.PeripheryName : ""))));
                result.Add(tempModel);
            }

            return result;
        }

        #region 获取审核记录视图

        #region 词条

        private EntryMain_1_0 InitExamineViewEntryMain(Entry entry)
        {
            var entryMainBefore = new EntryMain_1_0
            {
                Name = entry.Name,
                DisplayName = entry.DisplayName,
                AnotherName = entry.AnotherName,
                BriefIntroduction = entry.BriefIntroduction,
                MainPicture = _appHelper.GetImagePath(entry.MainPicture, "app.png"),
                Thumbnail = _appHelper.GetImagePath(entry.Thumbnail, "app.png"),
                BackgroundPicture = _appHelper.GetImagePath(entry.BackgroundPicture, "background.png"),
                SmallBackgroundPicture = _appHelper.GetImagePath(entry.SmallBackgroundPicture, "background.png"),
                Type = entry.Type
            };
            return entryMainBefore;
        }

        public async Task<bool> GetEstablishMainExamineView(Models.ExaminedViewModel model, Examine examine)
        {
            model.Type = "词条";
            var entry = await _entryRepository.FirstOrDefaultAsync(s => s.Id == examine.EntryId);
            if (entry == null)
            {
                return false;
            }
            model.EntryId = (int)examine.EntryId;
            model.EntryName = entry.Name;

            var entryMainBefore = InitExamineViewEntryMain(entry);

            //添加修改记录 
            await _entryService.UpdateEntryDataAsync(entry, examine);

            //序列化数据
            var entryMain = InitExamineViewEntryMain(entry);
            //json格式化
            model.EditOverview = _appHelper.GetJsonStringView(examine.Context);

            //判断是否是等待审核状态
            if (examine.IsPassed != null)
            {
                model.BeforeModel = entryMain;
                model.AfterModel = entryMainBefore;
            }
            else
            {
                model.AfterModel = entryMain;
                model.BeforeModel = entryMainBefore;
            }
            return true;
        }

        private static List<InformationsModel> InitExamineViewEntryAddinfor(Entry entry)
        {
            //当前词条的视图模型
            var information = new List<InformationsModel>();
            foreach (var item in entry.Information)
            {
                var isAdd = false;
                //如果信息值为空 则不显示
                if (string.IsNullOrWhiteSpace(item.DisplayValue) == true)
                {
                    continue;
                }
                //遍历信息列表寻找关键词
                foreach (var infor in information)
                {
                    if (infor.Modifier == item.Modifier)
                    {
                        //关键词相同则添加
                        infor.Informations.Add(new KeyValueModel
                        {
                            DisplayName = item.DisplayName,
                            DisplayValue = item.DisplayValue
                        });
                        isAdd = true;
                        break;
                    }
                }
                if (isAdd == false)
                {
                    //没有找到关键词 则新建关键词
                    var temp = new InformationsModel
                    {
                        Modifier = item.Modifier,
                        Informations = new List<KeyValueModel>()
                    };
                    temp.Informations.Add(new KeyValueModel
                    {
                        DisplayName = item.DisplayName,
                        DisplayValue = item.DisplayValue
                    });
                    information.Add(temp);
                }

            }
            return information;
        }

        public async Task<bool> GetEstablishAddInforExamineView(Models.ExaminedViewModel model, Examine examine)
        {
            model.Type = "词条";
            var entry = await _entryRepository.GetAll()
                   .Include(s => s.Information)
                     .ThenInclude(s => s.Additional)
                   .FirstOrDefaultAsync(s => s.Id == examine.EntryId);
            if (entry == null)
            {
                return false;
            }
            model.EntryId = (int)examine.EntryId;
            model.EntryName = examine.Entry.Name;
            //反序列化数据 用Json对比
            //新建审核数据对象
            var entryAddInfor = new EntryAddInfor
            {
                Information = new List<BasicEntryInformation_>()
            };

            //当前词条的视图模型
            var information = InitExamineViewEntryAddinfor(entry);

            //添加修改记录 
            await _entryService.UpdateEntryDataAsync(entry, examine);

            var information_examine = InitExamineViewEntryAddinfor(entry);

            //json格式化
            model.EditOverview = _appHelper.GetJsonStringView(examine.Context);

            //判断是否是等待审核状态
            if (examine.IsPassed != null)
            {
                model.BeforeModel = information_examine;

                model.AfterModel = information;
            }
            else
            {
                model.BeforeModel = information;
                model.AfterModel = information_examine;
            }
            return true;
        }

        private static List<EntryPicture> InitExamineViewEntryImages(Entry entry)
        {
            var pictures = new List<EntryPicture>();
            foreach (var item in entry.Pictures)
            {
                pictures.Add(new EntryPicture
                {
                    Url = item.Url,
                    Note = item.Note,
                    Modifier = item.Modifier
                });
            }

            return pictures;
        }

        public async Task<bool> GetEstablishImagesExamineView(Models.ExaminedViewModel model, Examine examine)
        {
            model.Type = "词条";
            var entry = await _entryRepository.GetAll()
                  .Include(s => s.Pictures)
                  .FirstOrDefaultAsync(s => s.Id == examine.EntryId);
            if (entry == null)
            {
                return false;
            }
            model.EntryId = (int)examine.EntryId;
            model.EntryName = examine.Entry.Name;

            var pictures = InitExamineViewEntryImages(entry);

            //添加修改记录 
            await _entryService.UpdateEntryDataAsync(entry, examine);

            var pictures_examine = InitExamineViewEntryImages(entry);

            //json格式化
            model.EditOverview = _appHelper.GetJsonStringView(examine.Context);

            //判断是否是等待审核状态
            if (examine.IsPassed != null)
            {
                model.BeforeModel = pictures_examine;
                model.AfterModel = pictures;
            }
            else
            {
                model.BeforeModel = pictures;
                model.AfterModel = pictures_examine;
            }
            return true;
        }

        private static List<RelevancesViewModel> InitExamineViewEntryRelevances(Entry entry)
        {
            var relevances = new List<RelevancesViewModel>();
            if (entry.Articles.Count > 0)
            {
                var temp = new List<RelevancesKeyValueModel>();
                relevances.Add(new RelevancesViewModel
                {
                    Informations = temp,
                    Modifier = "文章"
                });
                foreach (var item in entry.Articles)
                {
                    temp.Add(new RelevancesKeyValueModel
                    {
                        DisplayName = item.DisplayName,
                        DisplayValue = item.BriefIntroduction,
                        Link = "/articles/index/" + item.Id
                    });
                }
            }
            if (entry.EntryRelationFromEntryNavigation.Count > 0)
            {

                var temp = new List<RelevancesKeyValueModel>();
                relevances.Add(new RelevancesViewModel
                {
                    Informations = temp,
                    Modifier = "词条"
                });
                foreach (var nav in entry.EntryRelationFromEntryNavigation)
                {
                    var item = nav.ToEntryNavigation;
                    temp.Add(new RelevancesKeyValueModel
                    {
                        DisplayName = item.DisplayName,
                        DisplayValue = item.BriefIntroduction,
                        Link = "/entries/index/" + item.Id
                    });
                }
            }
            if (entry.Outlinks.Count > 0)
            {

                var temp = new List<RelevancesKeyValueModel>();
                relevances.Add(new RelevancesViewModel
                {
                    Informations = temp,
                    Modifier = "外部链接"
                });
                foreach (var item in entry.Outlinks)
                {
                    temp.Add(new RelevancesKeyValueModel
                    {
                        DisplayName = item.Name,
                        DisplayValue = item.BriefIntroduction,
                        Link = item.Link
                    });
                }
            }

            return relevances;
        }

        public async Task<bool> GetEstablishRelevancesExamineView(Models.ExaminedViewModel model, Examine examine)
        {
            model.Type = "词条";
            var entry = await _entryRepository.GetAll()
                    .Include(s => s.Outlinks)
                    .Include(s => s.EntryRelationFromEntryNavigation).ThenInclude(s => s.ToEntryNavigation)
                    .Include(s => s.Articles)
                    .ThenInclude(s => s.CreateUser)
                    .FirstOrDefaultAsync(s => s.Id == examine.EntryId);
            if (entry == null)
            {

                return false;
            }
            model.EntryId = (int)examine.EntryId;
            model.EntryName = examine.Entry.Name;
            //序列化相关性列表
            //先读取词条信息
            var relevances = InitExamineViewEntryRelevances(entry);

            //添加修改记录 
            await _entryService.UpdateEntryDataAsync(entry, examine);

            var relevances_examine = InitExamineViewEntryRelevances(entry);

            //json格式化
            model.EditOverview = _appHelper.GetJsonStringView(examine.Context);

            //判断是否是等待审核状态
            if (examine.IsPassed != null)
            {
                model.BeforeModel = relevances_examine;
                model.AfterModel = relevances;
            }
            else
            {
                model.BeforeModel = relevances;
                model.AfterModel = relevances_examine;
            }
            return true;
        }

        private static List<RelevancesViewModel> InitExamineViewEntryTags(Entry entry)
        {
            var relevances = new List<RelevancesViewModel>();
            var tags = entry.Tags;
            foreach (var item in tags)
            {
                var isAdd = false;

                //遍历信息列表寻找关键词
                foreach (var infor in relevances)
                {
                    if (infor.Modifier == "标签")
                    {
                        //关键词相同则添加
                        infor.Informations.Add(new RelevancesKeyValueModel
                        {
                            DisplayName = item.Name,
                            DisplayValue = item.BriefIntroduction,
                            Id = item.Id
                        });
                        isAdd = true;
                        break;
                    }
                }
                if (isAdd == false)
                {
                    //没有找到关键词 则新建关键词
                    var temp = new RelevancesViewModel
                    {
                        Modifier = "标签",
                        Informations = new List<RelevancesKeyValueModel>()
                    };
                    temp.Informations.Add(new RelevancesKeyValueModel
                    {
                        DisplayName = item.Name,
                        DisplayValue = item.BriefIntroduction,
                        Id = item.Id

                    });
                    relevances.Add(temp);
                }
            }
            return relevances;
        }

        public async Task<bool> GetEstablishTagsExamineView(Models.ExaminedViewModel model, Examine examine)
        {
            model.Type = "词条";
            var entry = await _entryRepository.GetAll().Include(s => s.Tags).FirstOrDefaultAsync(s => s.Id == examine.EntryId);
            if (entry == null)
            {
                return false;
            }
            model.EntryId = (int)examine.EntryId;
            model.EntryName = entry.Name;

            var tags = InitExamineViewEntryTags(entry);

            //添加修改记录 
            await _entryService.UpdateEntryDataAsync(entry, examine);

            var tags_examine = InitExamineViewEntryTags(entry);

            //json格式化
            model.EditOverview = _appHelper.GetJsonStringView(examine.Context);

            //判断是否是等待审核状态
            if (examine.IsPassed != null)
            {
                model.BeforeModel = tags_examine;
                model.AfterModel = tags;
            }
            else
            {
                model.BeforeModel = tags;
                model.AfterModel = tags_examine;
            }

            return true;
        }

        public async Task<bool> GetEstablishMainPageExamineView(Models.ExaminedViewModel model, Examine examine)
        {
            model.Type = "词条";
            var entry = await _entryRepository.FirstOrDefaultAsync(s => s.Id == examine.EntryId);
            if (entry == null)
            {

                return false;
            }
            model.EntryId = (int)examine.EntryId;
            model.EntryName = entry.Name;

            var mainpage = entry.MainPage;

            await _entryService.UpdateEntryDataAsync(entry, examine);

            var mainpage_examine = entry.MainPage;

            //判断是否是等待审核状态
            if (examine.IsPassed != null)
            {
                //序列化数据
                var htmlDiff = new HtmlDiff.HtmlDiff(mainpage_examine ?? "", mainpage ?? "");
                model.EditOverview = htmlDiff.Build().Replace("\r\n", "<br>");
                model.BeforeText = _appHelper.MarkdownToHtml(examine.Context);
                model.AfterText = _appHelper.MarkdownToHtml(mainpage_examine);
            }
            else
            {
                //序列化数据
                var htmlDiff = new HtmlDiff.HtmlDiff(mainpage ?? "", mainpage_examine ?? "");
                model.EditOverview = htmlDiff.Build().Replace("\r\n", "<br>");
                model.BeforeText = _appHelper.MarkdownToHtml(mainpage);
                model.AfterText = _appHelper.MarkdownToHtml(examine.Context);
            }
            return true;
        }

        #endregion

        #region 文章

        private ArticleMain_1_0 InitExamineViewArticleMain(Article article)
        {
            var articleMainBefore = new ArticleMain_1_0
            {
                Name = article.Name,
                BriefIntroduction = article.BriefIntroduction,
                MainPicture = _appHelper.GetImagePath(article.MainPicture, "app.png"),
                BackgroundPicture = _appHelper.GetImagePath(article.BackgroundPicture, "app.png"),
                SmallBackgroundPicture = _appHelper.GetImagePath(article.SmallBackgroundPicture, "app.png"),
                Type = article.Type,
                OriginalLink = article.OriginalLink,
                OriginalAuthor = article.OriginalAuthor,
                PubishTime = article.PubishTime,
                DisplayName = article.DisplayName,
                RealNewsTime = article.RealNewsTime,
                NewsType = article.NewsType,
            };

            return articleMainBefore;
        }

        public async Task<bool> GetEditArticleMainExamineView(Models.ExaminedViewModel model, Examine examine)
        {
            model.Type = "文章";
            var article = await _articleRepository.FirstOrDefaultAsync(s => s.Id == examine.ArticleId);
            if (article == null)
            {
                return false;
            }
            model.EntryId = article.Id;
            model.EntryName = article.Name;

            var articleMainBefore = InitExamineViewArticleMain(article);

            //添加修改记录 
            await _articleService.UpdateArticleData(article, examine);

            //序列化数据
            var articleMain = InitExamineViewArticleMain(article);

            //json格式化
            model.EditOverview = _appHelper.GetJsonStringView(examine.Context);
            //判断是否是等待审核状态
            if (examine.IsPassed != null)
            {
                model.BeforeModel = articleMain;
                model.AfterModel = articleMainBefore;
            }
            else
            {
                model.AfterModel = articleMain;
                model.BeforeModel = articleMainBefore;
            }

            return true;
        }

        private static List<RelevancesViewModel> InitExamineViewArticleRelevances(Article article)
        {
            var relevances = new List<RelevancesViewModel>();
            if (article.Entries.Count > 0)
            {
                var temp = new List<RelevancesKeyValueModel>();
                relevances.Add(new RelevancesViewModel
                {
                    Informations = temp,
                    Modifier = "词条"
                });
                foreach (var item in article.Entries)
                {
                    temp.Add(new RelevancesKeyValueModel
                    {
                        DisplayName = item.DisplayName,
                        DisplayValue = item.BriefIntroduction,
                        Link = "/entries/index/" + item.Id
                    });
                }
            }
            if (article.ArticleRelationFromArticleNavigation.Count > 0)
            {

                var temp = new List<RelevancesKeyValueModel>();
                relevances.Add(new RelevancesViewModel
                {
                    Informations = temp,
                    Modifier = "文章"
                });
                foreach (var nav in article.ArticleRelationFromArticleNavigation)
                {
                    var item = nav.ToArticleNavigation;
                    temp.Add(new RelevancesKeyValueModel
                    {
                        DisplayName = item.DisplayName,
                        DisplayValue = item.BriefIntroduction,
                        Link = "/articles/index/" + item.Id
                    });
                }
            }
            if (article.Outlinks.Count > 0)
            {

                var temp = new List<RelevancesKeyValueModel>();
                relevances.Add(new RelevancesViewModel
                {
                    Informations = temp,
                    Modifier = "外部链接"
                });
                foreach (var item in article.Outlinks)
                {
                    temp.Add(new RelevancesKeyValueModel
                    {
                        DisplayName = item.Name,
                        DisplayValue = item.BriefIntroduction,
                        Link = item.Link
                    });
                }
            }

            return relevances;
        }


        public async Task<bool> GetEditArticleRelevanesExamineView(Models.ExaminedViewModel model, Examine examine)
        {
            model.Type = "文章";
            var article = await _articleRepository.GetAll()
                .Include(s => s.ArticleRelationFromArticleNavigation).ThenInclude(s => s.ToArticleNavigation)
                .Include(s => s.Entries)
                .Include(s => s.Outlinks)
                .FirstOrDefaultAsync(s => s.Id == examine.ArticleId);
            if (article == null)
            {

                return false;
            }
            model.EntryId = article.Id;
            model.EntryName = article.Name;
            //序列化相关性列表
            //先读取词条信息
            var relevances = InitExamineViewArticleRelevances(article);

            //添加修改记录 
            await _articleService.UpdateArticleData(article, examine);

            var relevances_examine = InitExamineViewArticleRelevances(article);

            //json格式化
            model.EditOverview = _appHelper.GetJsonStringView(examine.Context);


            //判断是否是等待审核状态
            if (examine.IsPassed != null)
            {
                model.BeforeModel = relevances_examine;
                model.AfterModel = relevances;
            }
            else
            {
                model.BeforeModel = relevances;
                model.AfterModel = relevances_examine;
            }

            return true;
        }

        public async Task<bool> GetEditArticleMainPageExamineView(Models.ExaminedViewModel model, Examine examine)
        {
            model.Type = "文章";
            var article = await _articleRepository.FirstOrDefaultAsync(s => s.Id == examine.ArticleId);
            if (article == null)
            {
                return false;
            }
            model.EntryId = article.Id;
            model.EntryName = article.Name;
            //判断是否是等待审核状态
            if (examine.IsPassed != null)
            {
                //序列化数据
                var htmlDiff = new HtmlDiff.HtmlDiff(examine.Context ?? "", article.MainPage ?? "");
                model.EditOverview = htmlDiff.Build().Replace("\r\n", "<br>");
                model.BeforeText = _appHelper.MarkdownToHtml(examine.Context);
                model.AfterText = _appHelper.MarkdownToHtml(article.MainPage);
            }
            else
            {
                //序列化数据
                var htmlDiff = new HtmlDiff.HtmlDiff(article.MainPage ?? "", examine.Context ?? "");
                model.EditOverview = htmlDiff.Build().Replace("\r\n", "<br>");
                model.BeforeText = _appHelper.MarkdownToHtml(article.MainPage);
                model.AfterText = _appHelper.MarkdownToHtml(examine.Context);
            }

            return true;
        }
        #endregion

        #region 标签
        public async Task<bool> GetEditTagExamineView(Models.ExaminedViewModel model, Examine examine)
        {
            model.Type = "标签";
            var tag = await _tagRepository.GetAll()
                  .Include(s => s.InverseParentCodeNavigation)
                  .Include(s => s.ParentCodeNavigation)
                  .Include(s => s.Entries)
                  .FirstOrDefaultAsync(s => s.Id == examine.TagId);
            if (tag == null)
            {
                return false;
            }
            model.EntryId = (int)examine.TagId;
            model.EntryName = examine.Tag.Name;

            //读取标签现有信息
            var tagEditBefore = new TagEdit
            {
                Name = tag.Name ?? "",
                ParentTag = tag.ParentCodeNavigation == null ? "" : tag.ParentCodeNavigation.Name ?? "",
                ChildrenEntries = new List<TagEditAloneModel>(),
                ChildrenTags = new List<TagEditAloneModel>()
            };
            foreach (var item in tag.InverseParentCodeNavigation)
            {
                tagEditBefore.ChildrenTags.Add(new TagEditAloneModel
                {
                    Name = item.Name
                });
            }
            foreach (var item in tag.Entries)
            {
                tagEditBefore.ChildrenEntries.Add(new TagEditAloneModel
                {
                    Name = item.Name
                });
            }

            //读取标签修改后的信息
            TagEdit tagEditAfter = null;
            using (TextReader str = new StringReader(examine.Context))
            {
                var serializer = new JsonSerializer();
                tagEditAfter = (TagEdit)serializer.Deserialize(str, typeof(TagEdit));
            }
            //创建临时
            //读取标签现有信息
            var tagEditTemp = new TagEdit
            {
                Name = tag.Name ?? "",
                ParentTag = tag.ParentCodeNavigation == null ? "" : tag.ParentCodeNavigation.Name ?? "",
                ChildrenEntries = new List<TagEditAloneModel>(),
                ChildrenTags = new List<TagEditAloneModel>()
            };
            foreach (var item in tag.InverseParentCodeNavigation)
            {
                tagEditTemp.ChildrenTags.Add(new TagEditAloneModel
                {
                    Name = item.Name
                });
            }
            foreach (var item in tag.Entries)
            {
                tagEditTemp.ChildrenEntries.Add(new TagEditAloneModel
                {
                    Name = item.Name
                });
            }
            //遍历关联词条对应修改
            foreach (var item in tagEditAfter.ChildrenEntries)
            {
                var isAdd = false;
                foreach (var infor in tagEditTemp.ChildrenEntries)
                {
                    if (infor.Name == item.Name)
                    {
                        if (item.IsDelete == true)
                        {
                            tagEditTemp.ChildrenEntries.Remove(infor);

                        }
                        isAdd = true;
                        break;
                    }
                }
                if (isAdd == false && item.IsDelete == false)
                {
                    tagEditTemp.ChildrenEntries.Add(new TagEditAloneModel
                    {
                        Name = item.Name
                    });
                }
            }
            //遍历子标签对应修改
            foreach (var item in tagEditAfter.ChildrenTags)
            {
                var isAdd = false;
                foreach (var infor in tagEditTemp.ChildrenTags)
                {
                    if (infor.Name == item.Name)
                    {
                        if (item.IsDelete == true)
                        {
                            tagEditTemp.ChildrenTags.Remove(infor);

                        }
                        isAdd = true;
                        break;
                    }
                }
                if (isAdd == false && item.IsDelete == false)
                {
                    tagEditTemp.ChildrenTags.Add(new TagEditAloneModel
                    {
                        Name = item.Name
                    });
                }
            }

            //json格式化
            model.EditOverview = _appHelper.GetJsonStringView(examine.Context);

            //判断是否是等待审核状态
            if (examine.IsPassed != null)
            {
                model.BeforeModel = tagEditTemp;
                model.AfterModel = tagEditBefore;
            }
            else
            {
                model.BeforeModel = tagEditBefore;
                model.AfterModel = tagEditTemp;
            }

            return true;
        }

        private TagMain_1_0 InitExamineViewTagMain(Tag tag)
        {
            var model = new TagMain_1_0
            {
                Name = tag.Name,
                BriefIntroduction = tag.BriefIntroduction,
                MainPicture = _appHelper.GetImagePath(tag.MainPicture, "app.png"),
                Thumbnail = _appHelper.GetImagePath(tag.Thumbnail, "app.png"),
                BackgroundPicture = _appHelper.GetImagePath(tag.BackgroundPicture, "background.png"),
                SmallBackgroundPicture = _appHelper.GetImagePath(tag.SmallBackgroundPicture, "background.png"),
            };
            return model;
        }

        public async Task<bool> GetEditTagMainExamineView(Models.ExaminedViewModel model, Examine examine)
        {
            model.Type = "标签";
            var tag = await _tagRepository.GetAll().Include(s => s.ParentCodeNavigation).FirstOrDefaultAsync(s => s.Id == examine.TagId);
            if (tag == null)
            {
                return false;
            }
            model.EntryId = (int)examine.TagId;
            model.EntryName = tag.Name;

            var entryMainBefore = InitExamineViewTagMain(tag);

            //添加修改记录 
            await _tagService.UpdateTagDataAsync(tag, examine);

            //序列化数据
            var entryMain = InitExamineViewTagMain(tag);

            //json格式化
            model.EditOverview = _appHelper.GetJsonStringView(examine.Context);

            //判断是否是等待审核状态
            if (examine.IsPassed != null)
            {
                model.BeforeModel = entryMain;
                model.AfterModel = entryMainBefore;
            }
            else
            {
                model.AfterModel = entryMain;
                model.BeforeModel = entryMainBefore;
            }
            return true;
        }


        private static List<RelevancesViewModel> InitExamineViewEditChildTags(Tag tag)
        {
            var relevances = new List<RelevancesViewModel>();
            var tags = tag.InverseParentCodeNavigation;
            foreach (var item in tags)
            {
                var isAdd = false;

                //遍历信息列表寻找关键词
                foreach (var infor in relevances)
                {
                    if (infor.Modifier == "子标签")
                    {
                        //关键词相同则添加
                        infor.Informations.Add(new RelevancesKeyValueModel
                        {
                            DisplayName = item.Name,
                            DisplayValue = item.BriefIntroduction,
                            Id = item.Id
                        });
                        isAdd = true;
                        break;
                    }
                }
                if (isAdd == false)
                {
                    //没有找到关键词 则新建关键词
                    var temp = new RelevancesViewModel
                    {
                        Modifier = "子标签",
                        Informations = new List<RelevancesKeyValueModel>()
                    };
                    temp.Informations.Add(new RelevancesKeyValueModel
                    {
                        DisplayName = item.Name,
                        DisplayValue = item.BriefIntroduction,
                        Id = item.Id

                    });
                    relevances.Add(temp);
                }
            }
            return relevances;
        }

        public async Task<bool> GetEditTagChildTagsExamineView(Models.ExaminedViewModel model, Examine examine)
        {
            model.Type = "标签";
            var tag = await _tagRepository.GetAll().Include(s => s.InverseParentCodeNavigation).FirstOrDefaultAsync(s => s.Id == examine.TagId);
            if (tag == null)
            {
                return false;
            }
            model.EntryId = (int)examine.TagId;
            model.EntryName = tag.Name;

            //序列化相关性列表
            //先读取词条信息
            var relevances = InitExamineViewEditChildTags(tag);

            //添加修改记录 
            await _tagService.UpdateTagDataAsync(tag, examine);

            var relevances_examine = InitExamineViewEditChildTags(tag);

            //json格式化
            model.EditOverview = _appHelper.GetJsonStringView(examine.Context);

            //判断是否是等待审核状态
            if (examine.IsPassed != null)
            {
                model.BeforeModel = relevances_examine;
                model.AfterModel = relevances;
            }
            else
            {
                model.BeforeModel = relevances;
                model.AfterModel = relevances_examine;
            }
            return true;
        }


        private static List<RelevancesViewModel> InitExamineViewEditChildEntries(Tag tag)
        {
            var relevances = new List<RelevancesViewModel>();
            var entries = tag.Entries;
            foreach (var item in entries)
            {
                var isAdd = false;

                //遍历信息列表寻找关键词
                foreach (var infor in relevances)
                {
                    if (infor.Modifier == "子词条")
                    {
                        //关键词相同则添加
                        infor.Informations.Add(new RelevancesKeyValueModel
                        {
                            DisplayName = item.Name,
                            DisplayValue = item.BriefIntroduction,
                            Id = item.Id
                        });
                        isAdd = true;
                        break;
                    }
                }
                if (isAdd == false)
                {
                    //没有找到关键词 则新建关键词
                    var temp = new RelevancesViewModel
                    {
                        Modifier = "子词条",
                        Informations = new List<RelevancesKeyValueModel>()
                    };
                    temp.Informations.Add(new RelevancesKeyValueModel
                    {
                        DisplayName = item.Name,
                        DisplayValue = item.BriefIntroduction,
                        Id = item.Id

                    });
                    relevances.Add(temp);
                }
            }
            return relevances;
        }

        public async Task<bool> GetEditTagChildEntriesExamineView(Models.ExaminedViewModel model, Examine examine)
        {
            model.Type = "标签";
            var tag = await _tagRepository.GetAll().Include(s => s.Entries).FirstOrDefaultAsync(s => s.Id == examine.TagId);
            if (tag == null)
            {
                return false;
            }
            model.EntryId = (int)examine.TagId;
            model.EntryName = tag.Name;

            //序列化相关性列表
            //先读取词条信息
            var relevances = InitExamineViewEditChildEntries(tag);

            //添加修改记录 
            await _tagService.UpdateTagDataAsync(tag, examine);

            var relevances_examine = InitExamineViewEditChildEntries(tag);

            //json格式化
            model.EditOverview = _appHelper.GetJsonStringView(examine.Context);

            //判断是否是等待审核状态
            if (examine.IsPassed != null)
            {
                model.BeforeModel = relevances_examine;
                model.AfterModel = relevances;
            }
            else
            {
                model.BeforeModel = relevances;
                model.AfterModel = relevances_examine;
            }
            return true;
        }


        #endregion

        #region 评论

        public async Task<bool> GetPubulishCommentExamineView(Models.ExaminedViewModel model, Examine examine)
        {
            model.Type = "评论";
            var comment = await _commentRepository.FirstOrDefaultAsync(s => s.Id == examine.CommentId);
            if (comment == null)
            {
                return false;
            }
            model.EntryId = comment.Id;
            model.EntryName = "";
            //序列化数据
            CommentText commentText = null;
            using (TextReader str = new StringReader(examine.Context))
            {
                var serializer = new JsonSerializer();
                commentText = (CommentText)serializer.Deserialize(str, typeof(CommentText));
            }

            var result = $"评论时间<br>{commentText.CommentTime}<br>";
            result += $"评论内容<br>\n{commentText.Text}\n<br>";
            result += $"类型<br>{commentText.Type.GetDisplayName()}<br>";
            result += $"目标Id<br>{commentText.ObjectId}<br>";
            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().UseSoftlineBreakAsHardlineBreak().Build();
            model.EditOverview = Markdown.ToHtml(result, pipeline);
            model.BeforeModel = commentText;
            model.AfterModel = commentText;

            return true;
        }

        #endregion

        #region 消歧义页
        public async Task<bool> GetDisambigMainExamineView(Models.ExaminedViewModel model, Examine examine)
        {
            model.Type = "消歧义页";
            var disambig = await _disambigRepository.FirstOrDefaultAsync(s => s.Id == examine.DisambigId);
            if (disambig == null)
            {
                return false;
            }
            model.EntryId = disambig.Id;
            model.EntryName = disambig.Name;
            //序列化数据
            DisambigMain disambigMain = null;
            using (TextReader str = new StringReader(examine.Context))
            {
                var serializer = new JsonSerializer();
                disambigMain = (DisambigMain)serializer.Deserialize(str, typeof(DisambigMain));
            }

            var disambigMainBefore = new DisambigMain
            {
                Name = disambig.Name,
                BriefIntroduction = disambig.BriefIntroduction,
                MainPicture = _appHelper.GetImagePath(disambig.MainPicture, "app.png"),
                BackgroundPicture = _appHelper.GetImagePath(disambig.BackgroundPicture, "background.png"),
                SmallBackgroundPicture = _appHelper.GetImagePath(disambig.SmallBackgroundPicture, "background.png"),
            };
            disambigMain.MainPicture = _appHelper.GetImagePath(disambigMain.MainPicture, "app.png");
            disambigMain.BackgroundPicture = _appHelper.GetImagePath(disambigMain.BackgroundPicture, "background.png");
            disambigMain.SmallBackgroundPicture = _appHelper.GetImagePath(disambigMain.SmallBackgroundPicture, "background.png");

            //判断是否是等待审核状态
            if (examine.IsPassed != null)
            {
                var htmlDiff = new HtmlDiff.HtmlDiff(disambigMain.Name ?? "", disambig.Name ?? "");
                model.EditOverview = "<h5>名称</h5>" + "<h5>" + htmlDiff.Build().Replace("\r\n", "<br>") + "</h5>";


                htmlDiff = new HtmlDiff.HtmlDiff(disambigMain.BriefIntroduction ?? "", disambig.BriefIntroduction ?? "");
                model.EditOverview += "<h5>简介</h5>" + "<h5>" + htmlDiff.Build().Replace("\r\n", "<br>") + "</h5>";

                model.BeforeModel = disambigMain;
                model.AfterModel = disambigMainBefore;
            }
            else
            {
                var htmlDiff = new HtmlDiff.HtmlDiff(disambig.Name ?? "", disambigMain.Name ?? "");
                model.EditOverview = "<h5>名称</h5>" + "<h5>" + htmlDiff.Build().Replace("\r\n", "<br>") + "</h5>";

                htmlDiff = new HtmlDiff.HtmlDiff(disambig.BriefIntroduction ?? "", disambigMain.BriefIntroduction ?? "");
                model.EditOverview += "<h5>简介</h5>" + "<h5>" + htmlDiff.Build().Replace("\r\n", "<br>") + "</h5>";


                model.AfterModel = disambigMain;
                model.BeforeModel = disambigMainBefore;
            }

            return true;
        }

        public async Task<bool> GetDisambigRelevancesExamineView(Models.ExaminedViewModel model, Examine examine)
        {
            model.Type = "消歧义页";
            var disambig = await _disambigRepository.GetAll()
                 .Include(s => s.Entries).Include(s => s.Articles)
                 .FirstOrDefaultAsync(s => s.Id == examine.DisambigId);
            if (disambig == null)
            {

                return false;
            }
            model.EntryId = disambig.Id;
            model.EntryName = disambig.Name;
            //序列化相关性列表
            //先读取词条信息
            var disambigAloneModels = new List<DisambigAloneModel>();
            foreach (var item in disambig.Entries)
            {
                disambigAloneModels.Add(new DisambigAloneModel { entry = await _appHelper.GetEntryInforTipViewModel(item) });
            }
            foreach (var item in disambig.Articles)
            {
                disambigAloneModels.Add(new DisambigAloneModel { article = _appHelper.GetArticleInforTipViewModel(item) });
            }

            //序列化相关性列表
            //先读取词条信息
            var disambigAloneModels_examine = new List<DisambigAloneModel>();
            foreach (var item in disambigAloneModels)
            {
                disambigAloneModels_examine.Add(item);
            }

            //再读取当前用户等待审核的信息
            //序列化数据
            DisambigRelevances disambigRelevances = null;
            using (TextReader str = new StringReader(examine.Context))
            {
                var serializer = new JsonSerializer();
                disambigRelevances = (DisambigRelevances)serializer.Deserialize(str, typeof(DisambigRelevances));
            }
            foreach (var item in disambigRelevances.Relevances)
            {
                var isAdd = false;

                //遍历信息列表寻找关键词
                if (item.Type == DisambigRelevanceType.Entry)
                {
                    foreach (var infor in disambigAloneModels_examine.Where(s => s.entry != null))
                    {
                        //如果关键词相同
                        if (infor.entry.Id == item.EntryId)
                        {
                            if (item.IsDelete == true)
                            {
                                disambigAloneModels_examine.Remove(infor);

                            }
                            isAdd = true;
                            break;
                        }
                    }

                }
                else if (item.Type == DisambigRelevanceType.Article)
                {
                    foreach (var infor in disambigAloneModels_examine.Where(s => s.article != null))
                    {
                        //如果关键词相同
                        if (infor.article.Id == item.EntryId)
                        {
                            if (item.IsDelete == true)
                            {
                                disambigAloneModels_examine.Remove(infor);

                            }
                            isAdd = true;
                            break;
                        }
                    }

                }
                if (isAdd == false && item.IsDelete == false)
                {
                    if (item.Type == DisambigRelevanceType.Entry)
                    {
                        var temp = await _entryRepository.GetAll().Where(s => s.Id == item.EntryId).FirstOrDefaultAsync();
                        if (temp != null)
                        {
                            disambigAloneModels_examine.Add(new DisambigAloneModel { entry = await _appHelper.GetEntryInforTipViewModel(temp) });
                        }
                    }
                    else if (item.Type == DisambigRelevanceType.Article)
                    {
                        var temp = await _articleRepository.GetAll().Where(s => s.Id == item.EntryId).FirstOrDefaultAsync();
                        if (temp != null)
                        {
                            disambigAloneModels_examine.Add(new DisambigAloneModel { article = _appHelper.GetArticleInforTipViewModel(temp) });
                        }
                    }
                }

            }

            //json格式化
            model.EditOverview = _appHelper.GetJsonStringView(examine.Context);

            //判断是否是等待审核状态
            if (examine.IsPassed != null)
            {
                model.BeforeModel = disambigAloneModels_examine;
                model.AfterModel = disambigAloneModels;
            }
            else
            {
                model.BeforeModel = disambigAloneModels;
                model.AfterModel = disambigAloneModels_examine;
            }

            return false;
        }

        #endregion

        #region 用户

        public bool GetUserMainPageExamineView(Models.ExaminedViewModel model, Examine examine)
        {
            model.Type = "用户";
            //判断是否是等待审核状态
            if (examine.IsPassed != null)
            {
                var htmlDiff = new HtmlDiff.HtmlDiff(examine.Context ?? "", examine.ApplicationUser.MainPageContext ?? "");
                model.EditOverview = htmlDiff.Build().Replace("\r\n", "<br>");
                model.BeforeText = _appHelper.MarkdownToHtml(examine.Context);
                model.AfterText = _appHelper.MarkdownToHtml(examine.ApplicationUser.MainPageContext);
            }
            else
            {
                var htmlDiff = new HtmlDiff.HtmlDiff(examine.ApplicationUser.MainPageContext ?? "", examine.Context ?? "");
                model.EditOverview = htmlDiff.Build().Replace("\r\n", "<br>");
                model.BeforeText = _appHelper.MarkdownToHtml(examine.ApplicationUser.MainPageContext);
                model.AfterText = _appHelper.MarkdownToHtml(examine.Context);
            }

            model.EntryId = 0;
            model.EntryName = examine.ApplicationUser.UserName;

            return true;
        }

        private UserMain InitExamineViewUserMain(ApplicationUser user)
        {
            var userMainBefore = new UserMain
            {
                UserName = user.UserName,
                PersonalSignature = user.PersonalSignature,
                PhotoPath = _appHelper.GetImagePath(user.PhotoPath, "user.png"),
                BackgroundImage = _appHelper.GetImagePath(user.BackgroundImage, "userbackground.jpg"),
                SBgImage = _appHelper.GetImagePath(user.SBgImage, "background.png"),
                MBgImage = _appHelper.GetImagePath(user.MBgImage, "background.png")
            };
            return userMainBefore;
        }

        public async Task<bool> GetEditUserMainExamineView(Models.ExaminedViewModel model, Examine examine)
        {
            model.Type = "用户";
            var user = await _userRepository.FirstOrDefaultAsync(s => s.Id == examine.ApplicationUserId);
            if (user == null)
            {
                return false;
            }
            model.EntryId = 0;
            model.EntryName = examine.ApplicationUser.UserName;

            //序列化数据
            var userMainBefore = InitExamineViewUserMain(user);

            //添加修改记录 
            await _userService.UpdateUserData(user, examine);

            //序列化数据
            var userMain = InitExamineViewUserMain(user);

            //判断是否是等待审核状态
            if (examine.IsPassed != null)
            {
                var htmlDiff = new HtmlDiff.HtmlDiff(userMain.UserName ?? "", userMainBefore.UserName ?? "");
                model.EditOverview = "<h5>用户名</h5>" + htmlDiff.Build().Replace("\r\n", "<br>");

                htmlDiff = new HtmlDiff.HtmlDiff(userMain.PersonalSignature ?? "", userMainBefore.PersonalSignature ?? "");
                model.EditOverview += "<h5>个性签名</h5>" + htmlDiff.Build().Replace("\r\n", "<br>");

                model.BeforeModel = userMain;
                model.AfterModel = userMainBefore;
            }
            else
            {
                var htmlDiff = new HtmlDiff.HtmlDiff(userMainBefore.UserName ?? "", userMain.UserName ?? "");
                model.EditOverview = "<h5>用户名</h5>" + htmlDiff.Build().Replace("\r\n", "<br>");

                htmlDiff = new HtmlDiff.HtmlDiff(userMainBefore.PersonalSignature ?? "", userMain.PersonalSignature ?? "");
                model.EditOverview += "<h5>个性签名</h5>" + htmlDiff.Build().Replace("\r\n", "<br>");


                model.AfterModel = userMain;
                model.BeforeModel = userMainBefore;
            }

            return true;
        }

        #endregion

        #region 周边

        private PeripheryMain_1_0 InitExamineViewPeripheryMain(Periphery periphery)
        {
            var model = new PeripheryMain_1_0
            {
                Name = periphery.Name,
                DisplayName = periphery.DisplayName,
                BriefIntroduction = periphery.BriefIntroduction,
                MainPicture = _appHelper.GetImagePath(periphery.MainPicture, "app.png"),
                Thumbnail = _appHelper.GetImagePath(periphery.Thumbnail, "app.png"),
                BackgroundPicture = _appHelper.GetImagePath(periphery.BackgroundPicture, "background.png"),
                SmallBackgroundPicture = _appHelper.GetImagePath(periphery.SmallBackgroundPicture, "background.png"),
                Author = periphery.Author,
                Material = periphery.Material,
                PageCount = periphery.PageCount,
                Price = periphery.Price,
                IndividualParts = periphery.IndividualParts,
                Brand = periphery.Brand,
                IsAvailableItem = periphery.IsAvailableItem,
                IsReprint = periphery.IsReprint,
                Size = periphery.Size,
                SongCount = periphery.SongCount,
                Type = periphery.Type,
            };
            return model;
        }

        public async Task<bool> GetEditPeripheryMainExamineView(Models.ExaminedViewModel model, Examine examine)
        {
            model.Type = "周边";
            var periphery = await _peripheryRepository.FirstOrDefaultAsync(s => s.Id == examine.PeripheryId);
            if (periphery == null)
            {
                return false;
            }
            model.EntryId = (int)examine.PeripheryId;
            model.EntryName = periphery.Name;

            var entryMainBefore = InitExamineViewPeripheryMain(periphery);

            //添加修改记录 
            await _peripheryService.UpdatePeripheryDataAsync(periphery, examine);

            //序列化数据
            var entryMain = InitExamineViewPeripheryMain(periphery);

            //json格式化
            model.EditOverview = _appHelper.GetJsonStringView(examine.Context);


            //判断是否是等待审核状态
            if (examine.IsPassed != null)
            {
                model.BeforeModel = entryMain;
                model.AfterModel = entryMainBefore;
            }
            else
            {
                model.AfterModel = entryMain;
                model.BeforeModel = entryMainBefore;
            }
            return true;
        }


        private static List<EntryPicture> InitExamineViewPeripheryImages(Periphery periphery)
        {
            var pictures = new List<EntryPicture>();
            foreach (var item in periphery.Pictures)
            {
                pictures.Add(new EntryPicture
                {
                    Url = item.Url,
                    Note = item.Note,
                    Modifier = item.Modifier
                });
            }

            return pictures;
        }

        public async Task<bool> GetEditPeripheryImagesExamineView(Models.ExaminedViewModel model, Examine examine)
        {
            model.Type = "周边";
            var periphery = await _peripheryRepository.GetAll().Include(s => s.Pictures).FirstOrDefaultAsync(s => s.Id == examine.PeripheryId);
            if (periphery == null)
            {
                return false;
            }
            model.EntryId = (int)examine.PeripheryId;
            model.EntryName = periphery.Name;

            var pictures = InitExamineViewPeripheryImages(periphery);

            //添加修改记录 
            await _peripheryService.UpdatePeripheryDataAsync(periphery, examine);

            var pictures_examine = InitExamineViewPeripheryImages(periphery);

            //json格式化
            model.EditOverview = _appHelper.GetJsonStringView(examine.Context);

            //判断是否是等待审核状态
            if (examine.IsPassed != null)
            {
                model.BeforeModel = pictures_examine;
                model.AfterModel = pictures;
            }
            else
            {
                model.BeforeModel = pictures;
                model.AfterModel = pictures_examine;
            }
            return true;
        }

        private List<RelevancesViewModel> InitExamineViewPeripheryRelatedEntries(Periphery periphery)
        {
            var relevances = new List<RelevancesViewModel>();
            var entries = periphery.RelatedEntries.Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.BriefIntroduction
                })
                .ToList();
            foreach (var item in entries)
            {
                var isAdd = false;

                //遍历信息列表寻找关键词
                foreach (var infor in relevances)
                {
                    if (infor.Modifier == "词条")
                    {
                        //关键词相同则添加
                        infor.Informations.Add(new RelevancesKeyValueModel
                        {
                            DisplayName = item.Name,
                            DisplayValue = item.BriefIntroduction,
                            Id = item.Id
                        });
                        isAdd = true;
                        break;
                    }
                }
                if (isAdd == false)
                {
                    //没有找到关键词 则新建关键词
                    var temp = new RelevancesViewModel
                    {
                        Modifier = "词条",
                        Informations = new List<RelevancesKeyValueModel>()
                    };
                    temp.Informations.Add(new RelevancesKeyValueModel
                    {
                        DisplayName = item.Name,
                        DisplayValue = item.BriefIntroduction,
                        Id = item.Id

                    });
                    relevances.Add(temp);
                }
            }
            return relevances;
        }

        public async Task<bool> GetEditPeripheryRelatedEntriesExamineView(Models.ExaminedViewModel model, Examine examine)
        {
            model.Type = "周边";
            var periphery = await _peripheryRepository.GetAll().Include(s => s.RelatedEntries).FirstOrDefaultAsync(s => s.Id == examine.PeripheryId);
            if (periphery == null)
            {
                return false;
            }
            model.EntryId = (int)examine.PeripheryId;
            model.EntryName = periphery.Name;

            //序列化相关性列表
            //先读取词条信息
            var relevances =  InitExamineViewPeripheryRelatedEntries(periphery);

            //添加修改记录 
            await _peripheryService.UpdatePeripheryDataAsync(periphery, examine);

            var relevances_examine =  InitExamineViewPeripheryRelatedEntries(periphery);

            //json格式化
            model.EditOverview = _appHelper.GetJsonStringView(examine.Context);

            //判断是否是等待审核状态
            if (examine.IsPassed != null)
            {
                model.BeforeModel = relevances_examine;
                model.AfterModel = relevances;
            }
            else
            {
                model.BeforeModel = relevances;
                model.AfterModel = relevances_examine;
            }
            return true;
        }

        private List<RelevancesViewModel> InitExamineViewPeripheryRelatedPeripheries(Periphery periphery)
        {
            var relevances = new List<RelevancesViewModel>();
            var peripheries = periphery.PeripheryRelationFromPeripheryNavigation.Select(s => s.ToPeripheryNavigation);
            foreach (var item in peripheries)
            {
                var isAdd = false;

                //遍历信息列表寻找关键词
                foreach (var infor in relevances)
                {
                    if (infor.Modifier == "周边")
                    {
                        //关键词相同则添加
                        infor.Informations.Add(new RelevancesKeyValueModel
                        {
                            DisplayName = item.Name,
                            DisplayValue = item.BriefIntroduction,
                            Id = item.Id
                        });
                        isAdd = true;
                        break;
                    }
                }
                if (isAdd == false)
                {
                    //没有找到关键词 则新建关键词
                    var temp = new RelevancesViewModel
                    {
                        Modifier = "周边",
                        Informations = new List<RelevancesKeyValueModel>()
                    };
                    temp.Informations.Add(new RelevancesKeyValueModel
                    {
                        DisplayName = item.Name,
                        DisplayValue = item.BriefIntroduction,
                        Id = item.Id

                    });
                    relevances.Add(temp);
                }
            }
            return relevances;
        }

        public async Task<bool> GetEditPeripheryRelatedPeripheriesExamineView(Models.ExaminedViewModel model, Examine examine)
        {
            model.Type = "周边";
            var periphery = await _peripheryRepository.GetAll().Include(s => s.PeripheryRelationFromPeripheryNavigation).ThenInclude(s => s.ToPeripheryNavigation).ThenInclude(s => s.PeripheryRelationFromPeripheryNavigation).FirstOrDefaultAsync(s => s.Id == examine.PeripheryId);
            if (periphery == null)
            {
                return false;
            }
            model.EntryId = (int)examine.PeripheryId;
            model.EntryName = periphery.Name;

            //序列化相关性列表
            //先读取词条信息
            var relevances = InitExamineViewPeripheryRelatedPeripheries(periphery);

            //添加修改记录 
           await  _peripheryService.UpdatePeripheryDataAsync(periphery, examine);

            var relevances_examine = InitExamineViewPeripheryRelatedPeripheries(periphery);

            //json格式化
            model.EditOverview = _appHelper.GetJsonStringView(examine.Context);

            //判断是否是等待审核状态
            if (examine.IsPassed != null)
            {
                model.BeforeModel = relevances_examine;
                model.AfterModel = relevances;
            }
            else
            {
                model.BeforeModel = relevances;
                model.AfterModel = relevances_examine;
            }
            return true;
        }


        #endregion



        #endregion

        #region 使审核记录真实作用在目标上

        #region 词条
        public async Task ExamineEstablishMainAsync(Entry entry, ExamineMain examine)
        {
            //更新数据
            _entryService.UpdateEntryDataMain(entry, examine);
            //保存
            await _entryRepository.UpdateAsync(entry);

            //更新完善度
            await _perfectionService.UpdateEntryPerfectionResultAsync(entry.Id);
        }

        public async Task ExamineEstablishAddInforAsync(Entry entry, EntryAddInfor examine)
        {
            //更新数据
            _entryService.UpdateEntryDataAddInfor(entry, examine);
            //保存
            await _entryRepository.UpdateAsync(entry);

            //更新完善度
            await _perfectionService.UpdateEntryPerfectionResultAsync(entry.Id);

            var admin = await _userRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(s => s.Id == _configuration["ExamineAdminId"]);

            //反向关联
            foreach (var item in examine.Information)
            {
                if (item.IsDelete == false)
                {
                    if (entry.Type == EntryType.Game)
                    {
                        if (item.Modifier == "STAFF")
                        {
                            var temp = await _entryRepository.GetAll()
                                .Include(s => s.EntryRelationFromEntryNavigation).ThenInclude(s=>s.ToEntryNavigation)
                                .FirstOrDefaultAsync(s => s.Name == item.DisplayValue);

                            if (temp != null && temp.EntryRelationFromEntryNavigation.Any(s => s.ToEntry == entry.Id) == false)
                            {
                                //补全审核记录
                                //创建审核数据模型
                                var examinedModel = new EntryRelevances();

                                examinedModel.Relevances.Add(new EntryRelevancesAloneModel
                                {
                                    DisplayName = entry.Id.ToString(),
                                    DisplayValue = entry.Name,
                                    IsDelete = false,
                                    Type = RelevancesType.Entry,
                                });
                                var resulte = "";
                                using (TextWriter text = new StringWriter())
                                {
                                    var serializer = new JsonSerializer();
                                    serializer.Serialize(text, examinedModel);
                                    resulte = text.ToString();
                                }

                                await ExamineEstablishRelevancesAsync(temp, examinedModel);
                                await UniversalEditExaminedAsync(temp, admin, true, resulte, Operation.EstablishRelevances, "自动反向关联");

                            }
                        }
                    }
                    else if (entry.Type == EntryType.Role)
                    {
                        if (item.Modifier == "基本信息" && item.DisplayName == "声优" && string.IsNullOrWhiteSpace(item.DisplayValue) == false)
                        {
                            var temp = await _entryRepository.GetAll()
                                .Include(s => s.EntryRelationFromEntryNavigation).ThenInclude(s => s.ToEntryNavigation)
                                .FirstOrDefaultAsync(s => s.Name == item.DisplayValue);

                            if (temp != null && temp.EntryRelationFromEntryNavigation.Any(s => s.ToEntry == entry.Id) == false)
                            {
                                //补全审核记录
                                //创建审核数据模型
                                var examinedModel = new EntryRelevances();

                                examinedModel.Relevances.Add(new EntryRelevancesAloneModel
                                {
                                    DisplayName = entry.Id.ToString(),
                                    DisplayValue = entry.Name,
                                    IsDelete = false,
                                    Type = RelevancesType.Entry,
                                });
                                var resulte = "";
                                using (TextWriter text = new StringWriter())
                                {
                                    var serializer = new JsonSerializer();
                                    serializer.Serialize(text, examinedModel);
                                    resulte = text.ToString();
                                }

                                await ExamineEstablishRelevancesAsync(temp, examinedModel);
                                await UniversalEditExaminedAsync(temp, admin, true, resulte, Operation.EstablishRelevances, "自动反向关联");
                            }
                        }
                    }
                }
            }

        }

        public async Task ExamineEstablishImagesAsync(Entry entry, EntryImages examine)
        {
            //更新数据
            _entryService.UpdateEntryDataImages(entry, examine);
            //保存
            await _entryRepository.UpdateAsync(entry);

            //更新完善度
            await _perfectionService.UpdateEntryPerfectionResultAsync(entry.Id);

        }

        public async Task ExamineEstablishRelevancesAsync(Entry entry, EntryRelevances examine)
        {
            //更新数据
            await _entryService.UpdateEntryDataRelevances(entry, examine);
            await _entryRepository.UpdateAsync(entry);

            //更新完善度
            await _perfectionService.UpdateEntryPerfectionResultAsync(entry.Id);

            var admin = new ApplicationUser();
            admin.Id = _configuration["ExamineAdminId"];

            //反向关联 词条
            foreach (var item in examine.Relevances.Where(s => s.IsDelete == false && s.Type == RelevancesType.Entry))
            {
                //查找关联词条
                var temp = await _entryRepository.GetAll()
                    .Include(s => s.EntryRelationFromEntryNavigation).ThenInclude(s=>s.ToEntryNavigation)
                    .FirstOrDefaultAsync(s => s.Id.ToString() == item.DisplayName);
                if (temp != null && temp.EntryRelationFromEntryNavigation.Any(s => s.ToEntry == entry.Id) == false
                    && (entry.Type == EntryType.Staff && temp.Type == EntryType.Game) == false)
                {
                    //补全审核记录
                    //创建审核数据模型
                    var examinedModel = new EntryRelevances();

                    examinedModel.Relevances.Add(new EntryRelevancesAloneModel
                    {
                        DisplayName = entry.Id.ToString(),
                        DisplayValue = entry.Name,
                        IsDelete = false,
                        Type = RelevancesType.Entry,
                    });
                    var resulte = "";
                    using (TextWriter text = new StringWriter())
                    {
                        var serializer = new JsonSerializer();
                        serializer.Serialize(text, examinedModel);
                        resulte = text.ToString();
                    }

                    await ExamineEstablishRelevancesAsync(temp, examinedModel);
                    await UniversalEditExaminedAsync(temp, admin, true, resulte, Operation.EstablishRelevances, "自动反向关联");
                }


            }
            //反向关联 文章
            foreach (var item in examine.Relevances.Where(s => s.IsDelete == false && s.Type == RelevancesType.Article))
            {
                //查找关联词条
                var temp = await _articleRepository.GetAll().Include(s => s.Entries).FirstOrDefaultAsync(s => s.Id.ToString() == item.DisplayName);
                if (temp != null && temp.Entries.Any(s => s.Id == entry.Id) == false)
                {
                    //补全审核记录
                    //创建审核数据模型
                    var examinedModel = new ArticleRelevances();

                    examinedModel.Relevances.Add(new ArticleRelevancesAloneModel
                    {
                        DisplayName = entry.Id.ToString(),
                        DisplayValue = entry.Name,
                        IsDelete = false,
                        Type = RelevancesType.Entry,
                    });

                    var resulte = "";
                    using (TextWriter text = new StringWriter())
                    {
                        var serializer = new JsonSerializer();
                        serializer.Serialize(text, examinedModel);
                        resulte = text.ToString();
                    }

                    await ExamineEditArticleRelevancesAsync(temp, examinedModel);
                    await UniversalEditArticleExaminedAsync(temp, admin, true, resulte, Operation.EditArticleRelevanes, "自动反向关联");
                }


            }

        }

        public async Task ExamineEstablishTagsAsync(Entry entry, EntryTags examine)
        {
            //更新数据
            await _entryService.UpdateEntryDataTagsAsync(entry, examine);

            await _entryRepository.UpdateAsync(entry);

            //更新完善度
            await _perfectionService.UpdateEntryPerfectionResultAsync(entry.Id);

        }

        public async Task ExamineEstablishMainPageAsync(Entry entry, string examine)
        {
            //更新数据
            _entryService.UpdateEntryDataMainPage(entry, examine);

            await _entryRepository.UpdateAsync(entry);

            //更新完善度
            await _perfectionService.UpdateEntryPerfectionResultAsync(entry.Id);

        }
        #endregion

        #region 文章
        public async Task ExamineEditArticleMainAsync(Article article, ExamineMain examine)
        {
            _articleService.UpdateArticleDataMain(article, examine);

            await _articleRepository.UpdateAsync(article);
        }

        public async Task ExamineEditArticleRelevancesAsync(Article article, ArticleRelevances examine)
        {
            await _articleService.UpdateArticleDataRelevances(article, examine);
            await _articleRepository.UpdateAsync(article);

            var admin = new ApplicationUser();
            admin.Id = _configuration["ExamineAdminId"];
            //反向关联 文章
            foreach (var item in examine.Relevances.Where(s => s.IsDelete == false && s.Type == RelevancesType.Article))
            {
                //查找关联文章
                var temp = await _articleRepository.GetAll()
                    .Include(s => s.ArticleRelationFromArticleNavigation).ThenInclude(s=>s.ToArticleNavigation)
                    .FirstOrDefaultAsync(s => s.Id.ToString() == item.DisplayName);
                if (temp != null && temp.ArticleRelationFromArticleNavigation.Any(s => s.ToArticle == article.Id) == false)
                {
                    //补全审核记录
                    //创建审核数据模型
                    var examinedModel = new ArticleRelevances();

                    examinedModel.Relevances.Add(new ArticleRelevancesAloneModel
                    {
                        DisplayName = article.Id.ToString(),
                        DisplayValue = article.Name,
                        IsDelete = false,
                        Type = RelevancesType.Article,
                    });
                    var resulte = "";
                    using (TextWriter text = new StringWriter())
                    {
                        var serializer = new JsonSerializer();
                        serializer.Serialize(text, examinedModel);
                        resulte = text.ToString();
                    }

                    await ExamineEditArticleRelevancesAsync(temp, examinedModel);
                    await UniversalEditArticleExaminedAsync(temp, admin, true, resulte, Operation.EstablishRelevances, "自动反向关联");
                }


            }
            //反向关联 词条
            foreach (var item in examine.Relevances.Where(s => s.IsDelete == false && s.Type == RelevancesType.Entry))
            {
                //查找关联词条
                var temp = await _entryRepository.GetAll().Include(s => s.Articles).FirstOrDefaultAsync(s => s.Id.ToString() == item.DisplayName);
                if (temp != null && temp.Articles.Any(s => s.Id == article.Id) == false)
                {
                    //补全审核记录
                    //创建审核数据模型
                    var examinedModel = new EntryRelevances();

                    examinedModel.Relevances.Add(new EntryRelevancesAloneModel
                    {
                        DisplayName = article.Id.ToString(),
                        DisplayValue = article.Name,
                        IsDelete = false,
                        Type = RelevancesType.Article,
                    });

                    var resulte = "";
                    using (TextWriter text = new StringWriter())
                    {
                        var serializer = new JsonSerializer();
                        serializer.Serialize(text, examinedModel);
                        resulte = text.ToString();
                    }

                    await ExamineEstablishRelevancesAsync(temp, examinedModel);
                    await UniversalEditExaminedAsync(temp, admin, true, resulte, Operation.EstablishRelevances, "自动反向关联");
                }


            }

        }

        public async Task ExamineEditArticleMainPageAsync(Article article, string examine)
        {
            _articleService.UpdateArticleDataMainPage(article, examine);

            await _articleRepository.UpdateAsync(article);
        }
        #endregion

        #region 标签
        public async Task ExamineTagAsync(DataModel.Model.Tag tag, TagEdit examine)
        {
            await _tagService.UpdateTagDataOldAsync(tag, examine);

            await _tagRepository.UpdateAsync(tag);
        }

        public async Task ExamineEditTagMainAsync(Tag tag, ExamineMain examine)
        {
            await _tagService.UpdateTagDataMainAsync(tag, examine);

            await _tagRepository.UpdateAsync(tag);
        }

        public async Task ExamineEditTagChildTagsAsync(Tag tag, TagChildTags examine)
        {
            await _tagService.UpdateTagDataChildTagsAsync(tag, examine);

            await _tagRepository.UpdateAsync(tag);
        }

        public async Task ExamineEditTagChildEntriesAsync(Tag tag, TagChildEntries examine)
        {
            await _tagService.UpdateTagDataChildEntriesAsync(tag, examine);

            await _tagRepository.UpdateAsync(tag);
        }
        #endregion

        #region 消歧义页
        public async Task ExamineEditDisambigMainAsync(Disambig disambig, DisambigMain examine)
        {
            _disambigService.UpdateDisambigDataMain(disambig, examine);

            await _disambigRepository.UpdateAsync(disambig);
        }

        public async Task ExamineEditDisambigRelevancesAsync(Disambig disambig, DisambigRelevances examine)
        {

            await _disambigService.UpdateDisambigDataRelevancesAsync(disambig, examine);

            await _disambigRepository.UpdateAsync(disambig);
        }
        #endregion

        #region 用户

        public async Task ExamineEditUserMainAsync(ApplicationUser user, UserMain examine)
        {
            await _userService.UpdateUserDataMain(user, examine);

            await _userRepository.UpdateAsync(user);
        }
        public async Task ExamineEditUserMainPageAsync(ApplicationUser user, string examine)
        {
            await _userService.UpdateUserDataMainPage(user, examine);

            await _userRepository.UpdateAsync(user);
        }


        #endregion

        #region 周边

        public async Task ExamineEditPeripheryMainAsync(Periphery periphery, ExamineMain examine)
        {
            //更新数据
            _peripheryService.UpdatePeripheryDataMain(periphery, examine);
            //保存
            await _peripheryRepository.UpdateAsync(periphery);
        }

        public async Task ExamineEditPeripheryImagesAsync(Periphery periphery, PeripheryImages examine)
        {
            //更新数据
            _peripheryService.UpdatePeripheryDataImages(periphery, examine);
            //保存
            await _peripheryRepository.UpdateAsync(periphery);
        }

        public async Task ExamineEditPeripheryRelatedEntriesAsync(Periphery periphery, PeripheryRelatedEntries examine)
        {
            //更新数据
            await _peripheryService.UpdatePeripheryDataRelatedEntries(periphery, examine);
            //保存
            await _peripheryRepository.UpdateAsync(periphery);

        }

        public async Task ExamineEditPeripheryRelatedPeripheriesAsync(Periphery periphery, PeripheryRelatedPeripheries examine)
        {
            //更新数据
            await _peripheryService.UpdatePeripheryDataRelatedPeripheriesAsync(periphery, examine);
            //保存
            await _peripheryRepository.UpdateAsync(periphery);
        }

        #endregion



        #endregion

        #region 将审核记录添加到数据库

        public async Task UniversalEditExaminedAsync(Entry entry, ApplicationUser user, bool isAdmin, string examineStr, Operation operation, string note)
        {
            if (isAdmin)
            {
                //添加到审核列表
                var examine = new Examine
                {
                    Operation = operation,
                    Context = examineStr,
                    IsPassed = true,
                    EntryId = entry.Id,
                    PassedTime = DateTime.Now.ToCstTime(),
                    ApplyTime = DateTime.Now.ToCstTime(),
                    ApplicationUserId = user.Id,
                    PassedAdminName = user.UserName,
                    Note = note
                };
                await _examineRepository.InsertAsync(examine);
            }
            else
            {
                //查找是否在之前有审核
                //获取审核记录
                var examine = await GetUserEntryActiveExamineAsync(entry.Id, user.Id, operation);
                if (examine != null)
                {
                    examine.Context = examineStr;
                    examine.ApplyTime = DateTime.Now.ToCstTime();
                    await _examineRepository.UpdateAsync(examine);
                }
                else
                {
                    examine = new Examine
                    {
                        Operation = operation,
                        Context = examineStr,
                        IsPassed = null,
                        PassedTime = null,
                        EntryId = entry.Id,
                        ApplyTime = DateTime.Now.ToCstTime(),
                        ApplicationUserId = user.Id,
                        Note = note
                    };
                    //添加到审核列表
                    await _examineRepository.InsertAsync(examine);
                }
            }
        }

        public async Task<bool> UniversalEstablishExaminedAsync(Entry entry, ApplicationUser user, bool isAdmin, string examineStr, Operation operation, string note)
        {
            if (isAdmin)
            {
                //添加到审核列表
                var examine = new Examine
                {
                    Operation = operation,
                    Context = examineStr,
                    IsPassed = true,
                    PassedAdminName = user.UserName,
                    EntryId = entry.Id,
                    PassedTime = DateTime.Now.ToCstTime(),
                    ApplyTime = DateTime.Now.ToCstTime(),
                    ApplicationUserId = user.Id,
                    Note = note
                };
                await _examineRepository.InsertAsync(examine);

            }
            else
            {
                //根据词条Id查找前置审核
                long? examineId = null;
                if (operation != Operation.EstablishMain)
                {
                    var examine_1 = await GetUserEntryActiveExamineAsync(entry.Id, user.Id, Operation.EstablishMain);
                    if (examine_1 == null)
                    {
                        return false;
                    }
                    examineId = examine_1.Id;
                }

                //添加到审核列表
                var examine = new Examine
                {
                    Operation = operation,
                    Context = examineStr,
                    IsPassed = null,
                    EntryId = entry.Id,
                    PassedTime = null,
                    ApplyTime = DateTime.Now.ToCstTime(),
                    ApplicationUserId = user.Id,
                    PrepositionExamineId = examineId,
                    Note = note
                };
                await _examineRepository.InsertAsync(examine);

            }
            return true;
        }


        public async Task UniversalEditArticleExaminedAsync(Article article, ApplicationUser user, bool isAdmin, string examineStr, Operation operation, string note)
        {
            if (isAdmin)
            {
                //添加到审核列表
                var examine = new Examine
                {
                    Operation = operation,
                    Context = examineStr,
                    IsPassed = true,
                    PassedAdminName = user.UserName,
                    ArticleId = article.Id,
                    PassedTime = DateTime.Now.ToCstTime(),
                    ApplyTime = DateTime.Now.ToCstTime(),
                    ApplicationUserId = user.Id,
                };
                await _examineRepository.InsertAsync(examine);
            }
            else
            {
                //查找是否在之前有审核
                //获取审核记录
                var examine = await GetUserArticleActiveExamineAsync(article.Id, user.Id, operation);
                if (examine != null)
                {
                    examine.Context = examineStr;
                    examine.ApplyTime = DateTime.Now.ToCstTime();
                    await _examineRepository.UpdateAsync(examine);
                }
                else
                {
                    examine = new Examine
                    {
                        Operation = operation,
                        Context = examineStr,
                        IsPassed = null,
                        PassedTime = null,
                        ArticleId = article.Id,
                        ApplyTime = DateTime.Now.ToCstTime(),
                        ApplicationUserId = user.Id,
                    };
                    //添加到审核列表
                    await _examineRepository.InsertAsync(examine);
                }
            }
        }

        public async Task<bool> UniversalCreateArticleExaminedAsync(Article article, ApplicationUser user, bool isAdmin, string examineStr, Operation operation, string note)
        {
            if (isAdmin)
            {
                //添加到审核列表
                var examine = new Examine
                {
                    Operation = operation,
                    Context = examineStr,
                    IsPassed = true,
                    ArticleId = article.Id,
                    PassedAdminName = user.UserName,
                    PassedTime = DateTime.Now.ToCstTime(),
                    ApplyTime = DateTime.Now.ToCstTime(),
                    ApplicationUserId = user.Id,
                    Note = note
                };
                await _examineRepository.InsertAsync(examine);

            }
            else
            {
                //根据文章Id查找前置审核
                long? examineId = null;
                if (operation != Operation.EditArticleMain)
                {
                    var examine_1 = await GetUserArticleActiveExamineAsync(article.Id, user.Id, Operation.EditArticleMain);
                    if (examine_1 == null)
                    {
                        return false;
                    }
                    examineId = examine_1.Id;
                }

                //添加到审核列表
                var examine = new Examine
                {
                    Operation = operation,
                    Context = examineStr,
                    IsPassed = null,
                    ArticleId = article.Id,
                    PassedTime = null,
                    ApplyTime = DateTime.Now.ToCstTime(),
                    ApplicationUserId = user.Id,
                    PrepositionExamineId = examineId,
                    Note = note
                };
                await _examineRepository.InsertAsync(examine);

            }
            return true;
        }

        public async Task<bool> UniversalCreateTagExaminedAsync(Tag tag, ApplicationUser user, bool isAdmin, string examineStr, Operation operation, string note)
        {
            if (isAdmin)
            {
                //添加到审核列表
                var examine = new Examine
                {
                    Operation = operation,
                    Context = examineStr,
                    IsPassed = true,
                    TagId = tag.Id,
                    PassedAdminName = user.UserName,
                    PassedTime = DateTime.Now.ToCstTime(),
                    ApplyTime = DateTime.Now.ToCstTime(),
                    ApplicationUserId = user.Id,
                    Note = note
                };
                await _examineRepository.InsertAsync(examine);

            }
            else
            {
                //根据文章Id查找前置审核
                long? examineId = null;
                if (operation != Operation.EditTagMain)
                {
                    var examine_1 = await GetUserTagActiveExamineAsync(tag.Id, user.Id, Operation.EditTagMain);
                    if (examine_1 == null)
                    {
                        return false;
                    }
                    examineId = examine_1.Id;
                }

                //添加到审核列表
                var examine = new Examine
                {
                    Operation = operation,
                    Context = examineStr,
                    IsPassed = null,
                    TagId = tag.Id,
                    PassedTime = null,
                    ApplyTime = DateTime.Now.ToCstTime(),
                    ApplicationUserId = user.Id,
                    PrepositionExamineId = examineId,
                    Note = note
                };
                await _examineRepository.InsertAsync(examine);

            }
            return true;
        }

        public async Task UniversalEditTagExaminedAsync(Tag tag, ApplicationUser user, bool isAdmin, string examineStr, Operation operation, string note)
        {
            if (isAdmin)
            {
                //添加到审核列表
                var examine = new Examine
                {
                    Operation = operation,
                    Context = examineStr,
                    IsPassed = true,
                    PassedAdminName = user.UserName,
                    TagId = tag.Id,
                    PassedTime = DateTime.Now.ToCstTime(),
                    ApplyTime = DateTime.Now.ToCstTime(),
                    ApplicationUserId = user.Id,
                };
                await _examineRepository.InsertAsync(examine);
            }
            else
            {
                //查找是否在之前有审核
                //获取审核记录
                var examine = await GetUserTagActiveExamineAsync(tag.Id, user.Id, operation);
                if (examine != null)
                {
                    examine.Context = examineStr;
                    examine.ApplyTime = DateTime.Now.ToCstTime();
                    await _examineRepository.UpdateAsync(examine);
                }
                else
                {
                    examine = new Examine
                    {
                        Operation = operation,
                        Context = examineStr,
                        IsPassed = null,
                        PassedTime = null,
                        TagId = tag.Id,
                        ApplyTime = DateTime.Now.ToCstTime(),
                        ApplicationUserId = user.Id,
                    };
                    //添加到审核列表
                    await _examineRepository.InsertAsync(examine);
                }
            }
        }


        public async Task<bool> UniversalCreateDisambigExaminedAsync(Disambig disambig, ApplicationUser user, bool isAdmin, string examineStr, Operation operation, string note)
        {
            if (isAdmin)
            {
                //添加到审核列表
                var examine = new Examine
                {
                    Operation = operation,
                    Context = examineStr,
                    IsPassed = true,
                    DisambigId = disambig.Id,
                    PassedAdminName = user.UserName,
                    PassedTime = DateTime.Now.ToCstTime(),
                    ApplyTime = DateTime.Now.ToCstTime(),
                    ApplicationUserId = user.Id,
                    Note = note
                };
                await _examineRepository.InsertAsync(examine);

            }
            else
            {
                //根据文章Id查找前置审核
                long? examineId = null;
                if (operation != Operation.DisambigMain)
                {
                    var examine_1 = await GetUserDisambigActiveExamineAsync(disambig.Id, user.Id, Operation.DisambigMain);
                    if (examine_1 == null)
                    {
                        return false;
                    }
                    examineId = examine_1.Id;
                }

                //添加到审核列表
                var examine = new Examine
                {
                    Operation = operation,
                    Context = examineStr,
                    IsPassed = null,
                    DisambigId = disambig.Id,
                    PassedTime = null,
                    ApplyTime = DateTime.Now.ToCstTime(),
                    ApplicationUserId = user.Id,
                    PrepositionExamineId = examineId,
                    Note = note
                };
                await _examineRepository.InsertAsync(examine);

            }
            return true;
        }

        public async Task UniversalEditDisambigExaminedAsync(Disambig disambig, ApplicationUser user, bool isAdmin, string examineStr, Operation operation, string note)
        {
            if (isAdmin)
            {
                //添加到审核列表
                var examine = new Examine
                {
                    Operation = operation,
                    Context = examineStr,
                    IsPassed = true,
                    PassedAdminName = user.UserName,
                    DisambigId = disambig.Id,
                    PassedTime = DateTime.Now.ToCstTime(),
                    ApplyTime = DateTime.Now.ToCstTime(),
                    ApplicationUserId = user.Id,
                };
                await _examineRepository.InsertAsync(examine);
            }
            else
            {
                //查找是否在之前有审核
                //获取审核记录
                var examine = await GetUserDisambigActiveExamineAsync(disambig.Id, user.Id, operation);
                if (examine != null)
                {
                    examine.Context = examineStr;
                    examine.ApplyTime = DateTime.Now.ToCstTime();
                    await _examineRepository.UpdateAsync(examine);
                }
                else
                {
                    examine = new Examine
                    {
                        Operation = operation,
                        Context = examineStr,
                        IsPassed = null,
                        PassedTime = null,
                        DisambigId = disambig.Id,
                        ApplyTime = DateTime.Now.ToCstTime(),
                        ApplicationUserId = user.Id,
                    };
                    //添加到审核列表
                    await _examineRepository.InsertAsync(examine);
                }
            }
        }

        public async Task UniversalEditUserExaminedAsync(ApplicationUser user, bool isAdmin, string examineStr, Operation operation, string note)
        {
            if (isAdmin)
            {
                //添加到审核列表
                var examine = new Examine
                {
                    Operation = operation,
                    Context = examineStr,
                    IsPassed = true,
                    PassedAdminName = user.UserName,
                    PassedTime = DateTime.Now.ToCstTime(),
                    ApplyTime = DateTime.Now.ToCstTime(),
                    ApplicationUserId = user.Id,
                    Note = note
                };
                await _examineRepository.InsertAsync(examine);
            }
            else
            {
                //查找是否在之前有审核
                //获取审核记录
                var examine = await GetUserInforActiveExamineAsync(user.Id, operation);
                if (examine != null)
                {
                    examine.Context = examineStr;
                    examine.ApplyTime = DateTime.Now.ToCstTime();
                    await _examineRepository.UpdateAsync(examine);
                }
                else
                {
                    examine = new Examine
                    {
                        Operation = operation,
                        Context = examineStr,
                        IsPassed = null,
                        PassedTime = null,
                        ApplyTime = DateTime.Now.ToCstTime(),
                        ApplicationUserId = user.Id,
                        Note = note
                    };
                    //添加到审核列表
                    await _examineRepository.InsertAsync(examine);
                }
            }
        }

        public async Task<bool> UniversalCreatePeripheryExaminedAsync(Periphery periphery, ApplicationUser user, bool isAdmin, string examineStr, Operation operation, string note)
        {
            if (isAdmin)
            {
                //添加到审核列表
                var examine = new Examine
                {
                    Operation = operation,
                    Context = examineStr,
                    IsPassed = true,
                    PeripheryId = periphery.Id,
                    PassedAdminName = user.UserName,
                    PassedTime = DateTime.Now.ToCstTime(),
                    ApplyTime = DateTime.Now.ToCstTime(),
                    ApplicationUserId = user.Id,
                    Note = note
                };
                await _examineRepository.InsertAsync(examine);

            }
            else
            {
                //根据文章Id查找前置审核
                long? examineId = null;
                if (operation != Operation.DisambigMain)
                {
                    var examine_1 = await GetUserPeripheryActiveExamineAsync(periphery.Id, user.Id, Operation.EditPeripheryMain);
                    if (examine_1 == null)
                    {
                        return false;
                    }
                    examineId = examine_1.Id;
                }

                //添加到审核列表
                var examine = new Examine
                {
                    Operation = operation,
                    Context = examineStr,
                    IsPassed = null,
                    PeripheryId = periphery.Id,
                    PassedTime = null,
                    ApplyTime = DateTime.Now.ToCstTime(),
                    ApplicationUserId = user.Id,
                    PrepositionExamineId = examineId,
                    Note = note
                };
                await _examineRepository.InsertAsync(examine);

            }
            return true;
        }

        public async Task UniversalEditPeripheryExaminedAsync(Periphery periphery, ApplicationUser user, bool isAdmin, string examineStr, Operation operation, string note)
        {
            if (isAdmin)
            {
                //添加到审核列表
                var examine = new Examine
                {
                    Operation = operation,
                    Context = examineStr,
                    IsPassed = true,
                    PassedAdminName = user.UserName,
                    PeripheryId = periphery.Id,
                    PassedTime = DateTime.Now.ToCstTime(),
                    ApplyTime = DateTime.Now.ToCstTime(),
                    ApplicationUserId = user.Id,
                };
                await _examineRepository.InsertAsync(examine);
            }
            else
            {
                //查找是否在之前有审核
                //获取审核记录
                var examine = await GetUserPeripheryActiveExamineAsync(periphery.Id, user.Id, operation);
                if (examine != null)
                {
                    examine.Context = examineStr;
                    examine.ApplyTime = DateTime.Now.ToCstTime();
                    await _examineRepository.UpdateAsync(examine);
                }
                else
                {
                    examine = new Examine
                    {
                        Operation = operation,
                        Context = examineStr,
                        IsPassed = null,
                        PassedTime = null,
                        PeripheryId = periphery.Id,
                        ApplyTime = DateTime.Now.ToCstTime(),
                        ApplicationUserId = user.Id,
                    };
                    //添加到审核列表
                    await _examineRepository.InsertAsync(examine);
                }
            }
        }

        #endregion

        #region 创建新模型数据

        public async Task<Entry> AddNewEntryExaminesAsync(Entry model, ApplicationUser user, string note)
        {
            var examines = _entryService.ExaminesCompletion(new Entry(), model);

            if (examines.Any(s => s.Value == Operation.EstablishMain) == false)
            {
                throw new Exception("无法获取主要信息审核记录，请联系管理员");
            }
            var entry = new Entry();

            entry = await _entryRepository.InsertAsync(entry);

            examines = examines.OrderBy(s => s.Value).ToList();
            foreach (var item in examines)
            {

                var resulte = "";
                if (item.Value == Operation.EstablishMainPage)
                {
                    resulte = item.Key as string;
                }
                else
                {
                    using (TextWriter text = new StringWriter())
                    {
                        var serializer = new JsonSerializer();
                        serializer.Serialize(text, item.Key);
                        resulte = text.ToString();
                    }
                }
             
                if (await _userManager.IsInRoleAsync(user, "Editor") == true)
                {
                    switch (item.Value)
                    {
                        case Operation.EstablishMain:
                            await ExamineEstablishMainAsync(entry, item.Key as ExamineMain);
                            break;
                        case Operation.EstablishAddInfor:
                            await ExamineEstablishAddInforAsync(entry, item.Key as EntryAddInfor);
                            break;
                        case Operation.EstablishMainPage:
                            await ExamineEstablishMainPageAsync(entry, item.Key as string);
                            break;
                        case Operation.EstablishImages:
                            await ExamineEstablishImagesAsync(entry, item.Key as EntryImages);
                            break;
                        case Operation.EstablishRelevances:
                            await ExamineEstablishRelevancesAsync(entry, item.Key as EntryRelevances);
                            break;
                        case Operation.EstablishTags:
                            await ExamineEstablishTagsAsync(entry, item.Key as EntryTags);
                            break;
                        default:
                            throw new Exception("不支持的类型");
                    }

                    await UniversalEstablishExaminedAsync(entry, user, true, resulte, item.Value, note);
                    await _appHelper.AddUserContributionValueAsync(user.Id, entry.Id, item.Value);
                }
                else
                {
                    await UniversalEstablishExaminedAsync(entry, user, false, resulte, item.Value, note);
                }
            }

            return entry;
        }

        public async Task<Article> AddNewArticleExaminesAsync(Article model, ApplicationUser user, string note)
        {
            var examines = _articleService.ExaminesCompletion(new Article(), model);

            if (examines.Any(s => s.Value == Operation.EditArticleMain) == false)
            {
                throw new Exception("无法获取主要信息审核记录，请联系管理员");
            }
            var article = new Article();

            article = await _articleRepository.InsertAsync(article);
            article.CreateUser = user;
            article.CreateTime = model.CreateTime;

            examines = examines.OrderBy(s => s.Value).ToList();
            foreach (var item in examines)
            {

                var resulte = "";
                if (item.Value == Operation.EditArticleMainPage)
                {
                    resulte = item.Key as string ;
                }
                else
                {
                    using (TextWriter text = new StringWriter())
                    {
                        var serializer = new JsonSerializer();
                        serializer.Serialize(text, item.Key);
                        resulte = text.ToString();
                    }
                }
               
                if (await _userManager.IsInRoleAsync(user, "Editor") == true)
                {
                    switch (item.Value)
                    {
                        case Operation.EditArticleMain:
                            await ExamineEditArticleMainAsync(article, item.Key as ExamineMain);
                            break;
                        case Operation.EditArticleMainPage:
                            await ExamineEditArticleMainPageAsync(article, item.Key as string);
                            break;
                        case Operation.EditArticleRelevanes:
                            await ExamineEditArticleRelevancesAsync(article, item.Key as ArticleRelevances);
                            break;
                      
                        default:
                            throw new Exception("不支持的类型");
                    }

                    await UniversalCreateArticleExaminedAsync(article, user, true, resulte, item.Value, note);
                    await _appHelper.AddUserContributionValueAsync(user.Id, article.Id, item.Value);
                }
                else
                {
                    await UniversalCreateArticleExaminedAsync(article, user, false, resulte, item.Value, note);
                }
            }

            return article;
        }

        public async Task<Periphery> AddNewPeripheryExaminesAsync(Periphery model, ApplicationUser user, string note)
        {
            var examines = _peripheryService.ExaminesCompletion(new Periphery(), model);

            if (examines.Any(s => s.Value == Operation.EditPeripheryMain) == false)
            {
                throw new Exception("无法获取主要信息审核记录，请联系管理员");
            }
            var periphery = new Periphery();

            periphery = await _peripheryRepository.InsertAsync(periphery);

            examines = examines.OrderBy(s => s.Value).ToList();
            foreach (var item in examines)
            {

                var resulte = "";
                    using (TextWriter text = new StringWriter())
                    {
                        var serializer = new JsonSerializer();
                        serializer.Serialize(text, item.Key);
                        resulte = text.ToString();
                    }
                

                if (await _userManager.IsInRoleAsync(user, "Editor") == true)
                {
                    switch (item.Value)
                    {
                        case Operation.EditPeripheryMain:
                            await ExamineEditPeripheryMainAsync(periphery, item.Key as ExamineMain);
                            break;
                        case Operation.EditPeripheryImages:
                            await ExamineEditPeripheryImagesAsync(periphery, item.Key as PeripheryImages);
                            break;
                        case Operation.EditPeripheryRelatedEntries:
                            await ExamineEditPeripheryRelatedEntriesAsync(periphery, item.Key as PeripheryRelatedEntries);
                            break;
                        case Operation.EditPeripheryRelatedPeripheries:
                            await ExamineEditPeripheryRelatedPeripheriesAsync(periphery, item.Key as PeripheryRelatedPeripheries);
                            break;

                        default:
                            throw new Exception("不支持的类型");
                    }

                    await UniversalCreatePeripheryExaminedAsync(periphery, user, true, resulte, item.Value, note);
                    await _appHelper.AddUserContributionValueAsync(user.Id, periphery.Id, item.Value);
                }
                else
                {
                    await UniversalCreatePeripheryExaminedAsync(periphery, user, false, resulte, item.Value, note);
                }
            }

            return periphery;
        }

        public async Task<Tag> AddNewTagExaminesAsync(Tag model, ApplicationUser user, string note)
        {
            var examines = _tagService.ExaminesCompletion(new Tag(), model);

            if (examines.Any(s => s.Value == Operation.EditPeripheryMain) == false)
            {
                throw new Exception("无法获取主要信息审核记录，请联系管理员");
            }
            var tag = new Tag();

            tag = await _tagRepository.InsertAsync(tag);

            examines = examines.OrderBy(s => s.Value).ToList();
            foreach (var item in examines)
            {

                var resulte = "";
                using (TextWriter text = new StringWriter())
                {
                    var serializer = new JsonSerializer();
                    serializer.Serialize(text, item.Key);
                    resulte = text.ToString();
                }


                if (await _userManager.IsInRoleAsync(user, "Editor") == true)
                {
                    switch (item.Value)
                    {
                        case Operation.EditTagMain:
                            await ExamineEditTagMainAsync(tag, item.Key as ExamineMain);
                            break;
                        case Operation.EditTagChildEntries:
                            await ExamineEditTagChildEntriesAsync(tag, item.Key as TagChildEntries);
                            break;
                        case Operation.EditTagChildTags:
                            await ExamineEditTagChildTagsAsync(tag, item.Key as TagChildTags);
                            break;

                        default:
                            throw new Exception("不支持的类型");
                    }

                    await UniversalCreateTagExaminedAsync(tag, user, true, resulte, item.Value, note);
                    await _appHelper.AddUserContributionValueAsync(user.Id, tag.Id, item.Value);
                }
                else
                {
                    await UniversalCreateTagExaminedAsync(tag, user, false, resulte, item.Value, note);
                }
            }

            return tag;
        }


        #endregion

        #region 获取用户待审核记录
        public async Task<Examine> GetUserEntryActiveExamineAsync(int entryId, string userId, Operation operation)
        {
            return await _examineRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(s => s.EntryId == entryId && s.ApplicationUserId == userId && s.Operation == operation && s.IsPassed == null);
        }

        public async Task<Examine> GetUserArticleActiveExamineAsync(long articleId, string userId, Operation operation)
        {
            return await _examineRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(s => s.ArticleId == articleId && s.ApplicationUserId == userId && s.Operation == operation && s.IsPassed == null);
        }

        public async Task<Examine> GetUserTagActiveExamineAsync(int tagId, string userId, Operation operation)
        {
            return await _examineRepository.FirstOrDefaultAsync(s => s.TagId == tagId && s.ApplicationUserId == userId && s.Operation == operation && s.IsPassed == null);
        }

        public async Task<Examine> GetUserInforActiveExamineAsync(string userId, Operation operation)
        {
            return await _examineRepository.FirstOrDefaultAsync(s => s.ApplicationUserId == userId && s.Operation == operation && s.IsPassed == null);
        }

        public async Task<Examine> GetUserDisambigActiveExamineAsync(int disambigId, string userId, Operation operation)
        {
            return await _examineRepository.FirstOrDefaultAsync(s => s.DisambigId == disambigId && s.ApplicationUserId == userId && s.Operation == operation && s.IsPassed == null);
        }

        public async Task<Examine> GetUserPeripheryActiveExamineAsync(long peripheryId, string userId, Operation operation)
        {
            return await _examineRepository.FirstOrDefaultAsync(s => s.PeripheryId == peripheryId && s.ApplicationUserId == userId && s.Operation == operation && s.IsPassed == null);
        }

        #endregion

        #region 迁移审核记录

        public async Task MigrationEditEntryTagsExamineRecord()
        {
            await ReplaceEditEntryTagsExamineContext();
        }

        public async Task MigrationEditArticleRelevanceExamineRecord()
        {
            await ReplaceArticleRelevances();
            await ReplaceEditArticleRelevancesExamineContext();
        }
        public async Task MigrationEditEntryRelevanceExamineRecord()
        {
            await ReplaceEntryRelevances();
            await ReplaceEditEntryRelevancesExamineContext();
        }

        /// <summary>
        /// 迁移标签审核数据 主要信息部分
        /// </summary>
        /// <returns></returns>
        public async Task ReplaceEditTagMainExamineContext()
        {
            TagMain_1_0 oldExamineModel = null;
            ExamineMain newExamineModel = null;
            //获取要替换的所有审核记录ID
            var ids = await _examineRepository.GetAll().AsNoTracking().Where(s => s.Operation == Operation.EditTagMain && s.Version == ExamineVersion.V1_0).Select(s => s.Id).ToListAsync();

            //遍历列表 依次替换
            foreach (var id in ids)
            {
                var examine = await _examineRepository.FirstOrDefaultAsync(s => s.Id == id);
                if (examine != null)
                {
                    var newTag = new Tag();

                    //获取该词条之前的每次编辑主页的审核
                    var beforeexamine = await _examineRepository.GetAll()
                        .Where(s => s.IsPassed == true && s.Id < examine.Id && s.TagId == examine.TagId && s.Operation == Operation.EditTagMain).ToListAsync();
                    //若没有则与新模型对比
                    if (beforeexamine.Count == 0)
                    {
                        using (TextReader str = new StringReader(examine.Context))
                        {
                            var serializer = new JsonSerializer();
                            oldExamineModel = (TagMain_1_0)serializer.Deserialize(str, typeof(TagMain_1_0));
                        }
                        await _tagService.UpdateTagDataMainAsync(newTag, oldExamineModel);
                        newExamineModel = new ExamineMain
                        {
                            Items = ToolHelper.GetEditingRecordFromContrastData(new Tag(), newTag)
                        };
                    }
                    else
                    {
                        //将之前的审核记录叠加进新模型后对比
                        var currentTag = new Tag();
                        foreach (var item in beforeexamine)
                        {
                            using (TextReader str = new StringReader(item.Context))
                            {
                                var serializer = new JsonSerializer();
                                oldExamineModel = (TagMain_1_0)serializer.Deserialize(str, typeof(TagMain_1_0));
                            }
                            await _tagService.UpdateTagDataMainAsync(currentTag, oldExamineModel);
                        }

                        using (TextReader str = new StringReader(examine.Context))
                        {
                            var serializer = new JsonSerializer();
                            oldExamineModel = (TagMain_1_0)serializer.Deserialize(str, typeof(TagMain_1_0));
                        }
                        await _tagService.UpdateTagDataMainAsync(newTag, oldExamineModel);

                        newExamineModel = new ExamineMain
                        {
                            Items = ToolHelper.GetEditingRecordFromContrastData(new Tag(), newTag)
                        };
                    }

                    //序列化新数据模型
                    var resulte = "";
                    using (TextWriter text = new StringWriter())
                    {
                        var serializer = new JsonSerializer();
                        serializer.Serialize(text, newExamineModel);
                        resulte = text.ToString();
                    }

                    //保存
                    if (newExamineModel.Items.Count == 0)
                    {
                        examine.Note += "\n" + DateTime.Now.ToCstTime().ToString("yyyy年MM月dd日 HH:mm") + " 迁移词条主要信息编辑记录";
                    }
                    examine.Version = ExamineVersion.V1_1;
                    examine.Context = resulte;
                    await _examineRepository.UpdateAsync(examine);
                }
            }
        }

        /// <summary>
        /// 迁移周边审核数据 主要信息部分
        /// </summary>
        /// <returns></returns>
        public async Task ReplaceEditPeripheryMainExamineContext()
        {
            PeripheryMain_1_0 oldExamineModel = null;
            ExamineMain newExamineModel = null;
            //获取要替换的所有审核记录ID
            var ids = await _examineRepository.GetAll().AsNoTracking().Where(s => s.Operation == Operation.EditPeripheryMain && s.Version == ExamineVersion.V1_0).Select(s => s.Id).ToListAsync();

            //遍历列表 依次替换
            foreach (var id in ids)
            {
                var examine = await _examineRepository.FirstOrDefaultAsync(s => s.Id == id);
                if (examine != null)
                {
                    var newPeriphery = new Periphery();

                    //获取该词条之前的每次编辑主页的审核
                    var beforeexamine = await _examineRepository.GetAll()
                        .Where(s => s.IsPassed == true && s.Id < examine.Id && s.PeripheryId == examine.PeripheryId && s.Operation == Operation.EditPeripheryMain).ToListAsync();
                    //若没有则与新模型对比
                    if (beforeexamine.Count == 0)
                    {
                        using (TextReader str = new StringReader(examine.Context))
                        {
                            var serializer = new JsonSerializer();
                            oldExamineModel = (PeripheryMain_1_0)serializer.Deserialize(str, typeof(PeripheryMain_1_0));
                        }
                        _peripheryService.UpdatePeripheryDataMain(newPeriphery, oldExamineModel);
                        newExamineModel = new ExamineMain
                        {
                            Items = ToolHelper.GetEditingRecordFromContrastData(new Periphery(), newPeriphery)
                        };
                    }
                    else
                    {
                        //将之前的审核记录叠加进新模型后对比
                        var currentPeriphery = new Periphery();
                        foreach (var item in beforeexamine)
                        {
                            using (TextReader str = new StringReader(item.Context))
                            {
                                var serializer = new JsonSerializer();
                                oldExamineModel = (PeripheryMain_1_0)serializer.Deserialize(str, typeof(PeripheryMain_1_0));
                            }
                            _peripheryService.UpdatePeripheryDataMain(currentPeriphery, oldExamineModel);
                        }

                        using (TextReader str = new StringReader(examine.Context))
                        {
                            var serializer = new JsonSerializer();
                            oldExamineModel = (PeripheryMain_1_0)serializer.Deserialize(str, typeof(PeripheryMain_1_0));
                        }
                        _peripheryService.UpdatePeripheryDataMain(newPeriphery, oldExamineModel);

                        newExamineModel = new ExamineMain
                        {
                            Items = ToolHelper.GetEditingRecordFromContrastData(new Periphery(), newPeriphery)
                        };
                    }

                    //序列化新数据模型
                    var resulte = "";
                    using (TextWriter text = new StringWriter())
                    {
                        var serializer = new JsonSerializer();
                        serializer.Serialize(text, newExamineModel);
                        resulte = text.ToString();
                    }

                    //保存
                    if (newExamineModel.Items.Count == 0)
                    {
                        examine.Note += "\n" + DateTime.Now.ToCstTime().ToString("yyyy年MM月dd日 HH:mm") + " 迁移词条主要信息编辑记录";
                    }
                    examine.Version = ExamineVersion.V1_1;
                    examine.Context = resulte;
                    await _examineRepository.UpdateAsync(examine);
                }
            }
        }
        /// <summary>
        /// 迁移周边关联词条数据
        /// </summary>
        /// <returns></returns>
        public async Task ReplacePeripheryRelatedEntries()
        {
            var peripheries = await _peripheryRepository.GetAll()
                 .Include(s => s.Entries).ThenInclude(s => s.Entry)
                 .Where(s => s.Entries.Any())
                 .ToListAsync();

            foreach (var item in peripheries)
            {
                item.RelatedEntries = item.Entries.Select(s => s.Entry).ToList();
                item.Entries.Clear();
                await _peripheryRepository.UpdateAsync(item);
            }
        }
        /// <summary>
        /// 迁移词条关联周边数据
        /// </summary>
        /// <returns></returns>
        public async Task ReplaceEntryRelatedPeripheries()
        {
            var entries = await _entryRepository.GetAll()
                 .Include(s => s.Peripheries).ThenInclude(s => s.Periphery)
                 .Where(s => s.Peripheries.Any())
                 .ToListAsync();

            foreach (var item in entries)
            {
                item.RelatedPeripheries = item.Peripheries.Select(s => s.Periphery).ToList();
                item.Peripheries.Clear();
                await _entryRepository.UpdateAsync(item);
            }
        }
        /// <summary>
        /// 迁移文章审核数据 主要信息部分
        /// </summary>
        /// <returns></returns>
        public async Task ReplaceEditArticleMainExamineContext()
        {
            ArticleMain_1_0 oldExamineModel = null;
            ExamineMain newExamineModel = null;
            //获取要替换的所有审核记录ID
            var ids = await _examineRepository.GetAll().AsNoTracking().Where(s => s.Operation == Operation.EditArticleMain && s.Version == ExamineVersion.V1_0).Select(s => s.Id).ToListAsync();

            //遍历列表 依次替换
            foreach (var id in ids)
            {
                var examine = await _examineRepository.FirstOrDefaultAsync(s => s.Id == id);
                if (examine != null)
                {
                    var newArticle = new Article();

                    //获取该词条之前的每次编辑主页的审核
                    var beforeexamine = await _examineRepository.GetAll()
                        .Where(s => s.IsPassed == true && s.Id < examine.Id && s.ArticleId == examine.ArticleId && s.Operation == Operation.EditArticleMain).ToListAsync();
                    //若没有则与新模型对比
                    if (beforeexamine.Count == 0)
                    {
                        using (TextReader str = new StringReader(examine.Context))
                        {
                            var serializer = new JsonSerializer();
                            oldExamineModel = (ArticleMain_1_0)serializer.Deserialize(str, typeof(ArticleMain_1_0));
                        }
                        _articleService.UpdateArticleDataMain(newArticle, oldExamineModel);
                        newExamineModel = new ExamineMain
                        {
                            Items = ToolHelper.GetEditingRecordFromContrastData(new Article(), newArticle)
                        };
                    }
                    else
                    {
                        //将之前的审核记录叠加进新模型后对比
                        var currentArticle = new Article();
                        foreach (var item in beforeexamine)
                        {
                            using (TextReader str = new StringReader(item.Context))
                            {
                                var serializer = new JsonSerializer();
                                oldExamineModel = (ArticleMain_1_0)serializer.Deserialize(str, typeof(ArticleMain_1_0));
                            }
                            _articleService.UpdateArticleDataMain(currentArticle, oldExamineModel);
                        }

                        using (TextReader str = new StringReader(examine.Context))
                        {
                            var serializer = new JsonSerializer();
                            oldExamineModel = (ArticleMain_1_0)serializer.Deserialize(str, typeof(ArticleMain_1_0));
                        }
                        _articleService.UpdateArticleDataMain(newArticle, oldExamineModel);

                        newExamineModel = new ExamineMain
                        {
                            Items = ToolHelper.GetEditingRecordFromContrastData(new Article(), newArticle)
                        };
                    }

                    //序列化新数据模型
                    var resulte = "";
                    using (TextWriter text = new StringWriter())
                    {
                        var serializer = new JsonSerializer();
                        serializer.Serialize(text, newExamineModel);
                        resulte = text.ToString();
                    }

                    //保存
                    if (newExamineModel.Items.Count == 0)
                    {
                        examine.Note += ("\n" + DateTime.Now.ToCstTime().ToString("yyyy年MM月dd日 HH:mm") + " 迁移词条主要信息编辑记录");
                    }
                    examine.Version = ExamineVersion.V1_1;
                    examine.Context = resulte;
                    await _examineRepository.UpdateAsync(examine);
                }
            }
        }


        /// <summary>
        /// 迁移词条审核数据 主要信息部分
        /// </summary>
        /// <returns></returns>
        public async Task ReplaceEditEntryMainExamineContext()
        {
            EntryMain_1_0 oldExamineModel = null;
            ExamineMain newExamineModel = null;
            //获取要替换的所有审核记录ID
            var ids = await _examineRepository.GetAll().AsNoTracking().Where(s => s.Operation == Operation.EstablishMain && s.Version == ExamineVersion.V1_0).Select(s => s.Id).ToListAsync();

            //遍历列表 依次替换
            foreach (var id in ids)
            {
                var examine = await _examineRepository.FirstOrDefaultAsync(s => s.Id == id);
                if (examine != null)
                {
                    var newEntry = new Entry();

                    //获取该词条之前的每次编辑主页的审核
                    var beforeexamine = await _examineRepository.GetAll()
                        .Where(s =>s.IsPassed==true&& s.Id<examine.Id&& s.EntryId == examine.EntryId && s.Operation == Operation.EstablishMain).ToListAsync();
                    //若没有则与新模型对比
                    if (beforeexamine.Count == 0)
                    {
                        using (TextReader str = new StringReader(examine.Context))
                        {
                            var serializer = new JsonSerializer();
                            oldExamineModel = (EntryMain_1_0)serializer.Deserialize(str, typeof(EntryMain_1_0));
                        }
                        _entryService.UpdateEntryDataMain(newEntry, oldExamineModel);
                        newExamineModel = new ExamineMain
                        {
                            Items = ToolHelper.GetEditingRecordFromContrastData(new Entry(), newEntry)
                        };
                        newExamineModel.Items.RemoveAll(s => s.Key == "PubulishTime");

                    }
                    else
                    {
                        //将之前的审核记录叠加进新模型后对比
                        var currentEntry = new Entry();
                        foreach (var item in beforeexamine)
                        {
                            using (TextReader str = new StringReader(item.Context))
                            {
                                var serializer = new JsonSerializer();
                                oldExamineModel = (EntryMain_1_0)serializer.Deserialize(str, typeof(EntryMain_1_0));
                            }
                            _entryService.UpdateEntryDataMain(currentEntry, oldExamineModel);
                        }
                        
                        using (TextReader str = new StringReader(examine.Context))
                        {
                            var serializer = new JsonSerializer();
                            oldExamineModel = (EntryMain_1_0)serializer.Deserialize(str, typeof(EntryMain_1_0));
                        }
                        _entryService.UpdateEntryDataMain(newEntry, oldExamineModel);

                        newExamineModel = new ExamineMain
                        {
                            Items = ToolHelper.GetEditingRecordFromContrastData(new Entry(), newEntry)
                        };
                        newExamineModel.Items.RemoveAll(s => s.Key == "PubulishTime");

                    }

                    //序列化新数据模型
                    var resulte = "";
                    using (TextWriter text = new StringWriter())
                    {
                        var serializer = new JsonSerializer();
                        serializer.Serialize(text, newExamineModel);
                        resulte = text.ToString();
                    }

                    //保存
                    if (newExamineModel.Items.Count == 0)
                    {
                        examine.Note += ("\n" + DateTime.Now.ToCstTime().ToString("yyyy年MM月dd日 HH:mm") + " 迁移词条主要信息编辑记录");
                    }
                    examine.Version = ExamineVersion.V1_1;
                    examine.Context = resulte;
                    await _examineRepository.UpdateAsync(examine);
                }
            }
        }


        /// <summary>
        /// 迁移文章审核数据 关联部分
        /// </summary>
        /// <returns></returns>
        private async Task ReplaceEditArticleRelevancesExamineContext()
        {
            ArticleRelecancesModel_1_0 oldExamineModel = null;
            ArticleRelevances newExamineModel = null;
            //获取要替换的所有审核记录ID
            var ids = await _examineRepository.GetAll().AsNoTracking().Where(s => s.Operation == Operation.EditArticleRelevanes && s.Version == ExamineVersion.V1_0).Select(s => s.Id).ToListAsync();

            //遍历列表 依次替换
            foreach (var id in ids)
            {
                var examine = await _examineRepository.FirstOrDefaultAsync(s => s.Id == id);
                if (examine != null)
                {
                    //反序列化旧数据模型                   
                    using (TextReader str = new StringReader(examine.Context))
                    {
                        var serializer = new JsonSerializer();
                        oldExamineModel = (ArticleRelecancesModel_1_0)serializer.Deserialize(str, typeof(ArticleRelecancesModel_1_0));
                    }

                    newExamineModel = new ArticleRelevances();

                    //遍历对应复制
                    foreach (var item in oldExamineModel.Relevances)
                    {
                        if (item.Modifier == "词条" || item.Modifier == "游戏" || item.Modifier == "制作组" || item.Modifier == "STAFF" || item.Modifier == "角色")
                        {
                            var newEntry = await _entryRepository.FirstOrDefaultAsync(s => s.Name == item.DisplayName);
                            if (newEntry != null)
                            {
                                newExamineModel.Relevances.Add(new ArticleRelevancesAloneModel
                                {
                                    DisplayName = newEntry.Id.ToString(),
                                    DisplayValue = newEntry.DisplayName,
                                    IsDelete = item.IsDelete,
                                    Type = RelevancesType.Entry
                                });
                            }

                        }
                        else if (item.Modifier == "文章" || item.Modifier == "动态")
                        {
                            var newArticle = await _articleRepository.FirstOrDefaultAsync(s => s.Name == item.DisplayName);
                            newExamineModel.Relevances.Add(new ArticleRelevancesAloneModel
                            {
                                DisplayName = newArticle.Id.ToString(),
                                DisplayValue = newArticle.DisplayName,
                                IsDelete = item.IsDelete,
                                Type = RelevancesType.Article
                            });
                        }
                        else
                        {
                            newExamineModel.Relevances.Add(new ArticleRelevancesAloneModel
                            {

                                DisplayName = item.DisplayName,
                                DisplayValue = item.DisplayValue,
                                Link = item.Link,
                                IsDelete = item.IsDelete,
                                Type = RelevancesType.Outlink
                            });
                        }

                    }

                    //序列化新数据模型
                    var resulte = "";
                    using (TextWriter text = new StringWriter())
                    {
                        var serializer = new JsonSerializer();
                        serializer.Serialize(text, newExamineModel);
                        resulte = text.ToString();
                    }

                    //保存
                    if (newExamineModel.Relevances.Count == 0)
                    {
                        examine.Note += ("\n" + DateTime.Now.ToCstTime().ToString("yyyy年MM月dd日 HH:mm") + " 迁移文章关联信息编辑记录");
                    }
                    examine.Version = ExamineVersion.V1_1;
                    examine.Context = resulte;
                    await _examineRepository.UpdateAsync(examine);
                }
            }
        }
        /// <summary>
        /// 迁移文章关联数据
        /// </summary>
        /// <returns></returns>
        private async Task ReplaceArticleRelevances()
        {
            var articles = await _articleRepository.GetAll().Where(s => s.Relevances.Any()).Include(s => s.Relevances)
                 .Include(s => s.ArticleRelationFromArticleNavigation).ThenInclude(s => s.ToArticleNavigation)
                 .Include(s => s.Entries)
                 .Include(s => s.Outlinks)
                 .ToListAsync();

            foreach (var item in articles)
            {
                foreach (var temp in item.Relevances)
                {
                    if (temp.Modifier == "词条" || temp.Modifier == "游戏" || temp.Modifier == "制作组" || temp.Modifier == "STAFF" || temp.Modifier == "角色")
                    {
                        var newEntry = await _entryRepository.FirstOrDefaultAsync(s => s.Name == temp.DisplayName);
                        if (newEntry != null)
                        {
                            item.Entries.Add(newEntry);

                        }
                    }
                    else if (temp.Modifier == "文章" || temp.Modifier == "动态")
                    {
                        var newArticle = await _articleRepository.FirstOrDefaultAsync(s => s.Name == temp.DisplayName);
                        if (newArticle != null)
                        {
                            item.ArticleRelationFromArticleNavigation.Add(new ArticleRelation
                            {
                                FromArticle = item.Id,
                                FromArticleNavigation = item,
                                ToArticle = newArticle.Id,
                                ToArticleNavigation = newArticle
                            });
                        }
                    }
                    else
                    {
                        item.Outlinks.Add(new Outlink
                        {
                            BriefIntroduction = temp.DisplayValue,
                            Name = temp.DisplayName,
                            Link = temp.Link,
                        });
                    }
                }
                item.Relevances.Clear();
                await _articleRepository.UpdateAsync(item);
            }
        }


        /// <summary>
        /// 迁移词条审核数据 关联部分
        /// </summary>
        /// <returns></returns>
        private async Task ReplaceEditEntryRelevancesExamineContext()
        {
            EntryRelevancesModel_1_0 oldExamineModel = null;
            EntryRelevances newExamineModel = null;
            //获取要替换的所有审核记录ID
            var ids = await _examineRepository.GetAll().AsNoTracking()
                .Where(s => s.Operation == Operation.EstablishRelevances && s.Version == ExamineVersion.V1_0).Select(s => s.Id).ToListAsync();

            //遍历列表 依次替换
            foreach (var id in ids)
            {
                var examine = await _examineRepository.GetAll().Include(s => s.Entry).FirstOrDefaultAsync(s => s.Id == id);
                if (examine != null)
                {
                    //反序列化旧数据模型                   
                    using (TextReader str = new StringReader(examine.Context))
                    {
                        var serializer = new JsonSerializer();
                        oldExamineModel = (EntryRelevancesModel_1_0)serializer.Deserialize(str, typeof(EntryRelevancesModel_1_0));
                    }

                    newExamineModel = new EntryRelevances();

                    //遍历对应复制
                    foreach (var item in oldExamineModel.Relevances)
                    {
                        if (item.Modifier == "词条" || item.Modifier == "游戏" || item.Modifier == "制作组" || item.Modifier == "STAFF" || item.Modifier == "角色")
                        {
                            if (examine.Entry.Type == EntryType.Game && item.Modifier == "STAFF")
                            {
                                continue;
                            }
                            var newEntry = await _entryRepository.FirstOrDefaultAsync(s => s.Name == item.DisplayName);
                            if (newEntry != null)
                            {
                                newExamineModel.Relevances.Add(new EntryRelevancesAloneModel
                                {
                                    DisplayName = newEntry.Id.ToString(),
                                    DisplayValue = newEntry.DisplayName,
                                    IsDelete = item.IsDelete,
                                    Type = RelevancesType.Entry
                                });
                            }

                        }
                        else if (item.Modifier == "文章" || item.Modifier == "动态")
                        {
                            var newArticle = await _articleRepository.FirstOrDefaultAsync(s => s.Name == item.DisplayName);
                            if (newArticle != null)
                            {
                                newExamineModel.Relevances.Add(new EntryRelevancesAloneModel
                                {
                                    DisplayName = newArticle.Id.ToString(),
                                    DisplayValue = newArticle.DisplayName,
                                    IsDelete = item.IsDelete,
                                    Type = RelevancesType.Article
                                });
                            }
                        }
                        else
                        {
                            newExamineModel.Relevances.Add(new EntryRelevancesAloneModel
                            {

                                DisplayName = item.DisplayName,
                                DisplayValue = item.DisplayValue,
                                Link=item.Link,
                                IsDelete = item.IsDelete,
                                Type = RelevancesType.Outlink
                            });
                        }

                    }

                    //序列化新数据模型
                    var resulte = "";
                    using (TextWriter text = new StringWriter())
                    {
                        var serializer = new JsonSerializer();
                        serializer.Serialize(text, newExamineModel);
                        resulte = text.ToString();
                    }

                    //保存
                    if (newExamineModel.Relevances.Count == 0)
                    {
                        examine.Note += ("\n" + DateTime.Now.ToCstTime().ToString("yyyy年MM月dd日 HH:mm") + " 迁移词条关联信息编辑记录");
                    }
                    examine.Version = ExamineVersion.V1_1;
                    examine.Context = resulte;
                    await _examineRepository.UpdateAsync(examine);
                }
            }
        }
        /// <summary>
        /// 迁移词条关联数据
        /// </summary>
        /// <returns></returns>
        private async Task ReplaceEntryRelevances()
        {
            var entries = await _entryRepository.GetAll().Where(s => s.Relevances.Any()).Include(s => s.Relevances)
                 .Include(s => s.EntryRelationFromEntryNavigation).ThenInclude(s => s.ToEntryNavigation)
                 .Include(s => s.Articles)
                 .Include(s => s.Outlinks)
                 .ToListAsync();

            foreach (var item in entries)
            {
                foreach (var temp in item.Relevances)
                {
                    if (temp.Modifier == "词条" || temp.Modifier == "游戏" || temp.Modifier == "制作组" || temp.Modifier == "STAFF" || temp.Modifier == "角色")
                    {
                        if (item.Type == EntryType.Game && temp.Modifier == "STAFF")
                        {
                            continue;
                        }
                        var newEntry = await _entryRepository.FirstOrDefaultAsync(s => s.Name == temp.DisplayName);
                        if (newEntry != null)
                        {
                            item.EntryRelationFromEntryNavigation.Add(new EntryRelation
                            {
                                FromEntry = item.Id,
                                FromEntryNavigation = item,
                                ToEntry = newEntry.Id,
                                ToEntryNavigation = newEntry
                            });
                        }

                    }
                    else if (temp.Modifier == "文章" || temp.Modifier == "动态")
                    {
                        var newArticle = await _articleRepository.FirstOrDefaultAsync(s => s.Name == temp.DisplayName);
                        if (newArticle != null)
                        {
                            item.Articles.Add(newArticle);

                        }
                    }
                    else
                    {
                        item.Outlinks.Add(new Outlink
                        {
                            BriefIntroduction = temp.DisplayValue,
                            Name = temp.DisplayName,
                            Link = temp.Link,
                        });
                    }
                }
                item.Relevances.Clear();
                await _entryRepository.UpdateAsync(item);
            }
        }

        /// <summary>
        /// 替换 EditEntryTags 类型 旧版 审核记录 到 新版
        /// </summary>
        /// <returns></returns>
        private async Task ReplaceEditEntryTagsExamineContext()
        {
            EntryTagsModel_1_0 oldExamineModel = null;
            EntryTags newExamineModel = null;
            //获取要替换的所有审核记录ID
            var ids = await _examineRepository.GetAll().AsNoTracking().Where(s => s.Operation == Operation.EstablishTags && s.Version == ExamineVersion.V1_0).Select(s => s.Id).ToListAsync();

            //遍历列表 依次替换
            foreach (var id in ids)
            {
                var examine = await _examineRepository.FirstOrDefaultAsync(s => s.Id == id);
                if (examine != null)
                {
                    //反序列化旧数据模型                   
                    using (TextReader str = new StringReader(examine.Context))
                    {
                        var serializer = new JsonSerializer();
                        oldExamineModel = (EntryTagsModel_1_0)serializer.Deserialize(str, typeof(EntryTagsModel_1_0));
                    }

                    newExamineModel = new EntryTags();

                    //遍历对应复制
                    foreach (var item in oldExamineModel.Tags)
                    {
                        //根据名称查找标签是否存在
                        var tagId = await _tagRepository.GetAll().Where(s => s.Name == item.Name).Select(s => s.Id).FirstOrDefaultAsync();
                        if (tagId > 0)
                        {
                            newExamineModel.Tags.Add(new EntryTagsAloneModel
                            {
                                TagId = tagId,
                                IsDelete = item.IsDelete
                            });
                        }

                    }

                    //序列化新数据模型
                    var resulte = "";
                    using (TextWriter text = new StringWriter())
                    {
                        var serializer = new JsonSerializer();
                        serializer.Serialize(text, newExamineModel);
                        resulte = text.ToString();
                    }

                    //保存
                    examine.Version = ExamineVersion.V1_1;
                    examine.Context = resulte;
                    await _examineRepository.UpdateAsync(examine);
                }
            }
        }

        #endregion

        #region 通过审核记录生成模型 用于对比编辑

        public async Task ExaminesCompletion()
        {
            //补全词条
            var entryIds = await _entryRepository.GetAll().Where(s => string.IsNullOrWhiteSpace(s.Name) == false).Select(s => s.Id).ToListAsync();
            foreach (var entryId in entryIds)
            {
                var entry = await _entryRepository.GetAll().AsNoTracking()
                 .Include(s => s.Outlinks)
                 .Include(s => s.EntryRelationFromEntryNavigation).ThenInclude(s => s.ToEntryNavigation).ThenInclude(s => s.Information).ThenInclude(s => s.Additional)
                 .Include(s => s.EntryRelationFromEntryNavigation).ThenInclude(s => s.ToEntryNavigation).ThenInclude(s => s.EntryRelationFromEntryNavigation).ThenInclude(s => s.ToEntryNavigation)
                 .Include(s => s.Articles).ThenInclude(s => s.CreateUser)
                 .Include(s => s.Information).ThenInclude(s => s.Additional).Include(s => s.Tags).Include(s => s.Pictures)
                 .FirstOrDefaultAsync(s => s.Id == entryId);
                //获取通过审核记录叠加的旧模型
                var examineModel = await GenerateModelFromExamines(entry);
                //应用审核记录
                await ExaminesCompletionEntry(entry, examineModel as Entry);

            }
            //补全文章
            var articleIds = await _articleRepository.GetAll().Where(s => string.IsNullOrWhiteSpace(s.Name) == false).Select(s => s.Id).ToListAsync();
            foreach (var articleId in articleIds)
            {
                var article = await _articleRepository.GetAll().AsNoTracking()
                 .Include(s => s.Outlinks)
                 .Include(s => s.ArticleRelationFromArticleNavigation).ThenInclude(s => s.ToArticleNavigation)
                 .Include(s => s.Entries)
                 .FirstOrDefaultAsync(s => s.Id == articleId);
                //获取通过审核记录叠加的旧模型
                var examineModel = await GenerateModelFromExamines(article);
                //应用审核记录
                await ExaminesCompletionArticle(article, examineModel as Article);

            }
            //补全标签
            var tagIds = await _tagRepository.GetAll().Where(s => string.IsNullOrWhiteSpace(s.Name) == false).Select(s => s.Id).ToListAsync();
            foreach (var tagId in tagIds)
            {
                var tag = await _tagRepository.GetAll().AsNoTracking()
                 .Include(s => s.Entries)
                 .Include(s => s.ParentCodeNavigation)
                 .Include(s => s.InverseParentCodeNavigation)
                 .FirstOrDefaultAsync(s => s.Id == tagId);
                //获取通过审核记录叠加的旧模型
                var examineModel = await GenerateModelFromExamines(tag);
                //应用审核记录
                await ExaminesCompletionTag(tag, examineModel as Tag);

            }

            //补全周边
            var peripheryIds = await _peripheryRepository.GetAll().Where(s => string.IsNullOrWhiteSpace(s.Name) == false).Select(s => s.Id).ToListAsync();
            foreach (var peripheryId in peripheryIds)
            {
                var periphery = await _peripheryRepository.GetAll().AsNoTracking()
                 .Include(s => s.RelatedEntries)
                 .Include(s => s.PeripheryRelationFromPeripheryNavigation).ThenInclude(s=>s.ToPeripheryNavigation)
                 .Include(s => s.Pictures)
                 .FirstOrDefaultAsync(s => s.Id == peripheryId);
                //获取通过审核记录叠加的旧模型
                var examineModel = await GenerateModelFromExamines(periphery);
                //应用审核记录
                await ExaminesCompletionTag(periphery, examineModel as Periphery);

            }
        }

        private async Task ExaminesCompletionEntry(Entry newEntry, Entry currentEntry)
        {
            var admin = await _userRepository.FirstOrDefaultAsync(s => s.Id == _configuration["ExamineAdminId"]);

            //将当前模型和新模型对比 获取差异审核 并补全
            var examines = _entryService.ExaminesCompletion(currentEntry, newEntry);
            if (examines.Count == 0)
            {
                return;
            }
            var entry = await _entryRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(s => s.Id == newEntry.Id);
            foreach (var item in examines)
            {
                var resulte = "";
                if (item.Value == Operation.EstablishMainPage)
                {
                    resulte = item.Key as string;
                }
                else
                {
                    using (TextWriter text = new StringWriter())
                    {
                        var serializer = new JsonSerializer();
                        serializer.Serialize(text, item.Key);
                        resulte = text.ToString();
                    }
                }

                await UniversalEditExaminedAsync(entry, admin, true, resulte, item.Value, "补全审核记录");
            }
        }

        private async Task ExaminesCompletionArticle(Article newArticle, Article currentArticle)
        {
            var admin = await _userRepository.FirstOrDefaultAsync(s => s.Id == _configuration["ExamineAdminId"]);

            //将当前模型和新模型对比 获取差异审核 并补全
            var examines = _articleService.ExaminesCompletion(currentArticle, newArticle);
            if (examines.Count == 0)
            {
                return;
            }
            var article = await _articleRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(s => s.Id == newArticle.Id);
            foreach (var item in examines)
            {
                var resulte = "";
                if (item.Value == Operation.EditArticleMainPage)
                {
                    resulte = item.Key as string;
                }
                else
                {
                    using (TextWriter text = new StringWriter())
                    {
                        var serializer = new JsonSerializer();
                        serializer.Serialize(text, item.Key);
                        resulte = text.ToString();
                    }
                }

                await UniversalEditArticleExaminedAsync(article, admin, true, resulte, item.Value, "补全审核记录");
            }
        }

        private async Task ExaminesCompletionTag(Tag newTag, Tag currentTag)
        {
            var admin = await _userRepository.FirstOrDefaultAsync(s => s.Id == _configuration["ExamineAdminId"]);

            //将当前模型和新模型对比 获取差异审核 并补全
            var examines = _tagService.ExaminesCompletion(currentTag, newTag);
            if (examines.Count == 0)
            {
                return;
            }
            var tag = await _tagRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(s => s.Id == newTag.Id);
            foreach (var item in examines)
            {
                var resulte = "";
                    using (TextWriter text = new StringWriter())
                    {
                        var serializer = new JsonSerializer();
                        serializer.Serialize(text, item.Key);
                        resulte = text.ToString();
                    }
                

                await UniversalEditTagExaminedAsync(tag, admin, true, resulte, item.Value, "补全审核记录");
            }
        }

        private async Task ExaminesCompletionTag(Periphery newPeriphery, Periphery currentPeriphery)
        {
            var admin = await _userRepository.FirstOrDefaultAsync(s => s.Id == _configuration["ExamineAdminId"]);

            //将当前模型和新模型对比 获取差异审核 并补全
            var examines = _peripheryService.ExaminesCompletion(currentPeriphery, newPeriphery);
            if (examines.Count == 0)
            {
                return;
            }
            var periphery = await _peripheryRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(s => s.Id == newPeriphery.Id);
            foreach (var item in examines)
            {
                var resulte = "";
                using (TextWriter text = new StringWriter())
                {
                    var serializer = new JsonSerializer();
                    serializer.Serialize(text, item.Key);
                    resulte = text.ToString();
                }


                await UniversalEditPeripheryExaminedAsync(periphery, admin, true, resulte, item.Value, "补全审核记录");
            }
        }


        public async Task<object> GenerateModelFromExamines(object model)
        {
            if (model is Entry)
            {
                var entry = model as Entry;
                var exammines = await _examineRepository.GetAll().AsNoTracking().Where(s => s.EntryId == entry.Id && s.IsPassed == true).OrderBy(s => s.Id).ToListAsync();
                if (exammines.Any())
                {
                    return await GenerateModelFromExamines(exammines);
                }
                else
                {
                    throw new Exception("未找到该词条");
                }

            }
            else if (model is Article)
            {
                var article = model as Article;
                var exammines = await _examineRepository.GetAll().AsNoTracking().Where(s => s.ArticleId == article.Id && s.IsPassed == true).OrderBy(s => s.Id).ToListAsync();
                if (exammines.Any())
                {
                    return await GenerateModelFromExamines(exammines);
                }
                else
                {
                    throw new Exception("未找到该文章");
                }

            }
            else if (model is Tag)
            {
                var tag = model as Tag;
                var exammines = await _examineRepository.GetAll().AsNoTracking().Where(s => s.TagId == tag.Id && s.IsPassed == true).OrderBy(s => s.Id).ToListAsync();
                if (exammines.Any())
                {
                    return await GenerateModelFromExamines(exammines);
                }
                else
                {
                    return new Tag
                    {
                        Id= tag.Id
                    };
                }

            }
            else if (model is Periphery)
            {
                var periphery = model as Periphery;
                var exammines = await _examineRepository.GetAll().AsNoTracking().Where(s => s.PeripheryId == periphery.Id && s.IsPassed == true).OrderBy(s => s.Id).ToListAsync();
                if (exammines.Any())
                {
                    return await GenerateModelFromExamines(exammines);
                }
                else
                {
                    throw new Exception("未找到该周边");
                }

            }
            else
            {
                throw new Exception("不支持的类型");
            }
        }

        public async Task<object> GenerateModelFromExamines(List<Examine> examines)
        {
            if (examines.FirstOrDefault().EntryId != null)
            {
                var entry = new Entry();
                foreach (var item in examines)
                {
                    await _entryService.UpdateEntryDataAsync(entry, item);
                }
                return entry;
            }
            else if (examines.FirstOrDefault().ArticleId != null)
            {
                var article = new Article();
                foreach (var item in examines)
                {
                    await _articleService.UpdateArticleData(article, item);
                }
                return article;
            }
            else if (examines.FirstOrDefault().TagId != null)
            {
                var tag = new Tag();
                foreach (var item in examines)
                {
                    await _tagService.UpdateTagDataAsync(tag, item);
                }
                return tag;
            }
            else if (examines.FirstOrDefault().PeripheryId != null)
            {
                var periphery = new Periphery();
                foreach (var item in examines)
                {
                    await _peripheryService.UpdatePeripheryDataAsync(periphery, item);
                }
                return periphery;
            }
            else
            {
                throw new Exception("不支持的类型");
            }
          
        }

        #endregion
    }
}
