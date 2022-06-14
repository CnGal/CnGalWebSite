using BootstrapBlazor.Components;
using CnGalWebSite.APIServer.Application.Articles;
using CnGalWebSite.APIServer.Application.Comments;
using CnGalWebSite.APIServer.Application.Disambigs;
using CnGalWebSite.APIServer.Application.Entries;
using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.Application.Perfections;
using CnGalWebSite.APIServer.Application.Peripheries;
using CnGalWebSite.APIServer.Application.PlayedGames;
using CnGalWebSite.APIServer.Application.Ranks;
using CnGalWebSite.APIServer.Application.Tags;
using CnGalWebSite.APIServer.Application.Users;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.APIServer.ExamineX;
using CnGalWebSite.DataModel.Application.Dtos;
using CnGalWebSite.DataModel.ExamineModel;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Disambig;
using CnGalWebSite.Helper.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Message = CnGalWebSite.DataModel.Model.Message;
using Tag = CnGalWebSite.DataModel.Model.Tag;

namespace CnGalWebSite.APIServer.Application.Examines
{
    public class ExamineService : IExamineService
    {
        private readonly IRepository<Examine, int> _examineRepository;
        private readonly IAppHelper _appHelper;
        private readonly IRepository<Disambig, int> _disambigRepository;
        private readonly IRepository<Tag, int> _tagRepository;
        private readonly IRepository<Article, long> _articleRepository;
        private readonly IRepository<Comment, long> _commentRepository;
        private readonly IRepository<Entry, int> _entryRepository;
        private readonly IRepository<Periphery, long> _peripheryRepository;
        private readonly IRepository<ApplicationUser, string> _userRepository;
        private readonly IRepository<PlayedGame, string> _playedGameRepository;
        private readonly IEntryService _entryService;
        private readonly IPeripheryService _peripheryService;
        private readonly IArticleService _articleService;
        private readonly ITagService _tagService;
        private readonly IDisambigService _disambigService;
        private readonly IUserService _userService;
        private readonly IRankService _rankService;
        private readonly IPerfectionService _perfectionService;
        private readonly IPlayedGameService _playedGameService;
        private readonly ICommentService _commentService;
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ExamineService> _logger;
        private readonly IRepository<Message, long> _messageRepository;
        private readonly IRepository<Vote, long> _voteRepository;
        private readonly IRepository<Lottery, long> _lotteryRepository;


        private static readonly ConcurrentDictionary<Type, Func<IEnumerable<Examine>, string, SortOrder, IEnumerable<Examine>>> SortLambdaCacheEntry = new();

        public ExamineService(IRepository<Examine, int> examineRepository, IAppHelper appHelper, IRepository<Entry, int> entryRepository, IRankService rankService, IPerfectionService perfectionService,
        IArticleService articleService, ITagService tagService, IDisambigService disambigService, IUserService userService, IRepository<ApplicationUser, string> userRepository, IRepository<Message, long> messageRepository,
        IRepository<Article, long> articleRepository, IRepository<Tag, int> tagRepository, IEntryService entryService, IPeripheryService peripheryService, IPlayedGameService playedGameService,
        IRepository<Comment, long> commentRepository, IRepository<Disambig, int> disambigRepository, IRepository<Periphery, long> peripheryRepository, ILogger<ExamineService> logger, IRepository<Lottery, long> lotteryRepository,
        IConfiguration configuration, UserManager<ApplicationUser> userManager, IRepository<PlayedGame, string> playedGameRepository, ICommentService commentService, IRepository<Vote, long> voteRepository)
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
            _logger = logger;
            _playedGameService = playedGameService;
            _playedGameRepository = playedGameRepository;
            _messageRepository = messageRepository;
            _commentService = commentService;
            _lotteryRepository = lotteryRepository;
            _voteRepository = voteRepository;
        }

