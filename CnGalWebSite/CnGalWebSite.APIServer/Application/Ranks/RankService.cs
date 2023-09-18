
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
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

        public RankService(IRepository<Rank, int> rankRepository, IRepository<RankUser, int> rankUserRepository, IRepository<Examine, int> examineRepository,
            IRepository<ApplicationUser, string> userRepository)
        {
            _rankUserRepository = rankUserRepository;
            _rankRepository = rankRepository;
            _examineRepository = examineRepository;
            _userRepository = userRepository;
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
                _ = await _rankUserRepository.InsertAsync(new RankUser
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
                _ = await _rankUserRepository.InsertAsync(new RankUser
                {
                    ApplicationUserId = user.Id,
                    RankId = rank.Id,
                    Time = nowTime
                });
            }

            //编辑者
            if (await _examineRepository.GetAll().AnyAsync(s => s.ApplicationUserId == user.Id && s.IsPassed == true && s.Operation != Operation.UserMainPage && s.Operation != Operation.EditUserMain && s.Operation != Operation.PubulishComment && s.Operation != Operation.EditPlayedGameMain))
            {
                if (await _rankUserRepository.GetAll().AnyAsync(s => s.Rank.Name == "编辑者" && s.ApplicationUserId == user.Id) == false)
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
                    _ = await _rankUserRepository.InsertAsync(new RankUser
                    {
                        ApplicationUserId = user.Id,
                        RankId = rank.Id,
                        Time = nowTime
                    });
                }

            }
            else
            {
                //删除旧头衔
                await _rankUserRepository.DeleteAsync(s => s.ApplicationUserId == user.Id && s.Rank.Name.Contains("编辑者"));
            }

            //去重
            var ranks = await _rankUserRepository.GetAll().Include(s => s.Rank).Where(s => s.ApplicationUserId == user.Id).ToListAsync();
            var ranksCopy = ranks.ToArray();
            foreach (var rank in ranksCopy)
            {
                if (ranks.Count(s => s.Rank.Name == rank.Rank.Name) > 1)
                {
                    await _rankUserRepository.DeleteAsync(rank);
                    _ = ranks.Remove(rank);
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
                    Image=item.Image,
                    Type = item.Type,
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
            _ = await _rankUserRepository.GetAll().Where(s => ids.Contains(s.Id)).ExecuteUpdateAsync(s=>s.SetProperty(s => s.IsHidden, b => !b.IsHidden));
        }
    }
}
