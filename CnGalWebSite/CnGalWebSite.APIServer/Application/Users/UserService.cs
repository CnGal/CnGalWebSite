﻿using BootstrapBlazor.Components;
using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.Application.Ranks;
using CnGalWebSite.APIServer.Application.Users.Dtos;
using CnGalWebSite.APIServer.DataReositories;

using CnGalWebSite.DataModel.ExamineModel.Users;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Space;
using IdentityModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.Users
{
    public class UserService : IUserService
    {
        private readonly IRepository<ApplicationUser, string> _userRepository;
        private readonly IRepository<UserIntegral, string> _userIntegralRepository;
        private readonly IRepository<ThirdPartyLoginInfor, long> _thirdPartyLoginInforRepository;
        private readonly IRepository<Examine, long> _examineRepository;
        private readonly IRepository<Article, long> _articleRepository;
        private readonly IRepository<Entry, long> _entryRepository;
        private readonly IRepository<Video, long> _videoRepository;
        private readonly IRepository<UserCertification, long> _userCertificationRepository;
        private readonly IRepository<FavoriteObject, long> _favoriteObjectRepository;
        private readonly IRepository<SignInDay, long> _signInDayRepository;
        private readonly IRepository<PlayedGame, long> _playedGameRepository;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IAppHelper _appHelper;
        private readonly IRankService _rankService;
        private readonly IHttpContextAccessor _accessor;

        private static readonly ConcurrentDictionary<Type, Func<IEnumerable<ApplicationUser>, string, SortOrder, IEnumerable<ApplicationUser>>> SortLambdaCacheApplicationUser = new();

        public UserService(IRepository<ApplicationUser, string> userRepository, IConfiguration configuration, IHttpClientFactory clientFactory, IRepository<ThirdPartyLoginInfor, long> thirdPartyLoginInforRepository, IHttpContextAccessor accessor,
        IRepository<Examine, long> examineRepository, IRepository<UserIntegral, string> userIntegralRepository, IAppHelper appHelper, IRankService rankService, IRepository<Article, long> articleRepository, IRepository<FavoriteObject, long> favoriteObjectRepository,
        IRepository<SignInDay, long> signInDayRepository, IRepository<PlayedGame, long> playedGameRepository, IRepository<Entry, long> entryRepository, IRepository<UserCertification, long> userCertificationRepository, IRepository<Video, long> videoRepository)
        {
            _userRepository = userRepository;
            _configuration = configuration;
            _clientFactory = clientFactory;
            _thirdPartyLoginInforRepository = thirdPartyLoginInforRepository;
            _examineRepository = examineRepository;
            _userIntegralRepository = userIntegralRepository;
            _appHelper = appHelper;
            _rankService = rankService;
            _articleRepository = articleRepository;
            _favoriteObjectRepository = favoriteObjectRepository;
            
            _signInDayRepository = signInDayRepository;
            _playedGameRepository = playedGameRepository;
            _entryRepository = entryRepository;
            _userCertificationRepository = userCertificationRepository;
            _videoRepository = videoRepository;
            _accessor = accessor;
        }

        public async Task<PagedResultDto<ApplicationUser>> GetPaginatedResult(GetUserInput input)
        {
            var query = _userRepository.GetAll().AsNoTracking();

            //判断输入的查询名称是否为空
            if (!string.IsNullOrWhiteSpace(input.FilterText))
            {
                query = query.Where(s => s.UserName.Contains(input.FilterText)
                  || s.MainPageContext.Contains(input.FilterText)
                  || s.PersonalSignature.Contains(input.FilterText));
            }
            //统计查询数据的总条数
            var count = query.Count();
            //根据需求进行排序，然后进行分页逻辑的计算
            query = query.OrderBy(input.Sorting).Skip((input.CurrentPage - 1) * input.MaxResultCount).Take(input.MaxResultCount);

            //将结果转换为List集合 加载到内存中
            List<ApplicationUser> models = null;
            if (count != 0)
            {
                models = await query.AsNoTracking().Include(s => s.Examines).ToListAsync();
            }
            else
            {
                models = new List<ApplicationUser>();
            }


            var dtos = new PagedResultDto<ApplicationUser>
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

        public async Task<QueryData<ListUserAloneModel>> GetPaginatedResult(CnGalWebSite.DataModel.ViewModel.Search.QueryPageOptions options, ListUserAloneModel searchModel)
        {
            var items = _userRepository.GetAll().AsNoTracking();
            // 处理高级搜索
            if (!string.IsNullOrWhiteSpace(searchModel.UserName))
            {
                items = items.Where(item => item.UserName.Contains(searchModel.UserName, StringComparison.OrdinalIgnoreCase) );
            }

            if (!string.IsNullOrWhiteSpace(searchModel.PersonalSignature))
            {
                items = items.Where(item => item.PersonalSignature.Contains(searchModel.PersonalSignature, StringComparison.OrdinalIgnoreCase) );
            }
            if (!string.IsNullOrWhiteSpace(searchModel.Email))
            {
                items = items.Where(item => item.Email.Contains(searchModel.Email, StringComparison.OrdinalIgnoreCase) );
            }
            if (!string.IsNullOrWhiteSpace(searchModel.Id))
            {
                items = items.Where(item => item.Id.Contains(searchModel.Id, StringComparison.OrdinalIgnoreCase) );
            }

            // 处理 SearchText 模糊搜索
            if (!string.IsNullOrWhiteSpace(options.SearchText))
            {
                items = items.Where(item => (item.UserName !=null&& item.UserName.Contains(options.SearchText) )
                             || (item.PersonalSignature != null && item.PersonalSignature.Contains(options.SearchText))
                              || (item.Email != null && item.Email.Contains(options.SearchText))
                               || (item.Id != null && item.Id.Contains(options.SearchText))); 
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
            var itemsReal = await items.Skip((options.PageIndex - 1) * options.PageItems).Take(options.PageItems).ToListAsync();

            //复制数据
            var resultItems = new List<ListUserAloneModel>();
            foreach (var item in itemsReal)
            {
                resultItems.Add(new ListUserAloneModel
                {
                    Id = item.Id,
                    UserName = item.UserName,
                    Email = item.Email,
                    PersonalSignature = item.PersonalSignature,
                    Birthday = item.Birthday,
                    RegistTime = item.RegistTime,
                    CanComment = item.CanComment ?? true,
                    OnlineTime = (double)item.OnlineTime / (60 * 60),
                    LastOnlineTime = item.LastOnlineTime,
                    UnsealTime = item.UnsealTime,
                    DisplayIntegral = item.DisplayIntegral,
                    DisplayContributionValue = item.DisplayContributionValue,
                    IsPassedVerification = item.IsPassedVerification,
                    IsShowFavotites = item.IsShowFavotites
                });
            }

            return new QueryData<ListUserAloneModel>()
            {
                Items = resultItems,
                TotalCount = total,
                IsSorted = isSorted,
                // IsFiltered = isFiltered
            };
        }

        public Task UpdateUserDataMain(ApplicationUser user, UserMain examine)
        {
            user.UserName = examine.UserName;
            user.PersonalSignature = examine.PersonalSignature;
            user.BackgroundImage = examine.BackgroundImage;
            user.PhotoPath = examine.PhotoPath;
            user.SBgImage = examine.SBgImage;
            user.MBgImage = examine.MBgImage;
            return Task.CompletedTask;
        }
       
        public Task UpdateUserDataMainPage(ApplicationUser user, string examine)
        {
            user.MainPageContext = examine;
            return Task.CompletedTask;
        }

        public Task UpdateUserData(ApplicationUser user, Examine examine)
        {
            switch (examine.Operation)
            {
                case Operation.EditUserMain:
                    UserMain userMain = null;
                    using (TextReader str = new StringReader(examine.Context))
                    {
                        var serializer = new JsonSerializer();
                        userMain = (UserMain)serializer.Deserialize(str, typeof(UserMain));
                    }

                    UpdateUserDataMain(user, userMain);
                    break;
                case Operation.UserMainPage:
                    UpdateUserDataMainPage(user, examine.Context);
                    break;
            }
            return Task.CompletedTask;

        }

        public async Task<UserEditInforBindModel> GetUserEditInforBindModel(ApplicationUser user)
        {
            var model = new UserEditInforBindModel
            {
                Id = user.Id,
                Name = user.UserName
            };


            //拉取审核数据
            var examines = await _examineRepository.GetAll().AsNoTracking()
                .Where(s => s.ApplicationUserId == user.Id)
                .Select(n => new { n.IsPassed, n.Operation, n.ApplyTime, n.Id })
                .ToListAsync();

            model.EditCount = examines.Count;
            model.LastEditTime = examines.Count == 0 ? null : examines.Max(s => s.ApplyTime);

            //计算成功率和失败率和待审核率
            model.PassedExamineCount = examines.Count(s => s.IsPassed == true);
            model.UnpassedExamineCount = examines.Count(s => s.IsPassed == false);
            model.PassingExamineCount = examines.Count(s => s.IsPassed == null);


            //计算可用空间
            if (user.FileManager != null)
            {
                model.UsedFilesSpace = user.FileManager.UsedSize;
                model.TotalFilesSpace = user.FileManager.TotalSize;
            }
            else
            {
                model.UsedFilesSpace = 0;
                model.TotalFilesSpace = 500 * 1024 * 1024;
            }

            //计算编辑数据 获取过去30天的词条编辑数量
            //从30天前开始倒数 遇到第一个开始添加
            var MaxCountLineDay = DateTime.Now.DayOfYear;
            var tempDateTimeNow = DateTime.Now.ToCstTime();
            var editCounts = await _examineRepository.GetAll().AsNoTracking()
                .Where(s => s.ApplicationUserId == user.Id && s.ApplyTime.Date > tempDateTimeNow.Date.AddDays(-MaxCountLineDay))
                   // 先进行了时间字段变更为String字段，切只保留到天
                   // 采用拼接的方式
                   .Select(n => new { Time = n.ApplyTime.Date })
                  // 分类
                  .GroupBy(n => n.Time)
                  // 返回汇总样式
                  .Select(n => new KeyValuePair<DateTime, int>(n.Key, n.Count()))
                   .Sort("Time", BootstrapBlazor.Components.SortOrder.Asc)
                  .ToListAsync();
            model.EditCountList = editCounts;


            return model;
        }

        public async Task AddUserIntegral(AddUserIntegralModel model)
        {
            await _userIntegralRepository.InsertAsync(new UserIntegral
            {
                ApplicationUserId = model.UserId,
                Count = model.Count,
                Note = model.Note,
                Type = model.Type,
            });
        }

        public async Task<UserInforViewModel> GetUserInforViewModel(ApplicationUser user,bool ignoreAddinfor=false)
        {
            var model = new UserInforViewModel
            {
                Id = user.Id,
                PersonalSignature = user.PersonalSignature,
                Name = user.UserName,
                SBgImage=user.SBgImage,
                MBgImage=user.MBgImage,
                PhotoPath = _appHelper.GetImagePath(user.PhotoPath, "user.png"),
                BackgroundImage = _appHelper.GetImagePath(user.BackgroundImage, "userbackground.jpg"),
                Ranks = await _rankService.GetUserRanks(user),
            };

            if(ignoreAddinfor)
            {
                return model;
            }

            model.Integral = user.DisplayIntegral;
            model.EditCount = await _examineRepository.CountAsync(s => s.ApplicationUserId == user.Id && s.IsPassed == true);
            model.ArticleCount = await _articleRepository.CountAsync(s => s.CreateUserId == user.Id && string.IsNullOrWhiteSpace(s.Name) == false && s.IsHidden == false);
            model.VideoCount = await _videoRepository.CountAsync(s => s.CreateUserId == user.Id && string.IsNullOrWhiteSpace(s.Name) == false && s.IsHidden == false);
            model.ArticleReadCount = await _articleRepository.GetAll().AsNoTracking().Where(s => s.CreateUserId == user.Id && string.IsNullOrWhiteSpace(s.Name) == false && s.IsHidden == false).SumAsync(s=>s.ReaderCount) + await _videoRepository.GetAll().Where(s => s.CreateUserId == user.Id && string.IsNullOrWhiteSpace(s.Name) == false && s.IsHidden == false).SumAsync(s => s.ReaderCount);

            //计算连续签到天数和今天是否签到
            model.IsSignIn = false;
            model.SignInDays = 0;

            //东八区转换
            var now = DateTime.Now.ToCstTime();//.AddHours(8);

            if (user.SignInDays != null)
            {
                if (user.SignInDays.Any(s => s.Time.Date == now.Date))
                {
                    model.IsSignIn = true;
                    while (user.SignInDays.Any(s => s.Time.Date == now.AddDays(-model.SignInDays).Date))
                    {
                        model.SignInDays++;
                    }
                }
                else
                {
                    if (user.SignInDays.Any(s => s.Time.Date == now.Date.AddDays(-1)))
                    {
                        while (user.SignInDays.Any(s => s.Time.Date == now.AddDays(-model.SignInDays - 1).Date))
                        {
                            model.SignInDays++;
                        }
                    }
                }
            }
            return model;

        }


        public async Task<string> GenerateBindGroupQQCode(ApplicationUser user)
        {
            return await _appHelper.SetUserLoginKeyAsync(ToolHelper.Base64EncodeString(user.Id),true);
        }

        public async Task<Result> BindGroupQQ(string code, long groupQQ)
        {
            var userId = ToolHelper.Base64DecodeString(await _appHelper.GetUserFromLoginKeyAsync(code));
            if (string.IsNullOrWhiteSpace(userId))
            {
                await _appHelper.AddErrorCount(groupQQ.ToString());
                return new Result { Successful = false, Error = $"身份验证失败" };
            }
            var user  = await _userRepository.FirstOrDefaultAsync(s=>s.Id==userId);
            if(user == null)
            {
                return new Result { Successful = false, Error = $"找不到该用户" };
            }
            user.GroupQQ = groupQQ;
            await _userRepository.UpdateAsync(user);

            return new Result { Successful = true };
        }

        public async Task UpdateUserIntegral(ApplicationUser user)
        {
            //计算积分
            //编辑 词条  标签  消歧义页  评论  发表文章 游玩记录
            //积分  10    8      2         5     50       50
            //
            var signInDaysIntegral = 5;
            var entryIntegral = 50;

            var tagIntegral = signInDaysIntegral * 6;
            var disambigIntegral = signInDaysIntegral * 6;
            var peripheryIntegral = entryIntegral;
            var commentIntegral = signInDaysIntegral;
            var commentLimited = 5;
            var articleIntegral = entryIntegral;
            var playedGameIntegral = entryIntegral;
            var videoIntegral = articleIntegral;

            try
            {
                //编辑
                var temp_1 = await _examineRepository.GetAll().Where(s => s.ApplicationUserId == user.Id && s.IsPassed == true)
                 .Select(n => new { Time = n.ApplyTime.Date, n.EntryId, n.TagId, n.ArticleId, n.CommentId, n.DisambigId, n.PeripheryId })
                 // 分类
                 .ToListAsync();
                // 返回汇总样式
                var temp_2 = temp_1.GroupBy(s => s.Time);
                var integral_1 = (temp_2.Select(s => s.Where(s => s.EntryId != null).GroupBy(s => s.EntryId).Count()).Sum() * entryIntegral)
                                  + (temp_2.Select(s => s.Where(s => s.TagId != null).GroupBy(s => s.TagId).Count()).Sum() * tagIntegral)
                                  + (temp_2.Select(s => s.Where(s => s.DisambigId != null).GroupBy(s => s.DisambigId).Count()).Sum() * disambigIntegral)
                                  + (temp_2.Select(s => s.Where(s => s.PeripheryId != null).GroupBy(s => s.PeripheryId).Count()).Sum() * peripheryIntegral)
                                  + (temp_2.Select(s => s.Where(s => s.CommentId != null).GroupBy(s => s.DisambigId)
                                            .Select(s => s.Count() > commentLimited ? commentLimited : s.Count()).Sum()).Sum() * commentLimited);


                //发表文章
                var integral_2 = await _articleRepository.CountAsync(s => s.CreateUserId == user.Id&&s.IsHidden==false&&string.IsNullOrWhiteSpace(s.Name)==false) * articleIntegral;
                //签到
                var integral_3 = await _signInDayRepository.CountAsync(s => s.ApplicationUserId == user.Id) * signInDaysIntegral;

                var integral_4 = await _userIntegralRepository.GetAll().Where(s => s.ApplicationUserId == user.Id && s.Type == UserIntegralType.Integral).Select(s => s.Count).SumAsync();
                //游玩记录
                var integral_5 = await _playedGameRepository.CountAsync(s => s.ApplicationUserId == user.Id && s.ShowPublicly && string.IsNullOrWhiteSpace(s.PlayImpressions) == false && s.PlayImpressions.Length > ToolHelper.MinValidPlayImpressionsLength) * playedGameIntegral;
                //发表文章
                var integral_6 = await _videoRepository.CountAsync(s => s.CreateUserId == user.Id && s.IsHidden == false && string.IsNullOrWhiteSpace(s.Name) == false) * videoIntegral;

                user.DisplayIntegral = integral_1 + integral_2 + integral_3 + integral_4 + integral_5+integral_6;


                //计算贡献值

                var contributionValue_1 = await _userIntegralRepository.GetAll().Where(s => s.ApplicationUserId == user.Id && s.Type == UserIntegralType.ContributionValue).Select(s => s.Count).SumAsync();

                user.DisplayContributionValue = contributionValue_1 + await _examineRepository.GetAll().Where(s => s.ApplicationUserId == user.Id).SumAsync(s => s.ContributionValue);

                _ = await _userRepository.UpdateAsync(user);
            }
            catch (Exception)
            {

            }
        }

        public bool CheckCurrentUserRole(string role)
        {
            var id = _accessor.HttpContext.User?.Claims?.GetUserId();
            if (string.IsNullOrWhiteSpace(id))
            {
                return false;
            }

            return _accessor.HttpContext.User.Claims.Any(s => (s.Type == JwtClaimTypes.Role|| s.Type == ClaimTypes.Role) && s.Value == role);
        }

        #region 用户认证

        public async Task<QueryData<ListUserCertificationAloneModel>> GetPaginatedResult(DataModel.ViewModel.Search.QueryPageOptions options, ListUserCertificationAloneModel searchModel)
        {
            var items = _userCertificationRepository.GetAll()
                .Include(s => s.ApplicationUser)
                .Include(s => s.Entry)
                .Where(s => s.EntryId != null && s.EntryId != 0)
                .AsNoTracking();

            // 处理高级搜索

            if (!string.IsNullOrWhiteSpace(searchModel.UserId))
            {
                items = items.Where(item => item.ApplicationUserId.Contains(searchModel.UserId, StringComparison.OrdinalIgnoreCase));
            }
            if (!string.IsNullOrWhiteSpace(searchModel.UserName))
            {
                items = items.Where(item => item.ApplicationUser.UserName.Contains(searchModel.UserName, StringComparison.OrdinalIgnoreCase));
            }



            // 处理 SearchText 模糊搜索
            if (!string.IsNullOrWhiteSpace(options.SearchText))
            {
                items = items.Where(item => item.ApplicationUserId.Contains(options.SearchText));
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
            var itemsReal = await items.Skip((options.PageIndex - 1) * options.PageItems).Take(options.PageItems).ToListAsync();

            //复制数据
            var resultItems = new List<ListUserCertificationAloneModel>();
            foreach (var item in itemsReal)
            {
                resultItems.Add(new ListUserCertificationAloneModel
                {
                    Id = item.Id,
                    UserName = item.ApplicationUser?.UserName,
                    UserId = item.ApplicationUserId,
                    CertificationTime = item.CertificationTime,
                    EntryId = item.Entry.Id,
                    EntryName = item.Entry.Name
                });
            }

            return new QueryData<ListUserCertificationAloneModel>()
            {
                Items = resultItems,
                TotalCount = total,
                IsSorted = isSorted,
                // IsFiltered = isFiltered
            };
        }


        public async Task UpdateUserCertificationDataMain(UserCertification userCertification, UserCertificationMain examine)
        {
            if(examine.EntryId==0)
            {
                userCertification.EntryId = null;
                userCertification.Entry = null;
            }
            else
            {
                userCertification.EntryId = examine.EntryId;
                userCertification.Entry = await _entryRepository.FirstOrDefaultAsync(s => s.Id == examine.EntryId);
            }

            userCertification.CertificationTime = DateTime.Now.ToCstTime();
        }

        public async Task UpdateUserCertificationData(UserCertification userCertification, Examine examine)
        {
            switch (examine.Operation)
            {
                case Operation.RequestUserCertification:
                    UserCertificationMain userCertificationMain = null;
                    using (TextReader str = new StringReader(examine.Context))
                    {
                        var serializer = new JsonSerializer();
                        userCertificationMain = (UserCertificationMain)serializer.Deserialize(str, typeof(UserCertificationMain));
                    }

                    await UpdateUserCertificationDataMain(userCertification, userCertificationMain);
                    break;
            }

            return;

        }

        public async Task<List<string>> GetAllNotCertificatedEntriesAsync(EntryType? type = null)
        {
            var entryIds =await _userCertificationRepository.GetAll().Include(s => s.Entry).Where(s => string.IsNullOrWhiteSpace(s.ApplicationUserId) && s.EntryId != null).Select(s => s.EntryId.Value).ToListAsync();

            var temp = _entryRepository.GetAll().AsNoTracking().Where(s => s.IsHidden != true && string.IsNullOrWhiteSpace(s.Name) == false && entryIds.Contains(s.Id) == false);
            if (type != null)
            {
                temp = temp.Where(s => s.Type == type);
            }
            return await temp.Select(s => s.Name).ToListAsync();
        }
        #endregion
    }
}
