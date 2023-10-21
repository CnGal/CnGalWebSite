using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.Application.Lotteries;
using CnGalWebSite.APIServer.Application.OperationRecords;
using CnGalWebSite.APIServer.Application.Ranks;
using CnGalWebSite.APIServer.Application.Users;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.Core.Models;
using CnGalWebSite.Core.Services.Query;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Lotteries;
using CnGalWebSite.DataModel.ViewModel.Tables;
using Markdig;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;
using Result = CnGalWebSite.DataModel.Model.Result;


namespace CnGalWebSite.APIServer.Controllers
{


    [Authorize]
    [ApiController]
    [Route("api/lotteries/[action]")]
    public class LotteryAPIController : ControllerBase
    {
        
        private readonly IRepository<ApplicationUser, string> _userRepository;
        private readonly IAppHelper _appHelper;
        private readonly IRankService _rankService;
        private readonly ILotteryService _lotteryService;
        private readonly IRepository<GameNews, long> _gameNewsRepository;
        private readonly IRepository<Entry, int> _entryRepository;
        private readonly IRepository<WeeklyNews, long> _weeklyNewsRepository;
        private readonly IRepository<WeiboUserInfor, long> _weiboUserInforRepository;
        private readonly IRepository<Vote, long> _voteRepository;
        private readonly IRepository<VoteOption, long> _voteOptionRepository;
        private readonly IRepository<VoteUser, long> _voteUserRepository;
        private readonly IRepository<Lottery, long> _lotteryRepository;
        private readonly IRepository<LotteryUser, long> _lotteryUserRepository;
        private readonly IRepository<LotteryAward, long> _lotteryAwardRepository;
        private readonly IRepository<LotteryPrize, long> _lotteryPrizeRepository;
        private readonly IRepository<Booking, long> _bookingRepository;

        private readonly ILogger<LotteryAPIController> _logger;
        private readonly IOperationRecordService _operationRecordService;
        private readonly IUserService _userService;
        private readonly IQueryService _queryService;