        public async Task<PagedResultDto<ExaminedNormalListModel>> GetPaginatedResult(GetExamineInput input, int entryId = 0, string userId = "")
        {
            IQueryable<Examine> query = null;
            if (entryId > 0)
            {
                query = _examineRepository.GetAll().AsNoTracking().Where(s => s.EntryId == entryId && s.IsPassed == true);
            }
            else
            {
                query = string.IsNullOrWhiteSpace(userId) == false
                    ? _examineRepository.GetAll().AsNoTracking().Where(s => s.ApplicationUserId == userId)
                    : _examineRepository.GetAll().AsNoTracking();

            }

            //判断是否是条件筛选
            if (!string.IsNullOrWhiteSpace(input.ScreeningConditions))
            {
                switch (input.ScreeningConditions)
                {
                    case "Passing":
                        query = query.Where(s => s.IsPassed == null);
                        break;
                    case "Passed":
                        query = query.Where(s => s.IsPassed == true);
                        break;
                    case "UnPassed":
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
                  || s.Entry != null && s.Entry.Name.Contains(input.FilterText)
                  || s.Article != null && s.Article.Name.Contains(input.FilterText)
                  || s.Operation == operation);
            }
            //统计查询数据的总条数
            var count = query.Count();
            //根据需求进行排序，然后进行分页逻辑的计算
            query = input.IsVisual
                ? query.OrderBy(input.Sorting).Skip(input.CurrentPage).Take(input.MaxResultCount)
                : query.OrderBy(input.Sorting).Skip((input.CurrentPage - 1) * input.MaxResultCount).Take(input.MaxResultCount);

            //将结果转换为List集合 加载到内存中
            var models = count != 0 ? await GetExaminesToNormalListAsync(query, false) : new List<ExaminedNormalListModel>();
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

        public async Task<QueryData<ListExamineAloneModel>> GetPaginatedResult(DataModel.ViewModel.Search.QueryPageOptions options, ListExamineAloneModel searchModel)
        {
            var items = _examineRepository.GetAll().Include(s => s.ApplicationUser).AsNoTracking();
            // 处理高级搜索
            if (!string.IsNullOrWhiteSpace(searchModel.EntryId?.ToString()))
            {
                items = items.Where(item => item.EntryId.ToString().Contains(searchModel.EntryId.ToString(), StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(searchModel.ArticleId?.ToString()))
            {
                items = items.Where(item => item.ArticleId.ToString().Contains(searchModel.ArticleId.ToString(), StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(searchModel.TagId?.ToString()))
            {
                items = items.Where(item => item.TagId.ToString().Contains(searchModel.TagId.ToString(), StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(searchModel.ApplicationUserId?.ToString()))
            {
                items = items.Where(item => item.ApplicationUserId.ToString().Contains(searchModel.ApplicationUserId.ToString(), StringComparison.OrdinalIgnoreCase));
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
                items = items.Where(item => item.EntryId.ToString().Contains(options.SearchText)
                             || item.EntryId.ToString().Contains(options.SearchText)
                             || item.ArticleId.ToString().Contains(options.SearchText)
                             || item.TagId.ToString().Contains(options.SearchText)
                             || item.ApplicationUserId.ToString().Contains(options.SearchText));
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



        public async Task<bool> GetExamineView(ExamineViewModel model, Examine examine)
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
                Operation.EditPlayedGameMain => await GetEditPlayedGameMainExamineView(model, examine),
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
                              n.PlayedGameId,
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
                    Ranks = isShowRanks ? (await _rankService.GetUserRanks(item.UserId)).Where(s => s.Name != "编辑者").ToList() : new List<DataModel.ViewModel.Ranks.RankViewModel>(),
                    RelatedId = item.EntryId != null ? item.EntryId.ToString() : item.ArticleId != null ? item.ArticleId.ToString() : item.TagId != null ? item.TagId.ToString() : item.CommentId != null ? item.CommentId.ToString() : item.DisambigId != null ? item.DisambigId.ToString() : item.PeripheryId != null ? item.PeripheryId.ToString() : item.PlayedGameId != null ? item.PlayedGameId.ToString() : item.UserId,
                    Type = item.EntryId != null ? ExaminedNormalListModelType.Entry : item.ArticleId != null ? ExaminedNormalListModelType.Article : item.TagId != null ? ExaminedNormalListModelType.Tag : item.CommentId != null ? ExaminedNormalListModelType.Comment : item.DisambigId != null ? ExaminedNormalListModelType.Disambig : item.PeripheryId != null ? ExaminedNormalListModelType.Periphery : item.PlayedGameId != null ? ExaminedNormalListModelType.PlayedGame : ExaminedNormalListModelType.User,
                    RelatedName = item.EntryId != null ? item.EntryName : item.ArticleId != null ? item.ArticleName : item.TagId != null ? item.TagName : item.DisambigId != null ? item.DisambigName : item.PeripheryId != null ? item.PeripheryName : item.CommentId != null ? "" : item.PlayedGameId != null ? "" : item.UserName
                };
                result.Add(tempModel);
            }

            return result;
        }

        #region 获取审核记录视图

        #region 词条

        private ExaminePreDataModel InitExamineViewEntryMain(Entry entry)
        {
            var model = new ExaminePreDataModel();

            if (string.IsNullOrWhiteSpace(entry.MainPicture) == false)
            {
                model.Pictures.Add(new PicturesAloneViewModel
                {
                    Note = "主图",
                    Url = _appHelper.GetImagePath(entry.MainPicture, "app.png"),
                });
            }
            if (string.IsNullOrWhiteSpace(entry.Thumbnail) == false)
            {
                model.Pictures.Add(new PicturesAloneViewModel
                {
                    Note = "缩略图",
                    Url = _appHelper.GetImagePath(entry.Thumbnail, "app.png"),
                });
            }
            if (string.IsNullOrWhiteSpace(entry.BackgroundPicture) == false)
            {
                model.Pictures.Add(new PicturesAloneViewModel
                {
                    Note = "大背景图",
                    Url = _appHelper.GetImagePath(entry.BackgroundPicture, "app.png"),
                });
            }
            if (string.IsNullOrWhiteSpace(entry.SmallBackgroundPicture) == false)
            {
                model.Pictures.Add(new PicturesAloneViewModel
                {
                    Note = "小背景图",
                    Url = _appHelper.GetImagePath(entry.SmallBackgroundPicture, "app.png"),
                });
            }

            var texts = new List<KeyValueModel>();

            if (string.IsNullOrWhiteSpace(entry.Name) == false)
            {
                texts.Add(new KeyValueModel
                {
                    DisplayName = "唯一名称",
                    DisplayValue = entry.Name,
                });
            }
            if (string.IsNullOrWhiteSpace(entry.DisplayName) == false)
            {
                texts.Add(new KeyValueModel
                {
                    DisplayName = "显示名称",
                    DisplayValue = entry.DisplayName,
                });
            }
            if (string.IsNullOrWhiteSpace(entry.AnotherName) == false)
            {
                texts.Add(new KeyValueModel
                {
                    DisplayName = "别称",
                    DisplayValue = entry.AnotherName,
                });
            }
            if (string.IsNullOrWhiteSpace(entry.BriefIntroduction) == false)
            {
                texts.Add(new KeyValueModel
                {
                    DisplayName = "简介",
                    DisplayValue = entry.BriefIntroduction,
                });
            }
            texts.Add(new KeyValueModel
            {
                DisplayName = "类型",
                DisplayValue = entry.Type.GetDisplayName(),
            });

            model.Texts.Add(new InformationsModel
            {
                Modifier = "主要信息",
                Informations = texts
            });

            return model;
        }

        public async Task<bool> GetEstablishMainExamineView(ExamineViewModel model, Examine examine)
        {
            model.Type = ExaminedNormalListModelType.Entry;
            var entry = await _entryRepository.FirstOrDefaultAsync(s => s.Id == examine.EntryId);
            if (entry == null)
            {
                return false;
            }
            model.ObjectId = entry.Id;
            model.ObjectName = entry.Name;
            model.ObjectBriefIntroduction = entry.BriefIntroduction;
            if (entry.Type is EntryType.Game or EntryType.ProductionGroup)
            {
                model.Image = _appHelper.GetImagePath(entry.MainPicture, "app.png");
            }
            else
            {
                model.Image = _appHelper.GetImagePath(entry.Thumbnail, "user.png");
                model.IsThumbnail = true;
            }

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

        private static ExaminePreDataModel InitExamineViewEntryAddinfor(Entry entry)
        {
            var model = new ExaminePreDataModel();
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

            model.Texts = information;

            return model;
        }

        public async Task<bool> GetEstablishAddInforExamineView(ExamineViewModel model, Examine examine)
        {
            model.Type = ExaminedNormalListModelType.Entry;

            var entry = await _entryRepository.GetAll()
                   .Include(s => s.Information)
                     .ThenInclude(s => s.Additional)
                   .FirstOrDefaultAsync(s => s.Id == examine.EntryId);
            if (entry == null)
            {
                return false;
            }
            model.ObjectId = entry.Id;
            model.ObjectName = examine.Entry.Name;
            model.ObjectBriefIntroduction = entry.BriefIntroduction;
            if (entry.Type is EntryType.Game or EntryType.ProductionGroup)
            {
                model.Image = _appHelper.GetImagePath(entry.MainPicture, "app.png");
            }
            else
            {
                model.Image = _appHelper.GetImagePath(entry.Thumbnail, "user.png");
                model.IsThumbnail = true;
            }
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

        private static ExaminePreDataModel InitExamineViewEntryImages(Entry entry)
        {
            var model = new ExaminePreDataModel();
            foreach (var item in entry.Pictures)
            {
                model.Pictures.Add(new PicturesAloneViewModel
                {
                    Url = item.Url,
                    Note = item.Note
                });
            }

            return model;
        }

        public async Task<bool> GetEstablishImagesExamineView(ExamineViewModel model, Examine examine)
        {
            model.Type = ExaminedNormalListModelType.Entry;
            var entry = await _entryRepository.GetAll()
                  .Include(s => s.Pictures)
                  .FirstOrDefaultAsync(s => s.Id == examine.EntryId);
            if (entry == null)
            {
                return false;
            }
            model.ObjectId = entry.Id;
            model.ObjectName = entry.Name;
            model.ObjectBriefIntroduction = entry.BriefIntroduction;
            if (entry.Type is EntryType.Game or EntryType.ProductionGroup)
            {
                model.Image = _appHelper.GetImagePath(entry.MainPicture, "app.png");
            }
            else
            {
                model.Image = _appHelper.GetImagePath(entry.Thumbnail, "user.png");
                model.IsThumbnail = true;
            }
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

        private async Task<ExaminePreDataModel> InitExamineViewEntryRelevances(Entry entry)
        {
            var model = new ExaminePreDataModel();

            foreach (var item in entry.Articles)
            {
                model.Relevances.Add(new DataModel.ViewModel.Search.SearchAloneModel
                {
                    article = _appHelper.GetArticleInforTipViewModel(item)
                });

            }

            foreach (var item in entry.EntryRelationFromEntryNavigation)
            {
                model.Relevances.Add(new DataModel.ViewModel.Search.SearchAloneModel
                {
                    entry = await _appHelper.GetEntryInforTipViewModel(item.ToEntryNavigation)
                });

            }

            foreach (var item in entry.Outlinks)
            {
                model.Outlinks.Add(new RelevancesKeyValueModel
                {
                    DisplayName = item.Name,
                    DisplayValue = item.BriefIntroduction,
                    Link = item.Link
                });

            }


            return model;
        }

        public async Task<bool> GetEstablishRelevancesExamineView(ExamineViewModel model, Examine examine)
        {
            model.Type = ExaminedNormalListModelType.Entry;
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
            model.ObjectId = entry.Id;
            model.ObjectName = entry.Name;
            model.ObjectBriefIntroduction = entry.BriefIntroduction;
            if (entry.Type is EntryType.Game or EntryType.ProductionGroup)
            {
                model.Image = _appHelper.GetImagePath(entry.MainPicture, "app.png");
            }
            else
            {
                model.Image = _appHelper.GetImagePath(entry.Thumbnail, "user.png");
                model.IsThumbnail = true;
            }

            //序列化相关性列表
            //先读取词条信息
            var relevances = await InitExamineViewEntryRelevances(entry);

            //添加修改记录 
            await _entryService.UpdateEntryDataAsync(entry, examine);

            var relevances_examine = await InitExamineViewEntryRelevances(entry);

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

        private ExaminePreDataModel InitExamineViewEntryTags(Entry entry)
        {
            var model = new ExaminePreDataModel();

            foreach (var item in entry.Tags)
            {
                model.Relevances.Add(new DataModel.ViewModel.Search.SearchAloneModel
                {
                    tag = _appHelper.GetTagInforTipViewModel(item),
                });
            }
            return model;
        }

        public async Task<bool> GetEstablishTagsExamineView(ExamineViewModel model, Examine examine)
        {
            model.Type = ExaminedNormalListModelType.Entry;
            var entry = await _entryRepository.GetAll().Include(s => s.Tags).FirstOrDefaultAsync(s => s.Id == examine.EntryId);
            if (entry == null)
            {
                return false;
            }
            model.ObjectId = entry.Id;
            model.ObjectName = entry.Name;
            model.ObjectBriefIntroduction = entry.BriefIntroduction;
            if (entry.Type is EntryType.Game or EntryType.ProductionGroup)
            {
                model.Image = _appHelper.GetImagePath(entry.MainPicture, "app.png");
            }
            else
            {
                model.Image = _appHelper.GetImagePath(entry.Thumbnail, "user.png");
                model.IsThumbnail = true;
            }
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

        public async Task<bool> GetEstablishMainPageExamineView(ExamineViewModel model, Examine examine)
        {
            model.Type = ExaminedNormalListModelType.Entry;
            var entry = await _entryRepository.FirstOrDefaultAsync(s => s.Id == examine.EntryId);
            if (entry == null)
            {
                return false;
            }
            model.ObjectId = entry.Id;
            model.ObjectName = entry.Name;
            model.ObjectBriefIntroduction = entry.BriefIntroduction;
            if (entry.Type is EntryType.Game or EntryType.ProductionGroup)
            {
                model.Image = _appHelper.GetImagePath(entry.MainPicture, "app.png");
            }
            else
            {
                model.Image = _appHelper.GetImagePath(entry.Thumbnail, "user.png");
                model.IsThumbnail = true;
            }
            var mainpage = entry.MainPage;

            await _entryService.UpdateEntryDataAsync(entry, examine);

            var mainpage_examine = entry.MainPage;

            //判断是否是等待审核状态
            if (examine.IsPassed != null)
            {
                //序列化数据
                var htmlDiff = new HtmlDiff.HtmlDiff(mainpage_examine ?? "", mainpage ?? "");
                model.EditOverview = htmlDiff.Build().Replace("\r\n", "<br>");
                model.BeforeModel = new ExaminePreDataModel
                {
                    MainPage = _appHelper.MarkdownToHtml(examine.Context)
                };
                model.AfterModel = new ExaminePreDataModel
                {
                    MainPage = _appHelper.MarkdownToHtml(mainpage_examine)
                };
            }
            else
            {
                //序列化数据
                var htmlDiff = new HtmlDiff.HtmlDiff(mainpage ?? "", mainpage_examine ?? "");
                model.EditOverview = htmlDiff.Build().Replace("\r\n", "<br>");
                model.BeforeModel = new ExaminePreDataModel
                {
                    MainPage = _appHelper.MarkdownToHtml(mainpage)
                };
                model.AfterModel = new ExaminePreDataModel
                {
                    MainPage = _appHelper.MarkdownToHtml(examine.Context)
                };
            }
            return true;
        }

        #endregion

        #region 文章

        private ExaminePreDataModel InitExamineViewArticleMain(Article article)
        {
            var model = new ExaminePreDataModel();

            if (string.IsNullOrWhiteSpace(article.MainPicture) == false)
            {
                model.Pictures.Add(new PicturesAloneViewModel
                {
                    Note = "主图",
                    Url = _appHelper.GetImagePath(article.MainPicture, "app.png"),
                });
            }

            if (string.IsNullOrWhiteSpace(article.BackgroundPicture) == false)
            {
                model.Pictures.Add(new PicturesAloneViewModel
                {
                    Note = "大背景图",
                    Url = _appHelper.GetImagePath(article.BackgroundPicture, "app.png"),
                });
            }
            if (string.IsNullOrWhiteSpace(article.SmallBackgroundPicture) == false)
            {
                model.Pictures.Add(new PicturesAloneViewModel
                {
                    Note = "小背景图",
                    Url = _appHelper.GetImagePath(article.SmallBackgroundPicture, "app.png"),
                });
            }

            var texts = new List<KeyValueModel>();

            if (string.IsNullOrWhiteSpace(article.Name) == false)
            {
                texts.Add(new KeyValueModel
                {
                    DisplayName = "唯一名称",
                    DisplayValue = article.Name,
                });
            }
            if (string.IsNullOrWhiteSpace(article.DisplayName) == false)
            {
                texts.Add(new KeyValueModel
                {
                    DisplayName = "显示名称",
                    DisplayValue = article.DisplayName,
                });
            }

            if (string.IsNullOrWhiteSpace(article.BriefIntroduction) == false)
            {
                texts.Add(new KeyValueModel
                {
                    DisplayName = "简介",
                    DisplayValue = article.BriefIntroduction,
                });
            }
            texts.Add(new KeyValueModel
            {
                DisplayName = "类型",
                DisplayValue = article.Type.GetDisplayName(),
            });
            if (string.IsNullOrWhiteSpace(article.OriginalLink) == false)
            {
                texts.Add(new KeyValueModel
                {
                    DisplayName = "原文链接",
                    DisplayValue = article.OriginalLink,
                });
            }
            if (string.IsNullOrWhiteSpace(article.OriginalLink) == false)
            {
                texts.Add(new KeyValueModel
                {
                    DisplayName = "原文链接",
                    DisplayValue = article.OriginalLink,
                });
            }
            if (string.IsNullOrWhiteSpace(article.OriginalAuthor) == false)
            {
                texts.Add(new KeyValueModel
                {
                    DisplayName = "作者",
                    DisplayValue = article.OriginalAuthor,
                });
            }

            texts.Add(new KeyValueModel
            {
                DisplayName = "发布时间",
                DisplayValue = article.PubishTime.ToString("G"),
            });
            texts.Add(new KeyValueModel
            {
                DisplayName = "真实发生时间",
                DisplayValue = article.RealNewsTime?.ToString("G") ?? "Null",
            });
            if (string.IsNullOrWhiteSpace(article.NewsType) == false)
            {
                texts.Add(new KeyValueModel
                {
                    DisplayName = "动态类型",
                    DisplayValue = article.NewsType,
                });
            }
            model.Texts.Add(new InformationsModel
            {
                Modifier = "主要信息",
                Informations = texts
            });

            return model;
        }

        public async Task<bool> GetEditArticleMainExamineView(ExamineViewModel model, Examine examine)
        {
            model.Type = ExaminedNormalListModelType.Article;
            var article = await _articleRepository.FirstOrDefaultAsync(s => s.Id == examine.ArticleId);
            if (article == null)
            {
                return false;
            }
            model.ObjectId = article.Id;
            model.ObjectName = article.Name;
            model.ObjectBriefIntroduction = article.BriefIntroduction;
            model.Image = _appHelper.GetImagePath(article.MainPicture, "app.png");

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

        private async Task<ExaminePreDataModel> InitExamineViewArticleRelevances(Article article)
        {
            var model = new ExaminePreDataModel();

            foreach (var item in article.Entries)
            {
                model.Relevances.Add(new DataModel.ViewModel.Search.SearchAloneModel
                {
                    entry = await _appHelper.GetEntryInforTipViewModel(item)
                });

            }

            foreach (var item in article.ArticleRelationFromArticleNavigation)
            {
                model.Relevances.Add(new DataModel.ViewModel.Search.SearchAloneModel
                {
                    article = _appHelper.GetArticleInforTipViewModel(item.ToArticleNavigation)
                });

            }

            foreach (var item in article.Outlinks)
            {
                model.Outlinks.Add(new RelevancesKeyValueModel
                {
                    DisplayName = item.Name,
                    DisplayValue = item.BriefIntroduction,
                    Link = item.Link
                });

            }


            return model;
        }


        public async Task<bool> GetEditArticleRelevanesExamineView(ExamineViewModel model, Examine examine)
        {
            model.Type = ExaminedNormalListModelType.Article;
            var article = await _articleRepository.GetAll()
                .Include(s => s.ArticleRelationFromArticleNavigation).ThenInclude(s => s.ToArticleNavigation)
                .Include(s => s.Entries)
                .Include(s => s.Outlinks)
                .FirstOrDefaultAsync(s => s.Id == examine.ArticleId);
            if (article == null)
            {

                return false;
            }
            model.ObjectId = article.Id;
            model.ObjectName = article.Name;
            model.ObjectBriefIntroduction = article.BriefIntroduction;
            model.Image = _appHelper.GetImagePath(article.MainPicture, "app.png");


            //序列化相关性列表
            //先读取词条信息
            var relevances = await InitExamineViewArticleRelevances(article);

            //添加修改记录 
            await _articleService.UpdateArticleData(article, examine);

            var relevances_examine = await InitExamineViewArticleRelevances(article);

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

        public async Task<bool> GetEditArticleMainPageExamineView(ExamineViewModel model, Examine examine)
        {
            model.Type = ExaminedNormalListModelType.Article;
            var article = await _articleRepository.FirstOrDefaultAsync(s => s.Id == examine.ArticleId);
            if (article == null)
            {
                return false;
            }
            model.ObjectId = article.Id;
            model.ObjectName = article.Name;
            model.ObjectBriefIntroduction = article.BriefIntroduction;
            model.Image = _appHelper.GetImagePath(article.MainPicture, "app.png");


            var mainpage = article.MainPage;

            await _articleService.UpdateArticleData(article, examine);

            var mainpage_examine = article.MainPage;

            //判断是否是等待审核状态
            if (examine.IsPassed != null)
            {
                //序列化数据
                var htmlDiff = new HtmlDiff.HtmlDiff(mainpage_examine ?? "", mainpage ?? "");
                model.EditOverview = htmlDiff.Build().Replace("\r\n", "<br>");
                model.BeforeModel = new ExaminePreDataModel
                {
                    MainPage = _appHelper.MarkdownToHtml(examine.Context)
                };
                model.AfterModel = new ExaminePreDataModel
                {
                    MainPage = _appHelper.MarkdownToHtml(mainpage_examine)
                };
            }
            else
            {
                //序列化数据
                var htmlDiff = new HtmlDiff.HtmlDiff(mainpage ?? "", mainpage_examine ?? "");
                model.EditOverview = htmlDiff.Build().Replace("\r\n", "<br>");
                model.BeforeModel = new ExaminePreDataModel
                {
                    MainPage = _appHelper.MarkdownToHtml(mainpage)
                };
                model.AfterModel = new ExaminePreDataModel
                {
                    MainPage = _appHelper.MarkdownToHtml(examine.Context)
                };
            }

            return true;
        }
        #endregion

        #region 标签

        private ExaminePreDataModel InitExamineViewTagMain(Tag tag)
        {
            var model = new ExaminePreDataModel();

            if (string.IsNullOrWhiteSpace(tag.MainPicture) == false)
            {
                model.Pictures.Add(new PicturesAloneViewModel
                {
                    Note = "主图",
                    Url = _appHelper.GetImagePath(tag.MainPicture, "app.png"),
                });
            }
            if (string.IsNullOrWhiteSpace(tag.Thumbnail) == false)
            {
                model.Pictures.Add(new PicturesAloneViewModel
                {
                    Note = "缩略图",
                    Url = _appHelper.GetImagePath(tag.Thumbnail, "app.png"),
                });
            }
            if (string.IsNullOrWhiteSpace(tag.BackgroundPicture) == false)
            {
                model.Pictures.Add(new PicturesAloneViewModel
                {
                    Note = "大背景图",
                    Url = _appHelper.GetImagePath(tag.BackgroundPicture, "app.png"),
                });
            }
            if (string.IsNullOrWhiteSpace(tag.SmallBackgroundPicture) == false)
            {
                model.Pictures.Add(new PicturesAloneViewModel
                {
                    Note = "小背景图",
                    Url = _appHelper.GetImagePath(tag.SmallBackgroundPicture, "app.png"),
                });
            }

            var texts = new List<KeyValueModel>();

            if (string.IsNullOrWhiteSpace(tag.Name) == false)
            {
                texts.Add(new KeyValueModel
                {
                    DisplayName = "唯一名称",
                    DisplayValue = tag.Name,
                });
            }
            if (tag.ParentCodeNavigation != null)
            {
                texts.Add(new KeyValueModel
                {
                    DisplayName = "父标签",
                    DisplayValue = tag.ParentCodeNavigation.Name,
                });
            }

            if (string.IsNullOrWhiteSpace(tag.BriefIntroduction) == false)
            {
                texts.Add(new KeyValueModel
                {
                    DisplayName = "简介",
                    DisplayValue = tag.BriefIntroduction,
                });
            }

            if (texts.Count > 0)
            {
                model.Texts.Add(new InformationsModel
                {
                    Modifier = "主要信息",
                    Informations = texts
                });
            }
            if (tag.ParentCodeNavigation != null)
            {
                model.Relevances.Add(new DataModel.ViewModel.Search.SearchAloneModel
                {
                    tag = _appHelper.GetTagInforTipViewModel(tag.ParentCodeNavigation),
                });
            }

            return model;
        }

        public async Task<bool> GetEditTagMainExamineView(ExamineViewModel model, Examine examine)
        {
            model.Type = ExaminedNormalListModelType.Tag;
            var tag = await _tagRepository.GetAll().Include(s => s.ParentCodeNavigation).FirstOrDefaultAsync(s => s.Id == examine.TagId);
            if (tag == null)
            {
                return false;
            }
            model.ObjectId = tag.Id;
            model.ObjectName = tag.Name;
            model.ObjectBriefIntroduction = tag.BriefIntroduction;
            model.Image = _appHelper.GetImagePath(tag.MainPicture, "app.png");

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


        private ExaminePreDataModel InitExamineViewEditChildTags(Tag tag)
        {
            var model = new ExaminePreDataModel();

            foreach (var item in tag.InverseParentCodeNavigation)
            {
                model.Relevances.Add(new DataModel.ViewModel.Search.SearchAloneModel
                {
                    tag = _appHelper.GetTagInforTipViewModel(item)
                });
            }
            return model;
        }

        public async Task<bool> GetEditTagChildTagsExamineView(ExamineViewModel model, Examine examine)
        {
            model.Type = ExaminedNormalListModelType.Tag;
            var tag = await _tagRepository.GetAll().Include(s => s.InverseParentCodeNavigation).FirstOrDefaultAsync(s => s.Id == examine.TagId);
            if (tag == null)
            {
                return false;
            }
            model.ObjectId = tag.Id;
            model.ObjectName = tag.Name;
            model.ObjectBriefIntroduction = tag.BriefIntroduction;
            model.Image = _appHelper.GetImagePath(tag.MainPicture, "app.png");

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


        private async Task<ExaminePreDataModel> InitExamineViewEditChildEntries(Tag tag)
        {
            var model = new ExaminePreDataModel();

            foreach (var item in tag.Entries)
            {
                model.Relevances.Add(new DataModel.ViewModel.Search.SearchAloneModel
                {
                    entry = await _appHelper.GetEntryInforTipViewModel(item)
                });
            }
            return model;
        }

        public async Task<bool> GetEditTagChildEntriesExamineView(ExamineViewModel model, Examine examine)
        {
            model.Type = ExaminedNormalListModelType.Tag;
            var tag = await _tagRepository.GetAll().Include(s => s.Entries).FirstOrDefaultAsync(s => s.Id == examine.TagId);
            if (tag == null)
            {
                return false;
            }
            model.ObjectId = tag.Id;
            model.ObjectName = tag.Name;
            model.ObjectBriefIntroduction = tag.BriefIntroduction;
            model.Image = _appHelper.GetImagePath(tag.MainPicture, "app.png");

            //序列化相关性列表
            //先读取词条信息
            var relevances = await InitExamineViewEditChildEntries(tag);

            //添加修改记录 
            await _tagService.UpdateTagDataAsync(tag, examine);

            var relevances_examine = await InitExamineViewEditChildEntries(tag);

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

        public async Task<bool> GetPubulishCommentExamineView(ExamineViewModel model, Examine examine)
        {
            model.Type = ExaminedNormalListModelType.Comment;
            var comment = await _commentRepository.GetAll().AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == examine.CommentId);
            if (comment == null)
            {
                return false;
            }
            model.ObjectId = comment.Id;
            model.Image = _appHelper.GetImagePath("", "app.png");

            //序列化数据
            CommentText commentText = null;
            using (TextReader str = new StringReader(examine.Context))
            {
                var serializer = new JsonSerializer();
                commentText = (CommentText)serializer.Deserialize(str, typeof(CommentText));
            }

            //json格式化
            model.EditOverview = _appHelper.GetJsonStringView(examine.Context);

            model.AfterModel.Texts.Add(new InformationsModel
            {
                Modifier = "主要信息",
                Informations = new List<KeyValueModel>
                {
                    new KeyValueModel
                    {
                        DisplayName="时间",
                        DisplayValue=commentText.CommentTime.ToString("G"),
                    },
                    new KeyValueModel
                    {
                        DisplayName="类型",
                        DisplayValue=commentText.Type.GetDisplayName()
                    },
                    new KeyValueModel
                    {
                        DisplayName="目标Id",
                        DisplayValue=commentText.ObjectId
                    },
                }
            });

            model.AfterModel.MainPage = _appHelper.MarkdownToHtml(commentText.Text);

            while (commentText.Type == CommentType.ReplyComment)
            {
                var item = await _commentRepository.GetAll()
                    .Include(s => s.ParentCodeNavigation)
                    .FirstOrDefaultAsync(s => s.Id.ToString() == commentText.ObjectId);
                commentText.Type = item.Type;
                commentText.ObjectId = item.Type switch
                {
                    CommentType.ReplyComment => item.ParentCodeNavigation.Id.ToString(),
                    CommentType.CommentEntries => item.EntryId.ToString(),
                    CommentType.CommentArticle => item.ArticleId.ToString(),
                    CommentType.CommentPeriphery => item.PeripheryId.ToString(),
                    _ => null
                };
            }

            if (commentText.Type == CommentType.CommentArticle)
            {
                var item = await _articleRepository.FirstOrDefaultAsync(s => s.Id.ToString() == commentText.ObjectId);
                if (item != null)
                {
                    model.AfterModel.Relevances.Add(new DataModel.ViewModel.Search.SearchAloneModel
                    {
                        article = _appHelper.GetArticleInforTipViewModel(item)
                    });
                    model.ObjectBriefIntroduction = item.BriefIntroduction;
                    model.Image = _appHelper.GetImagePath(item.MainPicture, "app.png");

                }
            }
            else if (commentText.Type == CommentType.CommentEntries)
            {
                var item = await _entryRepository.FirstOrDefaultAsync(s => s.Id.ToString() == commentText.ObjectId);
                if (item != null)
                {
                    model.AfterModel.Relevances.Add(new DataModel.ViewModel.Search.SearchAloneModel
                    {
                        entry = await _appHelper.GetEntryInforTipViewModel(item)
                    });
                    model.ObjectBriefIntroduction = item.BriefIntroduction;
                    if (item.Type is EntryType.Game or EntryType.ProductionGroup)
                    {
                        model.Image = _appHelper.GetImagePath(item.MainPicture, "app.png");
                    }
                    else
                    {
                        model.Image = _appHelper.GetImagePath(item.Thumbnail, "user.png");
                        model.IsThumbnail = true;
                    }
                }
            }
            else if (commentText.Type == CommentType.CommentPeriphery)
            {
                var item = await _peripheryRepository.FirstOrDefaultAsync(s => s.Id.ToString() == commentText.ObjectId);
                if (item != null)
                {
                    model.AfterModel.Relevances.Add(new DataModel.ViewModel.Search.SearchAloneModel
                    {
                        periphery = _appHelper.GetPeripheryInforTipViewModel(item)
                    });
                    model.ObjectBriefIntroduction = item.BriefIntroduction;
                    model.Image = _appHelper.GetImagePath(item.MainPicture, "app.png");
                }
            }

            if (examine.IsPassed == true)
            {
                model.BeforeModel = model.AfterModel;
            }
            return true;
        }

        #endregion

        #region 消歧义页
        public async Task<bool> GetDisambigMainExamineView(ExamineViewModel model, Examine examine)
        {
            model.Type = ExaminedNormalListModelType.Disambig;
            var disambig = await _disambigRepository.FirstOrDefaultAsync(s => s.Id == examine.DisambigId);
            if (disambig == null)
            {
                return false;
            }
            model.ObjectId = disambig.Id;
            model.ObjectName = disambig.Name;
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

                //model.BeforeModel = disambigMain;
                //model.AfterModel = disambigMainBefore;
            }
            else
            {
                var htmlDiff = new HtmlDiff.HtmlDiff(disambig.Name ?? "", disambigMain.Name ?? "");
                model.EditOverview = "<h5>名称</h5>" + "<h5>" + htmlDiff.Build().Replace("\r\n", "<br>") + "</h5>";

                htmlDiff = new HtmlDiff.HtmlDiff(disambig.BriefIntroduction ?? "", disambigMain.BriefIntroduction ?? "");
                model.EditOverview += "<h5>简介</h5>" + "<h5>" + htmlDiff.Build().Replace("\r\n", "<br>") + "</h5>";


                //model.AfterModel = disambigMain;
                //model.BeforeModel = disambigMainBefore;
            }

            return true;
        }

        public async Task<bool> GetDisambigRelevancesExamineView(ExamineViewModel model, Examine examine)
        {
            model.Type = ExaminedNormalListModelType.Disambig;
            var disambig = await _disambigRepository.GetAll()
                 .Include(s => s.Entries).Include(s => s.Articles)
                 .FirstOrDefaultAsync(s => s.Id == examine.DisambigId);
            if (disambig == null)
            {

                return false;
            }
            model.ObjectId = disambig.Id;
            model.ObjectName = disambig.Name;
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
                                _ = disambigAloneModels_examine.Remove(infor);

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
                                _ = disambigAloneModels_examine.Remove(infor);

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

            ////判断是否是等待审核状态
            //if (examine.IsPassed != null)
            //{
            //    model.BeforeModel = disambigAloneModels_examine;
            //    model.AfterModel = disambigAloneModels;
            //}
            //else
            //{
            //    model.BeforeModel = disambigAloneModels;
            //    model.AfterModel = disambigAloneModels_examine;
            //}

            return true;
        }

        #endregion

        #region 用户

        public bool GetUserMainPageExamineView(ExamineViewModel model, Examine examine)
        {
            model.Type = ExaminedNormalListModelType.User;
            //model.ObjectId = examine.ApplicationUser.Id;
            model.ObjectName = examine.ApplicationUser.UserName;
            model.ObjectBriefIntroduction = examine.ApplicationUser.PersonalSignature;
            model.Image = _appHelper.GetImagePath(examine.ApplicationUser.PhotoPath, "user.png");
            model.IsThumbnail = true;

            //判断是否是等待审核状态
            if (examine.IsPassed != null)
            {
                var htmlDiff = new HtmlDiff.HtmlDiff(examine.Context ?? "", examine.ApplicationUser.MainPageContext ?? "");
                model.EditOverview = htmlDiff.Build().Replace("\r\n", "<br>");
                model.BeforeModel = new ExaminePreDataModel
                {
                    MainPage = _appHelper.MarkdownToHtml(examine.Context)
                };
                model.AfterModel = new ExaminePreDataModel
                {
                    MainPage = _appHelper.MarkdownToHtml(examine.ApplicationUser.MainPageContext)
                };
            }
            else
            {
                var htmlDiff = new HtmlDiff.HtmlDiff(examine.ApplicationUser.MainPageContext ?? "", examine.Context ?? "");
                model.EditOverview = htmlDiff.Build().Replace("\r\n", "<br>");
                model.BeforeModel = new ExaminePreDataModel
                {
                    MainPage = _appHelper.MarkdownToHtml(examine.ApplicationUser.MainPageContext)
                };
                model.AfterModel = new ExaminePreDataModel
                {
                    MainPage = _appHelper.MarkdownToHtml(examine.Context)
                };
            }

            return true;
        }

        private ExaminePreDataModel InitExamineViewUserMain(ApplicationUser user)
        {
            var model = new ExaminePreDataModel();

            if (string.IsNullOrWhiteSpace(user.PhotoPath) == false)
            {
                model.Pictures.Add(new PicturesAloneViewModel
                {
                    Note = "头像",
                    Url = _appHelper.GetImagePath(user.PhotoPath, "app.png"),
                });
            }
            if (string.IsNullOrWhiteSpace(user.BackgroundImage) == false)
            {
                model.Pictures.Add(new PicturesAloneViewModel
                {
                    Note = "空间头图",
                    Url = _appHelper.GetImagePath(user.BackgroundImage, "app.png"),
                });
            }
            if (string.IsNullOrWhiteSpace(user.MBgImage) == false)
            {
                model.Pictures.Add(new PicturesAloneViewModel
                {
                    Note = "大背景图",
                    Url = _appHelper.GetImagePath(user.MBgImage, "app.png"),
                });
            }
            if (string.IsNullOrWhiteSpace(user.SBgImage) == false)
            {
                model.Pictures.Add(new PicturesAloneViewModel
                {
                    Note = "小背景图",
                    Url = _appHelper.GetImagePath(user.SBgImage, "app.png"),
                });
            }

            var texts = new List<KeyValueModel>();

            if (string.IsNullOrWhiteSpace(user.UserName) == false)
            {
                texts.Add(new KeyValueModel
                {
                    DisplayName = "用户名",
                    DisplayValue = user.UserName,
                });
            }
            if (string.IsNullOrWhiteSpace(user.PersonalSignature) == false)
            {
                texts.Add(new KeyValueModel
                {
                    DisplayName = "个性签名",
                    DisplayValue = user.PersonalSignature,
                });
            }


            model.Texts.Add(new InformationsModel
            {
                Modifier = "主要信息",
                Informations = texts
            });

            return model;
        }

        public async Task<bool> GetEditUserMainExamineView(ExamineViewModel model, Examine examine)
        {
            model.Type = ExaminedNormalListModelType.User;

            model.ObjectName = examine.ApplicationUser.UserName;
            model.ObjectBriefIntroduction = examine.ApplicationUser.PersonalSignature;
            model.Image = _appHelper.GetImagePath(examine.ApplicationUser.PhotoPath, "user.png");
            model.IsThumbnail = true;

            //序列化数据
            var userMainBefore = InitExamineViewUserMain(examine.ApplicationUser);

            //添加修改记录 
            await _userService.UpdateUserData(examine.ApplicationUser, examine);

            //序列化数据
            var userMain = InitExamineViewUserMain(examine.ApplicationUser);

            //json格式化
            model.EditOverview = _appHelper.GetJsonStringView(examine.Context);


            //判断是否是等待审核状态
            if (examine.IsPassed != null)
            {
                model.BeforeModel = userMain;
                model.AfterModel = userMainBefore;
            }
            else
            {
                model.BeforeModel = userMainBefore;
                model.AfterModel = userMain;
            }
            return true;
        }

        #endregion

        #region 周边

        private ExaminePreDataModel InitExamineViewPeripheryMain(Periphery periphery)
        {
            var model = new ExaminePreDataModel();

            if (string.IsNullOrWhiteSpace(periphery.MainPicture) == false)
            {
                model.Pictures.Add(new PicturesAloneViewModel
                {
                    Note = "主图",
                    Url = _appHelper.GetImagePath(periphery.MainPicture, "app.png"),
                });
            }
            if (string.IsNullOrWhiteSpace(periphery.Thumbnail) == false)
            {
                model.Pictures.Add(new PicturesAloneViewModel
                {
                    Note = "缩略图图",
                    Url = _appHelper.GetImagePath(periphery.MainPicture, "app.png"),
                });
            }

            if (string.IsNullOrWhiteSpace(periphery.BackgroundPicture) == false)
            {
                model.Pictures.Add(new PicturesAloneViewModel
                {
                    Note = "大背景图",
                    Url = _appHelper.GetImagePath(periphery.BackgroundPicture, "app.png"),
                });
            }
            if (string.IsNullOrWhiteSpace(periphery.SmallBackgroundPicture) == false)
            {
                model.Pictures.Add(new PicturesAloneViewModel
                {
                    Note = "小背景图",
                    Url = _appHelper.GetImagePath(periphery.SmallBackgroundPicture, "app.png"),
                });
            }

            var texts = new List<KeyValueModel>();

            if (string.IsNullOrWhiteSpace(periphery.Name) == false)
            {
                texts.Add(new KeyValueModel
                {
                    DisplayName = "唯一名称",
                    DisplayValue = periphery.Name,
                });
            }
            if (string.IsNullOrWhiteSpace(periphery.DisplayName) == false)
            {
                texts.Add(new KeyValueModel
                {
                    DisplayName = "显示名称",
                    DisplayValue = periphery.DisplayName,
                });
            }

            if (string.IsNullOrWhiteSpace(periphery.BriefIntroduction) == false)
            {
                texts.Add(new KeyValueModel
                {
                    DisplayName = "简介",
                    DisplayValue = periphery.BriefIntroduction,
                });
            }
            texts.Add(new KeyValueModel
            {
                DisplayName = "类型",
                DisplayValue = periphery.Type.GetDisplayName(),
            });
            if (periphery.PageCount > 0)
            {
                texts.Add(new KeyValueModel
                {
                    DisplayName = "页数",
                    DisplayValue = periphery.PageCount.ToString(),
                });
            }
            if (periphery.SongCount > 0)
            {
                texts.Add(new KeyValueModel
                {
                    DisplayName = "歌曲数",
                    DisplayValue = periphery.SongCount.ToString(),
                });
            }
            if (string.IsNullOrWhiteSpace(periphery.Material) == false)
            {
                texts.Add(new KeyValueModel
                {
                    DisplayName = "材质",
                    DisplayValue = periphery.Material,
                });
            }
            if (string.IsNullOrWhiteSpace(periphery.Author) == false)
            {
                texts.Add(new KeyValueModel
                {
                    DisplayName = "作者",
                    DisplayValue = periphery.Author,
                });
            }
            if (string.IsNullOrWhiteSpace(periphery.Price) == false)
            {
                texts.Add(new KeyValueModel
                {
                    DisplayName = "价格",
                    DisplayValue = periphery.Price
                });
            }

            if (string.IsNullOrWhiteSpace(periphery.IndividualParts) == false)
            {
                texts.Add(new KeyValueModel
                {
                    DisplayName = "独立部件数量",
                    DisplayValue = periphery.IndividualParts
                });
            }
            if (string.IsNullOrWhiteSpace(periphery.Brand) == false)
            {
                texts.Add(new KeyValueModel
                {
                    DisplayName = "品牌",
                    DisplayValue = periphery.Brand
                });
            }
            if (periphery.IsAvailableItem)
            {
                texts.Add(new KeyValueModel
                {
                    DisplayName = "是否为装饰品",
                    DisplayValue = "是"
                });
            }
            if (periphery.IsReprint)
            {
                texts.Add(new KeyValueModel
                {
                    DisplayName = "是否再版",
                    DisplayValue = "是"
                });
            }
            if (string.IsNullOrWhiteSpace(periphery.Size) == false)
            {
                texts.Add(new KeyValueModel
                {
                    DisplayName = "尺寸",
                    DisplayValue = periphery.Size
                });
            }

            model.Texts.Add(new InformationsModel
            {
                Modifier = "主要信息",
                Informations = texts
            });

            return model;

        }

        public async Task<bool> GetEditPeripheryMainExamineView(ExamineViewModel model, Examine examine)
        {
            model.Type = ExaminedNormalListModelType.Periphery;
            var periphery = await _peripheryRepository.FirstOrDefaultAsync(s => s.Id == examine.PeripheryId);
            if (periphery == null)
            {
                return false;
            }
            model.ObjectId = periphery.Id;
            model.ObjectName = periphery.Name;
            model.ObjectBriefIntroduction = periphery.BriefIntroduction;
            model.Image = _appHelper.GetImagePath(periphery.MainPicture, "app.png");


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


        private static ExaminePreDataModel InitExamineViewPeripheryImages(Periphery periphery)
        {
            var model = new ExaminePreDataModel();
            foreach (var item in periphery.Pictures)
            {
                model.Pictures.Add(new PicturesAloneViewModel
                {
                    Url = item.Url,
                    Note = item.Note
                });
            }

            return model;
        }

        public async Task<bool> GetEditPeripheryImagesExamineView(ExamineViewModel model, Examine examine)
        {
            model.Type = ExaminedNormalListModelType.Periphery;
            var periphery = await _peripheryRepository.GetAll().Include(s => s.Pictures).FirstOrDefaultAsync(s => s.Id == examine.PeripheryId);
            if (periphery == null)
            {
                return false;
            }
            model.ObjectId = periphery.Id;
            model.ObjectName = periphery.Name;
            model.ObjectBriefIntroduction = periphery.BriefIntroduction;
            model.Image = _appHelper.GetImagePath(periphery.MainPicture, "app.png");


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

        private async Task<ExaminePreDataModel> InitExamineViewPeripheryRelatedEntries(Periphery periphery)
        {
            var model = new ExaminePreDataModel();

            foreach (var item in periphery.RelatedEntries)
            {
                model.Relevances.Add(new DataModel.ViewModel.Search.SearchAloneModel
                {
                    entry = await _appHelper.GetEntryInforTipViewModel(item)
                });

            }

            return model;
        }

        public async Task<bool> GetEditPeripheryRelatedEntriesExamineView(ExamineViewModel model, Examine examine)
        {
            model.Type = ExaminedNormalListModelType.Periphery;
            var periphery = await _peripheryRepository.GetAll().Include(s => s.RelatedEntries).FirstOrDefaultAsync(s => s.Id == examine.PeripheryId);
            if (periphery == null)
            {
                return false;
            }
            model.ObjectId = periphery.Id;
            model.ObjectName = periphery.Name;
            model.ObjectBriefIntroduction = periphery.BriefIntroduction;
            model.Image = _appHelper.GetImagePath(periphery.MainPicture, "app.png");


            //序列化相关性列表
            //先读取词条信息
            var relevances = await InitExamineViewPeripheryRelatedEntries(periphery);

            //添加修改记录 
            await _peripheryService.UpdatePeripheryDataAsync(periphery, examine);

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

        private ExaminePreDataModel InitExamineViewPeripheryRelatedPeripheries(Periphery periphery)
        {
            var model = new ExaminePreDataModel();

            foreach (var item in periphery.PeripheryRelationFromPeripheryNavigation)
            {
                model.Relevances.Add(new DataModel.ViewModel.Search.SearchAloneModel
                {
                    periphery = _appHelper.GetPeripheryInforTipViewModel(item.ToPeripheryNavigation)
                });

            }

            return model;
        }

        public async Task<bool> GetEditPeripheryRelatedPeripheriesExamineView(ExamineViewModel model, Examine examine)
        {
            model.Type = ExaminedNormalListModelType.Periphery;
            var periphery = await _peripheryRepository.GetAll().Include(s => s.PeripheryRelationFromPeripheryNavigation).ThenInclude(s => s.ToPeripheryNavigation).ThenInclude(s => s.PeripheryRelationFromPeripheryNavigation).FirstOrDefaultAsync(s => s.Id == examine.PeripheryId);
            if (periphery == null)
            {
                return false;
            }
            model.ObjectId = periphery.Id;
            model.ObjectName = periphery.Name;
            model.ObjectBriefIntroduction = periphery.BriefIntroduction;
            model.Image = _appHelper.GetImagePath(periphery.MainPicture, "app.png");


            //序列化相关性列表
            //先读取词条信息
            var relevances = InitExamineViewPeripheryRelatedPeripheries(periphery);

            //添加修改记录 
            await _peripheryService.UpdatePeripheryDataAsync(periphery, examine);

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

        #region 游玩记录

        public async Task<bool> GetEditPlayedGameMainExamineView(ExamineViewModel model, Examine examine)
        {
            model.Type = ExaminedNormalListModelType.PlayedGame;
            var playedGame = await _playedGameRepository.GetAll().AsNoTracking()
                .Include(s => s.Entry)
                .FirstOrDefaultAsync(s => s.Id == examine.PlayedGameId);
            if (playedGame == null)
            {
                return false;
            }
            model.ObjectId = playedGame.Id;
            model.Image = _appHelper.GetImagePath("", "app.png");

            //序列化数据
            PlayedGameMain playedGameMain = null;
            using (TextReader str = new StringReader(examine.Context))
            {
                var serializer = new JsonSerializer();
                playedGameMain = (PlayedGameMain)serializer.Deserialize(str, typeof(PlayedGameMain));
            }

            //json格式化
            model.EditOverview = _appHelper.GetJsonStringView(examine.Context);

            if (playedGame.Entry != null)
            {
                model.AfterModel.Relevances.Add(new DataModel.ViewModel.Search.SearchAloneModel
                {
                    entry = await _appHelper.GetEntryInforTipViewModel(playedGame.Entry)
                });
                model.ObjectBriefIntroduction = playedGame.Entry.BriefIntroduction;

                model.Image = _appHelper.GetImagePath(playedGame.Entry.MainPicture, "app.png");
            }

            model.AfterModel.Texts.Add(new InformationsModel
            {
                Modifier = "主要信息",
                Informations = new List<KeyValueModel>
                {
                    new KeyValueModel
                    {
                        DisplayName="评语",
                        DisplayValue=playedGameMain.PlayImpressions
                    },
                    new KeyValueModel
                    {
                        DisplayName="是否公开",
                        DisplayValue=playedGameMain.ShowPublicly?"是":"否"
                    },
                    new KeyValueModel
                    {
                        DisplayName="游戏",
                        DisplayValue=playedGame.Entry.DisplayName
                    },
                }
            });

            if (examine.IsPassed == true)
            {
                model.BeforeModel = model.AfterModel;
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
            _ = await _entryRepository.UpdateAsync(entry);

            //更新完善度
            //await _perfectionService.UpdateEntryPerfectionResultAsync(entry.Id);
        }

        public async Task ExamineEstablishAddInforAsync(Entry entry, EntryAddInfor examine)
        {
            //更新数据
            _entryService.UpdateEntryDataAddInfor(entry, examine);
            //保存
            entry = await _entryRepository.UpdateAsync(entry);

            //更新完善度
            //await _perfectionService.UpdateEntryPerfectionResultAsync(entry.Id);

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
                        else if (item.Modifier == "基本信息" && item.DisplayName == "制作组" && string.IsNullOrWhiteSpace(item.DisplayValue) == false)
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
            _ = await _entryRepository.UpdateAsync(entry);

            //更新完善度
            //await _perfectionService.UpdateEntryPerfectionResultAsync(entry.Id);

        }

        public async Task ExamineEstablishRelevancesAsync(Entry entry, EntryRelevances examine)
        {
            //更新数据
            await _entryService.UpdateEntryDataRelevances(entry, examine);
            _ = await _entryRepository.UpdateAsync(entry);

            //更新完善度
            //await _perfectionService.UpdateEntryPerfectionResultAsync(entry.Id);

            var admin = new ApplicationUser
            {
                Id = _configuration["ExamineAdminId"],
                UserName = "看板娘"
            };

            //反向关联 词条
            foreach (var item in examine.Relevances.Where(s => s.IsDelete == false && s.Type == RelevancesType.Entry))
            {
                //查找关联词条
                var temp = await _entryRepository.GetAll()
                    .Include(s => s.EntryRelationFromEntryNavigation).ThenInclude(s => s.ToEntryNavigation)
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

            _ = await _entryRepository.UpdateAsync(entry);

            //更新完善度
            //await _perfectionService.UpdateEntryPerfectionResultAsync(entry.Id);

        }

        public async Task ExamineEstablishMainPageAsync(Entry entry, string examine)
        {
            //更新数据
            _entryService.UpdateEntryDataMainPage(entry, examine);

            _ = await _entryRepository.UpdateAsync(entry);

            //更新完善度
            //await _perfectionService.UpdateEntryPerfectionResultAsync(entry.Id);

        }
        #endregion

        #region 文章
        public async Task ExamineEditArticleMainAsync(Article article, ExamineMain examine)
        {
            _articleService.UpdateArticleDataMain(article, examine);

            _ = await _articleRepository.UpdateAsync(article);
        }

        public async Task ExamineEditArticleRelevancesAsync(Article article, ArticleRelevances examine)
        {
            await _articleService.UpdateArticleDataRelevances(article, examine);
            _ = await _articleRepository.UpdateAsync(article);

            var admin = new ApplicationUser
            {
                Id = _configuration["ExamineAdminId"],
                UserName="看板娘"
            };
            //反向关联 文章
            foreach (var item in examine.Relevances.Where(s => s.IsDelete == false && s.Type == RelevancesType.Article))
            {
                //查找关联文章
                var temp = await _articleRepository.GetAll()
                    .Include(s => s.ArticleRelationFromArticleNavigation).ThenInclude(s => s.ToArticleNavigation)
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

            _ = await _articleRepository.UpdateAsync(article);
        }
        #endregion

        #region 标签
        public async Task ExamineTagAsync(Tag tag, TagEdit examine)
        {
            await _tagService.UpdateTagDataOldAsync(tag, examine);

            _ = await _tagRepository.UpdateAsync(tag);
        }

        public async Task ExamineEditTagMainAsync(Tag tag, ExamineMain examine)
        {
            await _tagService.UpdateTagDataMainAsync(tag, examine);

            _ = await _tagRepository.UpdateAsync(tag);
        }

        public async Task ExamineEditTagChildTagsAsync(Tag tag, TagChildTags examine)
        {
            await _tagService.UpdateTagDataChildTagsAsync(tag, examine);

            _ = await _tagRepository.UpdateAsync(tag);
        }

        public async Task ExamineEditTagChildEntriesAsync(Tag tag, TagChildEntries examine)
        {
            await _tagService.UpdateTagDataChildEntriesAsync(tag, examine);

            _ = await _tagRepository.UpdateAsync(tag);
        }
        #endregion

        #region 消歧义页
        public async Task ExamineEditDisambigMainAsync(Disambig disambig, DisambigMain examine)
        {
            _disambigService.UpdateDisambigDataMain(disambig, examine);

            _ = await _disambigRepository.UpdateAsync(disambig);
        }

        public async Task ExamineEditDisambigRelevancesAsync(Disambig disambig, DisambigRelevances examine)
        {

            await _disambigService.UpdateDisambigDataRelevancesAsync(disambig, examine);

            _ = await _disambigRepository.UpdateAsync(disambig);
        }
        #endregion

        #region 用户

        public async Task ExamineEditUserMainAsync(ApplicationUser user, UserMain examine)
        {
            await _userService.UpdateUserDataMain(user, examine);

            _ = await _userRepository.UpdateAsync(user);
        }
        public async Task ExamineEditUserMainPageAsync(ApplicationUser user, string examine)
        {
            await _userService.UpdateUserDataMainPage(user, examine);

            _ = await _userRepository.UpdateAsync(user);
        }


        #endregion

        #region 周边

        public async Task ExamineEditPeripheryMainAsync(Periphery periphery, ExamineMain examine)
        {
            //更新数据
            _peripheryService.UpdatePeripheryDataMain(periphery, examine);
            //保存
            _ = await _peripheryRepository.UpdateAsync(periphery);
        }

        public async Task ExamineEditPeripheryImagesAsync(Periphery periphery, PeripheryImages examine)
        {
            //更新数据
            _peripheryService.UpdatePeripheryDataImages(periphery, examine);
            //保存
            _ = await _peripheryRepository.UpdateAsync(periphery);
        }

        public async Task ExamineEditPeripheryRelatedEntriesAsync(Periphery periphery, PeripheryRelatedEntries examine)
        {
            //更新数据
            await _peripheryService.UpdatePeripheryDataRelatedEntries(periphery, examine);
            //保存
            _ = await _peripheryRepository.UpdateAsync(periphery);

        }

        public async Task ExamineEditPeripheryRelatedPeripheriesAsync(Periphery periphery, PeripheryRelatedPeripheries examine)
        {
            //更新数据
            await _peripheryService.UpdatePeripheryDataRelatedPeripheriesAsync(periphery, examine);
            //保存
            _ = await _peripheryRepository.UpdateAsync(periphery);
        }

        #endregion

        #region 评论
        public async Task ExaminePublishCommentTextAsync(Comment comment, CommentText examine)
        {

            await _commentService.UpdateCommentDataMainAsync(comment, examine);
            _userRepository.Clear();
            //保存
            comment = await _commentRepository.UpdateAsync(comment);

            //获取发表评论的用户
            var user = await _userRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(s => s.Id == examine.PubulicUserId);
            //向归属者发送消息
            Message message = null;
            switch (examine.Type)
            {
                case CommentType.CommentArticle:
                    if (examine.PubulicUserId == comment.Article.CreateUser.Id)
                    {
                        break;
                    }
                    message = new Message
                    {
                        Title = user.UserName,
                        PostTime = DateTime.Now.ToCstTime(),
                        Image = user.PhotoPath,
                        // Rank = "系统",
                        Text = "在你的文章『" + (comment.Article.DisplayName ?? comment.Article.Name) + "』下回复了你『\n" + examine.Text + "\n』",
                        Link = "articles/index/" + comment.Article.Id,
                        LinkTitle = comment.Article.Name,
                        Type = MessageType.ArticleReply,
                        ApplicationUserId = comment.Article.CreateUser.Id,
                        AdditionalInfor = comment.Id.ToString()
                    };
                    break;
                case CommentType.CommentUser:
                    if (user.Id == comment.UserSpaceCommentManager.ApplicationUser.Id)
                    {
                        break;
                    }
                    message = new Message
                    {
                        Title = user.UserName,
                        PostTime = DateTime.Now.ToCstTime(),
                        Image = user.PhotoPath,
                        // Rank = "系统",
                        Text = "在你的空间下留言『\n" + examine.Text + "\n』",
                        Link = "space/index/" + comment.UserSpaceCommentManager.ApplicationUser.Id,
                        LinkTitle = comment.ApplicationUser.UserName,
                        Type = MessageType.SpaceReply,
                        ApplicationUserId = comment.UserSpaceCommentManager.ApplicationUser.Id,
                        AdditionalInfor = comment.Id.ToString()
                    };
                    break;
                case CommentType.ReplyComment:
                    if (user.Id == comment.ParentCodeNavigation.ApplicationUser.Id)
                    {
                        break;
                    }
                    message = new Message
                    {
                        Title = user.UserName,
                        PostTime = DateTime.Now.ToCstTime(),
                        Image = user.PhotoPath,
                        // Rank = "系统",
                        Text = "在主题『" + (comment.Article?.DisplayName??comment.Entry?.DisplayName??comment.Periphery?.DisplayName??comment.Vote?.DisplayName??comment.Lottery?.DisplayName??comment.UserSpaceCommentManager?.ApplicationUser?.UserName) + "』你的评论『" + _appHelper.GetStringAbbreviation(comment.ParentCodeNavigation.Text, 20) + "』下回复了你『\n" + examine.Text + "\n』",
                        Type = MessageType.CommentReply,
                        Link = comment.ArticleId!=null? $"articles/index/{ comment.ArticleId}" :(comment.EntryId != null ? $"entries/index/{comment.EntryId}" : (comment.PeripheryId != null ? $"peripheries/index/{comment.PeripheryId}" : (comment.VoteId != null ? $"votes/index/{comment.VoteId}" : (comment.LotteryId != null ? $"lotteries/index/{comment.LotteryId}":"" )))),
                        ApplicationUserId = comment.ParentCodeNavigation.ApplicationUser.Id,
                        AdditionalInfor = comment.Id.ToString()
                    };
                    break;
            }
            if (message != null)
            {
                await _messageRepository.InsertAsync(message);

            } //缓存评论数
            var tempCount = 0;
            switch (comment.Type)
            {
                case CommentType.CommentArticle:
                    tempCount = await _commentRepository.CountAsync(s => s.ArticleId == comment.ArticleId);
                    await _articleRepository.GetRangeUpdateTable().Where(s => s.Id == comment.ArticleId).Set(s => s.CommentCount, b => tempCount).ExecuteAsync();
                    break;
                case CommentType.CommentEntries:
                    tempCount = await _commentRepository.CountAsync(s => s.EntryId == comment.EntryId);
                    await _entryRepository.GetRangeUpdateTable().Where(s => s.Id == comment.EntryId).Set(s => s.CommentCount, b => tempCount).ExecuteAsync();
                    break;
                case CommentType.CommentPeriphery:
                    tempCount = await _commentRepository.CountAsync(s => s.PeripheryId == comment.PeripheryId);
                    await _peripheryRepository.GetRangeUpdateTable().Where(s => s.Id == comment.PeripheryId).Set(s => s.CommentCount, b => tempCount).ExecuteAsync();
                    break;
                case CommentType.CommentVote:
                    tempCount = await _commentRepository.CountAsync(s => s.VoteId == comment.VoteId);
                    await _voteRepository.GetRangeUpdateTable().Where(s => s.Id == comment.VoteId).Set(s => s.CommentCount, b => tempCount).ExecuteAsync();
                    break;
                case CommentType.CommentLottery:
                    tempCount = await _commentRepository.CountAsync(s => s.LotteryId == comment.LotteryId);
                    await _lotteryRepository.GetRangeUpdateTable().Where(s => s.Id == comment.LotteryId).Set(s => s.CommentCount, b => tempCount).ExecuteAsync();
                    break;
            }
        }
        #endregion

        #region 游玩记录

        public async Task ExamineEditPlayedGameMainAsync(PlayedGame playedGame, PlayedGameMain examine)
        {
            //更新数据
            _playedGameService.UpdatePlayedGameDataMain(playedGame, examine);
            //保存
            _ = await _playedGameRepository.UpdateAsync(playedGame);
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
                _ = await _examineRepository.InsertAsync(examine);
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
                    _ = await _examineRepository.UpdateAsync(examine);
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
                    _ = await _examineRepository.InsertAsync(examine);
                }
            }

            //log
            _logger.LogInformation("{User}({Id})对 词条 - {Entry}({Id}) 进行{Operation}操作{Admin}", user.UserName, user.Id, entry.Name,entry.Id, operation.GetDisplayName(), isAdmin ? "(管理员身份忽略审核)" : "");
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
                _ = await _examineRepository.InsertAsync(examine);

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
                _ = await _examineRepository.InsertAsync(examine);

            }
            //log
            _logger.LogInformation("{User}({Id})创建词条({EntryId})，当前进行编辑{Operation}操作{Admin}", user.UserName, user.Id, entry.Id, operation.GetDisplayName(), isAdmin ? "(管理员身份忽略审核)" : "");

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
                _ = await _examineRepository.InsertAsync(examine);
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
                    _ = await _examineRepository.UpdateAsync(examine);
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
                    _ = await _examineRepository.InsertAsync(examine);
                }
            }
            //log
            _logger.LogInformation("{User}({Id})对 文章 - {Entry}({Id}) 进行{Operation}操作{Admin}", user.UserName, user.Id, article.Name,article.Id, operation.GetDisplayName(), isAdmin ? "(管理员身份忽略审核)" : "");

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
                _ = await _examineRepository.InsertAsync(examine);

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
                _ = await _examineRepository.InsertAsync(examine);

            }

            //log
            _logger.LogInformation("{User}({Id})创建文章({EntryId})，当前进行编辑{Operation}操作{Admin}", user.UserName, user.Id, article.Id, operation.GetDisplayName(), isAdmin ? "(管理员身份忽略审核)" : "");

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
                _ = await _examineRepository.InsertAsync(examine);

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
                _ = await _examineRepository.InsertAsync(examine);

            }
            //log
            _logger.LogInformation("{User}({Id})创建标签({EntryId})，当前进行编辑{Operation}操作{Admin}", user.UserName, user.Id, tag.Id, operation.GetDisplayName(), isAdmin ? "(管理员身份忽略审核)" : "");

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
                _ = await _examineRepository.InsertAsync(examine);
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
                    _ = await _examineRepository.UpdateAsync(examine);
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
                    _ = await _examineRepository.InsertAsync(examine);
                }
            }

            //log
            _logger.LogInformation("{User}({Id})对 标签 - {Entry}({Id}) 进行{Operation}操作{Admin}", user.UserName, user.Id, tag.Name,tag.Id, operation.GetDisplayName(), isAdmin ? "(管理员身份忽略审核)" : "");

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
                _ = await _examineRepository.InsertAsync(examine);

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
                _ = await _examineRepository.InsertAsync(examine);

            }
            //log
            _logger.LogInformation("{User}({Id})创建消歧义页({EntryId})，当前进行编辑{Operation}操作{Admin}", user.UserName, user.Id, disambig.Id, operation.GetDisplayName(), isAdmin ? "(管理员身份忽略审核)" : "");


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
                _ = await _examineRepository.InsertAsync(examine);
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
                    _ = await _examineRepository.UpdateAsync(examine);
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
                    _ = await _examineRepository.InsertAsync(examine);
                }
            }

            //log
            _logger.LogInformation("{User}({Id})对 消歧义页 - {Entry}({Id}) 进行{Operation}操作{Admin}", user.UserName, user.Id, disambig.Name,disambig.Id, operation.GetDisplayName(), isAdmin ? "(管理员身份忽略审核)" : "");

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
                _ = await _examineRepository.InsertAsync(examine);
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
                    _ = await _examineRepository.UpdateAsync(examine);
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
                    _ = await _examineRepository.InsertAsync(examine);
                }
            }

            //log
            _logger.LogInformation("{User}({Id})对个人资料进行{Operation}操作{Admin}", user.UserName, user.Id,operation.GetDisplayName(), isAdmin ? "(管理员身份忽略审核)" : "");

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
                _ = await _examineRepository.InsertAsync(examine);

            }
            else
            {
                //根据文章Id查找前置审核
                long? examineId = null;
                if (operation != Operation.EditPeripheryMain)
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
                _ = await _examineRepository.InsertAsync(examine);

            }
            //log
            _logger.LogInformation("{User}({Id})创建周边({EntryId})，当前进行编辑{Operation}操作{Admin}", user.UserName, user.Id, periphery.Id, operation.GetDisplayName(), isAdmin ? "(管理员身份忽略审核)" : "");

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
                _ = await _examineRepository.InsertAsync(examine);
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
                    _ = await _examineRepository.UpdateAsync(examine);
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
                    _ = await _examineRepository.InsertAsync(examine);
                }
            }


            //log
            _logger.LogInformation("{User}({Id})对 周边 - {Entry}({Id}) 进行{Operation}操作{Admin}", user.UserName, user.Id, periphery.Name,periphery.Id, operation.GetDisplayName(), isAdmin ? "(管理员身份忽略审核)" : "");

        }

        public async Task UniversalCommentExaminedAsync(Comment comment, ApplicationUser user, bool isAdmin, string examineStr, Operation operation, string note)
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
                    CommentId = comment.Id,
                    PassedTime = DateTime.Now.ToCstTime(),
                    ApplyTime = DateTime.Now.ToCstTime(),
                    ApplicationUserId = user.Id,
                    Note = note
                };
                await _examineRepository.InsertAsync(examine);
            }
            else
            {
                var examine = new Examine
                {
                    Operation = operation,
                    Context = examineStr,
                    IsPassed = null,
                    PassedTime = null,
                    CommentId = comment.Id,
                    ApplyTime = DateTime.Now.ToCstTime(),
                    ApplicationUserId = user.Id,
                    Note = note
                };
                //添加到审核列表
                await _examineRepository.InsertAsync(examine);
            }
        }

        public async Task UniversalEditPlayedGameExaminedAsync(PlayedGame playedGame, ApplicationUser user, bool isAdmin, string examineStr, Operation operation, string note)
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
                    PlayedGameId = playedGame.Id,
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
                var examine = await GetUserPlayedGameActiveExamineAsync(playedGame.Id, user.Id, operation);
                if (examine != null)
                {
                    examine.Context = examineStr;
                    examine.ApplyTime = DateTime.Now.ToCstTime();
                    _ = await _examineRepository.UpdateAsync(examine);
                }
                else
                {
                    examine = new Examine
                    {
                        Operation = operation,
                        Context = examineStr,
                        IsPassed = null,
                        PassedTime = null,
                        PlayedGameId = playedGame.Id,
                        ApplyTime = DateTime.Now.ToCstTime(),
                        ApplicationUserId = user.Id,
                        Note = note
                    };
                    //添加到审核列表
                    _ = await _examineRepository.InsertAsync(examine);
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
                    using TextWriter text = new StringWriter();
                    var serializer = new JsonSerializer();
                    serializer.Serialize(text, item.Key);
                    resulte = text.ToString();
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

                    _ = await UniversalEstablishExaminedAsync(entry, user, true, resulte, item.Value, note);
                    await _appHelper.AddUserContributionValueAsync(user.Id, entry.Id, item.Value);
                }
                else
                {
                    _ = await UniversalEstablishExaminedAsync(entry, user, false, resulte, item.Value, note);
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
                    resulte = item.Key as string;
                }
                else
                {
                    using TextWriter text = new StringWriter();
                    var serializer = new JsonSerializer();
                    serializer.Serialize(text, item.Key);
                    resulte = text.ToString();
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

                    _ = await UniversalCreateArticleExaminedAsync(article, user, true, resulte, item.Value, note);
                    await _appHelper.AddUserContributionValueAsync(user.Id, article.Id, item.Value);
                }
                else
                {
                    _ = await UniversalCreateArticleExaminedAsync(article, user, false, resulte, item.Value, note);
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

                    _ = await UniversalCreatePeripheryExaminedAsync(periphery, user, true, resulte, item.Value, note);
                    await _appHelper.AddUserContributionValueAsync(user.Id, periphery.Id, item.Value);
                }
                else
                {
                    _ = await UniversalCreatePeripheryExaminedAsync(periphery, user, false, resulte, item.Value, note);
                }
            }

