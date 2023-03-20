using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.IdentityServer.Models.DataModels.Account;
using CnGalWebSite.IdentityServer.Services.Account;
using CnGalWebSite.IdentityServer.Services.Geetest;
using CnGalWebSite.IdentityServer.Services.Messages;
using IdentityModel;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using static IdentityServer4.IdentityServerConstants;

namespace CnGalWebSite.IdentityServer.APIControllers
{
    [Authorize(LocalApi.PolicyName,Roles ="User")]
    [ApiController]
    [Route("api/account/[action]")]
    public class AccountAPIController : ControllerBase
    {
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IAccountService _accountService;
        private readonly IRepository<ApplicationUser, string> _userRepository;

        public AccountAPIController( IIdentityServerInteractionService interaction, IAccountService accountService,IRepository<ApplicationUser, string> userRepository)
        {
            _interaction = interaction;
            _userRepository = userRepository;
            _accountService = accountService;
        }

        [HttpGet]
        public async Task<ActionResult<AccountBindInfor>> GetBindInfor()
        {
            var user =await FindLoginUser();
            var context = await _interaction.GetAuthorizationContextAsync(string.Empty);

            return await _accountService.GetAccountBindInforAsync(context, user);
        }

        [HttpGet]
        public async Task<ActionResult<List<KeyValuePair<string,string>>>> GetUserClaims()
        {
            return await Task.FromResult(User?.Claims?.Select(s => new KeyValuePair<string, string>(s.Type, s.Value))?.ToList());
        }

        private async Task<ApplicationUser> FindLoginUser()
        {
            var id = User?.Claims?.FirstOrDefault(s => s.Type == JwtClaimTypes.Subject||s.Type == ClaimTypes.NameIdentifier)?.Value;
            return await _userRepository.FirstOrDefaultAsync(s => s.Id == id);
        }
    }
}
