﻿using CnGalWebSite.APIServer.Application.Articles;
using CnGalWebSite.APIServer.Application.Comments;
using CnGalWebSite.APIServer.Application.Entries;
using CnGalWebSite.APIServer.Application.ErrorCounts;
using CnGalWebSite.APIServer.Application.Favorites;
using CnGalWebSite.APIServer.Application.Files;
using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.Application.HistoryData;
using CnGalWebSite.APIServer.Application.Lotteries;
using CnGalWebSite.APIServer.Application.Messages;
using CnGalWebSite.APIServer.Application.News;
using CnGalWebSite.APIServer.Application.Perfections;
using CnGalWebSite.APIServer.Application.Peripheries;
using CnGalWebSite.APIServer.Application.Ranks;
using CnGalWebSite.APIServer.Application.Robots;
using CnGalWebSite.APIServer.Application.Search;
using CnGalWebSite.APIServer.Application.Users;
using CnGalWebSite.APIServer.Application.Votes;
using CnGalWebSite.APIServer.Application.WeiXin;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.APIServer.ExamineX;
using CnGalWebSite.DataModel.ExamineModel;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.ImportModel;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.Models;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Robots;
using CnGalWebSite.DataModel.ViewModel.TimedTasks;
using CnGalWebSite.Helper.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Senparc.Weixin.MP.AdvancedAPIs.MerChant;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private readonly IRobotService _robotService;

        public RobotAPIController(IRepository<UserOnlineInfor, long> userOnlineInforRepository, IRepository<UserFile, int> userFileRepository, IRepository<FavoriteObject, long> favoriteObjectRepository,
        IFileService fileService, IRepository<SignInDay, long> signInDayRepository, IRepository<ErrorCount, long> errorCountRepository, IRepository<BackUpArchiveDetail, long> backUpArchiveDetailRepository,
        IRepository<ThumbsUp, long> thumbsUpRepository, IRepository<Disambig, int> disambigRepository,  IRankService rankService, IHistoryDataService historyDataService,
        IRepository<ApplicationUser, string> userRepository, IMessageService messageService, ICommentService commentService, IRepository<Comment, long> commentRepository, IWeiXinService weiXinService,
        IRepository<Message, long> messageRepository, IErrorCountService errorCountService, IRepository<FavoriteFolder, long> favoriteFolderRepository,
        UserManager<ApplicationUser> userManager, IRepository<FriendLink, int> friendLinkRepository, IRepository<Carousel, int> carouselRepositor, IEntryService entryService, IRepository<RobotEvent, long> robotEventRepository,
        IArticleService articleService, IUserService userService, RoleManager<IdentityRole> roleManager, IExamineService examineService, IRepository<Rank, long> rankRepository,
        IRepository<Article, long> articleRepository, IAppHelper appHelper, IRepository<Entry, int> entryRepository, IFavoriteFolderService favoriteFolderService, IRepository<Periphery, long> peripheryRepository,
        IRepository<Examine, long> examineRepository, IRepository<Tag, int> tagRepository, IPeripheryService peripheryService, IRepository<GameNews, long> gameNewsRepository, IRobotService robotService,
        IVoteService voteService, IRepository<Vote, long> voteRepository, IRepository<SteamInfor, long> steamInforRepository, ILotteryService lotteryService, IRepository<RobotGroup, long> robotGroupRepository,
        IRepository<WeeklyNews, long> weeklyNewsRepository, IConfiguration configuration, IRepository<Lottery, long> lotteryRepository,ISearchHelper searchHelper, IRepository<RobotReply, long> robotReplyRepository)
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

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Result>> UpdateRobotEventDataAsync(ListRobotEventAloneModel model)
        {
            //检查数据合规性
            if(string.IsNullOrWhiteSpace(model.Text))
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
                    };
                }
            }

            //修改数据
            robot.Text = model.Text;
            robot.Time = model.Time;
            robot.IsHidden = model.IsHidden;
            robot.DelaySecond = model.DelaySecond;
            robot.Note = model.Note;

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
                    };
                }
            }

            //修改数据
            robot.GroupId = model.GroupId;
            robot.Note = model.Note;
            robot.IsHidden = model.IsHidden;

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
                        UpdateTime =DateTime.Now.ToCstTime(),
                        Value = model.Value,
                        AfterTime=model.AfterTime,
                        BeforeTime=model.BeforeTime,
                    };
                }
            }

            //修改数据
            robot.Key = model.Key;
            robot.Value = model.Value;
            robot.IsHidden = model.IsHidden;
            robot.AfterTime = model.AfterTime;
            robot.BeforeTime = model.BeforeTime;

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
                    bool first = item.LxKey.First() == '*';
                    bool last = item.LxKey.Last() == '*';

                    item.LxKey = item.LxKey.Replace("*", "([\\s\\S]*)");

                    if (first==false)
                    {
                        item.LxKey = '^' + item.LxKey;
                    }

                    if (last == false)
                    {
                        item.LxKey = item.LxKey + '$';
                    }
                }
                if(item.LxType== LxType.ExactMatch)
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
                    DateTime afterTime = DateTime.MinValue;
                    if (item.AfterTime != "-1")
                    {
                        afterTime = DateTime.ParseExact(item.AfterTime, "HHmm", null);
                    }
                    DateTime beforeTime = DateTime.MinValue.AddHours(23).AddMinutes(59).AddSeconds(59);
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
                catch (Exception ex)
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
        public async Task<ActionResult<Result>> ImportRobotEventsAsync(ImportRobotsModel model)
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
                try
                {
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

                    var time = DateTime.ParseExact(item.Time, "yyyy-MM-dd HH:mm:ss", null);
                    DateTime afterTime = DateTime.MinValue;
                    if (item.AfterTime != "-1")
                    {
                        afterTime = DateTime.ParseExact(item.AfterTime, "HHmm", null);
                    }
                    DateTime beforeTime = DateTime.MinValue.AddHours(23.9);
                    if (item.AfterTime != "-1")
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
                catch (Exception ex)
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
        public async Task<ActionResult<Result>> HiddenRobotEventAsync(HiddenRobotModel model)
        {
            await _robotEventRepository.GetRangeUpdateTable().Where(s => model.Ids.Contains(s.Id)).Set(s => s.IsHidden, b => model.IsHidden).ExecuteAsync();

            return new Result { Successful = true };
        }
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Result>> HiddenRobotGroupAsync(HiddenRobotModel model)
        {
            await _robotGroupRepository.GetRangeUpdateTable().Where(s => model.Ids.Contains(s.Id)).Set(s => s.IsHidden, b => model.IsHidden).ExecuteAsync();

            return new Result { Successful = true };
        }
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Result>> HiddenRobotReplyAsync(HiddenRobotModel model)
        {
            await _robotReplyRepository.GetRangeUpdateTable().Where(s => model.Ids.Contains(s.Id)).Set(s => s.IsHidden, b => model.IsHidden).ExecuteAsync();

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
        [HttpPost]
        public async Task<ActionResult<Result>> GetArgValueAsync(GetArgValueModel model)
        {
            if (model.Name == "auth")
            {
                var user = await _userRepository.GetAll().AsNoTracking()
                    .Include(s => s.ThirdPartyLoginInfors)
                    .FirstOrDefaultAsync(s => s.ThirdPartyLoginInfors.Any(s => s.Type == ThirdPartyLoginType.QQ && s.Id.ToString() == model.Infor));
                if (user == null)
                {
                    return new Result { Successful = false, Error = "你还没有绑定 CnGal资料站 账号哦~ （用QQ登入资料站就可以绑定了" };
                }

                return new Result { Successful = true, Error = (await _userManager.GetRolesAsync(user)).FirstOrDefault() };
            }
            else
            {
                var value = model.Name switch
                {
                    "recommend" => await _weiXinService.GetRandom(true),
                    _ => ""
                };
                value = value.Replace("</a>", "");

                while (true)
                {
                    var temp = value.MidStrEx("<a ", ">");

                    if(string.IsNullOrWhiteSpace(temp))
                    {
                        break;
                    }

                    value = value.Replace("<a " + temp + ">", "");

                }

                return new Result { Successful = true, Error = value };
            }

        }
    }
}