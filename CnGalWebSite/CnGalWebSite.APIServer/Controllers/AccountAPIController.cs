using Gt3_server_csharp_aspnetcoremvc_bypass.Controllers.Sdk;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.Application.Users;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Accounts;
using CnGalWebSite.DataModel.ViewModel.Admin;
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
        private readonly IConfiguration _configuration;


        public AccountAPIController(IRepository<UserOnlineInfor, long> userOnlineInforRepository, UserManager<ApplicationUser> userManager, IAppHelper appHelper, IRepository<ApplicationUser, int> userRepository,
        SignInManager<ApplicationUser> signInManager, IRepository<HistoryUser, int> historyUserRepository, IUserService userService, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _appHelper = appHelper;
            _userOnlineInforRepository = userOnlineInforRepository;
            _historyUserRepository = historyUserRepository;
            _userRepository = userRepository;
            _userService = userService;
            _configuration = configuration;
        }

        /// <summary>
        /// 注册用户
        /// </summary>
        /// <remarks>
        /// 需要使用极验人机验证并获取token 登入及二次验证 以下同理
        /// 极验初始化需要调用接口
        /// </remarks>
        /// <param name="model"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult<Result>> Register(RegisterModel model)
        {
            //判断是否是管理员
            var user_temp = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            //提前判断是否通过人机验证
            if (user_temp == null || await _userManager.IsInRoleAsync(user_temp, "Admin") == false)
            {
                if (_appHelper.CheckRecaptcha(model.Challenge, model.Validate, model.Seccode) == false)
                {
                    return new Result { Successful = false, Error = "没有通过人机验证" };
                }
            }



            var result = new IdentityResult();
            var result_1 = false;

            //判断用户名是否过长
            if (model.Name.Length > 20)
            {
                return new Result { Successful = false, Error = "用户名必须少于20字符" };
            }
            //判断用户名是否重复
            var user = await _userManager.FindByNameAsync(model.Name);
            if (user != null && user.EmailConfirmed == true)
            {
                return new Result { Successful = false, Error = "此用户名已经被注册" };
            }
            //判断邮箱是否重复
            user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null && user.EmailConfirmed == true)
            {
                return new Result { Successful = false, Error = "此电子邮箱已经被注册" };
            }
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = model.Name,
                    Email = model.Email,
                    PersonalSignature = "哇，这里什么都没有呢",
                    MainPageContext = "### 哇，这里什么都没有呢",
                    Integral = 0,
                    ContributionValue = 0,
                    RegistTime = DateTime.Now.ToCstTime(),
                    Birthday = null
                };
                result = await _userManager.CreateAsync(user, model.Password);
                try
                {
                    await _userManager.AddToRoleAsync(user, "User");
                }
                catch { }
            }
            else
            {
                result_1 = true;
            }
            if (result.Succeeded || result_1 == true)
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                //var confirmationLink = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, token = token }, Request.Scheme);

                //_logger.Log(LogLevel.Warning, token);

                if (user_temp != null && await _userManager.IsInRoleAsync(user_temp, "Admin") == true)
                {
                    //直接注册
                    result = await _userManager.ConfirmEmailAsync(user, token);
                    if (result.Succeeded)
                    {
                        return new Result { Successful = true, Error = "Admin" };
                    }
                    return new Result { Successful = false, Error = "尝试以管理员身份直接验证邮箱失败" };
                }
                //获取短验证码
                await _appHelper.GetShortTokenAsync(token, user.UserName);
                //因为此处已经通过人机验证 直接获取验证码
                var result_2 = await _appHelper.SendVerificationEmailAsync(null, model.Email, user.UserName);
                if (result_2 != null)
                {
                    return new Result { Successful = false, Error = "发送验证码的过程中发生错误，" + result_2 };
                }

                return new Result { Successful = true, Error = "User" };
            }
            else
            {
                return new Result { Successful = false, Error = result.Errors.First().Description };
            }
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

            if (_appHelper.CheckRecaptcha(model.Challenge, model.Validate, model.Seccode) == false)
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
                if (model.isNeedVerification && user.IsPassedVerification == false && user.RegistTime > DateTime.ParseExact("2021-09-01", "yyyy-MM-dd", null))
                {
                    return new LoginResult { Code = LoginResultCode.FailedRealNameValidation, ErrorDescribe = "没有通过身份验证", Token = await _appHelper.GetUserJWTokenAsync(user) };
                }
                else
                {
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
        /// 注册用户验证邮箱
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<ConfirmEmailRegisterResult>> ConfirmEmailRegister(ConfirmEmailRegisterModel model)
        {
            //首先判断是否错误次数超过上限
            if (await _appHelper.IsExceedMaxErrorCount(model.UserName, 10, 10))
            {
                return new ConfirmEmailRegisterResult { Successful = false, Error = "验证码错误次数过多，将在一段时间后解除锁定，请联系管理员" };
            }

            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user == null)
            {
                return new ConfirmEmailRegisterResult { Successful = false, Error = $"无法找到用户名为{model.UserName}的用户" };
            }

            //查找长令牌
            var token = await _appHelper.GetLongTokenAsync(model.NumToken);
            if (token != null)
            { //验证邮箱
                var result = await _userManager.ConfirmEmailAsync(user, token);
                if (result.Succeeded)
                {
                    return new ConfirmEmailRegisterResult { Successful = true, Token = await _appHelper.GetUserJWTokenAsync(user), LoginKey = await _appHelper.SetUserLoginKeyAsync(user.Id) };
                }
                else
                {
                    return new ConfirmEmailRegisterResult { Successful = false, Error = result.Errors.First().Description };
                }
            }

            await _appHelper.AddErrorCount(model.UserName);
            return new ConfirmEmailRegisterResult { Successful = false, Error = "验证码错误" };
        }

        /// <summary>
        /// 修改密码 验证邮箱
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<Result>> ChangePasswordAsync(ChangeUserPasswordModel model)
        {
            //首先判断是否错误次数超过上限
            if (await _appHelper.IsExceedMaxErrorCount(model.UserName, 10, 10))
            {
                return new Result { Successful = false, Error = "验证码错误次数过多，将在一段时间后解除锁定，请联系管理员" };
            }

            //先检查这个用户是否存在
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            var userId = await _appHelper.GetUserFromLoginKeyAsync(model.LoginKey);
            if (string.IsNullOrWhiteSpace(userId) || user == null || userId != user.Id || user.UserName != model.UserName)
            {
                await _appHelper.AddErrorCount(model.UserName);
                return new Result { Successful = false, Error = $"身份验证失败" };
            }

            //查找长令牌
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var result = await _userManager.ResetPasswordAsync(user, token, model.Password);

            if (result.Succeeded)
            {
                //验证成功 移除关联账号的错误计数器
                await _appHelper.RemoveErrorCount(user.UserName);
                await _appHelper.RemoveErrorCount(user.Email);

                return new Result { Successful = true };
            }
            else
            {
                return new Result { Successful = false, Error = result.Errors.First().Description };
            }

        }

        /// <summary>
        /// 修改电子邮件 验证是否成功并验证新邮箱
        /// </summary>
        /// <remarks>
        /// 这一操作分为三步：发送邮件验证旧邮箱、验证是否成功并验证新邮箱、验证新邮箱是否成功
        /// </remarks>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<Result>> ChangeEmailBeforeAsync(ChangeEmailBeforeModel model)
        {
            //首先判断是否错误次数超过上限
            if (await _appHelper.IsExceedMaxErrorCount(model.NewEmail, 10, 10))
            {
                return new Result { Successful = false, Error = "验证码错误次数过多，将在一段时间后解除锁定，请联系管理员" };
            }

            //先检查这个用户是否存在
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            var userId = await _appHelper.GetUserFromLoginKeyAsync(model.LoginKey);
            if (string.IsNullOrWhiteSpace(userId) || user == null || userId != user.Id)
            {
                await _appHelper.AddErrorCount(model.NewEmail);
                return new Result { Successful = false, Error = $"身份验证失败" };
            }


            //现在输入新的邮箱 生成令牌
            //获取长令牌
            var token = await _userManager.GenerateChangeEmailTokenAsync(user, model.NewEmail);
            //获取短验证码
            await _appHelper.GetShortTokenAsync(token, user.UserName);

            return new Result { Successful = true };
        }

        /// <summary>
        /// 修改电子邮件 验证新邮箱是否成功
        /// </summary>
        /// <remarks>
        /// 这一操作分为三步：发送邮件验证旧邮箱、验证是否成功并验证新邮箱、验证新邮箱是否成功
        /// </remarks>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<Result>> ChangeEmailAfterAsync(ChangeEmailAfterModel model)
        {
            //首先判断是否错误次数超过上限
            if (await _appHelper.IsExceedMaxErrorCount(model.NewEmail, 10, 10))
            {
                return new Result { Successful = false, Error = "验证码错误次数过多，将在一段时间后解除锁定，请联系管理员" };
            }

            //先检查这个用户是否存在
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            var userId = await _appHelper.GetUserFromLoginKeyAsync(model.LoginKey);
            if (string.IsNullOrWhiteSpace(userId) || user == null || userId != user.Id)
            {
                await _appHelper.AddErrorCount(model.NewEmail);
                return new Result { Successful = false, Error = $"身份验证失败" };
            }

            //查找长令牌
            var token = await _appHelper.GetLongTokenAsync(model.NumToken);
            if (token != null)
            {
                //验证邮箱
                var result = await _userManager.ChangeEmailAsync(user, model.NewEmail, token);
                if (result.Succeeded)
                {
                    return new Result { Successful = true };
                }
            }
            await _appHelper.AddErrorCount(model.NewEmail);
            return new Result { Successful = false, Error = "验证码错误" };
        }

        /// <summary>
        /// 当前用户在线 定期调用
        /// </summary>
        /// <remarks>
        /// 每次调用会增加用户在线时间
        /// 间隔超过十分钟的按十分钟计算
        /// 建议每十分钟调用一次
        /// </remarks>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<Result>> MakeUserOnlineAsync()
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            if (user != null)
            {
                //判断上次登入时间是否在 10分钟之前
                if (user.LastOnlineTime.AddSeconds(60 * 10) < DateTime.Now.ToCstTime())
                {
                    //添加在线时间
                    user.OnlineTime += 60 * 10;
                }
                else
                {
                    user.OnlineTime += (long)((DateTime.Now.ToCstTime() - user.LastOnlineTime).TotalSeconds);
                }
                user.LastOnlineTime = DateTime.Now.ToCstTime();

                var result = await _userManager.UpdateAsync(user);

                //判断是否写入登入信息
                var tempDateTimeNow = DateTime.Now.ToCstTime();
                if (await _userOnlineInforRepository.GetAll().AnyAsync(s => s.Date.Date == tempDateTimeNow.Date && s.ApplicationUserId == user.Id) == false)
                {
                    await _userOnlineInforRepository.InsertAsync(new UserOnlineInfor
                    {
                        ApplicationUser = user,
                        ApplicationUserId = user.Id,
                        Date = DateTime.Now.ToCstTime().Date
                    });
                }

                return new Result { Successful = true, Error = user.Email };
            }
            else
            {
                return new Result { Successful = false };
            }

        }

        /// <summary>
        /// 忘记密码
        /// </summary>
        /// <param name="mail">电子邮件</param>
        /// <returns></returns>
        [HttpGet("{mail}")]
        [AllowAnonymous]
        public async Task<ActionResult<Result>> ForgetPasswordAsync(string mail)
        {
            //先检查这个用户是否存在
            if (string.IsNullOrWhiteSpace(mail))
            {
                return new Result { Successful = false, Error = $"电子邮箱或手机号不能为空" };
            }
            ApplicationUser user = null;
            if (mail.Contains('@'))
            {
                user = await _userManager.FindByEmailAsync(mail);
            }
            else
            {
                //检查手机号码是否符合格式
                if (ToolHelper.CheckPhoneIsAble(mail) == false)
                {
                    return new Result { Successful = false, Error = "请输入有效的手机号码" };
                }
                user = await _userRepository.FirstOrDefaultAsync(s => s.PhoneNumber == mail);
            }
            if (user == null)
            {
                return new Result { Successful = true };
            }
            //获取长令牌
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            //获取短验证码
            await _appHelper.GetShortTokenAsync(token, user.UserName);

            return new Result { Successful = true };
        }

        /// <summary>
        /// 忘记密码 验证邮箱
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<Result>> ConfirmForgetPassword(ForgetPasswordModel model)
        {
            //首先判断是否错误次数超过上限
            if (await _appHelper.IsExceedMaxErrorCount(model.Mail, 10, 10))
            {
                return new Result { Successful = false, Error = "验证码错误次数过多，将在一段时间后解除锁定，请联系管理员" };
            }

            //先检查这个用户是否存在
            if (string.IsNullOrWhiteSpace(model.Mail))
            {
                return new Result { Successful = false, Error = $"电子邮箱或手机号不能为空" };
            }
            ApplicationUser user = null;
            if (model.Mail.Contains('@'))
            {
                user = await _userManager.FindByEmailAsync(model.Mail);
            }
            else
            {
                user = await _userRepository.FirstOrDefaultAsync(s => s.PhoneNumber == model.Mail);
            }

            if (user == null)
            {
                return new Result { Successful = false, Error = $"无法找到Email为{model.Mail}的用户" };
            }

            //查找长令牌
            var token = await _appHelper.GetLongTokenAsync(model.NumToken);
            if (token != null)
            { //验证邮箱
                var result = await _userManager.ResetPasswordAsync(user, token, model.Password);

                if (result.Succeeded)
                {
                    //验证成功 移除关联账号的错误计数器
                    await _appHelper.RemoveErrorCount(user.UserName);
                    await _appHelper.RemoveErrorCount(user.Email);

                    return new Result { Successful = true };
                }
                else
                {
                    return new Result { Successful = false, Error = result.Errors.First().Description };
                }
            }
            await _appHelper.AddErrorCount(model.Mail);
            return new Result { Successful = false, Error = "验证码错误" };

        }

        /// <summary>
        /// 封禁或解封用户
        /// </summary>
        /// <remarks>
        /// 封禁时长建议默认十年
        /// 解封操作需要将时间设置为null
        /// </remarks>
        /// <param name="model"></param>
        /// <returns></returns>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Result>> BanUser(BanUserModel model)
        {
            //获取当前用户ID
            foreach (var item in model.Ids)
            {
                var user = await _userManager.FindByIdAsync(item);
                if (user == null)
                {
                    return NotFound();
                }

                user.UnsealTime = model.UnsealTime;
                await _userManager.UpdateAsync(user);
            }

            return new Result { Successful = true };
        }

        /// <summary>
        /// 验证验证码是否存在 不等同于验证是否真实有效
        /// </summary>
        /// <remarks>
        /// 会记录调用ip 错误十次封禁十五分钟
        /// </remarks>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<Result>> CheckVerificationCode(CheckVerificationCodeModel model)
        {
            //首先判断是否错误次数超过上限
            if (await _appHelper.IsExceedMaxErrorCount(HttpContext.Connection.RemoteIpAddress.ToString(), 10, 15))
            {
                return new Result { Successful = false, Error = "验证失败次数过多，将在一段时间后解除锁定，请尝试联系管理员" };
            }

            if (await _appHelper.GetLongTokenAsync(model.Num) == null)
            {
                await _appHelper.AddErrorCount(HttpContext.Connection.RemoteIpAddress.ToString());
                return new Result { Successful = false, Error = "验证码错误" };
            }
            else
            {
                return new Result { Successful = true };
            }
        }

        /// <summary>
        /// 发送验证邮件或短信
        /// </summary>
        /// <remarks>
        /// 会记录调用ip 错误十次封禁十五分钟
        /// 需要使用极验人机验证并获取token 登入及二次验证 以下同理
        /// </remarks>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<Result>> PostVerificationCode(PostVerificationCodeModel model)
        {
            //提前判断是否通过人机验证
            var ip = Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(ip))
            {
                ip = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            }
            if (_appHelper.CheckRecaptcha(model.Challenge, model.Validate, model.Seccode) == false)
            {
                return new Result { Successful = false, Error = "没有通过人机验证" };
            }

            //优先判断用户名          
            if (string.IsNullOrWhiteSpace(model.UserName))
            {
                //再LoginKey
                var userId = await _appHelper.GetUserFromLoginKeyAsync(model.LoginKey);
                if (string.IsNullOrWhiteSpace(userId) == false)
                {
                    model.UserName = (await _userManager.FindByIdAsync(userId))?.UserName;
                }
                else
                {
                    //获取当前用户ID
                    var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
                    if (user != null)
                    {
                        model.UserName = user.UserName;
                    }
                    else
                    {
                        model.UserName = (await _userManager.FindByEmailAsync(model.Mail))?.UserName;
                        if (string.IsNullOrWhiteSpace(model.UserName))
                        {
                            model.UserName = (await _userRepository.FirstOrDefaultAsync(s => s.PhoneNumber == model.Mail))?.UserName;
                        }
                    }
                }
            }

            //判断目标电子邮箱或手机号码是否为空 为空则发送到默认电子邮箱
            if (string.IsNullOrWhiteSpace(model.Mail))
            {
                //获取当前用户ID
                var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

                if (model.SendType == SendType.Email)
                {
                    model.Mail = await _userRepository.GetAll().Where(s => s.UserName == model.UserName).Select(s => s.Email).FirstOrDefaultAsync();
                }
                else if (model.SendType == SendType.Phone)
                {
                    model.Mail = await _userRepository.GetAll().Where(s => s.UserName == model.UserName).Select(s => s.PhoneNumber).FirstOrDefaultAsync();
                }

                if (string.IsNullOrWhiteSpace(model.Mail))
                {
                    return new Result { Successful = false, Error = "电子邮件或手机号码不能为空" };
                }
            }

            //发送邮件或短信
            string result;
            if (model.Mail.Contains('@'))
            {

                result = await _appHelper.SendVerificationEmailAsync(null, model.Mail, model.UserName);
            }
            else
            {
                //检查手机号码是否符合格式
                if (ToolHelper.CheckPhoneIsAble(model.Mail) == false)
                {
                    return new Result { Successful = false, Error = "请输入有效的手机号码" };
                }
                result = await _appHelper.SendVerificationSMSAsync(null, model.Mail, model.UserName, model.SMSType);
            }

            if (result != null)
            {
                return new Result { Successful = false, Error = "发送验证码的过程中发生错误，" + result };
            }
            else
            {
                return new Result { Successful = true };
            }
        }
        /// <summary>
        /// 历史用户 重新设置密码
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<Result>> HistorUserChangePassword(HistorUserChangePasswordModel model)
        {
            //首先判断是否错误次数超过上限
            if (await _appHelper.IsExceedMaxErrorCount(model.UserName, 10, 10))
            {
                return new Result { Successful = false, Error = "验证码错误次数过多，将在一段时间后解除锁定，请联系管理员" };
            }

            //先检查这个用户是否存在
            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user == null)
            {
                return new Result { Successful = false, Error = $"无法找到用户名为{model.UserName}的用户" };
            }

            //查找长令牌
            var token = await _appHelper.GetLongTokenAsync(model.NumToken);
            if (token != null)
            { //验证邮箱
                var result = await _userManager.ResetPasswordAsync(user, token, model.Password);

                if (result.Succeeded)
                {
                    //验证成功 移除关联账号的错误计数器
                    await _appHelper.RemoveErrorCount(user.UserName);
                    await _appHelper.RemoveErrorCount(user.Email);
                    //删除历史用户
                    await _historyUserRepository.DeleteAsync(s => s.UserName == model.UserName);

                    return new Result { Successful = true };
                }
                else
                {
                    return new Result { Successful = false, Error = result.Errors.First().Description };
                }
            }
            await _appHelper.AddErrorCount(model.UserName);
            return new Result { Successful = false, Error = "验证码错误" };

        }

        /// <summary>
        /// 没有绑定手机号的用户 绑定手机 第一步发送验证码
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<Result>> AddPhoneNumber(AddPhoneNumberModel model)
        {
            //获取当前用户
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            if (user == null)
            {
                //尝试通过key获取id
                var userId = await _appHelper.GetUserFromLoginKeyAsync(model.LoginToken);
                if (string.IsNullOrWhiteSpace(userId) == false)
                {
                    //查找用户
                    user = await _userManager.FindByIdAsync(userId);
                    if (user == null)
                    {
                        return new Result { Successful = false, Error = "该用户不存在" };
                    }
                }
            }
            //检查手机号码是否有效
            if (ToolHelper.CheckPhoneIsAble(model.Phone) == false)
            {
                return new Result { Successful = false, Error = "请输入有效的手机号码" };
            }
            //检查手机号码是否已经被绑定
            if (await _userRepository.GetAll().AnyAsync(s => s.PhoneNumber == model.Phone))
            {
                return new Result { Successful = false, Error = "该号码已经被其他用户绑定" };
            }

            if (user.PhoneNumberConfirmed == true)
            {
                return new Result { Successful = false, Error = "该用户已经绑定了手机号码" };
            }


            var token = await _userManager.GenerateChangePhoneNumberTokenAsync(user, model.Phone);
            //获取短验证码
            await _appHelper.GetShortTokenAsync(token, user.UserName);

            return new Result { Successful = true, Error = user.UserName };
        }

        /// <summary>
        /// 没有绑定手机号的用户 绑定手机 第二步验证并添加
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<Result>> ConfirmAddPhoneNumber(ConfirmAddPhoneNumberModel model)
        {
            //首先判断是否错误次数超过上限
            if (await _appHelper.IsExceedMaxErrorCount(model.Phone, 10, 10))
            {
                return new Result { Successful = false, Error = "验证码错误次数过多，将在一段时间后解除锁定，请联系管理员" };
            }


            //获取当前用户
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            if (user == null)
            {
                //尝试通过key获取id
                var userId = await _appHelper.GetUserFromLoginKeyAsync(model.LoginToken);
                if (string.IsNullOrWhiteSpace(userId) == false)
                {
                    //查找用户
                    user = await _userManager.FindByIdAsync(userId);
                    if (user == null)
                    {
                        return new Result { Successful = false, Error = "该用户不存在" };
                    }
                }
            }

            //检查手机号码是否有效
            if (ToolHelper.CheckPhoneIsAble(model.Phone) == false)
            {
                return new Result { Successful = false, Error = "请输入有效的手机号码" };
            }
            //检查手机号码是否已经被绑定
            if (await _userRepository.GetAll().AnyAsync(s => s.PhoneNumber == model.Phone))
            {
                return new Result { Successful = false, Error = "该号码已经被其他用户绑定" };
            }

            //查找长令牌
            var token = await _appHelper.GetLongTokenAsync(model.NumToken);
            if (token != null)
            {
                //添加手机号码
                var result = await _userManager.ChangePhoneNumberAsync(user, model.Phone, token);
                if (result.Succeeded)
                {
                    //将该用户更改为已验证
                    user.IsPassedVerification = true;
                    await _userManager.UpdateAsync(user);

                    return new Result { Successful = true };
                }
                else
                {
                    return new Result { Successful = false, Error = result.Errors.First().Description };
                }
            }

            await _appHelper.AddErrorCount(model.Phone);
            return new Result { Successful = false, Error = "验证码错误" };
        }

        /// <summary>
        /// 修改电话号码 验证是否成功并验证新电话号码
        /// </summary>
        /// <remarks>
        /// 这一操作分为三步：发送邮件验证邮箱、验证是否成功并验证新电话号码、验证新电话号码是否成功
        /// </remarks>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<Result>> ChangePhoneNumberBeforeAsync(ChangePhoneNumberBeforeModel model)
        {
            //首先判断是否错误次数超过上限
            if (await _appHelper.IsExceedMaxErrorCount(model.NewPhone, 10, 10))
            {
                return new Result { Successful = false, Error = "验证码错误次数过多，将在一段时间后解除锁定，请联系管理员" };
            }

            //先检查这个用户是否存在
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            var userId = await _appHelper.GetUserFromLoginKeyAsync(model.LoginKey);
            if (string.IsNullOrWhiteSpace(userId) || user == null || userId != user.Id)
            {
                await _appHelper.AddErrorCount(model.NewPhone);
                return new Result { Successful = false, Error = $"身份验证失败" };
            }

            //检查手机号码是否符合格式
            if (ToolHelper.CheckPhoneIsAble(model.NewPhone) == false)
            {
                return new Result { Successful = false, Error = "请输入有效的手机号码" };
            }
            //检查手机号码是否已经被绑定
            if (await _userRepository.GetAll().AnyAsync(s => s.PhoneNumber == model.NewPhone))
            {
                return new Result { Successful = false, Error = "该号码已经被其他用户绑定" };
            }

            //获取长令牌
            var token = await _userManager.GenerateChangePhoneNumberTokenAsync(user, model.NewPhone);
            //获取短验证码
            await _appHelper.GetShortTokenAsync(token, user.UserName);

            return new Result { Successful = true };
        }


        /// <summary>
        /// 修改电话号码 验证新电话号码是否成功
        /// </summary>
        /// <remarks>
        /// 这一操作分为三步：发送邮件验证邮箱、验证是否成功并验证新电话号码、验证新电话号码是否成功
        /// </remarks>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<Result>> ChangePhoneNumberAfterAsync(ChangePhoneNumberAfterModel model)
        {
            //首先判断是否错误次数超过上限
            if (await _appHelper.IsExceedMaxErrorCount(model.NewPhone, 10, 10))
            {
                return new Result { Successful = false, Error = "验证码错误次数过多，将在一段时间后解除锁定，请联系管理员" };
            }

            //先检查这个用户是否存在
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            var userId = await _appHelper.GetUserFromLoginKeyAsync(model.LoginKey);
            if (string.IsNullOrWhiteSpace(userId) || user == null || userId != user.Id)
            {
                await _appHelper.AddErrorCount(model.NewPhone);
                return new Result { Successful = false, Error = $"身份验证失败" };
            }

            //查找长令牌
            var token = await _appHelper.GetLongTokenAsync(model.NumToken);
            if (token != null)
            {
                var result = await _userManager.ChangePhoneNumberAsync(user, model.NewPhone, token);
                if (result.Succeeded)
                {
                    return new Result { Successful = true };
                }
                else
                {
                    return new Result { Successful = false, Error = result.Errors.First().Description };
                }
            }

            await _appHelper.AddErrorCount(user.UserName);
            return new Result { Successful = false, Error = "验证码错误" };

        }

        /// <summary>
        /// 二次身份验证 获取验证码
        /// </summary>
        /// <remarks>
        /// 这一步只是获取账户关联的验证码到数据库 需要调用发送接口来发送给用户
        /// </remarks>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<Result>> SecondAuthenticationAsync()
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //获取长令牌
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            //获取短验证码
            await _appHelper.GetShortTokenAsync(token, user.UserName);

            return new Result { Successful = true };
        }

        /// <summary>
        /// 二次身份验证 验证
        /// </summary>
        /// <returns>若成功返回令牌</returns>
        [HttpPost]
        public async Task<ActionResult<Result>> SecondAuthenticationAsync(SecondAuthenticationModel model)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            //首先判断是否错误次数超过上限
            if (await _appHelper.IsExceedMaxErrorCount(user.UserName, 10, 10))
            {
                return new Result { Successful = false, Error = "验证码错误次数过多，将在一段时间后解除锁定，请联系管理员" };
            }

            //查找长令牌
            var token = await _appHelper.GetLongTokenAsync(model.NumToken);
            if (token != null)
            {
                //验证旧邮箱
                var result = await _userManager.ConfirmEmailAsync(user, token);
                if (result.Succeeded)
                {
                    return new Result { Successful = true, Error = await _appHelper.SetUserLoginKeyAsync(user.Id) };
                }
            }

            await _appHelper.AddErrorCount(user.UserName);
            return new Result { Successful = false, Error = "验证码错误" };
        }

        /// <summary>
        /// 判断二次身份验证码是否有效
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<Result>> CheckSecondAuthenticationAsync(CheckSecondAuthenticationModel model)
        {
            //首先判断是否错误次数超过上限
            if (await _appHelper.IsExceedMaxErrorCount(model.UserId, 10, 1))
            {
                return new Result { Successful = false, Error = "检查二次身份验证错误次数过多，将在一段时间后解除锁定，请联系管理员" };
            }

            //获取当前用户ID
            if (await _appHelper.GetUserFromLoginKeyAsync(model.LoginKey) != model.UserId)
            {
                await _appHelper.AddErrorCount(model.UserId);
                return new Result { Successful = false, Error = "验证失败" };

            }
            else
            {
                return new Result { Successful = true };
            }
        }

        /// <summary>
        /// 获取用户当前支持的二次身份验证方式
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<UserAuthenticationTypeModel>> GetUserAuthenticationTypeAsync()
        {

            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            var model = new UserAuthenticationTypeModel();

            if (string.IsNullOrWhiteSpace(user.Email) == false)
            {
                model.IsOnEmail = true;
            }
            if (string.IsNullOrWhiteSpace(user.PhoneNumber) == false)
            {
                model.IsOnPhone = true;
            }

            return model;
        }

        /// <summary>
        /// 第三方登入
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult<ThirdPartyLoginResult>> ThirdPartyLoginAsync(ThirdPartyLoginModel model)
        {
            var ip = Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(ip))
            {
                ip = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            }
            //首先判断是否错误次数超过上限
            if (await _appHelper.IsExceedMaxErrorCount(ip, 10, 1))
            {
                return new ThirdPartyLoginResult { Code = ThirdPartyLoginResultType.Failed, Error = "检查第三方验证错误次数过多，将在一段时间后解除锁定，请联系管理员" };
            }
            if (string.IsNullOrWhiteSpace(model.Code))
            {
                return new ThirdPartyLoginResult { Code = ThirdPartyLoginResultType.Failed, Error = "Code不能为空" };
            }
            //获取token_id
           var  token = await _userService.GetThirdPartyLoginIdToken(model.Code, model.ReturnUrl, model.IsSSR, model.Type);


            if (string.IsNullOrWhiteSpace(token))
            {
                await _appHelper.AddErrorCount(ip);
                return new ThirdPartyLoginResult { Code = ThirdPartyLoginResultType.Failed, Error = "第三方登入验证失败" };
            }
            else if (token == "code expire")
            {
                await _appHelper.AddErrorCount(ip);
                return new ThirdPartyLoginResult { Code = ThirdPartyLoginResultType.RepeatRequest, Error = "重复请求" };

            }
            //尝试登入
            var user = await _userService.GetThirdPartyLoginUser(token, model.Type);

            if (user == null)
            {
                return new ThirdPartyLoginResult { Code = ThirdPartyLoginResultType.NoAssociatedAccount, ThirdLoginKey = await _appHelper.SetUserLoginKeyAsync(token) };

            }
            else
            {
                return new ThirdPartyLoginResult { Code = ThirdPartyLoginResultType.LoginSuccessed, Token = await _appHelper.GetUserJWTokenAsync(user), UserName = user.UserName };

            }
        }

        /// <summary>
        /// 为账户绑定第三方登入
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<Result>> AddThirdPartyLoginInforAsync(AddThirdPartyLoginInforModel model)
        {
            var ip = Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(ip))
            {
                ip = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            }
            //首先判断是否错误次数超过上限
            if (await _appHelper.IsExceedMaxErrorCount(ip, 10, 1))
            {
                return new Result { Successful = false, Error = "检查第三方验证错误次数过多，将在一段时间后解除锁定，请联系管理员" };
            }
            //先检查这个用户是否存在
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            var userId = await _appHelper.GetUserFromLoginKeyAsync(model.LoginKey);
            if (string.IsNullOrWhiteSpace(userId) || user == null || userId != user.Id)
            {
                await _appHelper.AddErrorCount(ip);
                return new Result { Successful = false, Error = $"身份验证失败" };
            }




            if (string.IsNullOrWhiteSpace(model.ThirdPartyKey))
            {
                await _appHelper.AddErrorCount(ip);
                return new Result { Successful = false, Error = "第三方登入验证失败" };
            }


            //获取token_id
            var token = await _appHelper.GetUserFromLoginKeyAsync(model.ThirdPartyKey);
            if (string.IsNullOrWhiteSpace(token))
            {
                await _appHelper.AddErrorCount(ip);
                return new Result { Successful = false, Error = "第三方登入验证失败" };
            }
            //检查是否已经添加
            var temp = await _userService.GetThirdPartyLoginUser(token, model.Type);
            if (temp != null)
            {
                await _appHelper.AddErrorCount(ip);
                return new Result { Successful = false, Error = "该账户已经被绑定" };
            }

            //尝试添加
            var result = await _userService.AddUserThirdPartyLogin(token, user, model.Type);

            if (result == true)
            {
                return new Result { Successful = true };

            }
            else
            {
                await _appHelper.AddErrorCount(ip);
                return new Result { Successful = false, Error = "添加第三方登入失败，保存数据出错" };
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
    }
}
