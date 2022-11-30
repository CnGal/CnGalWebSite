using CnGalWebSite.APIServer.Application.Articles;
using CnGalWebSite.APIServer.Application.Comments;
using CnGalWebSite.APIServer.Application.Entries;
using CnGalWebSite.APIServer.Application.ErrorCounts;
using CnGalWebSite.APIServer.Application.Favorites;
using CnGalWebSite.APIServer.Application.Files;
using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.Application.HistoryData;
using CnGalWebSite.APIServer.Application.Lotteries;
using CnGalWebSite.APIServer.Application.Messages;
using CnGalWebSite.APIServer.Application.Peripheries;
using CnGalWebSite.APIServer.Application.Ranks;
using CnGalWebSite.APIServer.Application.Robots;
using CnGalWebSite.APIServer.Application.Search;
using CnGalWebSite.APIServer.Application.Users;
using CnGalWebSite.APIServer.Application.Votes;
using CnGalWebSite.APIServer.Application.WeiXin;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.APIServer.ExamineX;
using CnGalWebSite.DataModel.Application.Search.Dtos;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.ImportModel;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.Models;
using CnGalWebSite.DataModel.ViewModel.Robots;
using CnGalWebSite.Helper.Extensions;
using Elasticsearch.Net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Org.BouncyCastle.Utilities;
using Senparc.Weixin.MP.AdvancedAPIs.MerChant;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace CnGalWebSite.APIServer.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("api/robot/[action]")]
    public class RobotAPIController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IRepository<Entry, int> _entryRepository;
        private readonly IRepository<Examine, long> _examineRepository;
        private readonly IRepository<Tag, int> _tagRepository;
        private readonly IRepository<Article, long> _articleRepository;
        private readonly IRepository<FriendLink, int> _friendLinkRepository;
        private readonly IRepository<Carousel, int> _carouselRepository;
        private readonly IRepository<UserFile, int> _userFileRepository;
        private readonly IRepository<Disambig, int> _disambigRepository;
        private readonly IRepository<Message, long> _messageRepository;
        private readonly IRepository<Comment, long> _commentRepository;
        private readonly IRepository<ThumbsUp, long> _thumbsUpRepository;
        private readonly IRepository<BackUpArchiveDetail, long> _backUpArchiveDetailRepository;
        private readonly IRepository<UserOnlineInfor, long> _userOnlineInforRepository;
        private readonly IRepository<ApplicationUser, string> _userRepository;
        private readonly IRepository<SignInDay, long> _signInDayRepository;
        private readonly IRepository<ErrorCount, long> _errorCountRepository;
        private readonly IRepository<FavoriteFolder, long> _favoriteFolderRepository;
        private readonly IRepository<FavoriteObject, long> _favoriteObjectRepository;
        private readonly IRepository<Rank, long> _rankRepository;
        private readonly IRepository<Periphery, long> _peripheryRepository;
        private readonly IRepository<Vote, long> _voteRepository;
        private readonly IRepository<SteamInfor, long> _steamInforRepository;
        private readonly IUserService _userService;
        private readonly IAppHelper _appHelper;
        private readonly IExamineService _examineService;
        private readonly IEntryService _entryService;
        private readonly IArticleService _articleService;
        private readonly ICommentService _commentService;
        private readonly IMessageService _messageService;
        private readonly IFileService _fileService;
        private readonly IErrorCountService _errorCountService;
        private readonly IFavoriteFolderService _favoriteFolderService;
        private readonly IRankService _rankService;
        private readonly IPeripheryService _peripheryService;
        private readonly IVoteService _voteService;
        private readonly ILotteryService _lotteryService;
        private readonly IWeiXinService _weiXinService;
        private readonly IRepository<GameNews, long> _gameNewsRepository;
        private readonly IRepository<WeeklyNews, long> _weeklyNewsRepository;
        private readonly IRepository<Lottery, long> _lotteryRepository;
        private readonly IHistoryDataService _historyDataService;
        private readonly ISearchHelper _searchHelper;
        private readonly IConfiguration _configuration;
        private readonly IRepository<RobotReply, long> _robotReplyRepository;
        private readonly IRepository<RobotGroup, long> _robotGroupRepository;
        private readonly IRepository<RobotEvent, long> _robotEventRepository;
        private readonly IRepository<RobotFace, long> _robotFaceRepository;
        private readonly IRobotService _robotService;

        public RobotAPIController(IRepository<UserOnlineInfor, long> userOnlineInforRepository, IRepository<UserFile, int> userFileRepository, IRepository<FavoriteObject, long> favoriteObjectRepository,
        IFileService fileService, IRepository<SignInDay, long> signInDayRepository, IRepository<ErrorCount, long> errorCountRepository, IRepository<BackUpArchiveDetail, long> backUpArchiveDetailRepository,
        IRepository<ThumbsUp, long> thumbsUpRepository, IRepository<Disambig, int> disambigRepository, IRankService rankService, IHistoryDataService historyDataService,
        IRepository<ApplicationUser, string> userRepository, IMessageService messageService, ICommentService commentService, IRepository<Comment, long> commentRepository, IWeiXinService weiXinService,
        IRepository<Message, long> messageRepository, IErrorCountService errorCountService, IRepository<FavoriteFolder, long> favoriteFolderRepository, IRepository<RobotFace, long> robotFaceRepository,
        UserManager<ApplicationUser> userManager, IRepository<FriendLink, int> friendLinkRepository, IRepository<Carousel, int> carouselRepositor, IEntryService entryService, IRepository<RobotEvent, long> robotEventRepository,
        IArticleService articleService, IUserService userService, RoleManager<IdentityRole> roleManager, IExamineService examineService, IRepository<Rank, long> rankRepository,
        IRepository<Article, long> articleRepository, IAppHelper appHelper, IRepository<Entry, int> entryRepository, IFavoriteFolderService favoriteFolderService, IRepository<Periphery, long> peripheryRepository,
        IRepository<Examine, long> examineRepository, IRepository<Tag, int> tagRepository, IPeripheryService peripheryService, IRepository<GameNews, long> gameNewsRepository, IRobotService robotService,
        IVoteService voteService, IRepository<Vote, long> voteRepository, IRepository<SteamInfor, long> steamInforRepository, ILotteryService lotteryService, IRepository<RobotGroup, long> robotGroupRepository,
        IRepository<WeeklyNews, long> weeklyNewsRepository, IConfiguration configuration, IRepository<Lottery, long> lotteryRepository, ISearchHelper searchHelper, IRepository<RobotReply, long> robotReplyRepository)
        {
            _userManager = userManager;
            _entryRepository = entryRepository;
            _examineRepository = examineRepository;
            _tagRepository = tagRepository;
            _appHelper = appHelper;
            _articleRepository = articleRepository;
            _examineService = examineService;
            _roleManager = roleManager;
            _userService = userService;
            _entryService = entryService;
            _articleService = articleService;
            _friendLinkRepository = friendLinkRepository;
            _carouselRepository = carouselRepositor;
            _messageRepository = messageRepository;
            _commentRepository = commentRepository;
            _commentService = commentService;
            _messageService = messageService;
            _userRepository = userRepository;
            _userFileRepository = userFileRepository;
            _userOnlineInforRepository = userOnlineInforRepository;
            _thumbsUpRepository = thumbsUpRepository;
            _disambigRepository = disambigRepository;
            _fileService = fileService;
            _signInDayRepository = signInDayRepository;
            _errorCountService = errorCountService;
            _errorCountRepository = errorCountRepository;
            _favoriteFolderRepository = favoriteFolderRepository;
            _favoriteFolderService = favoriteFolderService;
            _backUpArchiveDetailRepository = backUpArchiveDetailRepository;
            _favoriteObjectRepository = favoriteObjectRepository;
            _rankRepository = rankRepository;
            _rankService = rankService;
            _peripheryRepository = peripheryRepository;
            _peripheryService = peripheryService;
            _gameNewsRepository = gameNewsRepository;
            _weeklyNewsRepository = weeklyNewsRepository;
            _historyDataService = historyDataService;
            _voteService = voteService;
            _voteRepository = voteRepository;
            _configuration = configuration;
            _steamInforRepository = steamInforRepository;
            _lotteryRepository = lotteryRepository;
            _lotteryService = lotteryService;
            _searchHelper = searchHelper;
            _weiXinService = weiXinService;
            _robotEventRepository = robotEventRepository;
            _robotGroupRepository = robotGroupRepository;
            _robotReplyRepository = robotReplyRepository;
            _robotService = robotService;
            _robotFaceRepository = robotFaceRepository;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<ListRobotsInforViewModel>> ListRobotsAsync()
        {
            try
            {
                var model = new ListRobotsInforViewModel
                {
                    Events = await _robotEventRepository.CountAsync(s => s.IsHidden == false),
                    Groups = await _robotGroupRepository.CountAsync(s => s.IsHidden == false),
                    Replies = await _robotReplyRepository.CountAsync(s => s.IsHidden == false),
                    Faces = await _robotFaceRepository.CountAsync(s => s.IsHidden == false),
                };

                return model;

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult<BootstrapBlazor.Components.QueryData<ListRobotEventAloneModel>>> GetRobotEventListAsync(RobotEventsPagesInfor input)
        {
            var dtos = await _robotService.GetPaginatedResult(input.Options, input.SearchModel);

            return dtos;
        }
        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult<BootstrapBlazor.Components.QueryData<ListRobotGroupAloneModel>>> GetRobotGroupListAsync(RobotGroupsPagesInfor input)
        {
            var dtos = await _robotService.GetPaginatedResult(input.Options, input.SearchModel);

            return dtos;
        }
        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult<BootstrapBlazor.Components.QueryData<ListRobotReplyAloneModel>>> GetRobotReplyListAsync(RobotRepliesPagesInfor input)
        {
            var dtos = await _robotService.GetPaginatedResult(input.Options, input.SearchModel);

            return dtos;
        }
        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult<BootstrapBlazor.Components.QueryData<ListRobotFaceAloneModel>>> GetRobotFaceListAsync(RobotFacesPagesInfor input)
        {
            var dtos = await _robotService.GetPaginatedResult(input.Options, input.SearchModel);

            return dtos;
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Result>> UpdateRobotEventDataAsync(ListRobotEventAloneModel model)
        {
            //检查数据合规性
            if (string.IsNullOrWhiteSpace(model.Text))
            {
                return new Result { Successful = false, Error = $"消息不能为空" };
            }
            //查找
            var robot = await _robotEventRepository.FirstOrDefaultAsync(s => s.Id == model.Id);
            if (robot == null)
            {
                if (model.Id != 0)
                {
                    return new Result { Successful = false, Error = $"未找到Id：{model.Id}的事件" };

                }
                else
                {
                    robot = new RobotEvent
                    {
                        IsHidden = model.IsHidden,
                        Text = model.Text,
                        Time = model.Time,
                        DelaySecond = model.DelaySecond,
                        Note = model.Note,
                        Probability = model.Probability,
                        Type = model.Type,
                    };
                }
            }

            //修改数据
            robot.Text = model.Text;
            robot.Time = model.Time;
            robot.IsHidden = model.IsHidden;
            robot.DelaySecond = model.DelaySecond;
            robot.Note = model.Note;
            robot.Probability = model.Probability;
            robot.Type = model.Type;

            //保存
            if (model.Id == 0)
            {
                await _robotEventRepository.InsertAsync(robot);

            }
            else
            {
                await _robotEventRepository.UpdateAsync(robot);

            }

            return new Result { Successful = true };
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Result>> UpdateRobotGroupDataAsync(ListRobotGroupAloneModel model)
        {
            //查找
            var robot = await _robotGroupRepository.FirstOrDefaultAsync(s => s.Id == model.Id);
            if (robot == null)
            {
                if (model.Id != 0)
                {
                    return new Result { Successful = false, Error = $"未找到Id：{model.Id}的群号" };

                }
                else
                {
                    robot = new RobotGroup
                    {
                        IsHidden = model.IsHidden,
                        GroupId = model.GroupId,
                        Note = model.Note,
                        ForceMatch = model.ForceMatch,
                    };
                }
            }

            //修改数据
            robot.GroupId = model.GroupId;
            robot.Note = model.Note;
            robot.IsHidden = model.IsHidden;
            robot.ForceMatch = model.ForceMatch;

            //保存
            if (model.Id == 0)
            {
                await _robotGroupRepository.InsertAsync(robot);

            }
            else
            {
                await _robotGroupRepository.UpdateAsync(robot);

            }

            return new Result { Successful = true };
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Result>> UpdateRobotReplyDataAsync(ListRobotReplyAloneModel model)
        {
            //检查数据合规性
            if (string.IsNullOrWhiteSpace(model.Key))
            {
                return new Result { Successful = false, Error = $"匹配表达式不能为空" };
            }
            if (string.IsNullOrWhiteSpace(model.Value))
            {
                return new Result { Successful = false, Error = $"回复不能为空" };
            }

            //查找
            var robot = await _robotReplyRepository.FirstOrDefaultAsync(s => s.Id == model.Id);
            if (robot == null)
            {
                if (model.Id != 0)
                {
                    return new Result { Successful = false, Error = $"未找到Id：{model.Id}的自动回复" };

                }
                else
                {
                    robot = new RobotReply
                    {
                        IsHidden = model.IsHidden,
                        Key = model.Key,
                        UpdateTime = DateTime.Now.ToCstTime(),
                        Value = model.Value,
                        AfterTime = model.AfterTime,
                        BeforeTime = model.BeforeTime,
                        Priority = model.Priority,
                    };
                }
            }

            //修改数据
            robot.Key = model.Key;
            robot.Value = model.Value;
            robot.IsHidden = model.IsHidden;
            robot.AfterTime = model.AfterTime;
            robot.BeforeTime = model.BeforeTime;
            robot.Priority = model.Priority;
            robot.Range = model.Range;
            robot.UpdateTime = DateTime.Now.ToCstTime();

            //保存
            if (model.Id == 0)
            {
                await _robotReplyRepository.InsertAsync(robot);

            }
            else
            {
                await _robotReplyRepository.UpdateAsync(robot);

            }

            return new Result { Successful = true };
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Result>> UpdateRobotFaceDataAsync(ListRobotFaceAloneModel model)
        {
            //检查数据合规性
            if (string.IsNullOrWhiteSpace(model.Key))
            {
                return new Result { Successful = false, Error = $"匹配表达式不能为空" };
            }
            if (string.IsNullOrWhiteSpace(model.Value))
            {
                return new Result { Successful = false, Error = $"回复不能为空" };
            }

            //查找
            var robot = await _robotFaceRepository.FirstOrDefaultAsync(s => s.Id == model.Id);
            if (robot == null)
            {
                if (model.Id != 0)
                {
                    return new Result { Successful = false, Error = $"未找到Id：{model.Id}的自动回复" };

                }
                else
                {
                    robot = new RobotFace
                    {
                        IsHidden = model.IsHidden,
                        Key = model.Key,
                        Value = model.Value,
                        Note = model.Note,
                    };
                }
            }

            //修改数据
            robot.Key = model.Key;
            robot.Value = model.Value;
            robot.Note = model.Note;
            robot.IsHidden = model.IsHidden;

            //保存
            if (model.Id == 0)
            {
                await _robotFaceRepository.InsertAsync(robot);

            }
            else
            {
                await _robotFaceRepository.UpdateAsync(robot);

            }

            return new Result { Successful = true };
        }


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Result>> ImportRobotRepliesAsync(ImportRobotsModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Value))
            {
                return new Result { Successful = false, Error = "导入内容不能为空" };
            }

            List<ImportRobotReplyModel> replies = null;
            using (TextReader str = new StringReader(model.Value))
            {
                var serializer = new JsonSerializer();
                replies = (List<ImportRobotReplyModel>)serializer.Deserialize(str, typeof(List<ImportRobotReplyModel>));
            }

            var errors = 0;

            foreach (var item in replies)
            {
                //检查数据合规
                if (string.IsNullOrWhiteSpace(item.LxKey))
                {
                    errors++;
                    continue;
                }
                if (string.IsNullOrWhiteSpace(item.LxValue))
                {
                    errors++;
                    continue;
                }

                //转换正则表达式
                if (item.LxType == LxType.Asterisk)
                {
                    var first = item.LxKey.First() == '*';
                    var last = item.LxKey.Last() == '*';

                    item.LxKey = item.LxKey.Replace("*", "([\\s\\S]*)");

                    if (first == false)
                    {
                        item.LxKey = '^' + item.LxKey;
                    }

                    if (last == false)
                    {
                        item.LxKey = item.LxKey + '$';
                    }
                }
                if (item.LxType == LxType.ExactMatch)
                {
                    item.LxKey = '^' + item.LxKey + '$';
                }

                //转换图片域名
                item.LxValue = item.LxValue.Replace("http://", "https://");

                if (await _robotReplyRepository.GetAll().AnyAsync(s => s.Value == item.LxValue && s.Key == item.LxKey))
                {
                    continue;
                }


                try
                {



                    var time = DateTime.ParseExact(item.Time, "yyyy-MM-dd HH:mm:ss", null);
                    var afterTime = DateTime.MinValue.AddYears(2022);
                    if (item.AfterTime != "-1")
                    {
                        afterTime = DateTime.ParseExact(item.AfterTime, "HHmm", null);
                    }
                    var beforeTime = DateTime.MinValue.AddYears(2022).AddHours(23).AddMinutes(59).AddSeconds(59);
                    if (item.BeforeTime != "-1")
                    {
                        beforeTime = DateTime.ParseExact(item.BeforeTime, "HHmm", null);
                    }

                    await _robotReplyRepository.InsertAsync(new RobotReply
                    {
                        IsHidden = false,
                        Key = item.LxKey,
                        UpdateTime = time,
                        Value = item.LxValue,
                        AfterTime = afterTime,
                        BeforeTime = beforeTime
                    });

                }
                catch (Exception)
                {
                    errors++;
                }
            }


            if (errors == 0)
            {
                return new Result { Successful = true };

            }
            else
            {
                return new Result { Successful = false, Error = $"总计{replies.Count}项，失败{errors}项" };

            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Result>> ImportRobotFacesAsync(ImportRobotsModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Value))
            {
                return new Result { Successful = false, Error = "导入内容不能为空" };
            }

            var lines = model.Value.Split('\n');
            var errors = 0;
            foreach (var item in lines)
            {
                var temp = item.Replace("[", "").Replace("]", "");

                var key = temp.MidStrEx("'", "'");

                temp = temp.Replace($"'{key}'", "");

                var value = temp.MidStrEx("'", "'").Replace("http://", "https://");

                if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
                {
                    errors++;
                    continue;
                }

                if (await _robotFaceRepository.GetAll().AnyAsync(s => s.Value == value && s.Key == key))
                {
                    continue;
                }


                await _robotFaceRepository.InsertAsync(new RobotFace
                {
                    Key = key,
                    Value = "[" + value + "]"
                });
            }


            if (errors == 0)
            {
                return new Result { Successful = true };

            }
            else
            {
                return new Result { Successful = false, Error = $"总计{lines.Count()}项，失败{errors}项" };

            }
        }


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Result>> HiddenRobotEventAsync(HiddenRobotModel model)
        {
            await _robotEventRepository.GetAll().Where(s => model.Ids.Contains(s.Id)).ExecuteUpdateAsync(s=>s.SetProperty(s => s.IsHidden, b => model.IsHidden));

            return new Result { Successful = true };
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Result>> HiddenRobotGroupAsync(HiddenRobotModel model)
        {
            await _robotGroupRepository.GetAll().Where(s => model.Ids.Contains(s.Id)).ExecuteUpdateAsync(s=>s.SetProperty(s => s.IsHidden, b => model.IsHidden));

            return new Result { Successful = true };
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Result>> HiddenRobotReplyAsync(HiddenRobotModel model)
        {
            await _robotReplyRepository.GetAll().Where(s => model.Ids.Contains(s.Id)).ExecuteUpdateAsync(s=>s.SetProperty(s => s.IsHidden, b => model.IsHidden));

            return new Result { Successful = true };
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Result>> HiddenRobotFaceAsync(HiddenRobotModel model)
        {
            await _robotFaceRepository.GetAll().Where(s => model.Ids.Contains(s.Id)).ExecuteUpdateAsync(s=>s.SetProperty(s => s.IsHidden, b => model.IsHidden));

            return new Result { Successful = true };
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<List<RobotReply>>> GetRobotRepliesAsync()
        {
            return await _robotReplyRepository.GetAllListAsync();
        }
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<List<RobotEvent>>> GetRobotEventsAsync()
        {
            return await _robotEventRepository.GetAllListAsync();
        }
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<List<RobotGroup>>> GetRobotGroupsAsync()
        {
            return await _robotGroupRepository.GetAllListAsync();
        }
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<List<RobotFace>>> GetRobotFacesAsync()
        {
            return await _robotFaceRepository.GetAllListAsync();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult<Result>> GetArgValueAsync(GetArgValueModel model)
        {
            if (model.Name == "auth")
            {
                var user = await _userRepository.GetAll().AsNoTracking()
                    .FirstOrDefaultAsync(s => s.GroupQQ == model.SenderId);

                if (user == null)
                {
                    return new Result { Successful = false, Error = "你还没有绑定账号哦~ \nPS：私聊“绑定账号”试试吧ヾ(•ω•`)o" };
                }

                return new Result { Successful = true, Error = (await _userManager.GetRolesAsync(user)).FirstOrDefault() };
            }
            else if (model.Name == "bindqq")
            {
                var temps = model.Infor.Split("绑定");
                if (temps.Length <= 1)
                {
                    return new Result { Successful = true, Error = "" };
                }
                var code = temps[1].Replace("绑定", "").Trim();


                if ((await _userService.BindGroupQQ(code, model.SenderId)).Successful)
                {
                    return new Result { Successful = true, Error = "o(〃＾▽＾〃)o 成功绑定账号" };
                }
                else
                {
                    return new Result { Successful = false, Error = "＞﹏＜ 看板娘觉得身份识别码错了喵~" };
                }
            }
            else if (model.Name == "introduce")
            {
                var entryName = model.Infor.Replace("看板娘介绍一下", "").Replace("看板娘介绍", "").Trim();

                var result = await _searchHelper.QueryAsync(SearchInputModel.Parse(new string[] { "Entry" }, null, entryName, null, 0)); 

                var entry = result.Data.FirstOrDefault(s => s.entry != null)?.entry;
                if (entry == null)
                {
                    return new Result { Successful = false, Error = "呜呜呜~~~ 看板娘找不到这个词条" };
                }
                else
                {
                    if (entry.Name != entryName && entry.Name != entryName)
                    {
                        return new Result { Successful = true, Error = (await _weiXinService.GetEntryInfor(entry.Id, true, true,model.SenderId!=0)).DeleteHtmlLinks() + "\n（看板娘不太确定是不是这个词条哦~" };
                    }
                    else
                    {
                        return new Result { Successful = true, Error = (await _weiXinService.GetEntryInfor(entry.Id, true, true, model.SenderId != 0)).DeleteHtmlLinks() };
                    }
                }
            }
            else if (model.Name == "website")
            {
                var urls = Regex.Matches(model.Infor, "http[s]?://(?:(?!http[s]?://)[a-zA-Z]|[0-9]|[$\\-_@.&+/]|[!*\\(\\),]|(?:%[0-9a-fA-F][0-9a-fA-F]))+");

                //处理链接
                foreach (var item in urls.Select(s => s.ToString().Trim()))
                {
                    if (item.Contains("entries"))
                    {
                        var idStr = item.Split('/').Last();
                        if (int.TryParse(idStr, out var id))
                        {
                            return new Result { Successful = true, Error = (await _weiXinService.GetEntryInfor(id, true)).DeleteHtmlLinks() };
                        }
                    }
                    else if (item.Contains("articles"))
                    {
                        var idStr = item.Split('/').Last();
                        if (int.TryParse(idStr, out var id))
                        {
                            return new Result { Successful = true, Error = (await _weiXinService.GetArticleInfor(id, true)).DeleteHtmlLinks() };
                        }
                    }
                }

                return new Result { Successful = false, Error = null };

            }
            else if (model.Name == "steamdiscount")
            {
                var count = await _steamInforRepository.GetAll().Include(s => s.Entry).CountAsync(s => s.CutNow > 0 && s.Entry.IsHidden == false && string.IsNullOrWhiteSpace(s.Entry.Name) == false);

                return new Result { Successful = true, Error = $"今天有{count}款作品打折中：https://www.cngal.org/discount" };
            }
            else
            {
                var value = model.Name switch
                {
                    "recommend" => await _weiXinService.GetRandom(true, true),
                    "birthday" => await _weiXinService.GetRoleBirthdays( true),
                    "BirthdayWithDefault" => await _weiXinService.GetRoleBirthdays(true)??"好像今天没人过生日~~~",
                    "NewestEditGames" => await _weiXinService.GetNewestEditGames(true),
                    "NewestUnPublishGames" => await _weiXinService.GetNewestUnPublishGames(true),
                    "NewestPublishGames" => await _weiXinService.GetNewestPublishGames(true),
                    "NewestNews" => await _weiXinService.GetNewestNews(true, model.SenderId != 0),
                    _ => ""
                };

                return new Result { Successful = true, Error = value.DeleteHtmlLinks() };
            }

        }
    }
}
