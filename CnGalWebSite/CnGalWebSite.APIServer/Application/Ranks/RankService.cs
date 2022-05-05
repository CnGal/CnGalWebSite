using BootstrapBlazor.Components;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Ranks;
using CnGalWebSite.DataModel.ViewModel.Space;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.Ranks
{
    public class RankService : IRankService
    {
        private readonly IRepository<Rank, int> _rankRepository;
        private readonly IRepository<ApplicationUser, string> _userRepository;
        private readonly IRepository<RankUser, int> _rankUserRepository;
        private readonly IRepository<Examine, int> _examineRepository;

        private static readonly ConcurrentDictionary<Type, Func<IEnumerable<Rank>, string, SortOrder, IEnumerable<Rank>>> SortLambdaCacheRank = new();
        private static readonly ConcurrentDictionary<Type, Func<IEnumerable<RankUser>, string, SortOrder, IEnumerable<RankUser>>> SortLambdaCacheRankUser = new();

        public RankService(IRepository<Rank, int> rankRepository, IRepository<RankUser, int> rankUserRepository, IRepository<Examine, int> examineRepository,
            IRepository<ApplicationUser, string> userRepository)
        {
            _rankUserRepository = rankUserRepository;
            _rankRepository = rankRepository;
            _examineRepository = examineRepository;
            _userRepository = userRepository;
        }

        public Task<QueryData<ListRankAloneModel>> GetPaginatedResult(CnGalWebSite.DataModel.ViewModel.Search.QueryPageOptions options, ListRankAloneModel searchModel)
        {
            IEnumerable<Rank> items = _rankRepository.GetAll().Include(s => s.RankUsers).Where(s => string.IsNullOrWhiteSpace(s.Name) == false).AsNoTracking();
            // 处理高级搜索
            if (!string.IsNullOrWhiteSpace(searchModel.Name))
            {
                items = items.Where(item => item.Name?.Contains(searchModel.Name, StringComparison.OrdinalIgnoreCase) ?? false);
            }

            if (!string.IsNullOrWhiteSpace(searchModel.CSS))
            {
                items = items.Where(item => item.CSS?.Contains(searchModel.CSS, StringComparison.OrdinalIgnoreCase) ?? false);
            }

            if (!string.IsNullOrWhiteSpace(searchModel.Styles))
            {
                items = items.Where(item => item.Styles?.Contains(searchModel.Styles, StringComparison.OrdinalIgnoreCase) ?? false);
            }


            // 处理 SearchText 模糊搜索
            if (!string.IsNullOrWhiteSpace(options.SearchText))
            {
                items = items.Where(item => (item.Name?.Contains(options.SearchText) ?? false)
                || (item.CSS?.Contains(options.SearchText) ?? false)
                || (item.Styles?.Contains(options.SearchText) ?? false));
            }

            // 排序
            var isSorted = false;
            if (!string.IsNullOrWhiteSpace(options.SortName))
            {
                // 外部未进行排序，内部自动进行排序处理
                var invoker = SortLambdaCacheRank.GetOrAdd(typeof(Rank), key => LambdaExtensions.GetSortLambda<Rank>().Compile());
                items = invoker(items, options.SortName, (BootstrapBlazor.Components.SortOrder)options.SortOrder);
                isSorted = true;
            }

            // 设置记录总数
            var total = items.Count();

            // 内存分页
            items = items.Skip((options.PageIndex - 1) * options.PageItems).Take(options.PageItems).ToList();

            //复制数据
            var resultItems = new List<ListRankAloneModel>();
            foreach (var item in items)
            {
                resultItems.Add(new ListRankAloneModel
                {
                    Id = item.Id,
                    Name = item.Name,
                    IsHidden = item.IsHidden,
                    CreateTime = item.CreateTime,
                    Styles = item.Styles,
                    CSS = item.CSS,
                    LastEditTime = item.LastEditTime,
                    Text = item.Text,
                    Priority = item.Priority,
                    Count = item.RankUsers.Count
                });
            }

            return Task.FromResult(new QueryData<ListRankAloneModel>()
            {
                Items = resultItems,
                TotalCount = total,
                IsSorted = isSorted,
                // IsFiltered = isFiltered
            });
        }

        public Task<QueryData<ListRankUserAloneModel>> GetPaginatedResult(CnGalWebSite.DataModel.ViewModel.Search.QueryPageOptions options, ListRankUserAloneModel searchModel, long rankId)
        {
            IEnumerable<RankUser> items = _rankUserRepository.GetAll().Include(s => s.ApplicationUser).Where(s => s.RankId == rankId).AsNoTracking();
            // 处理高级搜索
            if (!string.IsNullOrWhiteSpace(searchModel.UserId))
            {
                items = items.Where(item => item.ApplicationUserId?.Contains(searchModel.UserId, StringComparison.OrdinalIgnoreCase) ?? false);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.UserName))
            {
                items = items.Where(item => item.ApplicationUser.UserName?.Contains(searchModel.UserName, StringComparison.OrdinalIgnoreCase) ?? false);
            }



            // 处理 SearchText 模糊搜索
            if (!string.IsNullOrWhiteSpace(options.SearchText))
            {
                items = items.Where(item => (item.ApplicationUserId?.Contains(options.SearchText) ?? false));
            }


            // 排序
            var isSorted = false;
            if (!string.IsNullOrWhiteSpace(options.SortName))
            {
                // 外部未进行排序，内部自动进行排序处理
                var invoker = SortLambdaCacheRankUser.GetOrAdd(typeof(RankUser), key => LambdaExtensions.GetSortLambda<RankUser>().Compile());
                items = invoker(items, options.SortName, (BootstrapBlazor.Components.SortOrder)options.SortOrder);
                isSorted = true;
            }

            // 设置记录总数
            var total = items.Count();

            // 内存分页
            items = items.Skip((options.PageIndex - 1) * options.PageItems).Take(options.PageItems).ToList();

            //复制数据
            var resultItems = new List<ListRankUserAloneModel>();
            foreach (var item in items)
            {
                resultItems.Add(new ListRankUserAloneModel
                {
                    UserName = item.ApplicationUser.UserName,
                    UserId = item.ApplicationUserId,
                    Time = item.Time,
                    IsHidden = item.IsHidden,
                    Id = item.Id,
                });
            }

            return Task.FromResult(new QueryData<ListRankUserAloneModel>()
            {
                Items = resultItems,
                TotalCount = total,
                IsSorted = isSorted,
                // IsFiltered = isFiltered
            });
        }

        public Task<QueryData<ListUserRankAloneModel>> GetPaginatedResult(CnGalWebSite.DataModel.ViewModel.Search.QueryPageOptions options, ListUserRankAloneModel searchModel, string userId)
        {
            IEnumerable<RankUser> items = _rankUserRepository.GetAll().Include(s => s.Rank).Where(s => s.ApplicationUserId == userId).AsNoTracking();
            // 处理高级搜索
            if (!string.IsNullOrWhiteSpace(searchModel.Name))
            {
                items = items.Where(item => item.Rank.Name?.Contains(searchModel.Name, StringComparison.OrdinalIgnoreCase) ?? false);
            }


            // 处理 SearchText 模糊搜索
            if (!string.IsNullOrWhiteSpace(options.SearchText))
            {
                items = items.Where(item => (item.Rank.Name?.Contains(options.SearchText) ?? false));
            }

            // 排序
            var isSorted = false;
            if (!string.IsNullOrWhiteSpace(options.SortName))
            {
                // 外部未进行排序，内部自动进行排序处理
                var invoker = SortLambdaCacheRankUser.GetOrAdd(typeof(RankUser), key => LambdaExtensions.GetSortLambda<RankUser>().Compile());
                items = invoker(items, options.SortName, (BootstrapBlazor.Components.SortOrder)options.SortOrder);
                isSorted = true;
            }

            // 设置记录总数
            var total = items.Count();

            // 内存分页
            items = items.Skip((options.PageIndex - 1) * options.PageItems).Take(options.PageItems).ToList();

            //复制数据
            var resultItems = new List<ListUserRankAloneModel>();
            foreach (var item in items)
            {
                resultItems.Add(new ListUserRankAloneModel
                {
                    Name = item.Rank.Name,
                    RankId = item.RankId,
                    Time = item.Time,
                    IsHidden = item.IsHidden,
                    Id = item.Id,
                });
            }

            return Task.FromResult(new QueryData<ListUserRankAloneModel>()
            {
                Items = resultItems,
                TotalCount = total,
                IsSorted = isSorted,
                // IsFiltered = isFiltered
            });
        }

        public async Task UpdateUserRanks(ApplicationUser user)
        {
            var nowTime = DateTime.Now.ToCstTime();
            //用户等级头衔
            var level = ToolHelper.GetUserLevel(user.DisplayIntegral);
            //获取用户目前的等级头衔
            var levelRank = await _rankUserRepository.GetAll().Where(s => s.ApplicationUserId == user.Id && s.Rank.Name.Contains("Lv ")).Select(s => s.Rank.Name).FirstOrDefaultAsync();
            //比较是否过时
            if (levelRank != "Lv " + level)
            {
                //删除旧头衔
                await _rankUserRepository.DeleteAsync(s => s.ApplicationUserId == user.Id && s.Rank.Name.Contains("Lv "));
                //获取新头衔
                var rank = await _rankRepository.GetAll().FirstOrDefaultAsync(s => s.Name == "Lv " + level);
                if (rank == null)
                {
                    rank = await _rankRepository.InsertAsync(new Rank
                    {
                        Name = "Lv " + level,
                        Text = "Lv " + level,
                        CSS = "bg-main",
                        CreateTime = nowTime,
                        LastEditTime = nowTime,
                    });
                }
                //颁发新头衔
                await _rankUserRepository.InsertAsync(new RankUser
                {
                    ApplicationUserId = user.Id,
                    RankId = rank.Id,
                    Time = nowTime
                });
            }

            //通过验证头衔
            if (user.IsPassedVerification && await _rankUserRepository.GetAll().AnyAsync(s => s.ApplicationUserId == user.Id && s.Rank.Name == "已认证") == false)
            {
                //获取头衔
                var rank = await _rankRepository.GetAll().FirstOrDefaultAsync(s => s.Name == "已认证");
                if (rank == null)
                {
                    rank = await _rankRepository.InsertAsync(new Rank
                    {
                        Name = "已认证",
                        Text = "已认证",
                        CSS = "bg-primary",
                        CreateTime = nowTime,
                        LastEditTime = nowTime,
                    });

                }
                //颁发新头衔
                await _rankUserRepository.InsertAsync(new RankUser
                {
                    ApplicationUserId = user.Id,
                    RankId = rank.Id,
                    Time = nowTime
                });
            }

            //编辑者
            if (await _examineRepository.GetAll().AnyAsync(s => s.ApplicationUserId == user.Id && s.IsPassed == true && s.Operation != Operation.UserMainPage && s.Operation != Operation.EditUserMain && s.Operation != Operation.PubulishComment)
                && await _rankUserRepository.GetAll().AnyAsync(s => s.Rank.Name == "编辑者" && s.ApplicationUserId == user.Id) == false)
            {
                //获取头衔
                var rank = await _rankRepository.GetAll().FirstOrDefaultAsync(s => s.Name == "编辑者");
                if (rank == null)
                {
                    rank = await _rankRepository.InsertAsync(new Rank
                    {
                        Name = "编辑者",
                        Text = "编辑者",
                        CSS = "bg-success",
                        CreateTime = nowTime,
                        LastEditTime = nowTime,
                    });
                }
                //颁发新头衔
                await _rankUserRepository.InsertAsync(new RankUser
                {
                    ApplicationUserId = user.Id,
                    RankId = rank.Id,
                    Time = nowTime
                });
            }

            //去重
            var ranks = await _rankUserRepository.GetAll().Include(s => s.Rank).Where(s => s.ApplicationUserId == user.Id).ToListAsync();
            var ranksCopy = ranks.ToArray();
            foreach (var rank in ranksCopy)
            {
                if (ranks.Count(s => s.Rank.Name == rank.Rank.Name) > 1)
                {
                    await _rankUserRepository.DeleteAsync(rank);
                    ranks.Remove(rank);
                }
            }
        }

        public async Task UpdateUserRanks(string userId)
        {
            await UpdateUserRanks(await _userRepository.FirstOrDefaultAsync(s => s.Id == userId));
        }

        public async Task<List<RankViewModel>> GetUserRanks(ApplicationUser user)
        {
            return await GetUserRanks(user.Id, user);
        }

        public async Task<List<RankViewModel>> GetUserRanks(string userId, ApplicationUser user = null)
        {
            await UpdateUserRanks(userId);

            var ranks = await _rankUserRepository.GetAll().Include(s => s.Rank).Where(s => s.ApplicationUserId == userId && s.IsHidden == false && s.Rank.IsHidden == false).Select(s => s.Rank).OrderByDescending(s => s.Priority).AsNoTracking().ToListAsync();

            if (ranks.Count == 0)
            {
                //不可能没有头衔 尝试更新
                if (user == null)
                {
                    await UpdateUserRanks(userId);
                }
                else
                {
                    await UpdateUserRanks(user);
                }
                ranks = await _rankUserRepository.GetAll().Include(s => s.Rank).Where(s => s.ApplicationUserId == userId && s.IsHidden == false && s.Rank.IsHidden == false).Select(s => s.Rank).OrderByDescending(s => s.Priority).AsNoTracking().ToListAsync();
            }

            var model = new List<RankViewModel>();

            foreach (var item in ranks)
            {
                var temp = new RankViewModel
                {
                    Styles = item.Styles,
                    CSS = item.CSS,
                    Name = item.Name,
                    Text = item.Text,
                };
                model.Add(temp);
            }

            return model;
        }

        public async Task<List<UserEditRankIsShow>> GetUserRankListForEdit(ApplicationUser user)
        {
            await UpdateUserRanks(user);
            var ranks = await _rankUserRepository.GetAll().Include(s => s.Rank).Where(s => s.ApplicationUserId == user.Id && s.Rank.IsHidden == false).OrderByDescending(s => s.Rank.Priority).AsNoTracking()
                .Select(s => new UserEditRankIsShow
                {
                    IsShow = !s.IsHidden,
                    Name = s.Rank.Name
                }).ToListAsync();

            return ranks;
        }

        public async Task UpdateUserRanksIsHidden(ApplicationUser user, List<UserEditRankIsShow> model)
        {
            var ranks = await _rankUserRepository.GetAll().Include(s => s.Rank).Where(s => s.ApplicationUserId == user.Id && s.Rank.IsHidden == false).OrderByDescending(s => s.Rank.Priority).AsNoTracking()
                .Select(s => new
                {
                    IsShow = !s.IsHidden,
                    s.Rank.Name,
                    s.Id
                }).ToListAsync();

            //对比被修改的项目
            var ids = new List<long>();
            foreach (var item in ranks)
            {
                var temp = model.FirstOrDefault(s => s.Name == item.Name);
                if (temp == null)
                {
                    continue;
                }
                if (item.IsShow != temp.IsShow)
                {
                    ids.Add(item.Id);
                }
            }

            //应用修改项
            await _rankUserRepository.GetRangeUpdateTable().Where(s => ids.Contains(s.Id)).Set(s => s.IsHidden, b => !b.IsHidden).ExecuteAsync();
        }
    }
}
