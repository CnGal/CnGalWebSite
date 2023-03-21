using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using System;
using System.Linq;
using CnGalWebSite.IdentityServer.Models.DataModels.Account;
using CnGalWebSite.IdentityServer.Models.ViewModels.Account;
using BlazorComponent;
using IdentityServer4.Stores;
using IdentityServer4.Models;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.IdentityServer.Models.DataModels.Records;
using Microsoft.AspNetCore.Http;

namespace CnGalWebSite.IdentityServer.Services.Account
{
    public class AccountService:IAccountService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IClientStore _clientStore;
        private readonly IAuthenticationSchemeProvider _schemeProvider;
        private readonly IHttpContextAccessor _accessor;
        private readonly IRepository<OperationRecord, long> _operationRecordRepository;

        public AccountService(UserManager<ApplicationUser> userManager, IClientStore clientStore, IAuthenticationSchemeProvider schemeProvider, IRepository<OperationRecord, long> operationRecordRepository, IHttpContextAccessor accessor)
        {
            _userManager = userManager;
            _clientStore = clientStore;
            _schemeProvider = schemeProvider;
            _operationRecordRepository = operationRecordRepository;
            _accessor = accessor;   
        }

        /// <summary>
        /// 从外部身份验证提供程序中查找用户
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<(ApplicationUser user, string provider, string providerUserId, IEnumerable<Claim> claims)> FindUserFromExternalProviderAsync(AuthenticateResult result)
        {
            var externalUser = result.Principal;

            // try to determine the unique id of the external user (issued by the provider)
            // the most common claim type for that are the sub claim and the NameIdentifier
            // depending on the external provider, some other claim type might be used
            var userIdClaim = externalUser.FindFirst(JwtClaimTypes.Subject) ??
                              externalUser.FindFirst(ClaimTypes.NameIdentifier) ??
                              throw new Exception("Unknown userid");

            // remove the user id claim so we don't include it as an extra claim if/when we provision the user
            var claims = externalUser.Claims.ToList();
            claims.Remove(userIdClaim);

            var provider = result.Properties.Items["scheme"];
            var providerUserId = userIdClaim.Value;

            // find external user
            var user = await _userManager.FindByLoginAsync(provider, providerUserId);

            return (user, provider, providerUserId, claims);
        }

        public async Task<AccountBindInfor> GetAccountBindInforAsync(AuthorizationRequest context,ApplicationUser user)
        {
            var model = new AccountBindInfor();
            //密码
            model.AccountFields.Add(new SelectModifyAccountFieldModel
            {
                Type = SelectModifyFieldType.Password,
                Actions = new Dictionary<SelectModifyFieldActionType, string> { { SelectModifyFieldActionType.Edit, "ChangePassword" } }
            });
            //电子邮箱
            model.AccountFields.Add(new SelectModifyAccountFieldModel
            {
                Type = SelectModifyFieldType.Email,
                Actions = new Dictionary<SelectModifyFieldActionType, string> { { SelectModifyFieldActionType.Edit, "ChangeEmail" } }
            });

            //手机号
            if (user.PhoneNumberConfirmed)
            {
                model.AccountFields.Add(new SelectModifyAccountFieldModel
                {
                    Type = SelectModifyFieldType.PhoneNumber,
                    Actions = new Dictionary<SelectModifyFieldActionType, string> { { SelectModifyFieldActionType.Edit, "ChangePhoneNumber" }, { SelectModifyFieldActionType.Unbind, "Unbind" } }
                });
            }
            else
            {
                model.AccountFields.Add(new SelectModifyAccountFieldModel
                {
                    Type = SelectModifyFieldType.PhoneNumber,
                    Actions = new Dictionary<SelectModifyFieldActionType, string> { { SelectModifyFieldActionType.Bind, "AddPhoneNumber" } }
                });
            }

            //第三方登入
            var providers =await GetExternalProvidersAsync(context);
            var logins = (await _userManager.GetLoginsAsync(user)).Select(s => s.ProviderDisplayName);
            model.ExternalFields = providers.Select(s => new SelectModifyExternalFieldModel
            {
                AuthenticationScheme = s.AuthenticationScheme,
                DisplayName = s.DisplayName,
                Actions = logins.Contains(s.DisplayName) ? new Dictionary<SelectModifyFieldActionType, string> { { SelectModifyFieldActionType.Edit, null }, { SelectModifyFieldActionType.Unbind, "UnBind" } } : new Dictionary<SelectModifyFieldActionType, string> { { SelectModifyFieldActionType.Bind, null } }
            }).ToList();

            return model;
        }

        public async Task<List<ExternalProvider>> GetExternalProvidersAsync(AuthorizationRequest context)
        {
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

            return providers;
        }

        public async Task<bool> IsNewUserAsync(string id)
        {
            return await _operationRecordRepository.AnyAsync(s => s.ApplicationUserId == id && s.Type == OperationRecordType.Registe);
        }
    }
}
