using CnGalWebSite.APIServer.Application.Articles;
using CnGalWebSite.APIServer.Application.Entries;
using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.Application.OperationRecords;
using CnGalWebSite.APIServer.Application.Peripheries;
using CnGalWebSite.APIServer.Application.Ranks;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel;
using CnGalWebSite.DataModel.ViewModel.Votes;
using Markdig;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Result = CnGalWebSite.DataModel.Model.Result;

namespace CnGalWebSite.APIServer.Controllers
{

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("api/votes/[action]")]
    public class VoteAPIController : ControllerBase
    {
        private readonly IRepository<Entry, int> _entryRepository;
        private readonly IRepository<Article, long> _articleRepository;
        private readonly IRepository<ApplicationUser, string> _userRepository;
        private readonly IRepository<Periphery, long> _peripheryRepository;
        private readonly IAppHelper _appHelper;
        private readonly IEntryService _entryService;
        private readonly IArticleService _articleService;
        private readonly IRankService _rankService;
        private readonly IPeripheryService _peripheryService;
        private readonly IRepository<Vote, long> _voteRepository;
        private readonly IRepository<VoteOption, long> _voteOptionRepository;
        private readonly IRepository<VoteUser, long> _voteUserRepository;
        private readonly ILogger<VoteAPIController> _logger;
        private readonly IOperationRecordService _operationRecordService;

        public VoteAPIController(IRepository<Vote, long> voteRepository, IRepository<VoteOption, long> voteOptionRepository, IRepository<VoteUser, long> voteUserRepository, IOperationRecordService operationRecordService,
        IRepository<ApplicationUser, string> userRepository, IEntryService entryService, IArticleService articleService, IRankService rankService, ILogger<VoteAPIController> logger,
        IRepository<Article, long> articleRepository, IAppHelper appHelper, IRepository<Entry, int> entryRepository, IRepository<Periphery, long> peripheryRepository, IPeripheryService peripheryService)
        {
            _entryRepository = entryRepository;
            _appHelper = appHelper;
            _articleRepository = articleRepository;
            _entryService = entryService;
            _articleService = articleService;
            _userRepository = userRepository;
            _rankService = rankService;
            _peripheryRepository = peripheryRepository;
            _peripheryService = peripheryService;
            _voteRepository = voteRepository;
            _voteOptionRepository = voteOptionRepository;
            _voteUserRepository = voteUserRepository;
            _logger = logger;
            _operationRecordService = operationRecordService;
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<VoteViewModel>> GetVoteViewAsync(long id)
        {
            var vote = await _voteRepository.GetAll().Where(s => s.IsHidden == false && string.IsNullOrWhiteSpace(s.Name) == false)
                .Include(s => s.Articles).ThenInclude(s => s.CreateUser)
                .Include(s => s.Entries).ThenInclude(s => s.EntryRelationFromEntryNavigation).ThenInclude(s => s.ToEntryNavigation)
                .Include(s => s.Entries).ThenInclude(s => s.EntryStaffFromEntryNavigation).ThenInclude(s => s.ToEntryNavigation)
                .Include(s => s.Entries).ThenInclude(s => s.EntryRelationFromEntryNavigation).ThenInclude(s => s.ToEntryNavigation).ThenInclude(s => s.EntryStaffFromEntryNavigation).ThenInclude(s => s.ToEntryNavigation)
                .Include(s => s.Entries).ThenInclude(s => s.Information).ThenInclude(s => s.Additional)
                .Include(s => s.Peripheries)
               .Include(s => s.VoteUsers).ThenInclude(s => s.SeletedOptions)
               .Include(s => s.VoteOptions)
               .ThenInclude(s => s.VoteUsers).ThenInclude(s => s.SeletedOptions)
               .Include(s => s.VoteOptions).ThenInclude(s => s.Entry)
               .Include(s => s.VoteOptions).ThenInclude(s => s.Article)
               .Include(s => s.VoteOptions).ThenInclude(s => s.Periphery)
               .FirstOrDefaultAsync(s => s.Id == id);
            if (vote == null)
            {
                return NotFound("未找到目标投票");
            }

            var model = new VoteViewModel
            {
                SmallBackgroundPicture = vote.SmallBackgroundPicture,
                EndTime = vote.EndTime,
                LastEditTime = vote.LastEditTime,
                BackgroundPicture = vote.BackgroundPicture,
                BeginTime = vote.BeginTime,
                BriefIntroduction = vote.BriefIntroduction,
                CanComment = vote.CanComment ?? true,
                CommentCount = vote.CommentCount,
                Count = vote.VoteUsers.Count,
                CreateTime = vote.CreateTime,
                DisplayName = vote.DisplayName,
                IsAllowModification = vote.IsAllowModification,
                IsHidden = vote.IsHidden,
                MainPage = vote.MainPage,
                MainPicture = vote.MainPicture,
                Name = vote.Name,
                Id = vote.Id,
                ReaderCount = vote.ReaderCount,
                Thumbnail = vote.Thumbnail,
                Type = vote.Type,
                MaximumSelectionCount = vote.MaximumSelectionCount,
                MinimumSelectionCount = vote.MinimumSelectionCount,
            };
            //核对选项数
            if (model.Type == VoteType.SingleChoice)
            {
                model.MaximumSelectionCount = 1;
                model.MinimumSelectionCount = 1;
            }
            //初始化主页Html代码
            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().UseSoftlineBreakAsHardlineBreak().Build();
            model.MainPage = Markdown.ToHtml(model.MainPage ?? "", pipeline);

            foreach (var item in vote.Entries)
            {
                model.Entries.Add( _appHelper.GetEntryInforTipViewModel(item));
            }
            foreach (var item in vote.Articles)
            {
                model.Artciles.Add(_appHelper.GetArticleInforTipViewModel(item));
            }
            foreach (var item in vote.Peripheries)
            {
                model.Peripheries.Add(_appHelper.GetPeripheryInforTipViewModel(item));
            }

            foreach (var item in vote.VoteOptions)
            {
                var temp = new VoteOptionViewModel
                {
                    Count = item.VoteUsers.Count,
                    OptionId = item.Id
                };

                if (item.Article != null)
                {
                    temp.Image = item.Article.MainPicture;
                    temp.Name = item.Article.DisplayName;
                    temp.ObjectId = item.Article.Id;
                    temp.Type = VoteOptionType.Article;
                }
                else if (item.Entry != null)
                {
                    if (item.Entry.Type == EntryType.Game || item.Entry.Type == EntryType.ProductionGroup)
                    {
                        temp.Image = item.Entry.MainPicture;
                    }
                    else
                    {
                        temp.Image = item.Entry.Thumbnail;

                    }

                    temp.Name = item.Entry.DisplayName;
                    temp.ObjectId = item.Entry.Id;
                    temp.Type = VoteOptionType.Entry;
                }
                else if (item.Periphery != null)
                {
                    temp.Image = item.Periphery.MainPicture;
                    temp.Name = item.Periphery.DisplayName;
                    temp.ObjectId = item.Periphery.Id;
                    temp.Type = VoteOptionType.Periphery;
                }
                else
                {
                    temp.Name = item.Text;
                    temp.Type = VoteOptionType.Text;
                }
                model.Options.Add(temp);
            }
            //对选项进行排序
            model.Options = model.Options.OrderBy(s => s.Name).ToList();

            if (vote.VoteUsers.Count > 0 && vote.VoteUsers.Where(s => s.IsAnonymous == false).Any())
            {
                var userIds = vote.VoteUsers.Where(s => s.IsAnonymous == false).Select(s => s.ApplicationUserId);
                var users = await _userRepository.GetAll().Where(s => userIds.Contains(s.Id)).AsNoTracking().ToListAsync();
                foreach (var item in users)
                {
                    model.Users.Add(new VoteUserViewModel
                    {
                        PersonalSignature = item.PersonalSignature,
                        Ranks = await _rankService.GetUserRanks(item),
                        UserId = item.Id,
                        UserName = item.UserName,
                        Image = _appHelper.GetImagePath(item.PhotoPath, "user.png"),
                        VotedTime = vote.VoteUsers.FirstOrDefault(s => s.ApplicationUserId == item.Id)?.VotedTime ?? DateTime.MinValue,
                    });
                }
                model.Users = model.Users.OrderByDescending(s => s.VotedTime).Take(20).ToList();
            }


            //获取用户选择
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            if (user != null)
            {
                model.UserSelections = vote.VoteUsers.FirstOrDefault(s => s.ApplicationUserId == user.Id)?.SeletedOptions.Select(s => s.Id).ToList();
                if (model.UserSelections == null)
                {
                    model.UserSelections = new List<long>();
                }

            }
            //计算是否展示结果
            if (model.UserSelections.Count > 0 || DateTime.Now.ToCstTime() > model.EndTime)
            {
                model.ShowResult = true;
            }

            //增加阅读人数
            await _voteRepository.GetRangeUpdateTable().Where(s => s.Id == id).Set(s => s.ReaderCount, b => b.ReaderCount + 1).ExecuteAsync();

            return model;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<List<VoteCardViewModel>>> GetVoteCardsAsync()
        {
            var votes = await _voteRepository.GetAll().Include(s => s.VoteUsers).ThenInclude(s => s.ApplicationUser).AsNoTracking()
                .Where(s => s.IsHidden == false && string.IsNullOrWhiteSpace(s.Name) == false).ToListAsync();

            var model = new List<VoteCardViewModel>();

            foreach (var item in votes)
            {
                var temp = new VoteCardViewModel
                {
                    BeginTime = item.BeginTime,
                    EndTime = item.EndTime,
                    BriefIntroduction = item.BriefIntroduction,
                    Count = item.VoteUsers.Count,
                    MainPicture = _appHelper.GetImagePath(item.MainPicture, "app.png"),
                    Id = item.Id,
                    Name = item.Name,
                };
                if (item.VoteUsers.Count > 0 && item.VoteUsers.Any(s => s.IsAnonymous == false))
                {
                    foreach (var infor in item.VoteUsers.Where(s => s.IsAnonymous == false).OrderByDescending(s => s.VotedTime).Take(6))
                    {
                        temp.Users.Add(new VoteUserMinViewModel
                        {
                            Image = _appHelper.GetImagePath(infor.ApplicationUser.PhotoPath, "user.png"),
                            UserId = infor.ApplicationUserId,
                            UserName = infor.ApplicationUser.UserName
                        });
                    }
                }

                model.Add(temp);
            }

            return model;
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Result>> CreateVoteAsync(CreateVoteModel model)
        {
            if (await _voteRepository.GetAll().AnyAsync(s => s.Name == model.Name))
            {
                return new Result { Successful = false, Error = "已存在该名称的投票" };
            }
            //检查选项数是否合法
            if (model.Options.Count < 2)
            {
                return new Result { Successful = false, Error = "至少需要两个选项" };
            }
            if (model.Type == VoteType.MultipleChoice)
            {
                if (model.MinimumSelectionCount < 1)
                {
                    return new Result { Successful = false, Error = "只能最少同时选中项目数必须大于0" };
                }
                if (model.MaximumSelectionCount > model.Options.Count)
                {
                    return new Result { Successful = false, Error = "只能最多同时选中项目数必须小于等于选项数" };
                }
                if (model.MinimumSelectionCount > model.MaximumSelectionCount)
                {
                    return new Result { Successful = false, Error = "只能最少同时选中项目数必须小于等于最多同时选中项目数" };
                }
            }
            //检查时间
            if (model.BeginTime > model.EndTime)
            {
                return new Result { Successful = false, Error = "开始时间必须早于结束时间" };
            }
            //核对选项数
            if (model.Type == VoteType.SingleChoice)
            {
                model.MaximumSelectionCount = 1;
                model.MinimumSelectionCount = 1;
            }
            var vote = new Vote
            {
                Name = model.Name,
                DisplayName = model.DisplayName,
                BackgroundPicture = model.BackgroundPicture,
                EndTime = model.EndTime,
                LastEditTime = DateTime.Now.ToCstTime(),
                CreateTime = DateTime.Now.ToCstTime(),
                BeginTime = model.BeginTime,
                BriefIntroduction = model.BriefIntroduction,
                MainPage = model.MainPage,
                MainPicture = model.MainPicture,
                Type = model.Type,
                SmallBackgroundPicture = model.SmallBackgroundPicture,
                Thumbnail = model.Thumbnail,
                MinimumSelectionCount = model.MinimumSelectionCount,
                MaximumSelectionCount = model.MaximumSelectionCount,
            };

            //预处理 建立词条关联信息
            //判断关联是否存在
            var entryIds = new List<int>();
            var entryNames = new List<string>();

            var peripheryIds = new List<long>();
            var peripheryNames = new List<string>();

            var articleIds = new List<long>();
            var articleNames = new List<string>();

            entryNames.AddRange(model.Games.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));
            entryNames.AddRange(model.Groups.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));
            entryNames.AddRange(model.Staffs.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));
            entryNames.AddRange(model.Roles.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));
            entryNames.AddRange(model.Options.Where(s => s.Type == VoteOptionType.Entry && string.IsNullOrWhiteSpace(s.Text) == false).Select(s => s.Text));

            //建立周边关联信息

            peripheryNames.AddRange(model.Peripheries.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));
            peripheryNames.AddRange(model.Options.Where(s => s.Type == VoteOptionType.Periphery && string.IsNullOrWhiteSpace(s.Text) == false).Select(s => s.Text));

            //建立文章关联信息

            articleNames.AddRange(model.Articles.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));
            articleNames.AddRange(model.Options.Where(s => s.Type == VoteOptionType.Article && string.IsNullOrWhiteSpace(s.Text) == false).Select(s => s.Text));

            try
            {
                entryIds = await _entryService.GetEntryIdsFromNames(entryNames);
                peripheryIds = await _peripheryService.GetPeripheryIdsFromNames(peripheryNames);
                articleIds = await _articleService.GetArticleIdsFromNames(articleNames);
            }
            catch (Exception ex)
            {
                return new Result { Successful = false, Error = ex.Message };
            }

            //添加关联项目
            vote.Articles = await _articleRepository.GetAll().Where(s => articleIds.Contains(s.Id)).ToListAsync();
            vote.Entries = await _entryRepository.GetAll().Where(s => entryIds.Contains(s.Id)).ToListAsync();
            vote.Peripheries = await _peripheryRepository.GetAll().Where(s => peripheryIds.Contains(s.Id)).ToListAsync();

            //解析选项
            //重新获取Id
            entryNames.Clear();
            articleNames.Clear();
            peripheryNames.Clear();

            entryNames.AddRange(model.Options.Where(s => s.Type == VoteOptionType.Entry && string.IsNullOrWhiteSpace(s.Text) == false).Select(s => s.Text));
            peripheryNames.AddRange(model.Options.Where(s => s.Type == VoteOptionType.Periphery && string.IsNullOrWhiteSpace(s.Text) == false).Select(s => s.Text));
            articleNames.AddRange(model.Options.Where(s => s.Type == VoteOptionType.Article && string.IsNullOrWhiteSpace(s.Text) == false).Select(s => s.Text));

            try
            {
                entryIds = await _entryService.GetEntryIdsFromNames(entryNames);
                peripheryIds = await _peripheryService.GetPeripheryIdsFromNames(peripheryNames);
                articleIds = await _articleService.GetArticleIdsFromNames(articleNames);
            }
            catch (Exception ex)
            {
                return new Result { Successful = false, Error = ex.Message };
            }

            foreach (var item in entryIds)
            {
                vote.VoteOptions.Add(new VoteOption
                {
                    EntryId = item,
                    Type = VoteOptionType.Entry
                });
            }
            foreach (var item in articleIds)
            {
                vote.VoteOptions.Add(new VoteOption
                {
                    ArticleId = item,
                    Type = VoteOptionType.Article
                });
            }
            foreach (var item in peripheryIds)
            {
                vote.VoteOptions.Add(new VoteOption
                {
                    PeripheryId = item,
                    Type = VoteOptionType.Periphery
                });
            }
            foreach (var item in model.Options.Where(s => s.Type == VoteOptionType.Text))
            {
                vote.VoteOptions.Add(new VoteOption
                {
                    Text = item.Text,
                    Type = VoteOptionType.Text
                });
            }


            vote = await _voteRepository.InsertAsync(vote);

            return new Result { Successful = true, Error = vote.Id.ToString() };
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<ActionResult<EditVoteModel>> EditVoteAsync(long id)
        {
            var vote = await _voteRepository.GetAll().AsNoTracking()
               .Include(s => s.Entries).Include(s => s.Articles).Include(s => s.Peripheries)
               .Include(s => s.VoteOptions).ThenInclude(s => s.Entry)
               .Include(s => s.VoteOptions).ThenInclude(s => s.Article)
               .Include(s => s.VoteOptions).ThenInclude(s => s.Periphery)
               .FirstOrDefaultAsync(s => s.Id == id);
            if (vote == null)
            {
                return NotFound("未找到目标投票");
            }


            var model = new EditVoteModel
            {
                Id = id,
                BriefIntroduction = vote.BriefIntroduction,
                EndTime = vote.EndTime,
                MainPicture = _appHelper.GetImagePath(vote.MainPicture, ""),
                BackgroundPicture = _appHelper.GetImagePath(vote.BackgroundPicture, ""),
                SmallBackgroundPicture = _appHelper.GetImagePath(vote.SmallBackgroundPicture, ""),
                Thumbnail = _appHelper.GetImagePath(vote.Thumbnail, ""),
                BeginTime = vote.BeginTime,
                MainPage = vote.MainPage,
                Name = vote.Name,
                Type = vote.Type,
                DisplayName = vote.DisplayName,
                IsAllowModification = vote.IsAllowModification,
                MinimumSelectionCount = vote.MinimumSelectionCount,
                MaximumSelectionCount = vote.MaximumSelectionCount,
            };

            //处理关联信息

            foreach (var item in vote.Entries)
            {
                switch (item.Type)
                {
                    case EntryType.Game:
                        model.Games.Add(new RelevancesModel
                        {
                            DisplayName = item.Name
                        });
                        break;
                    case EntryType.Staff:
                        model.Staffs.Add(new RelevancesModel
                        {
                            DisplayName = item.Name
                        });
                        break;
                    case EntryType.ProductionGroup:
                        model.Groups.Add(new RelevancesModel
                        {
                            DisplayName = item.Name
                        });
                        break;
                    case EntryType.Role:
                        model.Roles.Add(new RelevancesModel
                        {
                            DisplayName = item.Name
                        });
                        break;

                }
            }

            foreach (var item in vote.Articles)
            {
                model.Articles.Add(new RelevancesModel
                {
                    DisplayName = item.Name
                });
            }
            foreach (var item in vote.Peripheries)
            {
                model.Peripheries.Add(new RelevancesModel
                {
                    DisplayName = item.Name
                });
            }

            //处理选项
            foreach (var item in vote.VoteOptions)
            {
                var temp = new EditVoteOptionModel
                {
                    RealId = item.Id,
                    TempId = item.Id,
                    Type = item.Type,
                };

                switch (item.Type)
                {
                    case VoteOptionType.Text:
                        temp.Text = item.Text;
                        break;
                    case VoteOptionType.Entry:
                        temp.Text = item.Entry.Name;
                        break;
                    case VoteOptionType.Article:
                        temp.Text = item.Article.Name;
                        break;
                    case VoteOptionType.Periphery:
                        temp.Text = item.Periphery.Name;
                        break;
                }
                model.Options.Add(temp);
            }

            return model;
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Result>> EditVoteAsync(EditVoteModel model)
        {
            var vote = await _voteRepository.GetAll()
                .Include(s => s.VoteUsers).ThenInclude(s => s.SeletedOptions)
                .Include(s => s.Entries).Include(s => s.Articles).Include(s => s.Peripheries).Include(s => s.VoteOptions)
                .ThenInclude(s => s.VoteUsers).ThenInclude(s => s.SeletedOptions)
                .FirstOrDefaultAsync(s => s.Id == model.Id);
            if (vote == null)
            {
                return new Result { Successful = false, Error = "未找到目标投票" };
            }
            //检查选项数是否合法
            if (model.Options.Count < 2)
            {
                return new Result { Successful = false, Error = "至少需要两个选项" };
            }
            if (model.Type == VoteType.MultipleChoice)
            {
                if (model.MinimumSelectionCount < 1)
                {
                    return new Result { Successful = false, Error = "只能最少同时选中项目数必须大于0" };
                }
                if (model.MaximumSelectionCount > model.Options.Count)
                {
                    return new Result { Successful = false, Error = "只能最多同时选中项目数必须小于等于选项数" };
                }
                if (model.MinimumSelectionCount > model.MaximumSelectionCount)
                {
                    return new Result { Successful = false, Error = "只能最少同时选中项目数必须小于等于最多同时选中项目数" };
                }
            }
            //核对选项数
            if (model.Type == VoteType.SingleChoice)
            {
                model.MaximumSelectionCount = 1;
                model.MinimumSelectionCount = 1;
            }
            //检查时间
            if (model.BeginTime > model.EndTime)
            {
                return new Result { Successful = false, Error = "开始时间必须早于结束时间" };
            }

            vote.Name = model.Name;
            vote.DisplayName = model.DisplayName;
            vote.BackgroundPicture = model.BackgroundPicture;
            vote.EndTime = model.EndTime;
            vote.LastEditTime = DateTime.Now.ToCstTime();
            vote.BeginTime = model.BeginTime;
            vote.BriefIntroduction = model.BriefIntroduction;
            vote.MainPage = model.MainPage;
            vote.MainPicture = model.MainPicture;
            vote.Type = model.Type;
            vote.SmallBackgroundPicture = model.SmallBackgroundPicture;
            vote.Thumbnail = model.Thumbnail;
            vote.IsAllowModification = model.IsAllowModification;
            vote.MinimumSelectionCount = model.MinimumSelectionCount;
            vote.MaximumSelectionCount = model.MaximumSelectionCount;

            //预处理 建立词条关联信息
            //判断关联是否存在
            var entryIds = new List<int>();
            var entryNames = new List<string>();

            var peripheryIds = new List<long>();
            var peripheryNames = new List<string>();

            var articleIds = new List<long>();
            var articleNames = new List<string>();

            entryNames.AddRange(model.Games.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));
            entryNames.AddRange(model.Groups.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));
            entryNames.AddRange(model.Staffs.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));
            entryNames.AddRange(model.Roles.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));
            entryNames.AddRange(model.Options.Where(s => s.Type == VoteOptionType.Entry && string.IsNullOrWhiteSpace(s.Text) == false).Select(s => s.Text));

            //建立周边关联信息

            peripheryNames.AddRange(model.Peripheries.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));
            peripheryNames.AddRange(model.Options.Where(s => s.Type == VoteOptionType.Periphery && string.IsNullOrWhiteSpace(s.Text) == false).Select(s => s.Text));

            //建立文章关联信息

            articleNames.AddRange(model.Articles.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));
            articleNames.AddRange(model.Options.Where(s => s.Type == VoteOptionType.Article && string.IsNullOrWhiteSpace(s.Text) == false).Select(s => s.Text));

            try
            {
                entryIds = await _entryService.GetEntryIdsFromNames(entryNames);
                peripheryIds = await _peripheryService.GetPeripheryIdsFromNames(peripheryNames);
                articleIds = await _articleService.GetArticleIdsFromNames(articleNames);
            }
            catch (Exception ex)
            {
                return new Result { Successful = false, Error = ex.Message };
            }

            //添加关联项目
            vote.Articles = await _articleRepository.GetAll().Where(s => articleIds.Contains(s.Id)).ToListAsync();
            vote.Entries = await _entryRepository.GetAll().Where(s => entryIds.Contains(s.Id)).ToListAsync();
            vote.Peripheries = await _peripheryRepository.GetAll().Where(s => peripheryIds.Contains(s.Id)).ToListAsync();

            //解析选项
            //重新获取Id
            entryNames.Clear();
            articleNames.Clear();
            peripheryNames.Clear();

            entryNames.AddRange(model.Options.Where(s => s.Type == VoteOptionType.Entry && string.IsNullOrWhiteSpace(s.Text) == false).Select(s => s.Text));
            peripheryNames.AddRange(model.Options.Where(s => s.Type == VoteOptionType.Periphery && string.IsNullOrWhiteSpace(s.Text) == false).Select(s => s.Text));
            articleNames.AddRange(model.Options.Where(s => s.Type == VoteOptionType.Article && string.IsNullOrWhiteSpace(s.Text) == false).Select(s => s.Text));

            try
            {
                entryIds = await _entryService.GetEntryIdsFromNames(entryNames);
                peripheryIds = await _peripheryService.GetPeripheryIdsFromNames(peripheryNames);
                articleIds = await _articleService.GetArticleIdsFromNames(articleNames);
            }
            catch (Exception ex)
            {
                return new Result { Successful = false, Error = ex.Message };
            }

            //删除在新选项中没有的旧项目
            var deleteOptions = new List<VoteOption>();
            foreach (var item in vote.VoteOptions.Where(s => s.Type != VoteOptionType.Text))
            {
                var isExist = false;
                if (item.EntryId != null && entryIds.Contains(item.EntryId.Value))
                {
                    isExist = true;
                }
                if (item.ArticleId != null && articleIds.Contains(item.ArticleId.Value))
                {
                    isExist = true;
                }
                if (item.PeripheryId != null && peripheryIds.Contains(item.PeripheryId.Value))
                {
                    isExist = true;
                }

                if (isExist == false)
                {
                    deleteOptions.Add(item);
                }
            }

            //删除不存在的文本
            var newTextId = model.Options.Where(s => s.Type == VoteOptionType.Text && s.RealId != 0).Select(s => s.RealId);
            foreach (var item in vote.VoteOptions.Where(s => s.Type == VoteOptionType.Text))
            {
                if (newTextId.Contains(item.Id) == false)
                {
                    deleteOptions.Add(item);
                }
            }

            //修改每个用户的选项
            foreach (var item in deleteOptions)
            {
                foreach (var temp in item.VoteUsers)
                {
                    temp.SeletedOptions.Remove(item);
                }
            }
            //删除没有选项的用户
            var deleteUsers = new List<VoteUser>();
            foreach (var item in vote.VoteUsers)
            {
                if (item.SeletedOptions.Any() == false)
                {
                    deleteUsers.Add(item);
                }
            }

            foreach (var item in deleteUsers)
            {
                vote.VoteUsers.Remove(item);
            }

            //添加新选项
            foreach (var item in entryIds)
            {
                if (vote.VoteOptions.Where(s => s.EntryId != null).Any(s => entryIds.Contains(s.EntryId.Value)) == false)
                {
                    vote.VoteOptions.Add(new VoteOption
                    {
                        EntryId = item,
                        Type = VoteOptionType.Entry,
                    });

                }
            }
            foreach (var item in articleIds)
            {
                if (vote.VoteOptions.Where(s => s.ArticleId != null).Any(s => articleIds.Contains(s.ArticleId.Value)) == false)
                {
                    vote.VoteOptions.Add(new VoteOption
                    {
                        ArticleId = item,
                        Type = VoteOptionType.Article,
                    });
                }

            }
            foreach (var item in peripheryIds)
            {
                if (vote.VoteOptions.Where(s => s.PeripheryId != null).Any(s => peripheryIds.Contains(s.PeripheryId.Value)) == false)
                {
                    vote.VoteOptions.Add(new VoteOption
                    {
                        PeripheryId = item,
                        Type = VoteOptionType.Periphery,
                    });
                }

            }
            foreach (var item in model.Options.Where(s => s.Type == VoteOptionType.Text))
            {
                if (item.RealId == 0)
                {
                    vote.VoteOptions.Add(new VoteOption
                    {
                        Text = item.Text,
                        Type = VoteOptionType.Text,
                    });
                }
                else
                {
                    var temp = vote.VoteOptions.FirstOrDefault(s => s.Id == item.RealId);
                    if (temp != null)
                    {
                        temp.Text = item.Text;
                    }
                }
            }

            //更新数据
            await _voteRepository.UpdateAsync(vote);

            //删除选项和用户
            await _voteOptionRepository.DeleteAsync(s => deleteOptions.Select(s => s.Id).Contains(s.Id));
            await _voteUserRepository.DeleteAsync(s => deleteUsers.Select(s => s.Id).Contains(s.Id));

            return new Result { Successful = true };
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        public async Task<ActionResult<Result>> UserVoteAsync(UserVoteModel model)
        {
            var vote = await _voteRepository.GetAll()
                 .Include(s => s.VoteUsers).ThenInclude(s => s.SeletedOptions)
                 .Include(s => s.VoteOptions)
                 .ThenInclude(s => s.VoteUsers).ThenInclude(s => s.SeletedOptions)
                 .FirstOrDefaultAsync(s => s.Id == model.VoteId);
            if (vote == null)
            {
                return new Result { Successful = false, Error = "未找到目标投票" };
            }
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            if (vote.IsAllowModification == false)
            {
                if (vote.VoteUsers.Any(s => s.ApplicationUserId == user.Id))
                {
                    return new Result { Successful = false, Error = "当前投票不允许修改" };
                }
            }

            //判断选项是否在当前投票中
            var options = vote.VoteOptions.Where(s => model.VoteOptionIds.Contains(s.Id));
            if (options.Any() == false)
            {
                return new Result { Successful = false, Error = "没有选中项目或选中项与投票不匹配" };
            }
            //判断选择是否符合规则
            if (vote.Type == VoteType.SingleChoice && options.Count() > 1)
            {
                return new Result { Successful = false, Error = "单选投票只能选择一个" };
            }
            if (vote.Type == VoteType.MultipleChoice && (options.Count() < vote.MinimumSelectionCount || options.Count() > vote.MaximumSelectionCount))
            {
                var str = vote.MinimumSelectionCount == vote.MaximumSelectionCount ? vote.MinimumSelectionCount.ToString() : vote.MinimumSelectionCount + " - " + vote.MaximumSelectionCount;
                return new Result { Successful = false, Error = "请选中 " + str + " 个项目" };
            }

            var voteUser = vote.VoteUsers.FirstOrDefault(s => s.ApplicationUserId == user.Id);
            if (voteUser == null)
            {
                voteUser = new VoteUser
                {
                    SeletedOptions = options.ToList(),
                    ApplicationUserId = user.Id,
                    IsAnonymous = model.IsAnonymous,
                    VotedTime = DateTime.Now.ToCstTime(),
                    VoteId = vote.Id
                };
                await _voteUserRepository.InsertAsync(voteUser);
            }
            else
            {
                voteUser.SeletedOptions = options.ToList();
                voteUser.IsAnonymous = model.IsAnonymous;
                voteUser.VotedTime = DateTime.Now.ToCstTime();

                await _voteUserRepository.UpdateAsync(voteUser);
            }

            try
            {
                await _operationRecordService.AddOperationRecord(OperationRecordType.Vote, vote.Id.ToString(), user, model.Identification, HttpContext);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "用户 {Name}({Id})身份识别失败", user.UserName, user.Id);
            }


            return new Result { Successful = true };
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Result>> HiddenVoteAsync(HiddenVoteModel model)
        {
            await _voteRepository.GetRangeUpdateTable().Where(s => model.Ids.Contains(s.Id)).Set(s => s.IsHidden, b => model.IsHidden).ExecuteAsync();
            return new Result { Successful = true };
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Result>> EditVotePriorityAsync(EditVotePriorityViewModel model)
        {
            await _voteRepository.GetRangeUpdateTable().Where(s => model.Ids.Contains(s.Id)).Set(s => s.Priority, b => b.Priority + model.PlusPriority).ExecuteAsync();

            return new Result { Successful = true };
        }

        [AllowAnonymous]
        [HttpGet("{type}/{id}")]
        public async Task<ActionResult<List<VoteCardViewModel>>> GetRelatedVotesAsync(VoteOptionType type, long id)
        {
            var votes = new List<Vote>();
            switch (type)
            {
                case VoteOptionType.Entry:
                    var entry = await _entryRepository.GetAll().Include(s => s.Votes).ThenInclude(s => s.VoteUsers).AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
                    if (entry == null)
                    {
                        return NotFound();
                    }
                    votes = entry.Votes.ToList();
                    break;
                case VoteOptionType.Article:
                    var article = await _articleRepository.GetAll().Include(s => s.Votes).ThenInclude(s => s.VoteUsers).AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
                    if (article == null)
                    {
                        return NotFound();
                    }
                    votes = article.Votes.ToList();
                    break;

                case VoteOptionType.Periphery:
                    var periphery = await _peripheryRepository.GetAll().Include(s => s.Votes).ThenInclude(s => s.VoteUsers).AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
                    if (periphery == null)
                    {
                        return NotFound();
                    }
                    votes = periphery.Votes.ToList();
                    break;
                default:
                    return NotFound("无效的投票类型");
            }


            var model = new List<VoteCardViewModel>();

            foreach (var item in votes)
            {
                var temp = new VoteCardViewModel
                {
                    BeginTime = item.BeginTime,
                    EndTime = item.EndTime,
                    BriefIntroduction = item.BriefIntroduction,
                    Count = item.VoteUsers.Count,
                    MainPicture = _appHelper.GetImagePath(item.MainPicture, "app.png"),
                    Id = item.Id,
                    Name = item.Name,
                };
                model.Add(temp);
            }

            return model;
        }
    }
}
