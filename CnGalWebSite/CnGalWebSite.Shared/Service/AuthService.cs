using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.Shared.Provider;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace CnGalWebSite.Shared.Service
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        private readonly ILocalStorageService _localStorage;

        public AuthService(HttpClient httpClient,
            AuthenticationStateProvider authenticationStateProvider,
            ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _authenticationStateProvider = authenticationStateProvider;
            _localStorage = localStorage;
        }
        /// <summary>
        /// 提交registerModel给accounts controller并返回RegisterResult给调用者
        /// </summary>
        /// <param name="registerModel"></param>
        /// <returns></returns>
        public async Task<Result> Register(RegisterModel registerModel)
        {
            try
            {
                var result = await _httpClient.PostAsJsonAsync<RegisterModel>(ToolHelper.WebApiPath + "api/account/register", registerModel);
                var jsonContent = result.Content.ReadAsStringAsync().Result;
                var obj = JsonSerializer.Deserialize<Result>(jsonContent, ToolHelper.options);
                return obj;
            }
            catch (Exception exc)
            {
                return new Result { Successful = false, Error = exc.Message };
            }

        }
        /// <summary>
        /// 类似于Register 方法，它将LoginModel 发送给login controller
        /// 但是，当返回一个成功的结果时
        /// 它将返回一个授权令牌并持久化到local storge
        /// </summary>
        /// <param name="loginModel"></param>
        /// <returns></returns>
        public async Task<LoginResult> Login(LoginModel loginModel)
        {
            try
            {
                var result = await _httpClient.PostAsJsonAsync<LoginModel>(ToolHelper.WebApiPath + "api/account/Login", loginModel);
                var jsonContent = result.Content.ReadAsStringAsync().Result;
                var obj = JsonSerializer.Deserialize<LoginResult>(jsonContent, ToolHelper.options);

                if (obj.Code == LoginResultCode.OK)
                {
                    //记住 则保存令牌到本地
                    if (loginModel.RememberMe)
                    {
                        await _localStorage.SetItemAsync("authToken", obj.Token);
                    }

                    ((ApiAuthenticationStateProvider)_authenticationStateProvider).MarkUserAsAuthenticated(obj.Token);
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", obj.Token);

                    return obj;
                }

                return obj;
            }
            catch (Exception exc)
            {
                return new LoginResult { Code = LoginResultCode.WrongUserNameOrPassword, ErrorDescribe = exc.Message };
            }
        }
        /// <summary>
        /// 类似于Register 方法，它将LoginModel 发送给login controller
        /// 但是，当返回一个成功的结果时
        /// 它将返回一个授权令牌并持久化到local storge
        /// </summary>
        /// <param name="loginModel"></param>
        /// <returns></returns>
        public async Task<bool> Login(string JwtToken)
        {
            if (string.IsNullOrWhiteSpace(JwtToken))
            {
                return false;
            }
            try
            {
                await _localStorage.SetItemAsync("authToken", JwtToken);
                ((ApiAuthenticationStateProvider)_authenticationStateProvider).MarkUserAsAuthenticated(JwtToken);
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JwtToken);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        /// <summary>
        /// 类似于 Login 方法
        /// 但是，当返回一个成功的结果时
        /// 它将返回一个授权令牌并持久化到local storge
        /// </summary>
        /// <returns></returns>
        public async Task<LoginResult> Refresh()
        {
            //是否记住用户
            var isRemerber = false;
            try
            {
                //判断是否登入
                var userState = await _authenticationStateProvider.GetAuthenticationStateAsync();
                if (userState.User.Identity.IsAuthenticated == false)
                {
                    return null;
                }

                //读取本地令牌到Http请求头
                var savedToken = await _localStorage.GetItemAsync<string>("authToken");
                if (string.IsNullOrWhiteSpace(savedToken) == false)
                {
                    isRemerber = true;
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", savedToken);
                }
            }
            catch
            {
                return null;
            }

            try
            {
                //调用刷新接口
                var result = await _httpClient.GetFromJsonAsync<LoginResult>(ToolHelper.WebApiPath + "api/account/RefreshJWToken");
                if (result.Code == LoginResultCode.OK)
                {
                    if (isRemerber)
                    {
                        await _localStorage.SetItemAsync("authToken", result.Token);
                    }

                    ((ApiAuthenticationStateProvider)_authenticationStateProvider).MarkUserAsAuthenticated(result.Token);
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.Token);

                    return result;
                }
                else
                {
                    //失败后清空登入
                    await _localStorage.RemoveItemAsync("authToken");
                    ((ApiAuthenticationStateProvider)_authenticationStateProvider).MarkUserAsLoggedOut();
                    _httpClient.DefaultRequestHeaders.Authorization = null;
                    return result;

                }
            }
            catch
            {
                await _localStorage.RemoveItemAsync("authToken");
                ((ApiAuthenticationStateProvider)_authenticationStateProvider).MarkUserAsLoggedOut();
                _httpClient.DefaultRequestHeaders.Authorization = null;
                return new LoginResult { Code = LoginResultCode.WrongUserNameOrPassword, ErrorDescribe = "登入已过期" };
            }
        }

        /// <summary>
        /// 执行与Login 方法相反的操作。
        /// </summary>
        /// <returns></returns>
        public async Task Logout()
        {
            await _localStorage.RemoveItemAsync("authToken");
            ((ApiAuthenticationStateProvider)_authenticationStateProvider).MarkUserAsLoggedOut();
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
        /// <summary>
        /// 注册用户时验证电子邮箱
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<ConfirmEmailRegisterResult> ConfirmEmailRegister(ConfirmEmailRegisterModel model)
        {
            var result = await _httpClient.PostAsJsonAsync<ConfirmEmailRegisterModel>(ToolHelper.WebApiPath + "api/account/ConfirmEmailRegister", model);
            var jsonContent = result.Content.ReadAsStringAsync().Result;
            var obj = JsonSerializer.Deserialize<ConfirmEmailRegisterResult>(jsonContent, ToolHelper.options);
            return obj;
        }

        /// <summary>
        /// 检查是否已经登入
        /// </summary>
        /// <returns></returns>
        public async Task<bool> IsLogin()
        {
            var userState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            if (userState.User.Identity.IsAuthenticated == false)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }

}
