using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.Application.OperationRecords;
using CnGalWebSite.APIServer.Application.Users;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Accounts;
using CnGalWebSite.DataModel.ViewModel.Admin;
using Gt3_server_csharp_aspnetcoremvc_bypass.Controllers.Sdk;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/account/[action]")]
    [ApiController]
    public class AccountAPIController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IAppHelper _appHelper;
        private readonly IRepository<UserOnlineInfor, long> _userOnlineInforRepository;
        private readonly IRepository<HistoryUser, int> _historyUserRepository;
        private readonly IRepository<ApplicationUser, int> _userRepository;
        private readonly IUserService _userService;
        private readonly IOperationRecordService _operationRecordService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AccountAPIController> _logger;


        public AccountAPIController(IRepository<UserOnlineInfor, long> userOnlineInforRepository, UserManager<ApplicationUser> userManager, IAppHelper appHelper, IRepository<ApplicationUser, int> userRepository,
        SignInManager<ApplicationUser> signInManager, IRepository<HistoryUser, int> historyUserRepository, IUserService userService, IConfiguration configuration, ILogger<AccountAPIController> logger, IOperationRecordService operationRecordService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _appHelper = appHelper;
            _userOnlineInforRepository = userOnlineInforRepository;
            _historyUserRepository = historyUserRepository;
            _userRepository = userRepository;
            _userService = userService;
            _configuration = configuration;
            _logger = logger;
            _operationRecordService = operationRecordService;
        }

        /// <summary>
        /// 登入
        /// </summary>
        /// <remarks>
        /// 需要使用极验人机验证并获取token 登入及二次验证 以下同理
        /// 每个用户名10分钟只能错误10次 其他验证同理
        /// </remarks>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResult>> Login(LoginModel model)
        {
            //提前判断是否通过人机验证
            if (_appHelper.CheckRecaptcha(model.Verification) == false)
            {
                return BadRequest(new LoginResult { Code = LoginResultCode.FailedRecaptchaValidation, ErrorDescribe = "没有通过人机验证" });
            }

            //首先判断是否错误次数超过上限
            if (await _appHelper.IsExceedMaxErrorCount(model.UserName, 10, 10))
            {
                return new LoginResult { Code = LoginResultCode.FailedTooMany, ErrorDescribe = "登入失败次数过多，将在一段时间后解除锁定，请尝试找回密码，或联系管理员" };
            }

            var user = await _userManager.FindByEmailAsync(model.UserName);
            if (user == null)
            {
                user = await _userManager.FindByNameAsync(model.UserName);
            }
            if (user == null)
            {
                user = await _userRepository.FirstOrDefaultAsync(s => s.PhoneNumber == model.UserName);
            }
            if (user == null)
            {
                //失败后尝试从历史账户中登入
                var history = await _appHelper.LoginHistoryUser(model.UserName);
                if (history != null)
                {
                    return new LoginResult { Code = LoginResultCode.HistoricalUser, ErrorDescribe = "历史用户", ErrorInfor = history };
                }
                else
                {
                    await _appHelper.AddErrorCount(model.UserName);
                    return new LoginResult { Code = LoginResultCode.WrongUserNameOrPassword, ErrorDescribe = "用户名或密码错误" };
                }

            }
            if (!user.EmailConfirmed)
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                //获取短验证码
                await _appHelper.GetShortTokenAsync(token, user.UserName);
                //因为此处已经通过人机验证 直接获取验证码
                await _appHelper.SendVerificationEmailAsync(null, user.Email, user.UserName);

                return new LoginResult { Code = LoginResultCode.FailedEmailValidation, ErrorInfor = user.UserName, ErrorDescribe = "你的电子邮件还没有进行验证" };
            }
            //判断用户是否被封禁
            if (user.UnsealTime != null)
            {
                if (user.UnsealTime < DateTime.Now.ToCstTime())
                {
                    user.UnsealTime = null;
                    await _userManager.UpdateAsync(user);
                }
                else
                {
                    return new LoginResult { Code = LoginResultCode.UserBanded, ErrorDescribe = "该用户已经被封禁，将在" + (user.UnsealTime?.ToString("D") ?? "") + "后解封" };
                }
            }
            //正常登入用户
            var result = await _signInManager.PasswordSignInAsync(user.UserName, model.Password, model.RememberMe, false);
            if (result.Succeeded)
            {
                //判断用户是否需要验证
                if (model.isNeedVerification && user.IsPassedVerification == false && user.RegistTime >new DateTime(2021,9,11))
                {
                    return new LoginResult { Code = LoginResultCode.FailedRealNameValidation, ErrorDescribe = "没有通过身份验证", Token = await _appHelper.GetUserJWTokenAsync(user) };
                }
                else
                {
                    //成功登入 记录ip
                    try
                    {
                       await _operationRecordService.AddOperationRecord(OperationRecordType.Login, null, user, model.Identification, HttpContext);
                    }
                    catch(Exception ex)
                    {
                        _logger.LogError(ex, "身份识别失败");
                        return new LoginResult { Code = LoginResultCode.FailedRecaptchaValidation,ErrorDescribe="身份识别失败",ErrorInfor=ex.Message };
                    }

                    return new LoginResult { Code = LoginResultCode.OK, Token = await _appHelper.GetUserJWTokenAsync(user) };
                }
            }
            else
            {
                //失败后尝试从历史账户中登入
                var history = await _appHelper.LoginHistoryUser(model.UserName);
                if (history != null)
                {
                    return new LoginResult { Code = LoginResultCode.HistoricalUser, ErrorDescribe = "历史用户", ErrorInfor = history };
                }
                else
                {
                    await _appHelper.AddErrorCount(model.UserName);
                    return new LoginResult { Code = LoginResultCode.WrongUserNameOrPassword, ErrorDescribe = "用户名或密码错误" };
                }

            }
        }

        /// <summary>
        /// 刷新JWT令牌
        /// </summary>
        /// <remarks>
        /// JWT令牌有效时长为15天
        /// 建议每次进入应用刷新一次
        /// </remarks>
        /// <returns></returns>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet]
        public async Task<ActionResult<LoginResult>> RefreshJWToken()
        {
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            if (user != null)
            {
                //判断用户是否被封禁
                if (user.UnsealTime != null)
                {
                    if (user.UnsealTime < DateTime.Now.ToCstTime())
                    {
                        user.UnsealTime = null;
                        await _userManager.UpdateAsync(user);
                    }
                    else
                    {
                        return new LoginResult { Code = LoginResultCode.UserBanded, ErrorDescribe = "该用户已经被封禁，将在" + (user.UnsealTime?.ToString("D") ?? "") + "后解封" };
                    }
                }

                return new LoginResult { Code = LoginResultCode.OK, Token = await _appHelper.GetUserJWTokenAsync(user) };
            }
            else
            {
                return new LoginResult { Code = LoginResultCode.WrongUserNameOrPassword, ErrorDescribe = "无效的JWT令牌" };
            }

        }
        /// <summary>
        /// 刷新JWT令牌
        /// </summary>
        /// <remarks>
        /// JWT令牌有效时长为15天
        /// 建议每次进入应用刷新一次
        /// </remarks>
        /// <returns></returns>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        public async Task<ActionResult<LoginResult>> RefreshJWToken(LoginModel model)
        {
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            if (user != null)
            {
                //判断用户是否被封禁
                if (user.UnsealTime != null)
                {
                    if (user.UnsealTime < DateTime.Now.ToCstTime())
                    {
                        user.UnsealTime = null;
                        await _userManager.UpdateAsync(user);
                    }
                    else
                    {
                        return new LoginResult { Code = LoginResultCode.UserBanded, ErrorDescribe = "该用户已经被封禁，将在" + (user.UnsealTime?.ToString("D") ?? "") + "后解封" };
                    }
                }

                return new LoginResult { Code = LoginResultCode.OK, Token = await _appHelper.GetUserJWTokenAsync(user) };
            }
            else
            {
                return new LoginResult { Code = LoginResultCode.WrongUserNameOrPassword, ErrorDescribe = "无效的JWT令牌" };
            }

        }
      
        /// <summary>
        /// 获取一次性代码用户跨网站维持登录状态
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<OneTimeCodeModel>> GetOneTimeCodeAsync()
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            return new OneTimeCodeModel { Code = await _appHelper.SetUserLoginKeyAsync(user.Id, true) };
        }

        /// <summary>
        /// 通过一次性代码登入
        /// </summary>
        /// <remarks>
        /// 一小时内有效
        /// </remarks>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult<LoginResult>> LoginByOneTimeCode(OneTimeCodeModel model)
        {
            var ip = Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(ip))
            {
                ip = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            }

            //首先判断是否错误次数超过上限
            if (await _appHelper.IsExceedMaxErrorCount(ip, 60, 1))
            {
                await _appHelper.AddErrorCount(ip);
                return new LoginResult { Code = LoginResultCode.FailedTooMany, ErrorDescribe = "登入失败次数过多，将在一段时间后解除锁定，请尝试找回密码，或联系管理员" };
            }



            var userId = await _appHelper.GetUserFromLoginKeyAsync(model.Code);
            if (string.IsNullOrWhiteSpace(userId))
            {
                await _appHelper.AddErrorCount(ip);
                return new LoginResult { Code = LoginResultCode.WrongUserNameOrPassword, ErrorDescribe = "无效的代码" };
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                //判断用户是否被封禁
                if (user.UnsealTime != null)
                {
                    if (user.UnsealTime < DateTime.Now.ToCstTime())
                    {
                        user.UnsealTime = null;
                        await _userManager.UpdateAsync(user);
                    }
                    else
                    {
                        await _appHelper.AddErrorCount(ip);
                        return new LoginResult { Code = LoginResultCode.UserBanded, ErrorDescribe = "该用户已经被封禁，将在" + (user.UnsealTime?.ToString("D") ?? "") + "后解封" };
                    }
                }

                return new LoginResult { Code = LoginResultCode.OK, Token = await _appHelper.GetUserJWTokenAsync(user) };
            }
            else
            {
                await _appHelper.AddErrorCount(ip);
                return new LoginResult { Code = LoginResultCode.WrongUserNameOrPassword, ErrorDescribe = "无效的代码" };
            }

        }

        /// <summary>
        /// 极验初始化验证接口
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<GeetestCodeModel>> GetGeetestCode()
        {
            var ip = Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(ip))
            {
                ip = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            }
            /*
            必传参数
                digestmod 此版本sdk可支持md5、sha256、hmac-sha256，md5之外的算法需特殊配置的账号，联系极验客服
            自定义参数,可选择添加
                user_id user_id作为客户端用户的唯一标识，确定用户的唯一性；作用于提供进阶数据分析服务，可在register和validate接口传入，不传入也不影响验证服务的使用；若担心用户信息风险，可作预处理(如哈希处理)再提供到极验
                client_type 客户端类型，web：电脑上的浏览器；h5：手机上的浏览器，包括移动应用内完全内置的web_view；native：通过原生sdk植入app应用的方式；unknown：未知
                ip_address 客户端请求sdk服务器的ip地址
            */
            var gtLib = new GeetestLib(_configuration["GEETEST_ID"], _configuration["GEETEST_KEY"]);
            var userId = ip;
            var digestmod = "md5";
            IDictionary<string, string> paramDict = new Dictionary<string, string> { { "digestmod", digestmod }, { "user_id", userId }, { "client_type", "web" }, { "ip_address", "127.0.0.1" } };
            var bypass_cache = "success";
            GeetestLibResult result;
            if (bypass_cache == "success")
            {
                result = gtLib.Register(digestmod, paramDict);
            }
            else
            {
                result = gtLib.LocalRegister();
            }
            var obj = JObject.Parse(result.GetData());
            var model = new GeetestCodeModel
            {
                Challenge = obj["challenge"].ToString(),
                Success = obj["success"].ToString(),
                Gt = obj["gt"].ToString()
            };
            return model;
        }

        /// <summary>
        /// 获取Ip地址
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<string>> GetIp()
        {
            return _operationRecordService.GetIp(HttpContext, null);
        }
    }
}
