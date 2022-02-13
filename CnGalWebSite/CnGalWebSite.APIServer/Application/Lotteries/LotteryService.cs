using BootstrapBlazor.Components;
using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.Application.Users;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.DataModel.ExamineModel;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Articles;
using CnGalWebSite.DataModel.ViewModel.Lotteries;
using CnGalWebSite.DataModel.ViewModel.Peripheries;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Nest;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;


namespace CnGalWebSite.APIServer.Application.Lotteries
{
    public class LotteryService:ILotteryService
    {
        private readonly IRepository<Lottery, long> _lotteryRepository;
        private readonly IRepository<LotteryUser, long> _lotteryUserRepository;
        private readonly IRepository<LotteryAward, long> _lotteryAwardRepository;
        private readonly IRepository<LotteryPrize, long> _lotteryPrizeRepository;
        private readonly IRepository<ApplicationUser, string> _userRepository;
        private readonly IAppHelper _appHelper;
        private readonly IUserService _userService;
        public LotteryService(IRepository<Lottery, long> lotteryRepository, IRepository<LotteryUser, long> lotteryUserRepository, IRepository<LotteryAward, long> lotteryAwardRepository,
            IRepository<LotteryPrize, long> lotteryPrizeRepository, IAppHelper appHelper, IUserService userService, IRepository<ApplicationUser, string> userRepository)
        {
            _lotteryRepository = lotteryRepository;
            _lotteryUserRepository = lotteryUserRepository;
            _lotteryAwardRepository = lotteryAwardRepository;
            _lotteryPrizeRepository = lotteryPrizeRepository;
            _appHelper = appHelper;
            _userService = userService;
            _userRepository = userRepository;
        }
        public Task<QueryData<ListLotteryAloneModel>> GetPaginatedResult(CnGalWebSite.DataModel.ViewModel.Search.QueryPageOptions options, ListLotteryAloneModel searchModel)
        {
            IQueryable<Lottery> items = _lotteryRepository.GetAll().Where(s => string.IsNullOrWhiteSpace(s.Name) == false).AsNoTracking();
            // 处理高级搜索
            if (!string.IsNullOrWhiteSpace(searchModel.Name))
            {
                items = items.Where(item => item.Name.Contains(searchModel.Name, StringComparison.OrdinalIgnoreCase) );
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

        public Task<QueryData<ListLotteryUserAloneModel>> GetPaginatedResult(CnGalWebSite.DataModel.ViewModel.Search.QueryPageOptions options, ListLotteryUserAloneModel searchModel,long lotteryId)
        {
            IQueryable<LotteryUser> items = _lotteryUserRepository.GetAll()
                .Include(s => s.ApplicationUser)
                .Where(s => s.LotteryId == lotteryId).AsNoTracking();

            // 处理高级搜索
            if (!string.IsNullOrWhiteSpace(searchModel.Name))
            {
                items = items.Where(item => item.ApplicationUser.UserName.Contains(searchModel.Name, StringComparison.OrdinalIgnoreCase));
            }
            if (!string.IsNullOrWhiteSpace(searchModel.Id))
            {
                items = items.Where(item => item.ApplicationUserId.Contains(searchModel.Id, StringComparison.OrdinalIgnoreCase));
            }



            // 处理 SearchText 模糊搜索
            if (!string.IsNullOrWhiteSpace(options.SearchText))
            {
                items = items.Where(item => item.ApplicationUser.UserName.Contains(options.SearchText)
                             || item.ApplicationUserId.Contains(options.SearchText));
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
            var resultItems = new List<ListLotteryUserAloneModel>();
            foreach (var item in list)
            {
                resultItems.Add(new ListLotteryUserAloneModel
                {
                    Id = item.ApplicationUser.Id,
                    Name=item.ApplicationUser.UserName,
                    Number=item.Number,
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

        public async Task SendPrizeToWinningUser(LotteryUser user,LotteryAward award)
        {
            user.LotteryAwardId = award.Id;
            if (award.Type == LotteryAwardType.ActivationCode)
            {
                var prize = await _lotteryPrizeRepository.FirstOrDefaultAsync(s => s.LotteryAwardId == award.Id && s.LotteryUserId == null);
                prize.LotteryUserId = user.Id;
                await _lotteryPrizeRepository.UpdateAsync(prize);
            }
            await _lotteryUserRepository.UpdateAsync(user);

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
                await _appHelper.UpdateUserIntegral(await _userRepository.FirstOrDefaultAsync(s => s.Id == user.ApplicationUserId));
            }
        }

        private async Task DrawLottery(Lottery lottery)
        {
            var NotWinnningUser = lottery.Users.ToList();

            foreach(var item in lottery.Awards)
            {
                NotWinnningUser.RemoveAll(s => item.WinningUsers.Select(s => s.ApplicationUserId).ToList().Contains(s.ApplicationUserId));
            }

            if(NotWinnningUser.Count<0)
            {
                return;
            }

            foreach(var item in lottery.Awards)
            {
                if(item.Count>item.WinningUsers.Count)
                {
                    var index = new Random().Next(0, NotWinnningUser.Count);
                    var winnningUser =NotWinnningUser[index];

                    item.WinningUsers.Add(winnningUser);
                    NotWinnningUser.Remove(winnningUser);

                   await SendPrizeToWinningUser(winnningUser, item);
                }
            }
        }

        public async Task DrawAllLottery()
        {
            DateTime time = DateTime.Now.ToCstTime();
            var ids = await _lotteryRepository.GetAll().AsNoTracking()
                .Where(s => s.IsEnd == false && s.Type == LotteryType.Automatic&&s.LotteryTime< time).Select(s=>s.Id)
                .ToListAsync();

            foreach(var item in ids)
            {
                var lottery = await _lotteryRepository.GetAll()
                    .Include(s => s.Awards).ThenInclude(s => s.WinningUsers)
                    .Include(s => s.Users)
                    .FirstOrDefaultAsync(s => s.Id == item);

               await DrawLottery(lottery);
            }

            _lotteryRepository.Clear();

            foreach (var item in ids)
            {
                var lottery = await _lotteryRepository.GetAll()
                    .FirstOrDefaultAsync(s => s.Id == item);
                lottery.IsEnd = true;
                await _lotteryRepository.UpdateAsync(lottery);
            }
        }


    }
}
