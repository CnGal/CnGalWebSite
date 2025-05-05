
using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.Application.OperationRecords;
using CnGalWebSite.APIServer.Application.SteamInfors;
using CnGalWebSite.APIServer.Application.Users;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.Core.Models;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Lotteries;
using CnGalWebSite.DataModel.ViewModel.OperationRecords;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;


namespace CnGalWebSite.APIServer.Application.Lotteries
{
    public class LotteryService : ILotteryService
    {
        private readonly IRepository<Lottery, long> _lotteryRepository;
        private readonly IRepository<LotteryUser, long> _lotteryUserRepository;
        private readonly IRepository<LotteryAward, long> _lotteryAwardRepository;
        private readonly IRepository<LotteryPrize, long> _lotteryPrizeRepository;
        private readonly IRepository<ApplicationUser, string> _userRepository;
        private readonly IRepository<Message, long> _messageRepository;
        private readonly IAppHelper _appHelper;
        private readonly IUserService _userService;
        private readonly ISteamInforService _steamInforService;

        private readonly IRepository<PlayedGame, long> _playedGameRepository;
        private readonly IRepository<Comment, long> _commentRepository;
        private readonly IRepository<BookingUser, long> _bookingUserRepository;
        private readonly IRepository<Entry, int> _entryRepository;
        private readonly IOperationRecordService _operationRecordService;
        private readonly ILogger<LotteryService> _logger;

        public LotteryService(IRepository<Lottery, long> lotteryRepository, IRepository<LotteryUser, long> lotteryUserRepository, IRepository<LotteryAward, long> lotteryAwardRepository, IRepository<Message, long> messageRepository,
        IRepository<LotteryPrize, long> lotteryPrizeRepository, IAppHelper appHelper, IUserService userService, IRepository<ApplicationUser, string> userRepository, ILogger<LotteryService> logger, IRepository<Entry, int> entryRepository,
        IRepository<BookingUser, long> bookingUserRepository, IRepository<Comment, long> commentRepository, IRepository<PlayedGame, long> playedGameRepository, IOperationRecordService operationRecordService, ISteamInforService steamInforService)
        {
            _lotteryRepository = lotteryRepository;
            _lotteryUserRepository = lotteryUserRepository;
            _lotteryAwardRepository = lotteryAwardRepository;
            _lotteryPrizeRepository = lotteryPrizeRepository;
            _appHelper = appHelper;
            _userService = userService;
            _userRepository = userRepository;

            _bookingUserRepository = bookingUserRepository;
            _commentRepository = commentRepository;
            _operationRecordService = operationRecordService;
            _playedGameRepository = playedGameRepository;
            _logger = logger;
            _messageRepository = messageRepository;

            _entryRepository = entryRepository;

            _steamInforService = steamInforService;
        }

        public async Task SendPrizeToWinningUser(LotteryUser user, LotteryAward award, Lottery lottery)
        {
            user.LotteryAwardId = award.Id;
            if (award.Type == LotteryAwardType.ActivationCode)
            {
                var prize = await _lotteryPrizeRepository.FirstOrDefaultAsync(s => s.LotteryAwardId == award.Id && s.LotteryUserId == null);
                prize.LotteryUserId = user.Id;
                _ = await _lotteryPrizeRepository.UpdateAsync(prize);
            }
            _ = await _lotteryUserRepository.UpdateAsync(user);

            if (award.Integral != 0)
            {
                await _userService.AddUserIntegral(new DataModel.ViewModel.Space.AddUserIntegralModel
                {
                    Count = award.Integral,
                    Note = $"抽奖Id：{lottery.Id}",
                    Type = UserIntegralType.Integral,
                    UserId = user.ApplicationUserId,
                    SourceType = UserIntegralSourceType.Lottery
                });
                //更新用户积分
                await _userService.UpdateUserIntegral(await _userRepository.FirstOrDefaultAsync(s => s.Id == user.ApplicationUserId));
            }

            //发送消息
            await _messageRepository.InsertAsync(new Message
            {
                Title = "恭喜你中奖了",
                PostTime = DateTime.Now.ToCstTime(),
                Image = "default/logo.png",
                Rank = "系统",
                Text = $"你在『{lottery.DisplayName}』中获得『{award.Name}』奖品，请前往抽奖页面查看详情",
                Link = "lotteries/index/" + lottery.Id,
                LinkTitle = lottery.DisplayName,
                Type = MessageType.LotteryWinner,
                ApplicationUserId = user.ApplicationUserId
            });
        }

