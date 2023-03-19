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

        public AccountAPIController(
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
