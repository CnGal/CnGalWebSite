// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityModel;
using IdentityServer4.Events;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using CnGalWebSite.IdentityServer.Services.Account;
using CnGalWebSite.IdentityServer.Services.Messages;
using CnGalWebSite.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Numerics;
using static IdentityServer4.Models.IdentityResources;
using CnGalWebSite.APIServer.DataReositories;
using IdentityServer4;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using static IdentityServer4.IdentityServerConstants;
using static IdentityServer4.Events.TokenIssuedSuccessEvent;
using CnGalWebSite.IdentityServer.Services.Geetest;
using CnGalWebSite.IdentityServer.Models.InputModels.Account;
using CnGalWebSite.IdentityServer.Models.ViewModels.Account;
using CnGalWebSite.IdentityServer.Models.DataModels.Account;

namespace IdentityServerHost.Quickstart.UI
{
    [Authorize]
    [SecurityHeaders]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IClientStore _clientStore;
        private readonly IAuthenticationSchemeProvider _schemeProvider;
        private readonly IEventService _events;
        private readonly IVerificationCodeService _verificationCodeService;
        private readonly IMessageService _messageService;
        private readonly IGeetestService _geetestService;
        private readonly IAccountService _accountService;
        private readonly IRepository<ApplicationUser, string> _userRepository;
        private readonly IConfiguration _configuration;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IIdentityServerInteractionService interaction,
            IClientStore clientStore, IAccountService accountService, IConfiguration configuration, IGeetestService geetestService,
        IAuthenticationSchemeProvider schemeProvider, IRepository<ApplicationUser, string> userRepository,
        IEventService events, IVerificationCodeService verificationCodeService, IMessageService messageService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _interaction = interaction;
            _clientStore = clientStore;
            _schemeProvider = schemeProvider;
            _events = events;
            _verificationCodeService = verificationCodeService;
            _messageService = messageService;
            _userRepository = userRepository;
            _accountService = accountService;
            _configuration = configuration;
            _geetestService = geetestService;
        }

        /// <summary>
        /// Entry point into the login workflow
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string returnUrl, bool showCpltToast)
        {
            //判断是否外部登入跳转
            var result = await HttpContext.AuthenticateAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);
            if (result?.Succeeded == true)
            {
                // lookup our user and external provider info
                var (user, provider, providerUserId, claims) = await _accountService.FindUserFromExternalProviderAsync(result);
                if (user != null)
                {
                    return await LoginFromExternalProvider(result, user, provider, providerUserId, returnUrl);
                }
            }

            // build a model so we know what to show on the login page
            var vm = await BuildLoginViewModelAsync(returnUrl);
            vm.ShowCpltToast = showCpltToast;

            if (vm.IsExternalLoginOnly)
            {
                // we only have one option for logging in and it's an external provider
                return RedirectToAction("Challenge", "External", new { scheme = vm.ExternalLoginScheme, returnUrl });
            }

            return View(vm);
        }

        /// <summary>
        /// Handle postback from username/password login
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginInputModel model, string button)
        {
            // check if we are in the context of an authorization request
            var context = await _interaction.GetAuthorizationContextAsync(model.ReturnUrl);

            // the user clicked the "cancel" button
            if (button == "cancel")
            {
                if (context != null)
                {
                    // if the user cancels, send a result back into IdentityServer as if they 
                    // denied the consent (even if this client does not require consent).
                    // this will send back an access denied OIDC error response to the client.
                    await _interaction.DenyAuthorizationAsync(context, AuthorizationError.AccessDenied);

                    // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
                    if (context.IsNativeClient())
                    {
                        // The client is native, so this change in how to
                        // return the response is for better UX for the end user.
                        return this.LoadingPage("Redirect", model.ReturnUrl);
                    }

                    return Redirect(model.ReturnUrl);
                }
                else
                {
                    // since we don't have a valid context, then we just go back to the home page
                    return Redirect("~/");
                }
            }

            if (!ModelState.IsValid)
            {
                return View(await BuildLoginViewModelAsync(model));
            }

            //提前判断是否通过人机验证
            if (_geetestService.CheckRecaptcha(model.VerifyResult) == false)
            {
                ModelState.AddModelError(string.Empty, "人机验证失败");
                return View(await BuildLoginViewModelAsync(model));
            }
            //查找用户
            var user = await _userManager.FindByEmailAsync(model.Username) ?? await _userManager.FindByNameAsync(model.Username) ?? await _userManager.Users.FirstOrDefaultAsync(s => s.PhoneNumber == model.Username);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, AccountOptions.InvalidCredentialsErrorMessage);
                return View(await BuildLoginViewModelAsync(model));
            }

            //登入
            var result = await _signInManager.PasswordSignInAsync(user.UserName, model.Password, model.RememberLogin, lockoutOnFailure: true);
            if (!result.Succeeded)
            {
                if (result.IsLockedOut)
                {
                    ModelState.AddModelError(string.Empty, "错误次数过多，账户被锁定，请稍后重试");
                    return View(await BuildLoginViewModelAsync(model));
                }
                ModelState.AddModelError(string.Empty, AccountOptions.InvalidCredentialsErrorMessage);
                return View(await BuildLoginViewModelAsync(model));
            }

            //检查并添加外部登入
            var idsResult = await CheckAndBindExternalToCurrent(user);
            if (!result.Succeeded)
            {
                this.AddModelStateErrors(idsResult.Errors);
                return View(BuildLoginViewModelAsync(model));
            }

            //检查实名验证
            var check = await CheckRealNameAuthentication(user, model.ReturnUrl);
            if (check != null)
            {
                //实名验证不通过则退出登录
                await _signInManager.SignOutAsync();
                return check;
            }

            //跳转
            if (context != null)
            {
                if (context.IsNativeClient())
                {
                    // The client is native, so this change in how to
                    // return the response is for better UX for the end user.
                    return this.LoadingPage("Redirect", model.ReturnUrl);
                }

                // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
                return Redirect(model.ReturnUrl);
            }

            // request for a local page
            if (Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }
            else if (string.IsNullOrEmpty(model.ReturnUrl))
            {
                return Redirect("~/");
            }
            else
            {
                // user might have clicked on a malicious link - should be logged
                throw new Exception("invalid return URL");
            }
        }

        /// <summary>
        /// Show logout page
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Logout(string logoutId)
        {
            // build a model so the logout page knows what to display
            var vm = await BuildLogoutViewModelAsync(logoutId);

            if (vm.ShowLogoutPrompt == false)
            {
                // if the request for logout was properly authenticated from IdentityServer, then
                // we don't need to show the prompt and can just log the user out directly.
                return await Logout(vm);
            }

            return View(vm);
        }

        /// <summary>
        /// Handle logout page postback
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Logout(LogoutInputModel model)
        {
            // build a model so the logged out page knows what to display
            var vm = await BuildLoggedOutViewModelAsync(model.LogoutId);

            if (User?.Identity.IsAuthenticated == true)
            {
                // delete local authentication cookie
                await _signInManager.SignOutAsync();

                // raise the logout event
                await _events.RaiseAsync(new UserLogoutSuccessEvent(User.GetSubjectId(), User.GetDisplayName()));
            }

            // check if we need to trigger sign-out at an upstream identity provider
            if (vm.TriggerExternalSignout)
            {
                // build a return URL so the upstream provider will redirect back
                // to us after the user has logged out. this allows us to then
                // complete our single sign-out processing.
                string url = Url.Action("Logout", new { logoutId = vm.LogoutId });

                // this triggers a redirect to the external provider for sign-out
                return SignOut(new AuthenticationProperties { RedirectUri = url }, vm.ExternalAuthenticationScheme);
            }

            return View("LoggedOut", vm);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Register(string returnUrl)
        {
            var model = new RegisterViewModel
            {
                ReturnUrl = returnUrl,
                GeetestCode = _geetestService.GetGeetestCode(this),
            };

            //判断是否外部登入跳转
            var result = await HttpContext.AuthenticateAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);
            if (result?.Succeeded == true)
            {
                // lookup our user and external provider info
                var (user, provider, providerUserId, claims) = await _accountService.FindUserFromExternalProviderAsync(result);
                if (user == null)
                {
                    //自动填充字段
                    var (email, name) = GetExternalUserInforAsync(claims);
                    model.Email = email;
                    model.Name = name;
                }
            }

            return await Task.FromResult(View(model));
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterInputModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(BuildRegisterViewModel(model));
            }

            //提前判断是否通过人机验证
            if (_geetestService.CheckRecaptcha(model.VerifyResult) == false)
            {
                ModelState.AddModelError(string.Empty, "人机验证失败");
                return View(BuildRegisterViewModel(model));
            }

            //判断用户名是否重复
            if (await _userRepository.AnyAsync(s => s.UserName == model.Name))
            {
                ModelState.AddModelError(string.Empty, "此用户名已经被注册");
                return View(BuildRegisterViewModel(model));
            }

            //判断邮箱是否重复
            if (await _userRepository.AnyAsync(s => s.Email == model.Email))
            {
                ModelState.AddModelError(string.Empty, "此电子邮箱已经被注册");
                return View(BuildRegisterViewModel(model));
            }

            //添加用户
            var user = new ApplicationUser
            {
                UserName = model.Name,
                Email = model.Email,
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                this.AddModelStateErrors(result.Errors);
                return View(BuildRegisterViewModel(model));
            }

            //添加角色
            result = await _userManager.AddToRoleAsync(user, "User");
            if (!result.Succeeded)
            {
                this.AddModelStateErrors(result.Errors);
                return View(BuildRegisterViewModel(model));
            }

            //检查并添加外部登入
            result = await CheckAndBindExternalToCurrent(user);
            if (!result.Succeeded)
            {
                this.AddModelStateErrors(result.Errors);
                return View(BuildRegisterViewModel(model));
            }


            //跳转验证邮箱
            return await RedirectToVerifyEmail(user, model.ReturnUrl);

        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyCode(string returnUrl, string userId, string newEmail, string newPhoneNumber, string secondCode, VerificationCodeType type)
        {
            var user = await _userRepository.FirstOrDefaultAsync(s => s.Id == userId);

            return await Task.FromResult(View(new VerifyCodeViewModel
            {
                IsDisableRepost = true,
                ReturnUrl = returnUrl,
                Email = type == VerificationCodeType.ChangeEmail ? newEmail : user?.Email?.ReplaceWithSpecialChar(),
                PhoneNumber = type == VerificationCodeType.ChangePhoneNumber ? newPhoneNumber : user?.PhoneNumber?.ReplaceWithSpecialChar(),
                NewEmail = newEmail,
                NewPhoneNumber = newPhoneNumber,
                UserId = userId,
                SecondCode = secondCode,
                GeetestCode = _geetestService.GetGeetestCode(this),
                Type = type
            }));
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyCode(VerifyCodeInputModel model, string button)
        {
            //查找用户
            var user = await FindLoginUser() ?? await _userRepository.FirstOrDefaultAsync(s => s.Id == model.UserId);
            if (user == null)
            {
                //伪装正确
                ModelState.AddModelError(string.Empty, "验证码错误");
                return View(BuildVerifyCodeViewModel(model, true));
            }

            if (button == "repost")
            {
                //提前判断是否通过人机验证
                if (_geetestService.CheckRecaptcha(model.VerifyResult) == false)
                {
                    ModelState.AddModelError(string.Empty, "人机验证失败");
                    return View(BuildVerifyCodeViewModel(model));
                }

                //重新获取验证码并发送
                if (model.Type.IsPhoneNumber())
                {
                    var phoneNumber = model.Type == VerificationCodeType.ChangePhoneNumber ? model.NewPhoneNumber : user.PhoneNumber;
                    var code = await _verificationCodeService.GetCodeAsync(null, user.Id, model.Type);
                    if (!await _messageService.SendVerificationSMSAsync(code, phoneNumber, user, model.Type))
                    {
                        ModelState.AddModelError(string.Empty, "验证码发送过于频繁");
                    }
                }
                else
                {
                    var email = model.Type == VerificationCodeType.ChangeEmail ? model.NewEmail : user.Email;
                    var code = await _verificationCodeService.GetCodeAsync(null, user.Id, model.Type);
                    if (!await _messageService.SendVerificationEmailAsync(code, email, user, model.Type))
                    {
                        ModelState.AddModelError(string.Empty, "验证码发送过于频繁");
                    }
                }

                //返回并禁用发送按钮
                return View(BuildVerifyCodeViewModel(model, true));
            }

            if (!ModelState.IsValid)
            {
                return View(BuildVerifyCodeViewModel(model));
            }

            //获取验证码
            string token = null;
            if (model.Type != VerificationCodeType.SecondVerify || model.Type != VerificationCodeType.ResetPassword)
            {
                token = await _verificationCodeService.GetTokenAsync(model.Code, user.Id, model.Type);
                if (string.IsNullOrWhiteSpace(token))
                {
                    ModelState.AddModelError(string.Empty, "验证码错误");
                    return View(BuildVerifyCodeViewModel(model));
                }
            }
            else
            {
                if (await _verificationCodeService.CheckAsync(model.Code, user.Id, model.Type))
                {
                    ModelState.AddModelError(string.Empty, "验证码错误");
                    return View(BuildVerifyCodeViewModel(model));
                }
            }

            //根据操作类型跳转
            if (model.Type == VerificationCodeType.Register)
            {
                //验证邮箱
                var result = await _userManager.ConfirmEmailAsync(user, token);
                if (!result.Succeeded)
                {
                    this.AddModelStateErrors(result.Errors);
                    return View(BuildVerifyCodeViewModel(model));
                }

                //检查实名验证
                var check = await CheckRealNameAuthentication(user, model.ReturnUrl);
                if (check != null)
                {
                    return check;
                }

                return RedirectToAction("Login", new { ShowCpltToast = true, model.ReturnUrl });
            }
            else if (model.Type == VerificationCodeType.AddPhoneNumber)
            {
                //验证手机号
                if (user.UserName.Sha256() != token)
                {
                    ModelState.AddModelError(string.Empty, "验证码错误");
                    return View(BuildVerifyCodeViewModel(model));
                }
                //设置手机号已验证
                user.PhoneNumberConfirmed = true;
                await _userRepository.UpdateAsync(user);

                //已有账号登入 跳转修改信息页面
                if (!string.IsNullOrWhiteSpace(model.SecondCode))
                {
                    return RedirectToAction("SelectModifyField", new { ShowCpltToast = true, model.SecondCode, model.ReturnUrl });
                }

                //完成所有流程 跳转登入
                return RedirectToAction("Login", new { ShowCpltToast = true, model.ReturnUrl });
            }
            else if (model.Type == VerificationCodeType.ResetPassword)
            {
                //重置密码
                //跳转新页面并传递验证码
                return RedirectToAction("SetNewPassword", new { UserId = user.Id, model.Code, model.ReturnUrl });
            }
            else if (model.Type == VerificationCodeType.SecondVerify)
            {
                //选择修改的字段
                //跳转新页面并传递验证码
                return RedirectToAction("SelectModifyField", new { SecondCode = model.Code, model.ReturnUrl });
            }
            else if (model.Type == VerificationCodeType.ChangeEmail)
            {
                //修改邮箱
                var result = await _userManager.ChangeEmailAsync(user, model.NewEmail, token);
                if (!result.Succeeded)
                {
                    this.AddModelStateErrors(result.Errors);
                    return View(BuildVerifyCodeViewModel(model));
                }
                return RedirectToAction("SelectModifyField", new { ShowCpltToast = true, model.SecondCode, model.ReturnUrl });
            }
            else if (model.Type == VerificationCodeType.ChangePhoneNumber)
            {
                //修改邮箱
                var result = await _userManager.ChangePhoneNumberAsync(user, model.NewPhoneNumber, token);
                if (!result.Succeeded)
                {
                    this.AddModelStateErrors(result.Errors);
                    return View(BuildVerifyCodeViewModel(model));
                }
                return RedirectToAction("SelectModifyField", new { ShowCpltToast = true, model.SecondCode, model.ReturnUrl });
            }
            else
            {
                throw new Exception("未知操作");
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> AddPhoneNumber(string returnUrl, string userId, string code, string secondCode)
        {
            return await Task.FromResult(View(new AddPhoneNumberViewModel
            {
                ReturnUrl = returnUrl,
                UserId = userId,
                Code = code,
                SecondCode = secondCode,
                GeetestCode = _geetestService.GetGeetestCode(this)
            }));
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> AddPhoneNumber(AddPhoneNumberInputModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(BuildAddPhoneNumberViewModel(model));
            }
            //提前判断是否通过人机验证
            if (_geetestService.CheckRecaptcha(model.VerifyResult) == false)
            {
                ModelState.AddModelError(string.Empty, "人机验证失败");
                return View(BuildAddPhoneNumberViewModel(model));
            }

            //查找用户
            var user = await FindLoginUser() ?? await _userRepository.FirstOrDefaultAsync(s => s.Id == model.UserId);
            if (user == null)
            {
                //伪装正确
                ModelState.AddModelError(string.Empty, "此手机号已经被绑定");
                return View(BuildAddPhoneNumberViewModel(model));
            }

            //检查通过二次身份验证或者注册邮箱验证或密码验证
            if (!await _verificationCodeService.CheckAsync(model.Code, user.Id, VerificationCodeType.Register) && !await _verificationCodeService.CheckAsync(model.Code, user.Id, VerificationCodeType.VerifyPassword) && !await _verificationCodeService.CheckAsync(model.SecondCode, user.Id, VerificationCodeType.SecondVerify))
            {
                ModelState.AddModelError(string.Empty, "身份验证失败");
                return View(BuildAddPhoneNumberViewModel(model));
            }

            //判断是否已经添加手机号
            if (user.PhoneNumberConfirmed)
            {
                ModelState.AddModelError(string.Empty, "已经绑定了手机号");
                return View(BuildAddPhoneNumberViewModel(model));
            }

            //检查是否被绑定
            if (await _userRepository.AnyAsync(s => s.PhoneNumber == model.PhoneNumber))
            {
                ModelState.AddModelError(string.Empty, "此手机号已经被绑定");
                return View(BuildAddPhoneNumberViewModel(model));
            }

            //设置手机号
            user.PhoneNumber = model.PhoneNumber;
            await _userRepository.UpdateAsync(user);

            //跳转验证手机号
            return await RedirectToVerifyPhoneNumber(user, model.ReturnUrl);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> SelectRealNameMethod(string returnUrl, string userId, bool showCpltToast, string code)
        {

            //检查支持的外部身份验证提供商
            var providers = _configuration["TrustedExternalAuthProviders"].Split(',').Select(s => s.Trim());

            return await Task.FromResult(View(new SelectRealNameMethodViewModel
            {
                ReturnUrl = returnUrl,
                ShowCpltToast = showCpltToast,
                Code = code,
                UserId = userId,
                ExternalProviders = (await _accountService.GetExternalProvidersAsync(await _interaction.GetAuthorizationContextAsync(returnUrl))).Where(s => providers.Contains(s.DisplayName))
            }));
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(string returnUrl)
        {
            return await Task.FromResult(View(new ResetPasswordViewModel
            {
                ReturnUrl = returnUrl,
                GeetestCode = _geetestService.GetGeetestCode(this),
            }));
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(ResetPasswordInputModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(BuildResetPasswordViewModel(model));
            }
            //提前判断是否通过人机验证
            if (_geetestService.CheckRecaptcha(model.VerifyResult) == false)
            {
                ModelState.AddModelError(string.Empty, "人机验证失败");
                return View(BuildResetPasswordViewModel(model));
            }

            //查找用户
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var code = await _verificationCodeService.GetCodeAsync(token, user.Id, VerificationCodeType.ResetPassword);
                if (!await _messageService.SendVerificationEmailAsync(code, user.Email, user, VerificationCodeType.ResetPassword))
                {
                    ModelState.AddModelError(string.Empty, "验证码发送过于频繁");
                    return View(BuildResetPasswordViewModel(model));
                }
            }

            //不管是否发送邮件 都伪装已发送
            return RedirectToAction("VerifyCode", new { UserId = user.Id, Type = VerificationCodeType.ResetPassword, model.ReturnUrl });
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> SetNewPassword(string returnUrl, string userId, string code)
        {
            return await Task.FromResult(View(new SetNewPasswordViewModel
            {
                ReturnUrl = returnUrl,
                UserId = userId,
                Code = code
            }));
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> SetNewPassword(SetNewPasswordInputModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(BuildSetNewPasswordViewModel(model));
            }

            //查找用户
            var user = await _userRepository.FirstOrDefaultAsync(s => s.Id == model.UserId);
            if (user == null)
            {
                //伪装
                return RedirectToAction("Login", new { model.ReturnUrl });
            }
            //查找Token
            var token = await _verificationCodeService.GetTokenAsync(model.Code, user.Id, VerificationCodeType.ResetPassword);
            if (string.IsNullOrWhiteSpace(token))
            {
                ModelState.AddModelError(string.Empty, "验证码错误");
                return View(BuildSetNewPasswordViewModel(model));
            }

            //重置密码
            var result = await _userManager.ResetPasswordAsync(user, token, model.Password);
            if (!result.Succeeded)
            {
                this.AddModelStateErrors(result.Errors);
                return View(BuildSetNewPasswordViewModel(model));
            }

            //跳转登入
            return RedirectToAction("Login", new { model.ReturnUrl });
        }

        [HttpGet]
        public async Task<IActionResult> SelectModifyField(string returnUrl, string secondCode, bool showCpltToast)
        {
            //查找用户
            var user = await FindLoginUser();
            if (user == null)
            {
                //跳转登录
                return RedirectToAction("Login", new { ReturnUrl = returnUrl });
            }

            var vm = await BuildSelectModifyFieldViewModel(user, returnUrl, secondCode, showCpltToast);
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> SelectModifyField(SelectModifyFieldInputModel model, string button)
        {
            if (button == "cancel")
            {
                //取消操作
                var context = await _interaction.GetAuthorizationContextAsync(model.ReturnUrl);
                if (context != null)
                {
                    if (context.IsNativeClient())
                    {
                        return this.LoadingPage("Redirect", model.ReturnUrl);
                    }
                    return Redirect(model.ReturnUrl);
                }
                else
                {
                    return Redirect("~/");
                }
            }
            //查找用户
            var user = await FindLoginUser();
            if (user == null)
            {
                //跳转登录
                return RedirectToAction("Login", new { model.ReturnUrl });
            }
            //提前判断是否通过人机验证
            if (_geetestService.CheckRecaptcha(model.VerifyResult) == false)
            {
                ModelState.AddModelError(string.Empty, "人机验证失败");
                return View(await BuildSelectModifyFieldViewModel(user, model.ReturnUrl, model.SecondCode));
            }
            //发送二次身份验证码
            var code = await _verificationCodeService.GetCodeAsync(user.UserName.Sha256(), user.Id, VerificationCodeType.SecondVerify);
            if (!await _messageService.SendVerificationEmailAsync(code, user.Email, user, VerificationCodeType.SecondVerify))
            {
                ModelState.AddModelError(string.Empty, "验证码发送过于频繁");
                return View(await BuildSelectModifyFieldViewModel(user, model.ReturnUrl, model.SecondCode));
            }

            return RedirectToAction("VerifyCode", new { UserId = user.Id, Type = VerificationCodeType.SecondVerify, model.ReturnUrl });
        }


        [HttpGet]
        public async Task<IActionResult> ChangePassword(string returnUrl, string secondCode)
        {
            return await Task.FromResult(View(new ChangePasswordViewModel
            {
                ReturnUrl = returnUrl,
                SecondCode = secondCode
            }));
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordInputModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(BuildChangePasswordViewModel(model));
            }

            //查找用户
            var user = await FindLoginUser();
            if (user == null)
            {
                return RedirectToAction("Login", new { model.ReturnUrl });
            }

            //检查二次身份验证码
            if (!await _verificationCodeService.CheckAsync(model.SecondCode, user.Id, VerificationCodeType.SecondVerify))
            {
                ModelState.AddModelError(string.Empty, "二次身份验证不通过");
                return View(BuildChangePasswordViewModel(model));
            }

            //重置密码
            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (!result.Succeeded)
            {
                this.AddModelStateErrors(result.Errors);
                return View(BuildChangePasswordViewModel(model));
            }

            //退出登入
            await _signInManager.SignOutAsync();

            //跳转登入
            return RedirectToAction("Login", new { model.ReturnUrl });
        }

        [HttpGet]
        public async Task<IActionResult> ChangeEmail(string returnUrl, string secondCode)
        {
            return await Task.FromResult(View(new ChangeEmailViewModel
            {
                ReturnUrl = returnUrl,
                SecondCode = secondCode,
                GeetestCode = _geetestService.GetGeetestCode(this),
            }));
        }

        [HttpPost]
        public async Task<IActionResult> ChangeEmail(ChangeEmailInputModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(BuildChangeEmailViewModel(model));
            }

            //提前判断是否通过人机验证
            if (_geetestService.CheckRecaptcha(model.VerifyResult) == false)
            {
                ModelState.AddModelError(string.Empty, "人机验证失败");
                return View(BuildChangeEmailViewModel(model));
            }

            //查找用户
            var user = await FindLoginUser();
            if (user == null)
            {
                return RedirectToAction("Login", new { model.ReturnUrl });
            }

            //检查二次身份验证
            if (!await _verificationCodeService.CheckAsync(model.SecondCode, user.Id, VerificationCodeType.SecondVerify))
            {
                ModelState.AddModelError(string.Empty, "二次身份验证不通过");
                return View(BuildChangeEmailViewModel(model));
            }

            //判断电子邮箱以及被绑定
            if (await _userRepository.AnyAsync(s => s.Email == model.Email))
            {
                ModelState.AddModelError(string.Empty, "此电子邮箱已经被绑定");
                return View(BuildChangeEmailViewModel(model));
            }

            //发送验证码
            var token = await _userManager.GenerateChangeEmailTokenAsync(user, model.Email);
            var code = await _verificationCodeService.GetCodeAsync(token, user.Id, VerificationCodeType.ChangeEmail);
            //向新地址发送
            if (!await _messageService.SendVerificationEmailAsync(code, model.Email, user, VerificationCodeType.ChangeEmail))
            {
                ModelState.AddModelError(string.Empty, "验证码发送过于频繁");
            }

            return RedirectToAction("VerifyCode", new { model.SecondCode, UserId = user.Id, NewEmail = model.Email, Type = VerificationCodeType.ChangeEmail, model.ReturnUrl });
        }

        [HttpGet]
        public async Task<IActionResult> ChangePhoneNumber(string returnUrl, string secondCode)
        {
            return await Task.FromResult(View(new ChangePhoneNumberViewModel
            {
                ReturnUrl = returnUrl,
                SecondCode = secondCode,
                GeetestCode = _geetestService.GetGeetestCode(this),
            }));
        }

        [HttpPost]
        public async Task<IActionResult> ChangePhoneNumber(ChangePhoneNumberInputModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(BuildChangePhoneNumberViewModel(model));
            }

            //提前判断是否通过人机验证
            if (_geetestService.CheckRecaptcha(model.VerifyResult) == false)
            {
                ModelState.AddModelError(string.Empty, "人机验证失败");
                return View(BuildChangePhoneNumberViewModel(model));
            }

            //查找用户
            var user = await FindLoginUser();
            if (user == null)
            {
                return RedirectToAction("Login", new { model.ReturnUrl });
            }

            //检查二次身份验证
            if (!await _verificationCodeService.CheckAsync(model.SecondCode, user.Id, VerificationCodeType.SecondVerify))
            {
                ModelState.AddModelError(string.Empty, "二次身份验证不通过");
                return View(BuildChangePhoneNumberViewModel(model));
            }

            //判断手机号是否被绑定
            if (await _userRepository.AnyAsync(s => s.PhoneNumber == model.PhoneNumber))
            {
                ModelState.AddModelError(string.Empty, "此手机号已经被绑定");
                return View(BuildChangePhoneNumberViewModel(model));
            }

            //发送验证码
            var token = await _userManager.GenerateChangePhoneNumberTokenAsync(user, model.PhoneNumber);
            var code = await _verificationCodeService.GetCodeAsync(token, user.Id, VerificationCodeType.ChangePhoneNumber);
            //向新地址发送
            if (!await _messageService.SendVerificationSMSAsync(code, model.PhoneNumber, user, VerificationCodeType.ChangePhoneNumber))
            {
                ModelState.AddModelError(string.Empty, "验证码发送过于频繁");
            }

            return RedirectToAction("VerifyCode", new { model.SecondCode, UserId = user.Id, NewPhoneNumber = model.PhoneNumber, Type = VerificationCodeType.ChangePhoneNumber, model.ReturnUrl });
        }

        [HttpGet]
        public async Task<IActionResult> UnBind(string returnUrl, string secondCode, SelectModifyFieldType type, string provider)
        {
            return await Task.FromResult(View(new UnBindViewModel
            {
                ReturnUrl = returnUrl,
                SecondCode = secondCode,
                Type = type,
                Provider = provider
            }));
        }

        [HttpPost]
        public async Task<IActionResult> UnBind(UnBindInputModel model, string button)
        {
            if (button == "cancel")
            {
                //取消操作
                return RedirectToAction("SelectModifyField", new { model.SecondCode, model.ReturnUrl });
            }

            //查找用户
            var user = await FindLoginUser();
            if (user == null)
            {
                return RedirectToAction("Login", new { model.ReturnUrl });
            }

            //检查二次身份验证
            if (!await _verificationCodeService.CheckAsync(model.SecondCode, user.Id, VerificationCodeType.SecondVerify))
            {
                ModelState.AddModelError(string.Empty, "二次身份验证不通过");
                return View(BuildUnBindViewModel(model));
            }

            //判断并解绑
            if (model.Type == SelectModifyFieldType.PhoneNumber)
            {
                user.PhoneNumber = null;
                user.PhoneNumberConfirmed = false;
                await _userRepository.UpdateAsync(user);
            }
            else
            {
                var provider = (await _userManager.GetLoginsAsync(user)).FirstOrDefault(s => s.ProviderDisplayName == model.Provider);
                if (provider != null)
                {
                    var result = await _userManager.RemoveLoginAsync(user, provider.LoginProvider, provider.ProviderKey);
                    if (!result.Succeeded)
                    {
                        this.AddModelStateErrors(result.Errors);
                        return View(BuildUnBindViewModel(model));
                    }
                }
            }

            //检查实名验证
            var check = await CheckRealNameAuthentication(user, model.ReturnUrl, true);
            if (check != null)
            {
                //实名验证不通过则退出登录
                await _signInManager.SignOutAsync();
                return check;
            }

            //跳转
            return RedirectToAction("SelectModifyField", new { ShowCpltToast = true, model.SecondCode, model.ReturnUrl });
        }


        #region helper APIs for the AccountController

        private async Task<LoginViewModel> BuildLoginViewModelAsync(string returnUrl)
        {
            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
            if (context?.IdP != null && await _schemeProvider.GetSchemeAsync(context.IdP) != null)
            {
                var local = context.IdP == IdentityServer4.IdentityServerConstants.LocalIdentityProvider;

                // this is meant to short circuit the UI and only trigger the one external IdP
                var vm = new LoginViewModel
                {
                    EnableLocalLogin = local,
                    ReturnUrl = returnUrl,
                    Username = context?.LoginHint,
                };

                if (!local)
                {
                    vm.ExternalProviders = new[] { new ExternalProvider { AuthenticationScheme = context.IdP } };
                }

                return vm;
            }

            var schemes = await _schemeProvider.GetAllSchemesAsync();

            var providers = schemes
                .Where(x => x.DisplayName != null)
                .Select(x => new ExternalProvider
                {
                    DisplayName = x.DisplayName ?? x.Name,
                    AuthenticationScheme = x.Name
                }).ToList();

            var allowLocal = true;
            if (context?.Client.ClientId != null)
            {
                var client = await _clientStore.FindEnabledClientByIdAsync(context.Client.ClientId);
                if (client != null)
                {
                    allowLocal = client.EnableLocalLogin;

                    if (client.IdentityProviderRestrictions != null && client.IdentityProviderRestrictions.Any())
                    {
                        providers = providers.Where(provider => client.IdentityProviderRestrictions.Contains(provider.AuthenticationScheme)).ToList();
                    }
                }
            }

            return new LoginViewModel
            {
                AllowRememberLogin = AccountOptions.AllowRememberLogin,
                EnableLocalLogin = allowLocal && AccountOptions.AllowLocalLogin,
                ReturnUrl = returnUrl,
                Username = context?.LoginHint,
                ExternalProviders = await _accountService.GetExternalProvidersAsync(context),
                GeetestCode = _geetestService.GetGeetestCode(this)
            };
        }

        private async Task<LoginViewModel> BuildLoginViewModelAsync(LoginInputModel model)
        {
            var vm = await BuildLoginViewModelAsync(model.ReturnUrl);
            vm.Username = model.Username;
            vm.RememberLogin = model.RememberLogin;
            vm.GeetestCode = _geetestService.GetGeetestCode(this);
            return vm;
        }

        private async Task<LogoutViewModel> BuildLogoutViewModelAsync(string logoutId)
        {
            var vm = new LogoutViewModel { LogoutId = logoutId, ShowLogoutPrompt = AccountOptions.ShowLogoutPrompt };

            if (User?.Identity.IsAuthenticated != true)
            {
                // if the user is not authenticated, then just show logged out page
                vm.ShowLogoutPrompt = false;
                return vm;
            }

            var context = await _interaction.GetLogoutContextAsync(logoutId);
            if (context?.ShowSignoutPrompt == false)
            {
                // it's safe to automatically sign-out
                vm.ShowLogoutPrompt = false;
                return vm;
            }

            // show the logout prompt. this prevents attacks where the user
            // is automatically signed out by another malicious web page.
            return vm;
        }

        private async Task<LoggedOutViewModel> BuildLoggedOutViewModelAsync(string logoutId)
        {
            // get context information (client name, post logout redirect URI and iframe for federated signout)
            var logout = await _interaction.GetLogoutContextAsync(logoutId);

            var vm = new LoggedOutViewModel
            {
                AutomaticRedirectAfterSignOut = AccountOptions.AutomaticRedirectAfterSignOut,
                PostLogoutRedirectUri = logout?.PostLogoutRedirectUri,
                ClientName = string.IsNullOrEmpty(logout?.ClientName) ? logout?.ClientId : logout?.ClientName,
                SignOutIframeUrl = logout?.SignOutIFrameUrl,
                LogoutId = logoutId
            };

            if (User?.Identity.IsAuthenticated == true)
            {
                var idp = User.FindFirst(JwtClaimTypes.IdentityProvider)?.Value;
                if (idp != null && idp != IdentityServer4.IdentityServerConstants.LocalIdentityProvider)
                {
                    var providerSupportsSignout = await HttpContext.GetSchemeSupportsSignOutAsync(idp);
                    if (providerSupportsSignout)
                    {
                        if (vm.LogoutId == null)
                        {
                            // if there's no current logout context, we need to create one
                            // this captures necessary info from the current logged in user
                            // before we signout and redirect away to the external IdP for signout
                            vm.LogoutId = await _interaction.CreateLogoutContextAsync();
                        }

                        vm.ExternalAuthenticationScheme = idp;
                    }
                }
            }

            return vm;
        }

        private RegisterViewModel BuildRegisterViewModel(RegisterInputModel model)
        {
            var vm = new RegisterViewModel();
            vm.SynchronizationProperties(model);
            vm.GeetestCode = _geetestService.GetGeetestCode(this);
            return vm;
        }

        private VerifyCodeViewModel BuildVerifyCodeViewModel(VerifyCodeInputModel model, bool isDisable = false)
        {
            var vm = new VerifyCodeViewModel();

            vm.SynchronizationProperties(model);
            vm.GeetestCode = _geetestService.GetGeetestCode(this);
            vm.IsDisableRepost = isDisable;
            return vm;
        }

        private AddPhoneNumberViewModel BuildAddPhoneNumberViewModel(AddPhoneNumberInputModel model)
        {
            var vm = new AddPhoneNumberViewModel();
            vm.SynchronizationProperties(model);
            vm.GeetestCode = _geetestService.GetGeetestCode(this);
            return vm;
        }

        private ResetPasswordViewModel BuildResetPasswordViewModel(ResetPasswordInputModel model)
        {
            var vm = new ResetPasswordViewModel();
            vm.SynchronizationProperties(model);
            vm.GeetestCode = _geetestService.GetGeetestCode(this);
            return vm;
        }

        private static SetNewPasswordViewModel BuildSetNewPasswordViewModel(SetNewPasswordInputModel model)
        {
            var vm = new SetNewPasswordViewModel();
            vm.SynchronizationProperties(model);
            return vm;
        }

        private static ChangePasswordViewModel BuildChangePasswordViewModel(ChangePasswordInputModel model)
        {
            var vm = new ChangePasswordViewModel();
            vm.SynchronizationProperties(model);
            return vm;
        }

        private async Task<SelectModifyFieldViewModel> BuildSelectModifyFieldViewModel(ApplicationUser user, string returnUrl, string secondCode, bool showCpltToast = false)
        {
            var model = new SelectModifyFieldViewModel();
            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);

            model.BindInfor = await _accountService.GetAccountBindInforAsync(context, user);
            model.ReturnUrl = returnUrl;
            model.SecondCode = secondCode;
            model.ShowCpltToast = showCpltToast;
            model.GeetestCode = _geetestService.GetGeetestCode(this);
            return model;
        }

        private ChangeEmailViewModel BuildChangeEmailViewModel(ChangeEmailInputModel model)
        {
            var vm = new ChangeEmailViewModel();
            vm.SynchronizationProperties(model);
            vm.GeetestCode = _geetestService.GetGeetestCode(this);
            return vm;
        }

        private ChangePhoneNumberViewModel BuildChangePhoneNumberViewModel(ChangePhoneNumberInputModel model)
        {
            var vm = new ChangePhoneNumberViewModel();
            vm.SynchronizationProperties(model);
            vm.GeetestCode = _geetestService.GetGeetestCode(this);
            return vm;
        }

        private static UnBindViewModel BuildUnBindViewModel(UnBindInputModel model)
        {
            var vm = new UnBindViewModel();
            vm.SynchronizationProperties(model);
            return vm;
        }

        #endregion

        private async Task<IActionResult> RedirectToVerifyEmail(ApplicationUser user, string returnUrl)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var code = await _verificationCodeService.GetCodeAsync(token, user.Id, VerificationCodeType.Register);
            if (!await _messageService.SendVerificationEmailAsync(code, user.Email, user, VerificationCodeType.Register))
            {
                ModelState.AddModelError(string.Empty, "验证码发送过于频繁");
            }

            return RedirectToAction("VerifyCode", new { UserId = user.Id, Type = VerificationCodeType.Register, ReturnUrl = returnUrl });

        }

        private async Task<IActionResult> RedirectToVerifyPhoneNumber(ApplicationUser user, string returnUrl)
        {
            var code = await _verificationCodeService.GetCodeAsync(user.UserName.Sha256(), user.Id, VerificationCodeType.AddPhoneNumber);
            if (!await _messageService.SendVerificationSMSAsync(code, user.PhoneNumber, user, VerificationCodeType.AddPhoneNumber))
            {
                ModelState.AddModelError(string.Empty, "验证码发送过于频繁");
            }

            return RedirectToAction("VerifyCode", new { UserId = user.Id, user.PhoneNumber, Type = VerificationCodeType.AddPhoneNumber, ReturnUrl = returnUrl });
        }

        /// <summary>
        /// 检查用户实名验证
        /// </summary>
        /// <param name="user"></param>
        /// <param name="returnUrl"></param>
        /// <returns>通过返回null</returns>
        private async Task<IActionResult> CheckRealNameAuthentication(ApplicationUser user, string returnUrl, bool showCpltToast = false)
        {
            //绑定电子邮箱
            if (string.IsNullOrWhiteSpace(user.Email))
            {
                throw new Exception("电子邮箱不存在，请重新注册账号，或者联系管理员");
            }
            //验证电子邮箱
            if (!user.EmailConfirmed)
            {
                return await RedirectToVerifyEmail(user, returnUrl);
            }

            //需要实名认证 绑定手机 或 绑定信任的第三方身份验证提供商
            if (!string.IsNullOrWhiteSpace(_configuration["TrustedExternalAuthProviders"]))
            {
                var providers = _configuration["TrustedExternalAuthProviders"].Split(',').Select(s => s.Trim());
                if ((await _userManager.GetLoginsAsync(user)).Any(s => providers.Contains(s.ProviderDisplayName)))
                {
                    //通过
                    return null;
                }
            }

            //检查手机号是否存在 或 未验证
            if (string.IsNullOrWhiteSpace(user.PhoneNumber) || !user.PhoneNumberConfirmed)
            {
                //添加验证码用于身份验证
                var code = await _verificationCodeService.GetCodeAsync(user.UserName.Sha256(), user.Id, VerificationCodeType.VerifyPassword);
                //跳转选择验证身份方式
                return RedirectToAction("SelectRealNameMethod", new { Code = code, UserId = user.Id, ReturnUrl = returnUrl, ShowCpltToast = showCpltToast });
            }

            //通过
            return null;
        }

        /// <summary>
        /// 用户登入
        /// </summary>
        /// <param name="result"></param>
        /// <param name="user"></param>
        /// <param name="provider"></param>
        /// <param name="providerUserId"></param>
        /// <returns></returns>
        private async Task<IActionResult> LoginFromExternalProvider(AuthenticateResult result, ApplicationUser user, string provider, string providerUserId, string returnUrl)
        {
            //检查实名验证
            var check = await CheckRealNameAuthentication(user, returnUrl);
            if (check != null)
            {
                return check;
            }

            // this allows us to collect any additional claims or properties
            // for the specific protocols used and store them in the local auth cookie.
            // this is typically used to store data needed for signout from those protocols.
            var additionalLocalClaims = new List<Claim>();
            var localSignInProps = new AuthenticationProperties();
            ProcessLoginCallback(result, additionalLocalClaims, localSignInProps);

            // issue authentication cookie for user
            // we must issue the cookie maually, and can't use the SignInManager because
            // it doesn't expose an API to issue additional claims from the login workflow
            var principal = await _signInManager.CreateUserPrincipalAsync(user);
            additionalLocalClaims.AddRange(principal.Claims);
            var name = principal.FindFirst(JwtClaimTypes.Name)?.Value ?? user.Id;

            var isuser = new IdentityServerUser(user.Id)
            {
                DisplayName = name,
                IdentityProvider = provider,
                AdditionalClaims = additionalLocalClaims
            };

            await HttpContext.SignInAsync(isuser, localSignInProps);

            // delete temporary cookie used during external authentication
            await HttpContext.SignOutAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);



            // check if external login is in the context of an OIDC request
            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
            await _events.RaiseAsync(new UserLoginSuccessEvent(provider, providerUserId, user.Id, name, true, context?.Client.ClientId));

            if (context != null)
            {
                if (context.IsNativeClient())
                {
                    // The client is native, so this change in how to
                    // return the response is for better UX for the end user.
                    return this.LoadingPage("Redirect", returnUrl);
                }
            }

            return Redirect(returnUrl ?? "~/");
        }

        // if the external login is OIDC-based, there are certain things we need to preserve to make logout work
        // this will be different for WS-Fed, SAML2p or other protocols
        private void ProcessLoginCallback(AuthenticateResult externalResult, List<Claim> localClaims, AuthenticationProperties localSignInProps)
        {
            // if the external system sent a session id claim, copy it over
            // so we can use it for single sign-out
            var sid = externalResult.Principal.Claims.FirstOrDefault(x => x.Type == JwtClaimTypes.SessionId);
            if (sid != null)
            {
                localClaims.Add(new Claim(JwtClaimTypes.SessionId, sid.Value));
            }

            // if the external provider issued an id_token, we'll keep it for signout
            var idToken = externalResult.Properties.GetTokenValue("id_token");
            if (idToken != null)
            {
                localSignInProps.StoreTokens(new[] { new AuthenticationToken { Name = "id_token", Value = idToken } });
            }
        }

        /// <summary>
        /// 获取外部身份验证提供的用户信息
        /// </summary>
        /// <param name="claims"></param>
        /// <returns></returns>
        private (string email, string name) GetExternalUserInforAsync(IEnumerable<Claim> claims)
        {
            // create a list of claims that we want to transfer into our store
            var filtered = new List<Claim>();

            // user's display name
            var name = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.Name)?.Value ??
                claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;
            if (name == null)
            {
                var first = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.GivenName)?.Value ??
                    claims.FirstOrDefault(x => x.Type == ClaimTypes.GivenName)?.Value;
                var last = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.FamilyName)?.Value ??
                    claims.FirstOrDefault(x => x.Type == ClaimTypes.Surname)?.Value;
                if (first != null && last != null)
                {
                    name = first + " " + last;
                }
                else if (first != null)
                {
                    name = first;
                }
                else if (last != null)
                {
                    name = last;
                }
            }

            // email
            var email = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.Email)?.Value ??
               claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;


            return (email, name);
        }

        /// <summary>
        /// 检查并绑定外部用户到当前账号
        /// </summary>
        /// <returns></returns>
        private async Task<IdentityResult> CheckAndBindExternalToCurrent(ApplicationUser user)
        {
            var idsResult = await HttpContext.AuthenticateAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);
            if (idsResult?.Succeeded == true)
            {
                // lookup our user and external provider info
                var (idsUser, provider, providerUserId, claims) = await _accountService.FindUserFromExternalProviderAsync(idsResult);
                if (idsUser == null)
                {
                    // delete temporary cookie used during external authentication
                    await HttpContext.SignOutAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);

                    return await _userManager.AddLoginAsync(user, new UserLoginInfo(provider, providerUserId, provider));
                }
            }

            return IdentityResult.Success;
        }

        private async Task<ApplicationUser> FindLoginUser()
        {
            var id = User?.Claims?.FirstOrDefault(s => s.Type == JwtClaimTypes.Subject)?.Value;
            return await _userRepository.FirstOrDefaultAsync(s => s.Id == id);
        }

    }
}