        private async Task<bool> DrawLottery(Lottery lottery)
        {
            var NotWinnningUser = lottery.Users.Where(s => s.IsHidden == false).ToList();

            foreach (var item in lottery.Awards)
            {
                _ = NotWinnningUser.RemoveAll(s => item.WinningUsers.Select(s => s.ApplicationUserId).ToList().Contains(s.ApplicationUserId));
            }

            foreach (var item in lottery.Awards)
            {
                if (NotWinnningUser.Count == 0)
                {
                    return false;
                }

                while (true)
                {
                    if (item.Count > item.WinningUsers.Count)
                    {
                        var index = new Random().Next(0, NotWinnningUser.Count);
                        var winnningUser = NotWinnningUser[index];

                        item.WinningUsers.Add(winnningUser);
                        _ = NotWinnningUser.Remove(winnningUser);

                        await SendPrizeToWinningUser(winnningUser, item, lottery);
                    }
                    else
                    {
                        break;
                    }

                }
            }

            return true;
        }

        public async Task DrawAllLottery()
        {

            var time = DateTime.Now.ToCstTime();
            var ids = await _lotteryRepository.GetAll().AsNoTracking()
                .Where(s => s.IsEnd == false && s.Type == LotteryType.Automatic && s.LotteryTime < time).Select(s => s.Id)
                .ToListAsync();

            foreach (var item in ids)
            {
                var lottery = await _lotteryRepository.GetAll()
                    .Include(s => s.Awards).ThenInclude(s => s.WinningUsers)
                    .Include(s => s.Users)
                    .FirstOrDefaultAsync(s => s.Id == item);

                var users = lottery.Users.ToList();

                var result = await DrawLottery(lottery);

                _lotteryRepository.Clear();

                if (result)
                {
                    lottery = await _lotteryRepository.GetAll().FirstOrDefaultAsync(s => s.Id == item);
                    lottery.IsEnd = true;
                    _ = await _lotteryRepository.UpdateAsync(lottery);
                }
            }
        }

        public async Task ClearLottery(long id)
        {
            var lottery = await _lotteryRepository.GetAll()
                    .Include(s => s.Awards).ThenInclude(s => s.WinningUsers)
                    .Include(s => s.Awards).ThenInclude(s => s.Prizes).ThenInclude(s => s.LotteryUser)
                    .FirstOrDefaultAsync(s => s.Id == id);

            foreach (var item in lottery.Awards)
            {
                item.WinningUsers.Clear();
                foreach (var temp in item.Prizes)
                {
                    temp.LotteryUser = null;
                    temp.LotteryUserId = null;
                }
            }

            lottery.IsEnd = false;

            _ = await _lotteryRepository.UpdateAsync(lottery);
        }