            return periphery;
        }

        public async Task<Tag> AddNewTagExaminesAsync(Tag model, ApplicationUser user, string note)
        {
            var examines = _tagService.ExaminesCompletion(new Tag(), model);

            if (examines.Any(s => s.Value == Operation.EditTagMain) == false)
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

                    _ = await UniversalCreateTagExaminedAsync(tag, user, true, resulte, item.Value, note);
                    await _appHelper.AddUserContributionValueAsync(user.Id, tag.Id, item.Value);
                }
                else
                {
                    _ = await UniversalCreateTagExaminedAsync(tag, user, false, resulte, item.Value, note);
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
        public async Task<Examine> GetUserPlayedGameActiveExamineAsync(long peripheryId, string userId, Operation operation)
        {
            return await _examineRepository.FirstOrDefaultAsync(s => s.PlayedGameId == peripheryId && s.ApplicationUserId == userId && s.Operation == operation && s.IsPassed == null);
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
        /// 迁移标签审核数据 EditTag模型部分
        /// </summary>
        /// <returns></returns>
        public async Task ReplaceEditTag_1_0_ExamineContext()
        {
            TagEdit oldExamineModel = null;

            //获取要替换的所有审核记录ID
            var ids = await _examineRepository.GetAll().AsNoTracking().Where(s => s.Operation == Operation.EditTag && s.Version == ExamineVersion.V1_0).Select(s => s.Id).ToListAsync();

            //遍历列表 依次替换
            foreach (var id in ids)
            {
                var examine = await _examineRepository.FirstOrDefaultAsync(s => s.Id == id);
                if (examine != null)
                {

                    var newExamineModel = new ExamineMain();
                    using (TextReader str = new StringReader(examine.Context))
                    {
                        var serializer = new JsonSerializer();
                        oldExamineModel = (TagEdit)serializer.Deserialize(str, typeof(TagEdit));
                    }
                    newExamineModel.Items.Add(new ExamineMainAlone
                    {
                        Key = "Name",
                        Value = oldExamineModel.Name
                    });
                    examine.Operation = Operation.EditTagMain;
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
                        examine.Note += "\n" + DateTime.Now.ToCstTime().ToString("yyyy年MM月dd日 HH:mm") + " 迁移标签旧综合编辑记录";
                    }
                    examine.Version = ExamineVersion.V1_1;
                    examine.Context = resulte;
                    _ = await _examineRepository.UpdateAsync(examine);
                }
            }
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
                        examine.Note += "\n" + DateTime.Now.ToCstTime().ToString("yyyy年MM月dd日 HH:mm") + " 迁移标签主要信息编辑记录";
                    }
                    examine.Version = ExamineVersion.V1_1;
                    examine.Context = resulte;
                    _ = await _examineRepository.UpdateAsync(examine);
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
                        examine.Note += "\n" + DateTime.Now.ToCstTime().ToString("yyyy年MM月dd日 HH:mm") + " 迁移周边主要信息编辑记录";
                    }
                    examine.Version = ExamineVersion.V1_1;
                    examine.Context = resulte;
                    _ = await _examineRepository.UpdateAsync(examine);
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
                _ = await _peripheryRepository.UpdateAsync(item);
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
                _ = await _entryRepository.UpdateAsync(item);
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
                        examine.Note += "\n" + DateTime.Now.ToCstTime().ToString("yyyy年MM月dd日 HH:mm") + " 迁移文章主要信息编辑记录";
                    }
                    examine.Version = ExamineVersion.V1_1;
                    examine.Context = resulte;
                    _ = await _examineRepository.UpdateAsync(examine);
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
                        .Where(s => s.IsPassed == true && s.Id < examine.Id && s.EntryId == examine.EntryId && s.Operation == Operation.EstablishMain).ToListAsync();
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
                        _ = newExamineModel.Items.RemoveAll(s => s.Key == "PubulishTime");

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
                        _ = newExamineModel.Items.RemoveAll(s => s.Key == "PubulishTime");

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
                    _ = await _examineRepository.UpdateAsync(examine);
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
                        if (item.Modifier is "词条" or "游戏" or "制作组" or "STAFF" or "角色")
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
                        else if (item.Modifier is "文章" or "动态")
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
                        examine.Note += "\n" + DateTime.Now.ToCstTime().ToString("yyyy年MM月dd日 HH:mm") + " 迁移文章关联信息编辑记录";
                    }
                    examine.Version = ExamineVersion.V1_1;
                    examine.Context = resulte;
                    _ = await _examineRepository.UpdateAsync(examine);
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
                    if (temp.Modifier is "词条" or "游戏" or "制作组" or "STAFF" or "角色")
                    {
                        var newEntry = await _entryRepository.FirstOrDefaultAsync(s => s.Name == temp.DisplayName);
                        if (newEntry != null)
                        {
                            item.Entries.Add(newEntry);

                        }
                    }
                    else if (temp.Modifier is "文章" or "动态")
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
                _ = await _articleRepository.UpdateAsync(item);
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
                        if (item.Modifier is "词条" or "游戏" or "制作组" or "STAFF" or "角色")
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
                        else if (item.Modifier is "文章" or "动态")
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
                        examine.Note += "\n" + DateTime.Now.ToCstTime().ToString("yyyy年MM月dd日 HH:mm") + " 迁移词条关联信息编辑记录";
                    }
                    examine.Version = ExamineVersion.V1_1;
                    examine.Context = resulte;
                    _ = await _examineRepository.UpdateAsync(examine);
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
                    if (temp.Modifier is "词条" or "游戏" or "制作组" or "STAFF" or "角色")
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
                    else if (temp.Modifier is "文章" or "动态")
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
                _ = await _entryRepository.UpdateAsync(item);
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
                    _ = await _examineRepository.UpdateAsync(examine);
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
                 .Include(s => s.PeripheryRelationFromPeripheryNavigation).ThenInclude(s => s.ToPeripheryNavigation)
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
                    using TextWriter text = new StringWriter();
                    var serializer = new JsonSerializer();
                    serializer.Serialize(text, item.Key);
                    resulte = text.ToString();
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
                    using TextWriter text = new StringWriter();
                    var serializer = new JsonSerializer();
                    serializer.Serialize(text, item.Key);
                    resulte = text.ToString();
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
                return exammines.Any() ? await GenerateModelFromExamines(exammines) : throw new Exception("未找到该词条");

            }
            else if (model is Article)
            {
                var article = model as Article;
                var exammines = await _examineRepository.GetAll().AsNoTracking().Where(s => s.ArticleId == article.Id && s.IsPassed == true).OrderBy(s => s.Id).ToListAsync();
                return exammines.Any() ? await GenerateModelFromExamines(exammines) : throw new Exception("未找到该文章");

            }
            else if (model is Tag)
            {
                var tag = model as Tag;
                var exammines = await _examineRepository.GetAll().AsNoTracking().Where(s => s.TagId == tag.Id && s.IsPassed == true).OrderBy(s => s.Id).ToListAsync();
                return exammines.Any()
                    ? await GenerateModelFromExamines(exammines)
                    : new Tag
                    {
                        Id = tag.Id
                    };

            }
            else if (model is Periphery)
            {
                var periphery = model as Periphery;
                var exammines = await _examineRepository.GetAll().AsNoTracking().Where(s => s.PeripheryId == periphery.Id && s.IsPassed == true).OrderBy(s => s.Id).ToListAsync();
                return exammines.Any() ? await GenerateModelFromExamines(exammines) : throw new Exception("未找到该周边");

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
