using BootstrapBlazor.Components;
using CnGalWebSite.APIServer.Application.Users.Dtos;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.DataModel.Application.Dtos;
using CnGalWebSite.DataModel.ExamineModel;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Space;
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
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.Users
{
    public class UserService : IUserService
    {
        private readonly IRepository<ApplicationUser, string> _userRepository;
        private readonly IRepository<UserIntegral, string> _userIntegralRepository;
        private readonly IRepository<ThirdPartyLoginInfor, long> _thirdPartyLoginInforRepository;
        private readonly IRepository<Examine, long> _examineRepository;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _clientFactory;

        private static readonly ConcurrentDictionary<Type, Func<IEnumerable<ApplicationUser>, string, SortOrder, IEnumerable<ApplicationUser>>> SortLambdaCacheApplicationUser = new();
        public UserService(IRepository<ApplicationUser, string> userRepository, IConfiguration configuration, IHttpClientFactory clientFactory, IRepository<ThirdPartyLoginInfor, long> thirdPartyLoginInforRepository,
            IRepository<Examine, long> examineRepository, IRepository<UserIntegral, string> userIntegralRepository)
        {
            _userRepository = userRepository;
            _configuration = configuration;
            _clientFactory = clientFactory;
            _thirdPartyLoginInforRepository = thirdPartyLoginInforRepository;
            _examineRepository = examineRepository;
            _userIntegralRepository = userIntegralRepository;
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

        public Task<QueryData<ListUserAloneModel>> GetPaginatedResult(CnGalWebSite.DataModel.ViewModel.Search.QueryPageOptions options, ListUserAloneModel searchModel)
        {
            IEnumerable<ApplicationUser> items = _userRepository.GetAll().AsNoTracking();
            // 处理高级搜索
            if (!string.IsNullOrWhiteSpace(searchModel.UserName))
            {
                items = items.Where(item => item.UserName?.Contains(searchModel.UserName, StringComparison.OrdinalIgnoreCase) ?? false);
            }

            if (!string.IsNullOrWhiteSpace(searchModel.PersonalSignature))
            {
                items = items.Where(item => item.PersonalSignature?.Contains(searchModel.PersonalSignature, StringComparison.OrdinalIgnoreCase) ?? false);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.Email))
            {
                items = items.Where(item => item.Email?.Contains(searchModel.Email, StringComparison.OrdinalIgnoreCase) ?? false);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.Id))
            {
                items = items.Where(item => item.Id?.Contains(searchModel.Id, StringComparison.OrdinalIgnoreCase) ?? false);
            }

            // 处理 SearchText 模糊搜索
            if (!string.IsNullOrWhiteSpace(options.SearchText))
            {
                items = items.Where(item => (item.UserName?.Contains(options.SearchText) ?? false)
                             || (item.PersonalSignature?.Contains(options.SearchText) ?? false)
                              || (item.Email?.Contains(options.SearchText) ?? false)
                               || (item.Id?.Contains(options.SearchText) ?? false));
            }

            // 排序
            var isSorted = false;
            if (!string.IsNullOrWhiteSpace(options.SortName))
            {
                // 外部未进行排序，内部自动进行排序处理
                var invoker = SortLambdaCacheApplicationUser.GetOrAdd(typeof(ApplicationUser), key => LambdaExtensions.GetSortLambda<ApplicationUser>().Compile());
                items = invoker(items, options.SortName, (BootstrapBlazor.Components.SortOrder)options.SortOrder);
                isSorted = true;
            }

            // 设置记录总数
            var total = items.Count();

            // 内存分页
            items = items.Skip((options.PageIndex - 1) * options.PageItems).Take(options.PageItems).ToList();

            //复制数据
            var resultItems = new List<ListUserAloneModel>();
            foreach (var item in items)
            {
                resultItems.Add(new ListUserAloneModel
                {
                    Id = item.Id,
                    UserName = item.UserName,
                    Email = item.Email,
                    PersonalSignature = item.PersonalSignature,
                    Birthday = item.Birthday,
                    RegistTime = item.RegistTime,
                    Integral = item.Integral,
                    CanComment = item.CanComment ?? true,
                    ContributionValue = item.ContributionValue,
                    OnlineTime = (double)item.OnlineTime / (60 * 60),
                    LastOnlineTime = item.LastOnlineTime,
                    UnsealTime = item.UnsealTime,
                    DisplayIntegral = item.DisplayIntegral,
                    DisplayContributionValue = item.DisplayContributionValue,
                    IsPassedVerification = item.IsPassedVerification,
                    IsShowFavotites = item.IsShowFavotites
                });
            }

            return Task.FromResult(new QueryData<ListUserAloneModel>()
            {
                Items = resultItems,
                TotalCount = total,
                IsSorted = isSorted,
                // IsFiltered = isFiltered
            });
        }

        #region 三方登入 计划重做

        public async Task<bool> AddUserThirdPartyLogin(string id_token, ApplicationUser user, ThirdPartyLoginType type)
        {
            return type switch
            {
                ThirdPartyLoginType.Microsoft => await AddUserMicrosoftThirdPartyLogin(id_token, user),
                ThirdPartyLoginType.GitHub => await AddUserGithubThirdPartyLogin(id_token, user),
                ThirdPartyLoginType.Gitee => await AddUserGiteeThirdPartyLogin(id_token, user),
                ThirdPartyLoginType.QQ => await AddUserQQThirdPartyLogin(id_token, user),
                _ => false,
            };
        }
        public async Task<ApplicationUser> GetThirdPartyLoginUser(string id_token, ThirdPartyLoginType type)
        {
            return type switch
            {
                ThirdPartyLoginType.Microsoft => await GetMicrosoftThirdPartyLoginUser(id_token),
                ThirdPartyLoginType.GitHub => await GetGithubThirdPartyLoginUser(id_token),
                ThirdPartyLoginType.Gitee => await GetGiteeThirdPartyLoginUser(id_token),
                ThirdPartyLoginType.QQ => await GetQQThirdPartyLoginUser(id_token),
                _ => null,
            };
        }
        public async Task<string> GetThirdPartyLoginIdToken(string code, string returnUrl, bool isSSR, ThirdPartyLoginType type)
        {
            return type switch
            {
                ThirdPartyLoginType.Microsoft => await GetMicrosoftThirdPartyLoginIdToken(code, returnUrl),
                ThirdPartyLoginType.GitHub => await GetGithubThirdPartyLoginIdToken(code, returnUrl, isSSR),
                ThirdPartyLoginType.Gitee => await GetGiteeThirdPartyLoginIdToken(code, returnUrl),
                ThirdPartyLoginType.QQ => await GetQQThirdPartyLoginIdToken(code, returnUrl),
                _ => null,
            };
        }

        private async Task<bool> AddUserMicrosoftThirdPartyLogin(string id_token, ApplicationUser user)
        {

            var result = ToolHelper.Base64DecodeString(id_token.Split('.')[1]);
            //获取名称 电子邮件 Id
            var obj = JObject.Parse(result);
            var name = obj["name"].ToString();
            var email = obj["email"].ToString();
            var id = obj["oid"].ToString();
            //查找是否已经存在三方登入信息 存在则覆盖
            var item = await _thirdPartyLoginInforRepository.FirstOrDefaultAsync(s => s.ApplicationUserId == user.Id && s.Type == ThirdPartyLoginType.Microsoft);
            if (item == null)
            {
                item = new ThirdPartyLoginInfor();
            }

            item.Email = email;
            item.ApplicationUser = user;
            item.ApplicationUserId = user.Id;
            item.Name = name;
            item.Type = ThirdPartyLoginType.Microsoft;
            item.UniqueId = id;

            if (item.Id == 0)
            {
                await _thirdPartyLoginInforRepository.InsertAsync(item);
            }
            else
            {
                await _thirdPartyLoginInforRepository.UpdateAsync(item);
            }

            return true;
        }

        private async Task<ApplicationUser> GetMicrosoftThirdPartyLoginUser(string id_token)
        {

            var result = ToolHelper.Base64DecodeString(id_token.Split('.')[1]);
            //获取名称 电子邮件 Id
            var obj = JObject.Parse(result);
            var id = obj["oid"].ToString();

            var item = await _thirdPartyLoginInforRepository.GetAll().AsNoTracking().Include(s => s.ApplicationUser).FirstOrDefaultAsync(s => s.UniqueId == id && s.Type == ThirdPartyLoginType.Microsoft);
            if (item == null)
            {
                return null;
            }
            else
            {
                return item.ApplicationUser;
            }
        }

        private async Task<string> GetMicrosoftThirdPartyLoginIdToken(string code, string returnUrl)
        {
            using var content = new MultipartFormDataContent();
            var client_id = _configuration["ThirdPartyLoginMicrosoft_client_id"];
            var client_secret = _configuration["ThirdPartyLoginMicrosoft_client_secret"];
            var resource = _configuration["ThirdPartyLoginMicrosoft_resource"];
            var tenant = _configuration["ThirdPartyLoginMicrosoft_tenant"];
            content.Add(
                content: new StringContent(client_id),
                name: "client_id");
            content.Add(
                content: new StringContent("authorization_code"),
                name: "grant_type");
            content.Add(
                content: new StringContent(returnUrl),
                name: "redirect_uri");
            content.Add(
                content: new StringContent(client_secret),
                name: "client_secret");
            content.Add(
                content: new StringContent(resource),
                name: "resource");
            content.Add(
                content: new StringContent(code),
                name: "code");

            var client = _clientFactory.CreateClient();

            var response = await client.PostAsync("https://login.microsoftonline.com/" + tenant + "/oauth2/token", content);
            if (response.IsSuccessStatusCode == false)
            {
                return null;
            }
            var newUploadResults = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(newUploadResults))
            {
                return null;
            }
            var obj = JObject.Parse(newUploadResults);
            var id_token = obj["id_token"].ToString();
            if (string.IsNullOrWhiteSpace(id_token))
            {
                return null;
            }
            //解码返回的信息

            return id_token;
        }


        private async Task<bool> AddUserGithubThirdPartyLogin(string id_token, ApplicationUser user)
        {

            //获取名称 电子邮件 Id
            var obj = JObject.Parse(id_token);
            var name = obj["name"].ToString();
            //string email = obj["email"].ToString();
            var id = obj["id"].ToString();
            //查找是否已经存在三方登入信息 存在则覆盖
            var item = await _thirdPartyLoginInforRepository.FirstOrDefaultAsync(s => s.ApplicationUserId == user.Id && s.Type == ThirdPartyLoginType.GitHub);
            if (item == null)
            {
                item = new ThirdPartyLoginInfor();
            }

            //item.Email = email;
            item.ApplicationUser = user;
            item.ApplicationUserId = user.Id;
            item.Name = name;
            item.Type = ThirdPartyLoginType.GitHub;
            item.UniqueId = id;

            if (item.Id == 0)
            {
                await _thirdPartyLoginInforRepository.InsertAsync(item);
            }
            else
            {
                await _thirdPartyLoginInforRepository.UpdateAsync(item);
            }

            return true;
        }

        private async Task<ApplicationUser> GetGithubThirdPartyLoginUser(string id_token)
        {

            //获取名称 电子邮件 Id
            var obj = JObject.Parse(id_token);
            var id = obj["id"].ToString();

            var item = await _thirdPartyLoginInforRepository.GetAll().AsNoTracking().Include(s => s.ApplicationUser).FirstOrDefaultAsync(s => s.UniqueId == id && s.Type == ThirdPartyLoginType.GitHub);
            if (item == null)
            {
                return null;
            }
            else
            {
                return item.ApplicationUser;
            }
        }

        private async Task<string> GetGithubThirdPartyLoginIdToken(string code, string returnUrl, bool isSSR)
        {
            using var content = new MultipartFormDataContent();
            string client_id;
            string client_secret;
            if (isSSR)
            {
                client_id = _configuration["ThirdPartyLoginGithub_SSR_client_id"];

                client_secret = _configuration["ThirdPartyLoginGithub_SSR_client_secret"];
            }
            else
            {
                client_id = _configuration["ThirdPartyLoginGithub_WASM_client_id"];

                client_secret = _configuration["ThirdPartyLoginGithub_WASM_client_secret"];
            }

            content.Add(
                content: new StringContent(client_id),
                name: "client_id");
            content.Add(
                content: new StringContent(returnUrl),
                name: "redirect_uri");
            content.Add(
                content: new StringContent(client_secret),
                name: "client_secret");
            content.Add(
                content: new StringContent(code),
                name: "code");

            var client = _clientFactory.CreateClient();

            var response = await client.PostAsync("https://github.com/login/oauth/access_token", content);
            if (response.IsSuccessStatusCode == false)
            {
                return null;
            }
            var newUploadResults = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(newUploadResults))
            {
                return null;
            }

            var access_token = ToolHelper.MidStrEx(newUploadResults, "access_token=", "&");

            if (string.IsNullOrWhiteSpace(access_token))
            {
                return null;
            }
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("token", access_token);
            //client.DefaultRequestHeaders.Add("Authorization", "token "+access_token);
            var re = await client.GetAsync("https://api.github.com/user");
            var id_token = await re.Content.ReadAsStringAsync();

            return id_token;
        }



        private async Task<bool> AddUserGiteeThirdPartyLogin(string id_token, ApplicationUser user)
        {
            //获取名称 电子邮件 Id
            var obj = JObject.Parse(id_token);
            var name = obj["name"].ToString();
            //string email = obj["email"].ToString();
            var id = obj["id"].ToString();
            //查找是否已经存在三方登入信息 存在则覆盖
            var item = await _thirdPartyLoginInforRepository.FirstOrDefaultAsync(s => s.ApplicationUserId == user.Id && s.Type == ThirdPartyLoginType.Gitee);
            if (item == null)
            {
                item = new ThirdPartyLoginInfor();
            }

            //item.Email = email;
            item.ApplicationUser = user;
            item.ApplicationUserId = user.Id;
            item.Name = name;
            item.Type = ThirdPartyLoginType.Gitee;
            item.UniqueId = id;

            if (item.Id == 0)
            {
                await _thirdPartyLoginInforRepository.InsertAsync(item);
            }
            else
            {
                await _thirdPartyLoginInforRepository.UpdateAsync(item);
            }

            return true;
        }

        private async Task<ApplicationUser> GetGiteeThirdPartyLoginUser(string id_token)
        {
            //获取名称 电子邮件 Id
            var obj = JObject.Parse(id_token);
            var id = obj["id"].ToString();

            var item = await _thirdPartyLoginInforRepository.GetAll().AsNoTracking().Include(s => s.ApplicationUser).FirstOrDefaultAsync(s => s.UniqueId == id && s.Type == ThirdPartyLoginType.Gitee);
            if (item == null)
            {
                return null;
            }
            else
            {
                return item.ApplicationUser;
            }
        }

        private async Task<string> GetGiteeThirdPartyLoginIdToken(string code, string returnUrl)
        {
            using var content = new MultipartFormDataContent();
            string client_id;
            string client_secret;

            client_id = _configuration["ThirdPartyLoginGitee_client_id"];

            client_secret = _configuration["ThirdPartyLoginGitee_client_secret"];

            var client = _clientFactory.CreateClient();

            var response = await client.PostAsync("https://gitee.com/oauth/token?grant_type=authorization_code&code=" + code + "&client_id=" + client_id + "&redirect_uri=" + returnUrl + "&client_secret=" + client_secret, content);
            if (response.IsSuccessStatusCode == false)
            {
                return "code expire";
            }
            var newUploadResults = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(newUploadResults))
            {
                return "code expire";
            }


            var obj = JObject.Parse(newUploadResults);
            var access_token = obj["access_token"].ToString();

            if (string.IsNullOrWhiteSpace(access_token))
            {
                return null;
            }
            var id_token = await client.GetStringAsync("https://gitee.com/api/v5/user?access_token=" + access_token);

            return id_token;
        }


        private async Task<bool> AddUserQQThirdPartyLogin(string id_token, ApplicationUser user)
        {
            //获取名称 电子邮件 Id
            var id = id_token.Split("|")[0];
            var name = id_token.Split("|")[1];

            // string name = obj["name"].ToString();
            //string email = obj["email"].ToString();
            //查找是否已经存在三方登入信息 存在则覆盖
            var item = await _thirdPartyLoginInforRepository.FirstOrDefaultAsync(s => s.ApplicationUserId == user.Id && s.Type == ThirdPartyLoginType.QQ);
            if (item == null)
            {
                item = new ThirdPartyLoginInfor();
            }

            //item.Email = email;
            item.ApplicationUser = user;
            item.ApplicationUserId = user.Id;
            item.Name = name;
            item.Type = ThirdPartyLoginType.QQ;
            item.UniqueId = id;

            if (item.Id == 0)
            {
                await _thirdPartyLoginInforRepository.InsertAsync(item);
            }
            else
            {
                await _thirdPartyLoginInforRepository.UpdateAsync(item);
            }

            //更新验证状态
            if (user.IsPassedVerification == false)
            {
                user.IsPassedVerification = true;
                await _userRepository.UpdateAsync(user);
            }

            return true;
        }

        private async Task<ApplicationUser> GetQQThirdPartyLoginUser(string id_token)
        {
            //获取名称 电子邮件 Id
            var id = id_token.Split("|")[0];

            var item = await _thirdPartyLoginInforRepository.GetAll().AsNoTracking().Include(s => s.ApplicationUser).FirstOrDefaultAsync(s => s.UniqueId == id && s.Type == ThirdPartyLoginType.QQ);
            if (item == null)
            {
                return null;
            }
            else
            {
                return item.ApplicationUser;
            }
        }

        private async Task<string> GetQQThirdPartyLoginIdToken(string code, string returnUrl)
        {
            string client_id;
            string client_secret;

            client_id = _configuration["ThirdPartyLoginQQ_client_id"];

            client_secret = _configuration["ThirdPartyLoginQQ_client_secret"];

            var client = _clientFactory.CreateClient();

            var newUploadResults = await client.GetStringAsync("https://graph.qq.com/oauth2.0/token?grant_type=authorization_code&code=" + code + "&client_id=" + client_id + "&redirect_uri=" + returnUrl + "&client_secret=" + client_secret + "&fmt=json");


            if (string.IsNullOrWhiteSpace(newUploadResults))
            {
                return null;
            }
            else if (newUploadResults.Contains("code expire") || newUploadResults.Contains("code is reused error"))
            {
                return "code expire";
            }
            else if (newUploadResults.Contains("access_token") == false)
            {
                return null;
            }

            var obj = JObject.Parse(newUploadResults);
            var access_token = obj["access_token"].ToString();

            if (string.IsNullOrWhiteSpace(access_token))
            {
                return null;
            }
            var id_token = await client.GetStringAsync("https://graph.qq.com/oauth2.0/me?access_token=" + access_token + "&fmt=json");
            var jsonObj = JObject.Parse(id_token);
            var id = jsonObj["openid"].ToString();

            var namejson = await client.GetStringAsync("https://graph.qq.com/user/get_user_info?access_token=" + access_token + "&oauth_consumer_key=" + client_id + "&openid=" + id);
            jsonObj = JObject.Parse(namejson);
            var result = id + "|" + jsonObj["nickname"].ToString();

            return result;
        }
        #endregion

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

        public int GetUserLevel(ApplicationUser user)
        {
            return ToolHelper.GetUserLevel(user.DisplayIntegral);
        }

        public async Task<UserEditInforBindModel> GetUserEditInforBindModel(ApplicationUser user)
        {
            var model = new UserEditInforBindModel
            {
                Id = user.Id,
                Name = user.UserName
            };


            //拉取审核数据
            var examines = await _examineRepository.GetAll().Where(s => s.ApplicationUserId == user.Id)
                .Select(n => new { n.IsPassed, n.Operation, n.Context, n.EntryId, n.ArticleId, n.TagId, n.ApplyTime, n.Id })
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
            var editCounts = await _examineRepository.GetAll().Where(s => s.ApplicationUserId == user.Id && s.ApplyTime.Date > tempDateTimeNow.Date.AddDays(-MaxCountLineDay))
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

    }
}