        public async Task<string> CheckCondition(ApplicationUser user, Lottery lottery)
        {
            try
            {
                if (lottery.ConditionType == LotteryConditionType.GameRecord)
                {
                    if (await _playedGameRepository.GetAll().AnyAsync(s => s.ApplicationUserId == user.Id) == false)
                    {
                        return "参加该抽奖需要至少有一条游玩记录";
                    }
                }
                else if (lottery.ConditionType == LotteryConditionType.CommentLottery)
                {
                    if (await _commentRepository.GetAll().AnyAsync(s => s.ApplicationUserId == user.Id && s.LotteryId == lottery.Id && s.Type == CommentType.CommentLottery) == false)
                    {
                        return "参加该抽奖需要评论该抽奖，并通过审核";
                    }
                }
                else if (lottery.ConditionType == LotteryConditionType.BookingGame)
                {
                    if (await _bookingUserRepository.GetAll().Include(s => s.Booking).AsNoTracking().AnyAsync(s => s.Booking.EntryId == lottery.GameId && s.ApplicationUserId == user.Id) == false)
                    {
                        return "参加该抽奖需要预约游戏";
                    }
                }
                else if (lottery.ConditionType == LotteryConditionType.Wishlist)
                {
                    if (await _steamInforService.CheckUserWishlist(user, lottery.GameSteamId) == false)
                    {
                        return "参加该抽奖需要将游戏添加到愿望单";
                    }
                }
                else if (lottery.ConditionType == LotteryConditionType.NewGameRecord)
                {
                    if (await _playedGameRepository.GetAll().AnyAsync(s => s.ApplicationUserId == user.Id && s.LastEditTime > lottery.BeginTime && string.IsNullOrWhiteSpace(s.PlayImpressions) == false) == false)
                    {
                        return "参加该抽奖需要至少有一条游玩记录创建时间大于抽奖开始时间";
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "参与抽奖失败");
                return ex.Message;
            }


            return null;
        }

        public async Task<int> AddUserToLottery(Lottery lottery, ApplicationUser user, HttpContext httpContext, DeviceIdentificationModel identification)
        {
            //检查抽奖条件
            var result = await CheckCondition(user, lottery);
            if (result != null)
            {
                throw new Exception(result);
            }

            if (lottery.Users.Any(s => s.ApplicationUserId == user.Id))
            {
                throw new Exception("你已经参加了这个抽奖");
            }

            // 周年庆特殊处理 G币
            if (lottery.Id == 22)
            {
                //尝试添加G币
                await _userService.TryAddGCoins(user.Id, UserIntegralSourceType.AnniversariesLotteries, 1, null);
            }


            var time = DateTime.Now.ToCstTime();

            var lotteryUser = new LotteryUser
            {
                ApplicationUserId = user.Id,
                LotteryId = lottery.Id,
                ParticipationTime = time,
                Number = lottery.Users.Count + 1,
                //查找是否有相同的特征值
                IsHidden = await _operationRecordService.CheckOperationRecord(OperationRecordType.Lottery, lottery.Id.ToString(), user, identification, httpContext)
            };

            await _lotteryUserRepository.InsertAsync(lotteryUser);

            try
            {
                await _operationRecordService.AddOperationRecord(OperationRecordType.Lottery, lottery.Id.ToString(), user, identification, httpContext);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "用户 {Name}({Id})身份识别失败", user.UserName, user.Id);
            }

            return lotteryUser.Number;
        }

        public async Task CopyUserFromBookingToLottery(Booking booking, Lottery lottery)
        {
            var users = booking.Users.Where(s => lottery.Users.Any(x => x.ApplicationUserId == s.ApplicationUserId) == false).Select(s => s.ApplicationUser);

            var time = DateTime.Now.ToCstTime();
            var count = lottery.Users.Count;
            foreach (var item in users)
            {
                await _operationRecordService.CopyOperationRecord(OperationRecordType.Booking, booking.Id.ToString(), OperationRecordType.Lottery, lottery.Id.ToString(), item);

                await _lotteryUserRepository.InsertAsync(new LotteryUser
                {
                    ApplicationUserId = item.Id,
                    LotteryId = lottery.Id,
                    ParticipationTime = time,
                    Number = count + 1,
                    //查找是否有相同的特征值
                    IsHidden = await _operationRecordService.CheckOperationRecord(OperationRecordType.Lottery, lottery.Id.ToString(), item)
                });
            }
        }


    }
}