        public LotteryAPIController(IRepository<Vote, long> voteRepository, IRepository<VoteOption, long> voteOptionRepository, IRepository<VoteUser, long> voteUserRepository, IRankService rankService, IUserService userService,
        IRepository<WeiboUserInfor, long> weiboUserInforRepository, IRepository<ApplicationUser, string> userRepository, ILogger<LotteryAPIController> logger, IOperationRecordService operationRecordService,
         IAppHelper appHelper, IRepository<GameNews, long> gameNewsRepository, IRepository<Comment, long> commentRepository, IRepository<Booking, long> bookingRepository, IQueryService queryService,
        IRepository<WeeklyNews, long> weeklyNewsRepository, IRepository<Lottery, long> lotteryRepository, IRepository<LotteryUser, long> lotteryUserRepository, IRepository<LotteryAward, long> lotteryAwardRepository,
             IRepository<LotteryPrize, long> lotteryPrizeRepository, ILotteryService lotteryService, IRepository<PlayedGame, long> playedGameRepository, IRepository<Entry, int> entryRepository, IRepository<BookingUser, long> bookingUserRepository)
        {
            
            _appHelper = appHelper;
            _rankService = rankService;
            _gameNewsRepository = gameNewsRepository;
            _weeklyNewsRepository = weeklyNewsRepository;
            _weiboUserInforRepository = weiboUserInforRepository;
            _voteRepository = voteRepository;
            _voteOptionRepository = voteOptionRepository;
            _voteUserRepository = voteUserRepository;
            _lotteryRepository = lotteryRepository;
            _lotteryUserRepository = lotteryUserRepository;
            _lotteryAwardRepository = lotteryAwardRepository;
            _lotteryPrizeRepository = lotteryPrizeRepository;
            _lotteryService = lotteryService;
            _userRepository = userRepository;
            _logger = logger;
            _operationRecordService = operationRecordService;
            _entryRepository = entryRepository;
            _bookingRepository = bookingRepository;
            _userService = userService;
            _queryService = queryService;
        }


        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<ActionResult<DrawLotteryDataModel>> GetLotteryDataAsync(long id)
        {
            try
            {
                var lottery = await _lotteryRepository.GetAll().AsNoTracking()
                            .Include(s => s.Awards).ThenInclude(s => s.WinningUsers)
                            .Include(s => s.Users).ThenInclude(s => s.ApplicationUser)
                            .FirstOrDefaultAsync(s => s.Id == id);
                var model = new DrawLotteryDataModel
                {
                    Name = lottery.Name,
                    Id = lottery.Id,
                    NotWinningUsers = lottery.Users.Where(s=>s.IsHidden==false).Select(s => new LotteryUserDataModel
                    {
                        Id = s.ApplicationUserId,
                        Name = s.ApplicationUser.UserName,
                        Number = s.Number,
                        IsHidden=s.IsHidden
                    }).ToList()
                };

                foreach (var item in lottery.Awards.OrderByDescending(s=>s.Priority))
                {
                    model.Awards.Add(new LotteryAwardDataModel
                    {
                        Id = item.Id,
                        Name = item.Name,
                        Priority = item.Priority,
                        TotalCount = item.Count,
                        Sponsor = item.Sponsor,
                        Image = item.Image,
                        Link = item.Link,
                        WinningUsers = model.NotWinningUsers.Where(s => item.WinningUsers.Select(s => s.ApplicationUserId).Contains(s.Id)).ToList()
                    });
                    model.NotWinningUsers.RemoveAll(s => item.WinningUsers.Select(s => s.ApplicationUserId).Contains(s.Id));
                }

                return model;
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }

        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<LotteryViewModel>> GetLotteryViewAsync(long id)
        {
            var lottery = await _lotteryRepository.GetAll().AsNoTracking()
                .Include(s => s.Awards).ThenInclude(s => s.WinningUsers).ThenInclude(s => s.ApplicationUser)
                .Include(s => s.Users)
                .FirstOrDefaultAsync(s => s.Id == id);
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);


            if (lottery == null)
            {
                return NotFound("未找到该抽奖");
            }

            //判断当前是否隐藏
            if (lottery.IsHidden == true)
            {
                if (user == null || _userService.CheckCurrentUserRole( "Editor") != true)
                {
                    return NotFound();
                }
            }

            var model = new LotteryViewModel
            {
                Id = id,
                DisplayName = lottery.DisplayName,
                BackgroundPicture = lottery.BackgroundPicture,
                BeginTime = lottery.BeginTime,
                BriefIntroduction = lottery.BriefIntroduction,
                CanComment = lottery.CanComment ?? false,
                CommentCount = lottery.CommentCount,
                Count = lottery.Users.Count,
                SmallBackgroundPicture = lottery.SmallBackgroundPicture,
                CreateTime = lottery.CreateTime,
                EndTime = lottery.EndTime,
                IsHidden = lottery.IsHidden,
                LastEditTime = lottery.LastEditTime,
                LotteryTime = lottery.LotteryTime,
                MainPage = lottery.MainPage,
                MainPicture = lottery.MainPicture,
                Name = lottery.Name,
                ReaderCount = lottery.ReaderCount,
                Thumbnail = lottery.Thumbnail,
                Type = lottery.Type,
                IsEnd = lottery.IsEnd,
                ConditionType = lottery.ConditionType,
            };
            //初始化主页Html代码
            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().UseSoftlineBreakAsHardlineBreak().Build();
            model.MainPage = Markdown.ToHtml(model.MainPage ?? "", pipeline);

            //可能需要添加检测抽奖是否结束
            foreach (var item in lottery.Awards.OrderByDescending(s=>s.Priority))
            {
                var temp = new LotteryAwardViewModel
                {
                    Count = item.Count,
                    Id = item.Id,
                    Integral = item.Integral,
                    Name = item.Name,
                    Priority = item.Priority,
                    Type = item.Type,
                    Sponsor = item.Sponsor,
                    Image = _appHelper.GetImagePath(item.Image, "app.png"),
                    Link = item.Link,
                };

                foreach (var infor in item.WinningUsers.Select(s => s.ApplicationUser))
                {
                    temp.Users.Add(new DataModel.ViewModel.Space.UserInforViewModel
                    {
                        PersonalSignature = infor.PersonalSignature,
                        PhotoPath = _appHelper.GetImagePath(infor.PhotoPath, "user.png"),
                        Ranks = await _rankService.GetUserRanks(infor),
                        Id = infor.Id,
                        Name = infor.UserName
                    });
                }

                model.Awards.Add(temp);
            }

            //增加阅读人数
            await _lotteryRepository.GetAll().Where(s => s.Id == id).ExecuteUpdateAsync(s=>s.SetProperty(s => s.ReaderCount, b => b.ReaderCount + 1));

            return model;

        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserLotteryStateModel>> GetUserLotteryState(long id)
        {
            var lottery = await _lotteryRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);

            if (lottery == null)
            {
                return NotFound("未找到该抽奖");
            }
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            user = await _userRepository.GetAll().AsNoTracking()
                .Include(s => s.UserAddress)
                .FirstOrDefaultAsync(s => s.Id == user.Id);
            var model = new UserLotteryStateModel();

            //当前用户中奖状态
            if (user == null)
            {
                model.State = UserLotteryState.NotLogin;
            }
            else
            {
                var award = await _lotteryUserRepository.GetAll().AsNoTracking()
                    .Include(s => s.LotteryAward)
                    .Include(s => s.LotteryPrize)
                    .FirstOrDefaultAsync(s => s.ApplicationUserId == user.Id && s.LotteryId == id);

                if (award == null)
                {
                    if (lottery.EndTime < DateTime.Now.ToCstTime())
                    {
                        model.State = UserLotteryState.NotInvolved;
                    }
                    else
                    {
                        if (await _lotteryService.CheckCondition(user, lottery) == null)
                        {
                            model.State = UserLotteryState.NotInvolved;
                        }
                        else
                        {
                            model.State = UserLotteryState.NoCondition;
                        }

                    }
                }
                else
                {
                    model.Number = award.Number;
                    if (lottery.EndTime > DateTime.Now.ToCstTime())
                    {
                        model.State = UserLotteryState.WaitingDraw;
                    }
                    else
                    {
                        if (award.LotteryAward == null)
                        {
                            if (lottery.IsEnd)
                            {
                                model.State = UserLotteryState.NotWin;
                            }
                            else
                            {
                                model.State = UserLotteryState.WaitingDraw;
                            }

                        }
                        else
                        {
                            model.Award = new LotteryAwardViewModel
                            {
                                Count = award.LotteryAward.Count,
                                Id = award.LotteryAward.Id,
                                Integral = award.LotteryAward.Integral,
                                Name = award.LotteryAward.Name,
                                Priority = award.LotteryAward.Priority,
                                Type = award.LotteryAward.Type,
                            };
                            if (award.LotteryAward.Type == LotteryAwardType.RealThing)
                            {


                                if (user.UserAddress == null)
                                {
                                    model.State = UserLotteryState.WaitAddress;
                                }
                                else
                                {
                                    model.State = UserLotteryState.WaitShipments;
                                }


                                if (award.LotteryPrize != null&&string.IsNullOrWhiteSpace(award.LotteryPrize.Context)==false)
                                {
                                    model.State = UserLotteryState.Shipped;
                                }
                            }
                            else
                            {
                                model.State = UserLotteryState.Win;

                            }


                        }
                    }

                }
            }

            return model;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<List<LotteryCardViewModel>>> GetLotteryCardsAsync()
        {
            var lotteries = await _lotteryRepository.GetAll().AsNoTracking()
                .Include(s => s.Users)
                .Where(s => s.IsHidden == false && string.IsNullOrWhiteSpace(s.Name) == false)
                .ToListAsync();

            var model = new List<LotteryCardViewModel>();

            foreach (var item in lotteries)
            {
                model.Add(new LotteryCardViewModel
                {
                    BeginTime = item.BeginTime,
                    BriefIntroduction = item.BriefIntroduction,
                    Count = item.Users.Count,
                    EndTime = item.EndTime,
                    Id = item.Id,
                    MainPicture = _appHelper.GetImagePath(item.MainPicture, "app.png"),
                    Name = item.Name,
                });
            }

            return model;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Result>> CreateLottery(EditLotteryModel model)
        {
            if (await _lotteryRepository.GetAll().AnyAsync(s => s.Name == model.Name))
            {
                return new Result { Successful = false, Error = "已存在该名称的抽奖" };
            }
            //检查选项数是否合法
            if (model.Awards.Count == 0)
            {
                return new Result { Successful = false, Error = "至少需要一个奖项" };
            }
            if (model.Awards.Any(s => s.Count == 0))
            {
                return new Result { Successful = false, Error = "奖品数量不能为零" };
            }
            foreach (var item in model.Awards)
            {
                if (item.Type == LotteryAwardType.ActivationCode && item.Prizes.Count != item.Count)
                {
                    return new Result { Successful = false, Error = "激活码数量应和奖项中填写的数量对应" };
                }
            }

            var lottery = new Lottery
            {
                Name = model.Name,
                EndTime = model.EndTime,
                LastEditTime = DateTime.Now.ToCstTime(),
                SmallBackgroundPicture = model.SmallBackgroundPicture,
                BackgroundPicture = model.BackgroundPicture,
                BeginTime = model.BeginTime,
                BriefIntroduction = model.BriefIntroduction,
                DisplayName = model.DisplayName,
                CreateTime = DateTime.Now.ToCstTime(),
                LotteryTime = model.LotteryTime,
                Type = model.Type,
                MainPage = model.MainPage,
                MainPicture = model.MainPicture,
                Thumbnail = model.Thumbnail,
                ConditionType = model.ConditionType,
            };

            foreach (var item in model.Awards)
            {
                var temp = new LotteryAward
                {
                    Count = item.Count,
                    Integral = item.Integral,
                    Name = item.Name,
                    Priority = item.Priority,
                    Type = item.Type,
                    Link = item.Link,
                    Sponsor=item.Sponsor,
                    Image = item.Image,
                    
                };
                foreach (var infor in item.Prizes)
                {
                    temp.Prizes.Add(new LotteryPrize
                    {
                        Context = infor.Context
                    });
                }

                lottery.Awards.Add(temp);

            }

            lottery = await _lotteryRepository.InsertAsync(lottery);

            int gameId = 0;
            if (string.IsNullOrWhiteSpace(model.GameName) == false)
            {
                var temp = await _entryRepository.GetAll().AsNoTracking().Where(s => s.Name == model.GameName && s.IsHidden == false).Select(s => new { BookingId = s.Booking.Id, s.Id }).FirstOrDefaultAsync();
                if (temp == null)
                {
                    return new Result { Successful = false, Error = "关联的游戏不存在" };
                }
                gameId = temp.Id;

                if (lottery.GameId != gameId)
                {
                    var booking = await _bookingRepository.GetAll().AsNoTracking().Include(s => s.Users).ThenInclude(s=>s.ApplicationUser).FirstOrDefaultAsync(s => s.Id == temp.BookingId);
                    var tempLottery = await _lotteryRepository.GetAll().AsNoTracking().Include(s => s.Users).FirstOrDefaultAsync(s => s.Id == lottery.Id);
                    //第一次添加游戏 复制预约用户到抽奖
                    await _lotteryService.CopyUserFromBookingToLottery(booking, tempLottery);

                    lottery = await _lotteryRepository.UpdateAsync(lottery);
                }
            }

            return new Result { Successful = true, Error = lottery.Id.ToString() };

        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<ActionResult<EditLotteryModel>> EditLottery(long id)
        {
            var lottery = await _lotteryRepository.GetAll().AsNoTracking()
                .Include(s => s.Awards).ThenInclude(s => s.Prizes)
                .FirstOrDefaultAsync(s => s.Id == id);


            if (lottery == null)
            {
                return NotFound("未找到该抽奖");
            }

            var model = new EditLotteryModel
            {
                Id = id,
                DisplayName = lottery.DisplayName,
                BackgroundPicture = lottery.BackgroundPicture,
                BeginTime = lottery.BeginTime,
                BriefIntroduction = lottery.BriefIntroduction,
                SmallBackgroundPicture = lottery.SmallBackgroundPicture,
                EndTime = lottery.EndTime,
                LotteryTime = lottery.LotteryTime,
                MainPage = lottery.MainPage,
                MainPicture = lottery.MainPicture,
                Name = lottery.Name,
                Thumbnail = lottery.Thumbnail,
                Type = lottery.Type,
                ConditionType = lottery.ConditionType,
            };

            if(lottery.GameId!=0)
            {
                model.GameName = await _entryRepository.GetAll().AsNoTracking().Where(s => s.Id == lottery.GameId && s.IsHidden == false).Select(s => s.Name).FirstOrDefaultAsync();
            }

            foreach (var item in lottery.Awards)
            {
                var temp = new EditLotteryAwardModel
                {
                    Count = item.Count,
                    Id = item.Id,
                    Integral = item.Integral,
                    Name = item.Name,
                    Priority = item.Priority,
                    Type = item.Type,
                    Sponsor = item.Sponsor,
                    Image = item.Image,
                    Link = item.Link,
                    
                };
                foreach (var infor in item.Prizes)
                {
                    temp.Prizes.Add(new EditLotteryPrizeModel
                    {
                        Context = infor.Context,
                        Id = infor.Id,
                    });
                }
                model.Awards.Add(temp);
            }

            return model;

        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Result>> EditLottery(EditLotteryModel model)
        {
            var lottery = await _lotteryRepository.GetAll()
                .Include(s => s.Awards).ThenInclude(s => s.Prizes)
                .FirstOrDefaultAsync(s => s.Id == model.Id);

            if (lottery == null)
            {
                return new Result { Successful = false, Error = "未找到该抽奖" };
            }
            if (lottery.Name != model.Name && await _voteRepository.GetAll().AnyAsync(s => s.Name == model.Name))
            {
                return new Result { Successful = false, Error = "已存在该名称的抽奖" };
            }
            //检查选项数是否合法
            if (model.Awards.Count == 0)
            {
                return new Result { Successful = false, Error = "至少需要一个奖项" };
            }
            if (model.Awards.Any(s => s.Count == 0))
            {
                return new Result { Successful = false, Error = "奖品数量不能为零" };
            }
            foreach (var item in model.Awards)
            {
                if (item.Type == LotteryAwardType.ActivationCode && item.Prizes.Count != item.Count)
                {
                    return new Result { Successful = false, Error = "激活码数量应和奖项中填写的数量对应" };
                }
            }
            int gameId = 0;
            if (string.IsNullOrWhiteSpace(model.GameName) == false)
            {
                var temp = await _entryRepository.GetAll().AsNoTracking().Where(s => s.Name == model.GameName && s.IsHidden == false).Select(s =>new {BookingId= s.Booking.Id, s.Id }).FirstOrDefaultAsync();
                if(temp==null)
                {
                    return new Result { Successful = false, Error = "关联的游戏不存在" };
                }
                gameId = temp.Id;

                if(lottery.GameId!=gameId)
                {
                    var booking = await _bookingRepository.GetAll().AsNoTracking().Include(s => s.Users).ThenInclude(s => s.ApplicationUser).FirstOrDefaultAsync(s => s.Id == temp.BookingId);
                    var tempLottery=await _lotteryRepository.GetAll().AsNoTracking().Include(s => s.Users).FirstOrDefaultAsync(s => s.Id == lottery.Id);
                    //第一次添加游戏 复制预约用户到抽奖
                    await _lotteryService.CopyUserFromBookingToLottery(booking, tempLottery);
                }
            }

            lottery.Name = model.Name;
            lottery.EndTime = model.EndTime;
            lottery.LastEditTime = DateTime.Now.ToCstTime();
            lottery.SmallBackgroundPicture = model.SmallBackgroundPicture;
            lottery.BackgroundPicture = model.BackgroundPicture;
            lottery.BeginTime = model.BeginTime;
            lottery.BriefIntroduction = model.BriefIntroduction;
            lottery.DisplayName = model.DisplayName;
            lottery.LotteryTime = model.LotteryTime;
            lottery.Type = model.Type;
            lottery.MainPage = model.MainPage;
            lottery.MainPicture = model.MainPicture;
            lottery.Thumbnail = model.Thumbnail;
            lottery.ConditionType = model.ConditionType;
            lottery.GameId = gameId;

            //记录现有的Ids
            var awardIds = model.Awards.Select(s => s.Id).ToList();
            awardIds.RemoveAll(s => s == 0);

            var tempList = lottery.Awards.ToList();
            tempList.RemoveAll(s => awardIds.Contains(s.Id) == false);
            lottery.Awards = tempList;

            foreach (var item in model.Awards)
            {
                if (item.Id == 0)
                {
                    var temp = new LotteryAward
                    {
                        Count = item.Count,
                        Integral = item.Integral,
                        Name = item.Name,
                        Priority = item.Priority,
                        Type = item.Type,
                        Sponsor = item.Sponsor,
                        Link = item.Link,
                        Image = item.Image,
                        
                    };
                    foreach (var infor in item.Prizes)
                    {
                        temp.Prizes.Add(new LotteryPrize
                        {
                            Context = infor.Context
                        });
                    }

                    lottery.Awards.Add(temp);
                }
                else
                {
                    var temp = lottery.Awards.FirstOrDefault(s => s.Id == item.Id);
                    if (temp != null)
                    {
                        temp.Count = item.Count;
                        temp.Integral = item.Integral;
                        temp.Name = item.Name;
                        temp.Priority = item.Priority;
                        temp.Type = item.Type;
                        temp.Sponsor = item.Sponsor;
                        temp.Link = item.Link;
                        temp.Image = item.Image;

                        var prizeIds = item.Prizes.Select(s => s.Id).ToList();
                        prizeIds.RemoveAll(s => s == 0);

                        var tempList_1 = temp.Prizes.ToList();
                        tempList_1.RemoveAll(s => prizeIds.Contains(s.Id) == false);
                        temp.Prizes = tempList_1;

                        foreach (var infor in item.Prizes)
                        {
                            if (infor.Id == 0)
                            {
                                temp.Prizes.Add(new LotteryPrize
                                {
                                    Context = infor.Context
                                });
                            }
                            else
                            {
                                var infor_1 = temp.Prizes.FirstOrDefault(s => s.Id == infor.Id);
                                if (infor_1 != null)
                                {
                                    infor_1.Context = infor.Context;
                                }
                            }
                        }
                    }
                }
            }

            await _lotteryRepository.UpdateAsync(lottery);

            return new Result { Successful = true };

        }

        [HttpPost]
        public async Task<ActionResult<Result>> ParticipateInLottery(ParticipateInLotteryModel model)
        {
            if(_userService.CheckCurrentUserRole("Admin"))
            {
                return new Result { Successful = false, Error = "管理员不允许参与抽奖！"};
            }

            var time = DateTime.Now.ToCstTime();
            var lottery = await _lotteryRepository.GetAll().AsNoTracking()
                .Include(s => s.Users)
                .FirstOrDefaultAsync(s => s.Id == model.Id && s.EndTime > time && s.BeginTime < time);
            if (lottery == null)
            {
                return new Result { Successful = false, Error = "这个时间不能参与抽奖哦~" };
            }
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            try
            {
                await _lotteryService.AddUserToLottery(lottery, user, HttpContext, model.Identification);
            }
            catch(Exception ex)
            {
                return new Result { Successful = false, Error = ex.Message };
            }
       
            return new Result { Successful = true };
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Result>> DrawLotteryAsync(ManualLotteryModel model)
        {
            var time = DateTime.Now.ToCstTime();
            if (await _lotteryRepository.GetAll().AnyAsync(s => s.Id == model.LotteryId /*&& s.EndTime < time*/) == false)
            {
                return new Result { Successful = false, Error = "未找到该抽奖或抽奖未结束" };
            }

            if (await _lotteryAwardRepository.GetAll().Include(s => s.WinningUsers).AnyAsync(s => s.Id == model.LotteryAwardId && s.LotteryId == model.LotteryId && s.Count > s.WinningUsers.Count) == false)
            {
                return new Result { Successful = false, Error = "当前奖项人数已满，或未找到该奖项，请核对抽奖Id" };
            }

            var userAward = await _lotteryUserRepository.GetAll().FirstOrDefaultAsync(s => s.ApplicationUserId == model.UserId && s.LotteryId == model.LotteryId);

            if (userAward == null)
            {
                return new Result { Successful = false, Error = "当前用户未参加该抽奖" };
            }

            var award = await _lotteryAwardRepository.GetAll().AsNoTracking()
                .Include(s => s.WinningUsers)
                .Include(s=>s.Lottery)
                .FirstOrDefaultAsync(s => s.Id == model.LotteryAwardId && s.LotteryId == model.LotteryId && s.Count > s.WinningUsers.Count);
            if (award == null)
            {
                return new Result { Successful = false, Error = "当前奖项人数已满，或未找到该奖项，请核对抽奖Id" };
            }

            await _lotteryService.SendPrizeToWinningUser(userAward, award,award.Lottery);

            return new Result { Successful = true };
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Result>> HiddenLotteryAsync(HiddenLotteryModel model)
        {
            await _lotteryRepository.GetAll().Where(s => model.Ids.Contains(s.Id)).ExecuteUpdateAsync(s=>s.SetProperty(s => s.IsHidden, b => model.IsHidden));
            return new Result { Successful = true };
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Result>> EndLotteryAsync(EndLotteryModel model)
        {
            await _lotteryRepository.GetAll().Where(s => model.Ids.Contains(s.Id)).ExecuteUpdateAsync(s=>s.SetProperty(s => s.IsEnd, b => model.IsEnd));
            return new Result { Successful = true };
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Result>> EditLotteryPriorityAsync(EditLotteryPriorityViewModel model)
        {
            await _lotteryRepository.GetAll().Where(s => model.Ids.Contains(s.Id)).ExecuteUpdateAsync(s=>s.SetProperty(s => s.Priority, b => b.Priority + model.PlusPriority));

            return new Result { Successful = true };
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PrizeViewModel>> GetUserPrizeAsync(long id)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            var award = await _lotteryUserRepository.GetAll().AsNoTracking()
                .Include(s => s.LotteryPrize)
                .Include(s => s.LotteryAward)
                .FirstOrDefaultAsync(s => s.ApplicationUserId == user.Id && s.LotteryId == id);

            if (award == null)
            {
                return NotFound("未找到该奖品");
            }

            if (award.LotteryPrize == null)
            {
                return NotFound("该用户未中奖或未发放奖品");
            }

            var model = new PrizeViewModel
            {
                Context = award.LotteryPrize.Context,
                Type = award.LotteryAward.Type
            };

            return model;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<QueryResultModel<LotteryUserOverviewModel>> ListLotteryUsers(QueryParameterModel model, [FromQuery]long lotteryId)
        {
            var (items, total) = await _queryService.QueryAsync<LotteryUser, long>(_lotteryUserRepository.GetAll().AsSingleQuery().Include(s => s.ApplicationUser).ThenInclude(s => s.OperationRecords)
                .Where(s => s.LotteryId == lotteryId), model,
                s => string.IsNullOrWhiteSpace(model.SearchText) || (s.ApplicationUser.UserName.Contains(model.SearchText)));

            return new QueryResultModel<LotteryUserOverviewModel>
            {
                Items = await items.Select(s => new LotteryUserOverviewModel
                {
                    UserId = s.ApplicationUser.Id,
                    Name = s.ApplicationUser.UserName,
                    Number = s.Number,
                    LotteryUserId = s.Id,
                    IsHidden = s.IsHidden,
                    Cookie = s.ApplicationUser.OperationRecords.First().Cookie,
                    Ip = s.ApplicationUser.OperationRecords.First().Ip,
                }).ToListAsync(),
                Total = total,
                Parameter = model
            };
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<ActionResult<List<WinnerOverviewModel>>> GetWinnerDatas(long id)
        {
            var lottery = await _lotteryRepository.GetAll().AsNoTracking()
                           .Include(s => s.Awards).ThenInclude(s => s.WinningUsers).ThenInclude(s=>s.LotteryPrize)
                           .Include(s => s.Awards).ThenInclude(s => s.WinningUsers).ThenInclude(s=>s.ApplicationUser).ThenInclude(s=>s.UserAddress)
                           .FirstOrDefaultAsync(s => s.Id == id);
            var model = new List<WinnerOverviewModel>();

            foreach (var item in lottery.Awards)
            {
                foreach(var temp in item.WinningUsers)
                {
                    model.Add(new WinnerOverviewModel
                    {
                        AwardId = item.Id,
                        ActivationCode = item.Type == LotteryAwardType.ActivationCode ? temp.LotteryPrize?.Context : null,
                        TrackingNumber = item.Type == LotteryAwardType.RealThing ? temp.LotteryPrize?.Context : null,
                        AwardName = item.Name,
                        Email = temp.ApplicationUser.Email,
                        PrizeId = temp.LotteryPrize?.Id ?? 0,
                        UserId = temp.ApplicationUser.Id,
                        UserName=temp.ApplicationUser.UserName,
                        AwardType=item.Type,
                        Number=temp.Number,
                        Address = temp.ApplicationUser.UserAddress?.Address,
                        Phone = temp.ApplicationUser.UserAddress?.PhoneNumber,
                        RealName = temp.ApplicationUser.UserAddress?.RealName,
                    });
                }
            }

            return model;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Result>> EditUserPrize(EditUserPrizeModel model)
        {
            //检查用户是否中奖
            if(await _lotteryAwardRepository.GetAll().AnyAsync(s=>s.Id==model.LotteryAwardId&&s.WinningUsers.Any(s=>s.ApplicationUserId==model.UserId))==false)
            {
                return new Result { Error = "未找到该中奖用户" };
            }

            var user = await _lotteryUserRepository.GetAll().FirstOrDefaultAsync(s => s.LotteryAwardId == model.LotteryAwardId && s.ApplicationUserId == model.UserId);
            if(user == null)
            {
                return new Result { Error = "未找到该中奖用户" };
            }

            var prize = await _lotteryPrizeRepository.GetAll().Include(s=>s.LotteryAward).Include(s=>s.LotteryUser).FirstOrDefaultAsync(s => s.LotteryAwardId == model.LotteryAwardId && s.LotteryUserId == user.Id);

            if(prize==null)
            {
                await _lotteryPrizeRepository.InsertAsync(new LotteryPrize
                {
                    Context = model.Context,
                    LotteryAwardId = model.LotteryAwardId,
                    LotteryUserId = user.Id
                });
            }
            else
            {
                prize.Context = model.Context;
                await _lotteryPrizeRepository.UpdateAsync(prize);
            }

            return new Result { Successful = true };
        }

        /// <summary>
        /// 修改用户能否被抽取
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Result>> HiddenLotteryUserAsync(HiddenLotteryModel model)
        {
            await _lotteryUserRepository.GetAll().Where(s => model.Ids.Contains(s.Id)).ExecuteUpdateAsync(s=>s.SetProperty(s => s.IsHidden, b => model.IsHidden));
            return new Result { Successful = true };
        }

        /// <summary>
        /// 获取输入提示
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<string>>> GetNamesAsync()
        {
            return await _lotteryRepository.GetAll().AsNoTracking().Where(s => s.IsHidden != true && string.IsNullOrWhiteSpace(s.Name) == false).Select(s => s.Name).ToArrayAsync();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<QueryResultModel<LotteryOverviewModel>> List(QueryParameterModel model)
        {
            var (items, total) = await _queryService.QueryAsync<Lottery, long>(_lotteryRepository.GetAll().AsSingleQuery().Where(s => string.IsNullOrWhiteSpace(s.Name) == false), model,
                s => string.IsNullOrWhiteSpace(model.SearchText) || (s.Name.Contains(model.SearchText)));

            return new QueryResultModel<LotteryOverviewModel>
            {
                Items = await items.Select(s => new LotteryOverviewModel
                {
                    Id = s.Id,
                    Name = s.Name,
                    IsHidden = s.IsHidden,
                    CanComment = s.CanComment ?? true,
                    Priority = s.Priority,
                    Type = s.Type,
                    LastEditTime = s.LastEditTime,
                    BeginTime = s.BeginTime,
                    EndTime = s.EndTime,
                    IsEnd = s.IsEnd,
                    LotteryTime = s.LotteryTime,
                }).ToListAsync(),
                Total = total,
                Parameter = model
            };
        }


    }
}
