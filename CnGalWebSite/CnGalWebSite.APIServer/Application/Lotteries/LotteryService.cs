using BootstrapBlazor.Components;
using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.Application.OperationRecords;
using CnGalWebSite.APIServer.Application.Users;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Admin;
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
        private readonly IAppHelper _appHelper;
        private readonly IUserService _userService;
        
        private readonly IRepository<PlayedGame, long> _playedGameRepository;
        private readonly IRepository<Comment, long> _commentRepository;
        private readonly IRepository<BookingUser, long> _bookingUserRepository;
        private readonly IOperationRecordService _operationRecordService;
        private readonly ILogger<LotteryService> _logger;

        public LotteryService(IRepository<Lottery, long> lotteryRepository, IRepository<LotteryUser, long> lotteryUserRepository, IRepository<LotteryAward, long> lotteryAwardRepository,
            IRepository<LotteryPrize, long> lotteryPrizeRepository, IAppHelper appHelper, IUserService userService, IRepository<ApplicationUser, string> userRepository, ILogger<LotteryService> logger,
         IRepository<BookingUser, long> bookingUserRepository, IRepository<Comment, long> commentRepository, IRepository<PlayedGame, long> playedGameRepository, IOperationRecordService operationRecordService)
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
            _operationRecordService=operationRecordService;
            _playedGameRepository = playedGameRepository;
            _logger = logger;
        }
        public Task<QueryData<ListLotteryAloneModel>> GetPaginatedResult(CnGalWebSite.DataModel.ViewModel.Search.QueryPageOptions options, ListLotteryAloneModel searchModel)
        {
            var items = _lotteryRepository.GetAll().Where(s => string.IsNullOrWhiteSpace(s.Name) == false).AsNoTracking();
            // 处理高级搜索
            if (!string.IsNullOrWhiteSpace(searchModel.Name))
            {
                items = items.Where(item => item.Name.Contains(searchModel.Name, StringComparison.OrdinalIgnoreCase));
            }
            if (!string.IsNullOrWhiteSpace(searchModel.BriefIntroduction))
            {
                items = items.Where(item => item.BriefIntroduction.Contains(searchModel.BriefIntroduction, StringComparison.OrdinalIgnoreCase));
            }



            // 处理 SearchText 模糊搜索
            if (!string.IsNullOrWhiteSpace(options.SearchText))
            {
                items = items.Where(item => item.Name.Contains(options.SearchText)
                             || item.BriefIntroduction.Contains(options.SearchText));
            }


            // 排序
            var isSorted = false;
            if (!string.IsNullOrWhiteSpace(options.SortName))
            {
                items = items.OrderBy(s => s.Id).Sort(options.SortName, (BootstrapBlazor.Components.SortOrder)options.SortOrder);
                isSorted = true;
            }

            // 设置记录总数
            var total = items.Count();

            // 内存分页
            var list = items.Skip((options.PageIndex - 1) * options.PageItems).Take(options.PageItems).ToList();

            //复制数据
            var resultItems = new List<ListLotteryAloneModel>();
            foreach (var item in list)
            {
                resultItems.Add(new ListLotteryAloneModel
                {
                    Id = item.Id,
                    Name = item.Name,
                    IsHidden = item.IsHidden,
                    BriefIntroduction = _appHelper.GetStringAbbreviation(item.BriefIntroduction, 20),
                    Priority = item.Priority,
                    LastEditTime = item.LastEditTime,
                    ReaderCount = item.ReaderCount,
                    CanComment = item.CanComment ?? true,
                    CommentCount = item.CommentCount,
                    BeginTime = item.BeginTime,
                    EndTime = item.EndTime,
                    Type = item.Type,
                    IsEnd = item.IsEnd,
                    LotteryTime = item.LotteryTime,
                });
            }

            return Task.FromResult(new QueryData<ListLotteryAloneModel>()
            {
                Items = resultItems,
                TotalCount = total,
                IsSorted = isSorted,
                // IsFiltered = isFiltered
            });
        }

        public Task<QueryData<ListLotteryUserAloneModel>> GetPaginatedResult(CnGalWebSite.DataModel.ViewModel.Search.QueryPageOptions options, ListLotteryUserAloneModel searchModel, long lotteryId)
        {
            var items = _lotteryUserRepository.GetAll()
                .Include(s => s.ApplicationUser).ThenInclude(s => s.OperationRecords)
                .Where(s => s.LotteryId == lotteryId)
                .Select(s => new
                {
                    UserId = s.ApplicationUser.Id,
                    Name = s.ApplicationUser.UserName,
                    s.Number,
                    LotteryUserId = s.Id,
                    s.IsHidden,
                    OperationRecords = s.ApplicationUser.OperationRecords.Where(s => s.Type == OperationRecordType.Lottery && s.ObjectId == lotteryId.ToString())
                })
                .AsNoTracking();

            // 处理高级搜索
            if (!string.IsNullOrWhiteSpace(searchModel.Name))
            {
                items = items.Where(item => item.Name.Contains(searchModel.Name, StringComparison.OrdinalIgnoreCase));
            }
            if (!string.IsNullOrWhiteSpace(searchModel.UserId))
            {
                items = items.Where(item => item.UserId.Contains(searchModel.UserId, StringComparison.OrdinalIgnoreCase));
            }



            // 处理 SearchText 模糊搜索
            if (!string.IsNullOrWhiteSpace(options.SearchText))
            {
                items = items.Where(item => item.Name.Contains(options.SearchText)
                             || item.UserId.Contains(options.SearchText));
            }


            // 排序
            var isSorted = false;
            if (!string.IsNullOrWhiteSpace(options.SortName))
            {
                items = items.OrderBy(s => s.LotteryUserId).Sort(options.SortName, (BootstrapBlazor.Components.SortOrder)options.SortOrder);
                isSorted = true;
            }

            // 设置记录总数
            var total = items.Count();

            // 内存分页
            var list = items.Skip((options.PageIndex - 1) * options.PageItems).Take(options.PageItems).ToList();

            //复制数据
            var resultItems = new List<ListLotteryUserAloneModel>();
            foreach (var item in list)
            {
                resultItems.Add(new ListLotteryUserAloneModel
                {
                    UserId = item.UserId,
                    Name = item.Name,
                    Number = item.Number,
                    LotteryUserId = item.LotteryUserId,
                    IsHidden = item.IsHidden,
                    Cookie = item.OperationRecords.FirstOrDefault()?.Cookie,
                    Ip = item.OperationRecords.FirstOrDefault()?.Ip,
                });
            }

            return Task.FromResult(new QueryData<ListLotteryUserAloneModel>()
            {
                Items = resultItems,
                TotalCount = total,
                IsSorted = isSorted,
                // IsFiltered = isFiltered
            });
        }

        public async Task SendPrizeToWinningUser(LotteryUser user, LotteryAward award)
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
                    Note = "抽奖赠送",
                    Type = UserIntegralType.Integral,
                    UserId = user.ApplicationUserId
                });
                //更新用户积分
                await _userService.UpdateUserIntegral(await _userRepository.FirstOrDefaultAsync(s => s.Id == user.ApplicationUserId));
            }
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

                        await SendPrizeToWinningUser(winnningUser, item);
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

                foreach (var temp in lottery.Users)
                {
                    var user = await _userRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(s => s.Id == temp.ApplicationUserId);
                    if (_userService.CheckCurrentUserRole( "Admin"))
                    {
                        temp.IsHidden = true;
                    }
                }

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

        public async Task<string> CheckCondition(ApplicationUser user,Lottery lottery)
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
                if (await _commentRepository.GetAll().AnyAsync(s => s.ApplicationUserId == user.Id && s.LotteryId == lottery.Id && s.Type == CommentType.CommentLottery && string.IsNullOrWhiteSpace(s.Text) == false) == false)
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

            return null;
        }

        public async Task AddUserToLottery(Lottery lottery,ApplicationUser user, HttpContext httpContext, DeviceIdentificationModel identification)
        {
            //检查抽奖条件
           var result=await CheckCondition(user, lottery);
            if (result != null)
            {
                throw new Exception(result);
            }

            if (lottery.Users.Any(s => s.ApplicationUserId == user.Id))
            {
                throw new Exception("你已经参加了这个抽奖");
            }
            var time = DateTime.Now.ToCstTime();
            await _lotteryUserRepository.InsertAsync(new LotteryUser
            {
                ApplicationUserId = user.Id,
                LotteryId = lottery.Id,
                ParticipationTime = time,
                Number = lottery.Users.Count + 1,
                //查找是否有相同的特征值
                IsHidden = await _operationRecordService.CheckOperationRecord(OperationRecordType.Lottery, lottery.Id.ToString(), user, identification, httpContext)
            });


            try
            {
                await _operationRecordService.AddOperationRecord(OperationRecordType.Lottery, lottery.Id.ToString(), user, identification, httpContext);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "用户 {Name}({Id})身份识别失败", user.UserName, user.Id);
            }
        }

        public async Task CopyUserFromBookingToLottery(Booking booking,Lottery lottery)
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
