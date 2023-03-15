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
using CnGalWebSite.IdentityServer.Models.Account;
using CnGalWebSite.IdentityServer.Services.Account;
using CnGalWebSite.IdentityServer.Services.Messages;
using CnGalWebSite.IdentityServer.Models.Messages;
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

namespace IdentityServerHost.Quickstart.UI
{
    [SecurityHeaders]
    [AllowAnonymous]
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
        private readonly IAccountService _accountService;
        private readonly IRepository<ApplicationUser, string> _userRepository;
        private readonly IConfiguration _configuration;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IIdentityServerInteractionService interaction,
            IClientStore clientStore, IAccountService accountService, IConfiguration configuration,
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
            _userRepository= userRepository;
            _accountService=accountService;
            _configuration = configuration;
        }

        /// <summary>
        /// Entry point into the login workflow
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Login(string returnUrl,bool showCpltRegistToast)
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
            vm.ShowCpltRegistToast = showCpltRegistToast;

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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginInputModel model, string button)
        {
            // check if we are in the context of an authorization request
            var context = await _interaction.GetAuthorizationContextAsync(model.ReturnUrl);

            // the user clicked the "cancel" button
            if (button != "login")
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

            //查找用户
            var user = await _userManager.FindByEmailAsync(model.Username) ?? await _userManager.FindByNameAsync(model.Username) ?? await _userManager.Users.FirstOrDefaultAsync(s => s.PhoneNumber == model.Username);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, AccountOptions.InvalidCredentialsErrorMessage);
                return View(await BuildLoginViewModelAsync(model));
            }

            //登入
            var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberLogin, lockoutOnFailure: true);
            if (!result.Succeeded)
            {
                if (!result.IsLockedOut)
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
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Register(string returnUrl)
        {
            var model = new RegisterViewModel
            {
                ReturnUrl = returnUrl,
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
                    model.Name=name;
                }
            }

            return await Task.FromResult(View(model));
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterInputModel model)
        {
            if (!ModelState.IsValid)
            {
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
            result =await CheckAndBindExternalToCurrent(user);
            if (!result.Succeeded)
            {
                this.AddModelStateErrors(result.Errors);
                return View(BuildRegisterViewModel(model));
            }


            //跳转验证邮箱
            return await RedirectToVerifyEmail(user, model.ReturnUrl);

        }

        [HttpGet]
        public async Task<IActionResult> VerifyCode(string returnUrl, string email, VerificationCodeType type)
        {
            return await Task.FromResult(View(new VerifyCodeViewModel
            {
                IsDisableRepost = true,
                ReturnUrl = returnUrl,
                Email = email,
                Type = type
            }));
        }

        [HttpPost]
        public async Task<IActionResult> VerifyCode(VerifyCodeInputModel model, string button)
        {


            //查找用户
            var user = await _userManager.Users.FirstOrDefaultAsync(s => s.Email == model.Email);
            if (user == null)
            {
                //伪装正确
                ModelState.AddModelError(string.Empty, "验证码错误");
                return View(BuildVerifyCodeViewModel(model, true));
            }

            if (button == "repost")
            {
                //重新获取验证码并发送
                if (model.Type.IsPhoneNumber())
                {
                    var code = await _verificationCodeService.GetCodeAsync(null, user.PhoneNumber, model.Type);
                    if (!await _messageService.SendVerificationSMSAsync(code, user, model.Type))
                    {
                        ModelState.AddModelError(string.Empty, "验证码发送过于频繁");
                    }
                }
                else
                {
                    var code = await _verificationCodeService.GetCodeAsync(null, user.Email, model.Type);
                    if (!await _messageService.SendVerificationEmailAsync(code, user.Email, user, model.Type))
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

            var token = await _verificationCodeService.GetTokenAsync(model.Code, model.Type.IsPhoneNumber() ? model.PhoneNumber : model.Email, model.Type);
            if (string.IsNullOrWhiteSpace(token))
            {
                ModelState.AddModelError(string.Empty, "验证码错误");
                return View(BuildVerifyCodeViewModel(model));
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
                //跳转绑定手机
                return RedirectToAction("ChooseRealNameMethod", new { user.Email, model.ReturnUrl });
            }
            else if (model.Type == VerificationCodeType.AddPhoneNumber)
            {
                //验证手机号
                if (user.UserName.Sha256()!=token)
                {
                    ModelState.AddModelError(string.Empty, "验证码错误");
                    return View(BuildVerifyCodeViewModel(model));
                }
                //设置手机号已验证
                user.PhoneNumberConfirmed = true;
                await _userRepository.UpdateAsync(user);

                //完成所有流程 跳转登入
                return RedirectToAction("Login", new { ShowCpltRegistToast = true, model.ReturnUrl });
            }
            else
            {
                throw new Exception("未知操作");
            }
        }

        [HttpGet]
        public async Task<IActionResult> AddPhoneNumber(string returnUrl, string email)
        {
            return await Task.FromResult(View(new AddPhoneNumberViewModel
            {
                ReturnUrl = returnUrl,
                Email = email,
            }));
        }

        [HttpPost]
        public async Task<IActionResult> AddPhoneNumber(AddPhoneNumberInputModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(BuildAddPhoneNumberViewModel(model));
            }

            //查找用户
            var user = await _userRepository.FirstOrDefaultAsync(s => s.Email == model.Email);
            if (user == null)
            {
                //伪装正确
                ModelState.AddModelError(string.Empty, "此手机号已经被绑定");
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
        public async Task<IActionResult> ChooseRealNameMethod(string returnUrl, string email)
        {

            //检查支持的外部身份验证提供商
            var vm =await BuildLoginViewModelAsync(returnUrl);
            var providers = _configuration["TrustedExternalAuthProviders"].Split(',').Select(s => s.Trim());

            return await Task.FromResult(View(new ChooseRealNameMethodViewModel
            {
                ReturnUrl = returnUrl,
                Email = email,
                ExternalProviders = vm.ExternalProviders.Where(s => providers.Contains(s.DisplayName))
            }));
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
                ExternalProviders = providers.ToArray()
            };
        }

        private async Task<LoginViewModel> BuildLoginViewModelAsync(LoginInputModel model)
        {
            var vm = await BuildLoginViewModelAsync(model.ReturnUrl);
            vm.Username = model.Username;
            vm.RememberLogin = model.RememberLogin;
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

        private static RegisterViewModel BuildRegisterViewModel(RegisterInputModel model)
        {
            var vm = new RegisterViewModel();
            vm.SynchronizationProperties(model);
            return vm;
        }

        private static VerifyCodeViewModel BuildVerifyCodeViewModel(VerifyCodeInputModel model, bool isDisable = false)
        {
            var vm = new VerifyCodeViewModel();
            vm.IsDisableRepost = isDisable;
            vm.SynchronizationProperties(model);
            return vm;
        }

        private static AddPhoneNumberViewModel BuildAddPhoneNumberViewModel(AddPhoneNumberInputModel model)
        {
            var vm = new AddPhoneNumberViewModel();
            vm.SynchronizationProperties(model);
            return vm;
        }

        #endregion

        private async Task<IActionResult> RedirectToVerifyEmail(ApplicationUser user, string returnUrl)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var code = await _verificationCodeService.GetCodeAsync(token, user.Email, VerificationCodeType.Register);
            if (!await _messageService.SendVerificationEmailAsync(code, user.Email, user, VerificationCodeType.Register))
            {
                ModelState.AddModelError(string.Empty, "验证码发送过于频繁");
            }

            return RedirectToAction("VerifyCode", new { user.Email, Type = VerificationCodeType.Register, ReturnUrl = returnUrl });

        }

        private async Task<IActionResult> RedirectToVerifyPhoneNumber(ApplicationUser user, string returnUrl)
        {
            var code = await _verificationCodeService.GetCodeAsync(user.UserName.Sha256(), user.PhoneNumber, VerificationCodeType.AddPhoneNumber);
            if (!await _messageService.SendVerificationSMSAsync(code, user, VerificationCodeType.AddPhoneNumber))
            {
                ModelState.AddModelError(string.Empty, "验证码发送过于频繁");
            }

            return RedirectToAction("VerifyCode", new { user.Email, user.PhoneNumber, Type = VerificationCodeType.AddPhoneNumber, ReturnUrl = returnUrl });

        }

        /// <summary>
        /// 检查用户实名验证
        /// </summary>
        /// <param name="user"></param>
        /// <param name="returnUrl"></param>
        /// <returns>通过返回null</returns>
        private async Task<IActionResult> CheckRealNameAuthentication(ApplicationUser user, string returnUrl)
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
                //跳转选择验证身份方式
                return RedirectToAction("ChooseRealNameMethod", new { user.Email, ReturnUrl = returnUrl });
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
        private async Task<IActionResult> LoginFromExternalProvider(AuthenticateResult result, ApplicationUser user, string provider, string providerUserId,string returnUrl)
        {
            //检查实名验证
            var check = await CheckRealNameAuthentication(user, returnUrl);
            if(check != null)
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

            return Redirect(returnUrl);
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
        private  (string email, string name) GetExternalUserInforAsync( IEnumerable<Claim> claims)
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

    }
}
