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

        private static readonly ConcurrentDictionary<Type, Func<IEnumerable<Examine>, string, SortOrder, IEnumerable<Examine>>> SortLambdaCacheEntry = new();

        public ExamineService(IRepository<Examine, int> examineRepository, IAppHelper appHelper, IRepository<Entry, int> entryRepository, IRankService rankService, IPerfectionService perfectionService,
        IArticleService articleService, ITagService tagService, IDisambigService disambigService, IUserService userService, IRepository<ApplicationUser, string> userRepository,
        IRepository<Article, long> articleRepository, IRepository<DataModel.Model.Tag, int> tagRepository, IEntryService entryService, IPeripheryService peripheryService,
        IRepository<Comment, long> commentRepository, IRepository<Disambig, int> disambigRepository, IRepository<Periphery, long> peripheryRepository,
        IConfiguration configuration)
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

        public Task<QueryData<ListExamineAloneModel>> GetPaginatedResult(CnGalWebSite.DataModel.ViewModel.Search.QueryPageOptions options, ListExamineAloneModel searchModel)
        {
            IEnumerable<Examine> items = _examineRepository.GetAll().Include(s => s.ApplicationUser).AsNoTracking();
            // 处理高级搜索
            if (!string.IsNullOrWhiteSpace(searchModel.EntryId?.ToString()))
            {
                items = items.Where(item => item.EntryId.ToString()?.Contains(searchModel.EntryId.ToString(), StringComparison.OrdinalIgnoreCase) ?? false);
            }

            if (!string.IsNullOrWhiteSpace(searchModel.ArticleId?.ToString()))
            {
                items = items.Where(item => item.ArticleId.ToString()?.Contains(searchModel.ArticleId.ToString(), StringComparison.OrdinalIgnoreCase) ?? false);
            }

            if (!string.IsNullOrWhiteSpace(searchModel.TagId?.ToString()))
            {
                items = items.Where(item => item.TagId.ToString()?.Contains(searchModel.TagId.ToString(), StringComparison.OrdinalIgnoreCase) ?? false);
            }

            if (!string.IsNullOrWhiteSpace(searchModel.ApplicationUserId?.ToString()))
            {
                items = items.Where(item => item.ApplicationUserId.ToString()?.Contains(searchModel.ApplicationUserId.ToString(), StringComparison.OrdinalIgnoreCase) ?? false);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.Comments))
            {
                items = items.Where(item => item.Comments?.Contains(searchModel.Comments, StringComparison.OrdinalIgnoreCase) ?? false);
            }
            if (searchModel.Operation != null)
            {
                items = items.Where(item => item.Operation == searchModel.Operation);
            }



            // 处理 SearchText 模糊搜索
            if (!string.IsNullOrWhiteSpace(options.SearchText))
            {
                items = items.Where(item => (item.EntryId.ToString()?.Contains(options.SearchText) ?? false)
                             || (item.EntryId.ToString()?.Contains(options.SearchText) ?? false)
                             || (item.ArticleId.ToString()?.Contains(options.SearchText) ?? false)
                             || (item.TagId.ToString()?.Contains(options.SearchText) ?? false)
                             || (item.ApplicationUserId.ToString()?.Contains(options.SearchText) ?? false));
            }

            // 排序
            var isSorted = false;
            if (!string.IsNullOrWhiteSpace(options.SortName))
            {
                // 外部未进行排序，内部自动进行排序处理
                var invoker = SortLambdaCacheEntry.GetOrAdd(typeof(Examine), key => LambdaExtensions.GetSortLambda<Examine>().Compile());
                items = invoker(items, options.SortName, (BootstrapBlazor.Components.SortOrder)options.SortOrder);
                isSorted = true;
            }

            // 设置记录总数
            var total = items.Count();

            // 内存分页
            items = items.Skip((options.PageIndex - 1) * options.PageItems).Take(options.PageItems).ToList();

            //复制数据
            var resultItems = new List<ListExamineAloneModel>();
            foreach (var item in items)
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

            return Task.FromResult(new QueryData<ListExamineAloneModel>()
            {
                Items = resultItems,
                TotalCount = total,
                IsSorted = isSorted,
                // IsFiltered = isFiltered
            });
        }



        public async Task<bool> GetExamineView(Models.ExaminedViewModel model, Examine examine)
        {
            return model.Operation switch
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

        private EntryMain InitExamineViewEntryMain(Entry entry)
        {
            var entryMainBefore = new EntryMain
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

            //判断是否是等待审核状态
            if (examine.IsPassed != null)
            {
                var htmlDiff = new HtmlDiff.HtmlDiff(entryMain.Name ?? "", entryMainBefore.Name ?? "");
                model.EditOverview = "<h5>唯一名称</h5>" + "<h5>" + htmlDiff.Build().Replace("\r\n", "<br>") + "</h5>";

                htmlDiff = new HtmlDiff.HtmlDiff(entryMain.DisplayName ?? "", entryMainBefore.DisplayName ?? "");
                model.EditOverview += "<h5>显示名称</h5>" + "<h5>" + htmlDiff.Build().Replace("\r\n", "<br>") + "</h5>";

                htmlDiff = new HtmlDiff.HtmlDiff(entryMain.AnotherName ?? "", entryMainBefore.AnotherName ?? "");
                model.EditOverview += "<h5>别称</h5>" + "<h5>" + htmlDiff.Build().Replace("\r\n", "<br>") + "</h5>";


                htmlDiff = new HtmlDiff.HtmlDiff(entryMain.BriefIntroduction ?? "", entryMainBefore.BriefIntroduction ?? "");
                model.EditOverview += "<h5>简介</h5>" + "<h5>" + htmlDiff.Build().Replace("\r\n", "<br>") + "</h5>";

                htmlDiff = new HtmlDiff.HtmlDiff(entryMain.Type.GetDisplayName() ?? "", entryMainBefore.Type.GetDisplayName() ?? "");
                model.EditOverview += "<h5>类型</h5>" + "<h5>" + htmlDiff.Build().Replace("\r\n", "<br>") + "</h5>";
                model.BeforeModel = entryMain;
                model.AfterModel = entryMainBefore;
            }
            else
            {
                var htmlDiff = new HtmlDiff.HtmlDiff(entryMainBefore.Name ?? "", entryMain.Name ?? "");
                model.EditOverview = "<h5>唯一名称</h5>" + "<h5>" + htmlDiff.Build().Replace("\r\n", "<br>") + "</h5>";

                htmlDiff = new HtmlDiff.HtmlDiff(entryMainBefore.DisplayName ?? "", entryMain.DisplayName ?? "");
                model.EditOverview += "<h5>显示名称</h5>" + "<h5>" + htmlDiff.Build().Replace("\r\n", "<br>") + "</h5>";

                htmlDiff = new HtmlDiff.HtmlDiff(entryMainBefore.AnotherName ?? "", entryMain.AnotherName ?? "");
                model.EditOverview += "<h5>别称</h5>" + "<h5>" + htmlDiff.Build().Replace("\r\n", "<br>") + "</h5>";


                htmlDiff = new HtmlDiff.HtmlDiff(entryMainBefore.BriefIntroduction ?? "", entryMain.BriefIntroduction ?? "");
                model.EditOverview += "<h5>简介</h5>" + "<h5>" + htmlDiff.Build().Replace("\r\n", "<br>") + "</h5>";

                htmlDiff = new HtmlDiff.HtmlDiff(entryMainBefore.Type.GetDisplayName() ?? "", entryMain.Type.GetDisplayName() ?? "");
                model.EditOverview += "<h5>类型</h5>" + "<h5>" + htmlDiff.Build().Replace("\r\n", "<br>") + "</h5>";

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
            //判断是否是等待审核状态
            if (examine.IsPassed != null)
            {
                //序列化数据
                var htmlDiff = new HtmlDiff.HtmlDiff(examine.Context ?? "", entry.MainPage ?? "");
                model.EditOverview = htmlDiff.Build().Replace("\r\n", "<br>");
                model.BeforeText = _appHelper.MarkdownToHtml(examine.Context);
                model.AfterText = _appHelper.MarkdownToHtml(entry.MainPage);
            }
            else
            {
                //序列化数据
                var htmlDiff = new HtmlDiff.HtmlDiff(entry.MainPage ?? "", examine.Context ?? "");
                model.EditOverview = htmlDiff.Build().Replace("\r\n", "<br>");
                model.BeforeText = _appHelper.MarkdownToHtml(entry.MainPage);
                model.AfterText = _appHelper.MarkdownToHtml(examine.Context);
            }
            return true;
        }

        #endregion

        #region 文章
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
            //序列化数据
            ArticleMain articleMain = null;
            using (TextReader str = new StringReader(examine.Context))
            {
                var serializer = new JsonSerializer();
                articleMain = (ArticleMain)serializer.Deserialize(str, typeof(ArticleMain));
            }

            var articleMainBefore = new ArticleMain
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
                DisplayName = article.DisplayName
            };
            articleMain.MainPicture = _appHelper.GetImagePath(articleMain.MainPicture, "app.png");
            articleMain.BackgroundPicture = _appHelper.GetImagePath(articleMain.BackgroundPicture, "app.png");
            articleMain.SmallBackgroundPicture = _appHelper.GetImagePath(articleMain.SmallBackgroundPicture, "app.png");
            //判断是否是等待审核状态
            if (examine.IsPassed != null)
            {
                var htmlDiff = new HtmlDiff.HtmlDiff(articleMain.Name ?? "", article.Name ?? "");
                model.EditOverview = "<h5>名称</h5>" + htmlDiff.Build().Replace("\r\n", "<br>");
                htmlDiff = new HtmlDiff.HtmlDiff(articleMain.BriefIntroduction ?? "", article.BriefIntroduction ?? "");
                model.EditOverview += "<h5>简介</h5>" + htmlDiff.Build().Replace("\r\n", "<br>");
                htmlDiff = new HtmlDiff.HtmlDiff(articleMain.Type.ToString() ?? "", article.Type.ToString() ?? "");
                model.EditOverview += "<h5>类型</h5>" + htmlDiff.Build().Replace("\r\n", "<br>");
                htmlDiff = new HtmlDiff.HtmlDiff(articleMain.PubishTime.ToString("D") ?? "", article.PubishTime.ToString("D") ?? "");
                model.EditOverview += "<h5>发布日期</h5>" + htmlDiff.Build().Replace("\r\n", "<br>");
                htmlDiff = new HtmlDiff.HtmlDiff(articleMain.OriginalAuthor ?? "", article.OriginalAuthor ?? "");
                model.EditOverview += "<h5>原作者</h5>" + htmlDiff.Build().Replace("\r\n", "<br>");
                htmlDiff = new HtmlDiff.HtmlDiff(articleMain.OriginalLink ?? "", article.OriginalLink ?? "");
                model.EditOverview += "<h5>原文链接</h5>" + htmlDiff.Build().Replace("\r\n", "<br>");

                model.BeforeModel = articleMain;
                model.AfterModel = articleMainBefore;
            }
            else
            {
                var htmlDiff = new HtmlDiff.HtmlDiff(article.Name ?? "", articleMain.Name ?? "");
                model.EditOverview = "<h5>名称</h5>" + htmlDiff.Build().Replace("\r\n", "<br>");
                htmlDiff = new HtmlDiff.HtmlDiff(article.BriefIntroduction ?? "", articleMain.BriefIntroduction ?? "");
                model.EditOverview += "<h5>简介</h5>" + htmlDiff.Build().Replace("\r\n", "<br>");
                htmlDiff = new HtmlDiff.HtmlDiff(article.Type.ToString() ?? "", articleMain.Type.ToString() ?? "");
                model.EditOverview += "<h5>类型</h5>" + htmlDiff.Build().Replace("\r\n", "<br>");
                htmlDiff = new HtmlDiff.HtmlDiff(article.PubishTime.ToString("D") ?? "", articleMain.PubishTime.ToString("D") ?? "");
                model.EditOverview += "<h5>发布日期</h5>" + htmlDiff.Build().Replace("\r\n", "<br>");
                htmlDiff = new HtmlDiff.HtmlDiff(article.OriginalAuthor ?? "", articleMain.OriginalAuthor ?? "");
                model.EditOverview += "<h5>原作者</h5>" + htmlDiff.Build().Replace("\r\n", "<br>");
                htmlDiff = new HtmlDiff.HtmlDiff(article.OriginalLink ?? "", articleMain.OriginalLink ?? "");
                model.EditOverview += "<h5>原文链接</h5>" + htmlDiff.Build().Replace("\r\n", "<br>");



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

        private TagMain InitExamineViewTagMain(Tag tag)
        {
            var model = new TagMain
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

            //判断是否是等待审核状态
            if (examine.IsPassed != null)
            {
                var htmlDiff = new HtmlDiff.HtmlDiff(entryMain.Name ?? "", entryMainBefore.Name ?? "");
                model.EditOverview = "<h5>唯一名称</h5>" + "<h5>" + htmlDiff.Build().Replace("\r\n", "<br>") + "</h5>";

                htmlDiff = new HtmlDiff.HtmlDiff(entryMain.BriefIntroduction ?? "", entryMainBefore.BriefIntroduction ?? "");
                model.EditOverview += "<h5>简介</h5>" + "<h5>" + htmlDiff.Build().Replace("\r\n", "<br>") + "</h5>";


                model.BeforeModel = entryMain;
                model.AfterModel = entryMainBefore;
            }
            else
            {
                var htmlDiff = new HtmlDiff.HtmlDiff(entryMainBefore.Name ?? "", entryMain.Name ?? "");
                model.EditOverview = "<h5>唯一名称</h5>" + "<h5>" + htmlDiff.Build().Replace("\r\n", "<br>") + "</h5>";

                htmlDiff = new HtmlDiff.HtmlDiff(entryMainBefore.BriefIntroduction ?? "", entryMain.BriefIntroduction ?? "");
                model.EditOverview += "<h5>简介</h5>" + "<h5>" + htmlDiff.Build().Replace("\r\n", "<br>") + "</h5>";


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

        private PeripheryMain InitExamineViewPeripheryMain(Periphery periphery)
        {
            var model = new PeripheryMain
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
            _peripheryService.UpdatePeripheryData(periphery, examine);

            //序列化数据
            var entryMain = InitExamineViewPeripheryMain(periphery);

            //判断是否是等待审核状态
            if (examine.IsPassed != null)
            {
                var htmlDiff = new HtmlDiff.HtmlDiff(entryMain.Name ?? "", entryMainBefore.Name ?? "");
                model.EditOverview = "<h5>唯一名称</h5>" + "<h5>" + htmlDiff.Build().Replace("\r\n", "<br>") + "</h5>";

                htmlDiff = new HtmlDiff.HtmlDiff(entryMain.DisplayName ?? "", entryMainBefore.DisplayName ?? "");
                model.EditOverview += "<h5>显示名称</h5>" + "<h5>" + htmlDiff.Build().Replace("\r\n", "<br>") + "</h5>";

                htmlDiff = new HtmlDiff.HtmlDiff(entryMain.BriefIntroduction ?? "", entryMainBefore.BriefIntroduction ?? "");
                model.EditOverview += "<h5>简介</h5>" + "<h5>" + htmlDiff.Build().Replace("\r\n", "<br>") + "</h5>";

                htmlDiff = new HtmlDiff.HtmlDiff(entryMain.Material ?? "", entryMainBefore.Material ?? "");
                model.EditOverview += "<h5>材质</h5>" + "<h5>" + htmlDiff.Build().Replace("\r\n", "<br>") + "</h5>";


                model.BeforeModel = entryMain;
                model.AfterModel = entryMainBefore;
            }
            else
            {
                var htmlDiff = new HtmlDiff.HtmlDiff(entryMainBefore.Name ?? "", entryMain.Name ?? "");
                model.EditOverview = "<h5>唯一名称</h5>" + "<h5>" + htmlDiff.Build().Replace("\r\n", "<br>") + "</h5>";

                htmlDiff = new HtmlDiff.HtmlDiff(entryMainBefore.DisplayName ?? "", entryMain.DisplayName ?? "");
                model.EditOverview += "<h5>显示名称</h5>" + "<h5>" + htmlDiff.Build().Replace("\r\n", "<br>") + "</h5>";

                htmlDiff = new HtmlDiff.HtmlDiff(entryMainBefore.BriefIntroduction ?? "", entryMain.BriefIntroduction ?? "");
                model.EditOverview += "<h5>简介</h5>" + "<h5>" + htmlDiff.Build().Replace("\r\n", "<br>") + "</h5>";

                htmlDiff = new HtmlDiff.HtmlDiff(entryMainBefore.Material ?? "", entryMain.Material ?? "");
                model.EditOverview += "<h5>材质</h5>" + "<h5>" + htmlDiff.Build().Replace("\r\n", "<br>") + "</h5>";


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
            _peripheryService.UpdatePeripheryData(periphery, examine);

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

        private async Task<List<RelevancesViewModel>> InitExamineViewPeripheryRelatedEntries(Periphery periphery)
        {
            var relevances = new List<RelevancesViewModel>();
            var ids = periphery.Entries.Select(s => s.EntryId).ToList();
            var entries = await _entryRepository.GetAll().Where(s => ids.Contains(s.Id))
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.BriefIntroduction
                })
                .ToListAsync();
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
            var periphery = await _peripheryRepository.GetAll().Include(s => s.Entries).FirstOrDefaultAsync(s => s.Id == examine.PeripheryId);
            if (periphery == null)
            {
                return false;
            }
            model.EntryId = (int)examine.PeripheryId;
            model.EntryName = periphery.Name;

            //序列化相关性列表
            //先读取词条信息
            var relevances = await InitExamineViewPeripheryRelatedEntries(periphery);

            //添加修改记录 
            _peripheryService.UpdatePeripheryData(periphery, examine);

            var relevances_examine = await InitExamineViewPeripheryRelatedEntries(periphery);

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

        private static List<RelevancesViewModel> InitExamineViewPeripheryRelatedPeripheries(Periphery periphery)
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
            _peripheryService.UpdatePeripheryData(periphery, examine);

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
        public async Task ExamineEstablishMainAsync(Entry entry, EntryMain examine)
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
            var admin = await _userRepository.FirstOrDefaultAsync(s => s.Id == _configuration["ExamineAdminId"]);

            //反向关联
            foreach (var item in examine.Information)
            {
                if (item.IsDelete == false)
                {
                    if (entry.Type == EntryType.Game)
                    {
                        if (item.Modifier == "STAFF")
                        {
                            var temp = await _entryRepository.GetAll().Include(s => s.EntryRelationFromEntryNavigation).FirstOrDefaultAsync(s => s.Name == item.DisplayValue);

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
                            var temp = await _entryRepository.GetAll().Include(s => s.EntryRelationFromEntryNavigation).FirstOrDefaultAsync(s => s.Name == item.DisplayValue);

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
            //保存
            await _entryRepository.UpdateAsync(entry);

            //更新完善度
            await _perfectionService.UpdateEntryPerfectionResultAsync(entry.Id);

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

            var admin = await _userRepository.FirstOrDefaultAsync(s => s.Id == _configuration["ExamineAdminId"]);
            //反向关联 词条
            foreach (var item in examine.Relevances.Where(s => s.IsDelete == false && s.Type == RelevancesType.Entry))
            {
                //查找关联词条
                var temp = await _entryRepository.GetAll().Include(s => s.EntryRelationFromEntryNavigation).FirstOrDefaultAsync(s => s.Id.ToString() == item.DisplayName);
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

            //更新完善度
            await _perfectionService.UpdateEntryPerfectionResultAsync(entry.Id);

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
        public async Task ExamineEditArticleMainAsync(Article article, ArticleMain examine)
        {
            _articleService.UpdateArticleDataMain(article, examine);

            await _articleRepository.UpdateAsync(article);
        }

        public async Task ExamineEditArticleRelevancesAsync(Article article, ArticleRelevances examine)
        {
            await _articleService.UpdateArticleDataRelevances(article, examine);
            await _articleRepository.UpdateAsync(article);


            var admin = await _userRepository.FirstOrDefaultAsync(s => s.Id == _configuration["ExamineAdminId"]);
            //反向关联 词条
            foreach (var item in examine.Relevances.Where(s => s.IsDelete == false && s.Type == RelevancesType.Article))
            {
                //查找关联词条
                var temp = await _articleRepository.GetAll().Include(s => s.ArticleRelationFromArticleNavigation).FirstOrDefaultAsync(s => s.Id.ToString() == item.DisplayName);
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
            //反向关联 文章
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

            _tagRepository.Clear();
        }

        public async Task ExamineEditTagMainAsync(Tag tag, TagMain examine)
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

        public async Task ExamineEditPeripheryMainAsync(Periphery periphery, PeripheryMain examine)
        {
            //更新数据
            _peripheryService.UpdatePeripheryDataMain(periphery, examine);
            //保存
            await _peripheryRepository.UpdateAsync(periphery);
            _peripheryRepository.Clear();
        }

        public async Task ExamineEditPeripheryImagesAsync(Periphery periphery, PeripheryImages examine)
        {
            //更新数据
            _peripheryService.UpdatePeripheryDataImages(periphery, examine);
            //保存
            await _peripheryRepository.UpdateAsync(periphery);
            _peripheryRepository.Clear();
        }

        public async Task ExamineEditPeripheryRelatedEntriesAsync(Periphery periphery, PeripheryRelatedEntries examine)
        {
            //更新数据
            _peripheryService.UpdatePeripheryDataRelatedEntries(periphery, examine);
            _peripheryRepository.Clear();
            _entryRepository.Clear();
            //保存
            await _peripheryService.RealUpdateRelevances(periphery);

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

        #region 批量导入词条文章时 创建审核记录
        public async Task AddBatchEtryExaminesAsync(Entry model, ApplicationUser user, string note)
        {
            //第一步 建立词条主要信息

            //添加修改记录
            //新建审核数据对象
            var entryMain = new EntryMain
            {
                Name = model.Name,
                BriefIntroduction = model.BriefIntroduction,
                MainPicture = model.MainPicture,
                Thumbnail = model.Thumbnail,
                BackgroundPicture = model.BackgroundPicture,
                Type = model.Type,
                DisplayName = model.DisplayName,
                SmallBackgroundPicture = model.SmallBackgroundPicture,
                AnotherName = model.AnotherName
            };
            //序列化
            var resulte = "";
            using (TextWriter text = new StringWriter())
            {
                var serializer = new JsonSerializer();
                serializer.Serialize(text, entryMain);
                resulte = text.ToString();
            }
            //将空词条添加到数据库中 目的是为了获取索引
            var entry = new Entry
            {
                Type = model.Type,
                LastEditTime = DateTime.Now.ToCstTime()
            };
            entry = await _entryRepository.InsertAsync(entry);
            //初始化列表


            await ExamineEstablishMainAsync(entry, entryMain);
            await UniversalEstablishExaminedAsync(entry, user, true, resulte, Operation.EstablishMain, note);


            //第二步 建立词条附加信息
            //根据类别进行序列化操作
            var basics = model.Information?.ToList();

            //新建审核数据对象
            var entryAddInfor = new EntryAddInfor
            {
                Information = new List<BasicEntryInformation_>()
            };
            //添加修改记录
            if (basics != null)
            {
                foreach (var item in basics)
                {
                    var addInfors = new List<BasicEntryInformationAdditional_>();
                    if (item.Additional != null)
                    {
                        addInfors = new List<BasicEntryInformationAdditional_>();
                        foreach (var item_1 in item.Additional)
                        {
                            addInfors.Add(new BasicEntryInformationAdditional_
                            {
                                DisplayName = item_1.DisplayName,
                                DisplayValue = item_1.DisplayValue,
                                IsDelete = false
                            });
                        }

                    }
                    entryAddInfor.Information.Add(new BasicEntryInformation_
                    {
                        DisplayName = item.DisplayName,
                        DisplayValue = item.DisplayValue,
                        IsDelete = false,
                        Additional = addInfors,
                        Modifier = item.Modifier
                    });
                }
            }
            //判断审核是否为空
            if (entryAddInfor.Information.Count != 0)
            { //序列化
                resulte = "";
                using (TextWriter text = new StringWriter())
                {
                    var serializer = new JsonSerializer();
                    serializer.Serialize(text, entryAddInfor);
                    resulte = text.ToString();
                }
                await ExamineEstablishAddInforAsync(entry, entryAddInfor);
                await UniversalEstablishExaminedAsync(entry, user, true, resulte, Operation.EstablishAddInfor, note);

            }

            //第三步 建立词条图片

            //创建审核数据模型
            var pictures = model.Pictures?.ToList();
            var entryImages = new EntryImages
            {
                Images = new List<EntryImage>()
            };
            if (pictures != null)
            {
                foreach (var item in pictures)
                {
                    if (item.Url != "app.png")
                    {
                        entryImages.Images.Add(new EntryImage
                        {
                            Url = item.Url,
                            Note = item.Note,
                            Modifier = item.Modifier,
                            IsDelete = false
                        });
                    }
                }
            }

            //判断审核是否为空
            if (pictures != null && pictures.Count != 0)
            {
                //序列化JSON
                resulte = "";
                using (TextWriter text = new StringWriter())
                {
                    var serializer = new JsonSerializer();
                    serializer.Serialize(text, entryImages);
                    resulte = text.ToString();
                }
                await ExamineEstablishImagesAsync(entry, entryImages);
                await UniversalEstablishExaminedAsync(entry, user, true, resulte, Operation.EstablishImages, note);
            }


            //第四步 建立词条关联信息

            //转化为标准词条相关性列表格式
            var entryRelevances = model.Relevances?.ToList();
            //判断审核是否为空
            if (entryRelevances != null && entryRelevances.Count != 0)
            {
                //创建审核数据模型
                var examinedModel = new EntryRelevances();
                var entryTyps = new List<string>
                {
                    "词条",
                    "游戏",
                    "制作组",
                    "STAFF"
                };
                var articleTyps = new List<string>
                {
                    "文章",
                    "动态",
                };
                foreach (var item in entryRelevances)
                {
                    examinedModel.Relevances.Add(new EntryRelevancesAloneModel
                    {
                        IsDelete = false,
                        DisplayName = item.DisplayName,
                        Type = (entryTyps.Contains(item.Modifier) ? RelevancesType.Entry : (articleTyps.Contains(item.Modifier) ? RelevancesType.Article : RelevancesType.Outlink)),
                        DisplayValue = item.DisplayValue,
                        Link = item.Link
                    });
                }
                //序列化JSON
                resulte = "";
                using (TextWriter text = new StringWriter())
                {
                    var serializer = new JsonSerializer();
                    serializer.Serialize(text, examinedModel);
                    resulte = text.ToString();
                }
                await ExamineEstablishRelevancesAsync(entry, examinedModel);
                await UniversalEstablishExaminedAsync(entry, user, true, resulte, Operation.EstablishRelevances, note);
            }

            //第五步 建立词条主页
            //判断是否为空
            if (string.IsNullOrWhiteSpace(model.MainPage) == false)
            {
                await ExamineEstablishMainPageAsync(entry, model.MainPage);
                await UniversalEstablishExaminedAsync(entry, user, true, model.MainPage, Operation.EstablishMainPage, note);
            }

            //第六步 建立词条标签
            //创建审核数据模型

            var entryTags = new EntryTags();

            //添加新建项目
            foreach (var item in model.Tags.Select(s => s.Id))
            {
                entryTags.Tags.Add(new EntryTagsAloneModel
                {
                    TagId = item,
                    IsDelete = false
                });

            }

            //判断审核是否为空
            if (entryTags.Tags.Count != 0)
            {
                //序列化JSON
                resulte = "";
                using (TextWriter text = new StringWriter())
                {
                    var serializer = new JsonSerializer();
                    serializer.Serialize(text, entryTags);
                    resulte = text.ToString();
                }
                await ExamineEstablishTagsAsync(entry, entryTags);
                await UniversalEstablishExaminedAsync(entry, user, true, resulte, Operation.EstablishTags, note);
            }
        }

        public async Task AddBatchArticleExaminesAsync(Article model, ApplicationUser user, string note)
        {
            //第一步 处理主要信息

            //新建审核数据对象
            var entryMain = new ArticleMain
            {
                Name = model.Name,
                BriefIntroduction = model.BriefIntroduction,
                MainPicture = model.MainPicture,
                BackgroundPicture = model.BackgroundPicture,
                SmallBackgroundPicture = model.SmallBackgroundPicture,
                Type = model.Type,
                OriginalAuthor = model.OriginalAuthor,
                OriginalLink = model.OriginalLink,
                PubishTime = model.PubishTime,
                RealNewsTime = model.RealNewsTime,
                DisplayName = model.DisplayName,
                NewsType = model.NewsType,
            };
            //序列化
            var resulte = "";
            using (TextWriter text = new StringWriter())
            {
                var serializer = new JsonSerializer();
                serializer.Serialize(text, entryMain);
                resulte = text.ToString();
            }
            //将空文章添加到数据库中 目的是为了获取索引
            var article = new Article
            {
                Type = model.Type,
                CreateUser = user,
                CreateTime = model.CreateTime,
                LastEditTime = DateTime.Now.ToCstTime()
            };
            article = await _articleRepository.InsertAsync(article);
            await ExamineEditArticleMainAsync(article, entryMain);
            await UniversalCreateArticleExaminedAsync(article, user, true, resulte, Operation.EditArticleMain, note);

            //第二步 处理关联词条

            //转化为标准词条相关性列表格式
            var articleRelevance = model.Relevances?.ToList();
            //判断审核是否为空
            if (articleRelevance != null && articleRelevance.Count != 0)
            {
                var entryTyps = new List<string>
                {
                    "词条",
                    "游戏",
                    "制作组",
                    "STAFF"
                };
                var articleTyps = new List<string>
                {
                    "文章",
                    "动态",
                };
                //创建审核数据模型
                var examinedModel = new ArticleRelevances();
                foreach (var item in articleRelevance)
                {
                    examinedModel.Relevances.Add(new ArticleRelevancesAloneModel
                    {
                        IsDelete = false,
                        DisplayName = item.DisplayName,
                        Type = (entryTyps.Contains(item.Modifier) ? RelevancesType.Entry : (articleTyps.Contains(item.Modifier) ? RelevancesType.Article : RelevancesType.Outlink)),
                        DisplayValue = item.DisplayValue,
                        Link = item.Link
                    });
                }
                //序列化JSON
                resulte = "";
                using (TextWriter text = new StringWriter())
                {
                    var serializer = new JsonSerializer();
                    serializer.Serialize(text, examinedModel);
                    resulte = text.ToString();
                }
                article = await _articleRepository.GetAll().Include(s => s.Relevances).FirstOrDefaultAsync(s => s.Id == article.Id);
                await ExamineEditArticleRelevancesAsync(article, examinedModel);
                await UniversalCreateArticleExaminedAsync(article, user, true, resulte, Operation.EditArticleRelevanes, note);
            }

            //第三步 添加正文

            //判断是否为空
            if (string.IsNullOrWhiteSpace(model.MainPage) == false)
            {
                await ExamineEditArticleMainPageAsync(article, model.MainPage);
                await UniversalCreateArticleExaminedAsync(article, user, true, model.MainPage, Operation.EditArticleMainPage, note);
            }
        }

        public async Task<string> AddBatchPeeripheryExaminesAsync(ImportPeripheryModel model, ApplicationUser user, string note)
        {
            //检查周边是否重名
            if (await _peripheryRepository.GetAll().AnyAsync(s => s.Name == model.Name))
            {
                return "该周边的名称与其他周边重复";
            }
            //预处理 建立词条关联信息
            //判断关联是否存在
            var entryId = new List<int>();

            var entryNames = new List<string>();
            entryNames.AddRange(model.Entries.Where(s => string.IsNullOrWhiteSpace(s) == false));
            //确保至少有一个关联词条
            if (entryNames.Any() == false)
            {
                return "至少需要关联一个词条";
            }

            foreach (var item in entryNames)
            {
                var infor = await _entryRepository.GetAll().Where(s => s.Name == item).Select(s => s.Id).FirstOrDefaultAsync();
                if (infor <= 0)
                {
                    return "词条 " + item + " 不存在";
                }
                else
                {
                    entryId.Add(infor);
                }
            }
            //删除重复数据
            entryId = entryId.Distinct().ToList();

            //预处理 建立周边关联信息
            //判断关联是否存在
            var peripheryIds = new List<long>();

            var peripheryNames = new List<string>();
            peripheryNames.AddRange(model.Peripheries.Where(s => string.IsNullOrWhiteSpace(s) == false));


            foreach (var item in peripheryNames)
            {
                var infor = await _peripheryRepository.GetAll().Where(s => s.Name == item).Select(s => s.Id).FirstOrDefaultAsync();
                if (infor <= 0)
                {
                    return "周边 " + item + " 不存在";
                }
                else
                {
                    peripheryIds.Add(infor);
                }
            }
            //删除重复数据
            peripheryIds = peripheryIds.Distinct().ToList();

            //第一步 处理主要信息

            //新建审核数据对象
            var peripheryMain = new PeripheryMain
            {
                Name = model.Name,
                BriefIntroduction = model.BriefIntroduction,
                MainPicture = model.MainPicture,
                BackgroundPicture = model.BackgroundPicture,
                SmallBackgroundPicture = model.SmallBackgroundPicture,
                Thumbnail = model.Thumbnail,
                Author = model.Author,
                Material = model.Material,
                Brand = model.Brand,
                IndividualParts = model.IndividualParts,
                IsAvailableItem = model.IsAvailableItem,
                PageCount = model.PageCount,
                Price = model.Price,
                IsReprint = model.IsReprint,
                Size = model.Size,
                SongCount = model.SongCount,
                Type = model.Type,
                Category = model.Category,
                SaleLink = model.SaleLink,
                DisplayName = model.DisplayName,
            };
            //序列化
            var resulte = "";
            using (TextWriter text = new StringWriter())
            {
                var serializer = new JsonSerializer();
                serializer.Serialize(text, peripheryMain);
                resulte = text.ToString();
            }
            //将空文章添加到数据库中 目的是为了获取索引
            var periphery = new Periphery();

            periphery = await _peripheryRepository.InsertAsync(periphery);

            await ExamineEditPeripheryMainAsync(periphery, peripheryMain);
            await UniversalCreatePeripheryExaminedAsync(periphery, user, true, resulte, Operation.EditPeripheryMain, note);


            //第二步 建立词条图片

            //创建审核数据模型
            periphery.Pictures = new List<EntryPicture>();

            var peripheryImages = new PeripheryImages
            {
                Images = new List<EntryImage>()
            };
            if (model.Pictures != null)
            {
                foreach (var item in model.Pictures)
                {
                    if (item != "app.png")
                    {
                        //复制到审核的列表中
                        peripheryImages.Images.Add(new EntryImage
                        {
                            Url = item,
                            IsDelete = false
                        });
                    }
                }
            }

            //判断审核是否为空
            if (peripheryImages.Images.Count != 0)
            {
                //序列化JSON
                resulte = "";
                using (TextWriter text = new StringWriter())
                {
                    var serializer = new JsonSerializer();
                    serializer.Serialize(text, peripheryImages);
                    resulte = text.ToString();
                }

                await ExamineEditPeripheryImagesAsync(periphery, peripheryImages);
                await UniversalEditPeripheryExaminedAsync(periphery, user, true, resulte, Operation.EditPeripheryImages, note);
            }

            //第五步 建立周边关联周边信息
            //判断审核是否为空
            if (model.Peripheries.Count != 0)
            {   //创建审核数据模型
                var peripheryRelatedPeripheries = new PeripheryRelatedPeripheries();

                foreach (var item in peripheryIds)
                {
                    peripheryRelatedPeripheries.Relevances.Add(new PeripheryRelatedPeripheriesAloneModel
                    {
                        IsDelete = false,
                        PeripheryId = item

                    });
                }
                //序列化JSON
                resulte = "";
                using (TextWriter text = new StringWriter())
                {
                    var serializer = new JsonSerializer();
                    serializer.Serialize(text, peripheryRelatedPeripheries);
                    resulte = text.ToString();
                }
                await ExamineEditPeripheryRelatedPeripheriesAsync(periphery, peripheryRelatedPeripheries);
                await UniversalEditPeripheryExaminedAsync(periphery, user, true, resulte, Operation.EditPeripheryRelatedPeripheries, note);

            }

            //第四步 建立周边关联词条信息
            //判断审核是否为空
            if (entryId.Count != 0)
            {
                //创建审核数据模型
                var examinedModel = new PeripheryRelatedEntries();

                foreach (var item in entryId)
                {
                    examinedModel.Relevances.Add(new PeripheryRelatedEntryAloneModel
                    {
                        IsDelete = false,
                        EntryId = item

                    });
                }
                periphery.Entries = new List<PeripheryRelevanceEntry>();
                //序列化JSON
                resulte = "";
                using (TextWriter text = new StringWriter())
                {
                    var serializer = new JsonSerializer();
                    serializer.Serialize(text, examinedModel);
                    resulte = text.ToString();
                }
                //判断是否是管理员
                await ExamineEditPeripheryRelatedEntriesAsync(periphery, examinedModel);
                await UniversalEditPeripheryExaminedAsync(periphery, user, true, resulte, Operation.EditPeripheryRelatedEntries, note);
            }



            return null;
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
            // await ReplaceArticleRelevances();
            await ReplaceEditArticleRelevancesExamineContext();
        }
        public async Task MigrationEditEntryRelevanceExamineRecord()
        {
            await ReplaceEntryRelevances();
            await ReplaceEditEntryRelevancesExamineContext();
        }

        /// <summary>
        /// 迁移词条审核数据 关联部分
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
        /// 迁移词条关联数据
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
    }
}
