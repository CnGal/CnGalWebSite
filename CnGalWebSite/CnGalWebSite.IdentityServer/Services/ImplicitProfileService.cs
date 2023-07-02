
using CnGalWebSite.IdentityServer.Models.DataModels.Account;
using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CnGalWebSite.IdentityServer.Services
{
    public class ImplicitProfileService : IProfileService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public ImplicitProfileService(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<List<Claim>> GetClaimsFromUserAsync(ApplicationUser user)
        {
            var claims = new List<Claim> {
                new Claim(JwtClaimTypes.Subject,user.Id.ToString()),
                new Claim(JwtClaimTypes.Name,user.UserName),
                new Claim(JwtClaimTypes.PreferredUserName,user.UserName)
            };

            var role = await _userManager.GetRolesAsync(user);
            role.ToList().ForEach(f =>
            {
                claims.Add(new Claim(JwtClaimTypes.Role, f));
            });


            //添加SteamId
            var steamUrl = (await _userManager.GetLoginsAsync(user)).FirstOrDefault(s => s.ProviderDisplayName == "Steam")?.ProviderKey;
            if (string.IsNullOrWhiteSpace(steamUrl)==false)
            {
                claims.Add(new Claim("SteamId", steamUrl.Split('/').Last()));

            }
            return claims;
        }

        /// <summary>
        /// 获取用户Claims
        /// 用户请求userinfo endpoint时会触发该方法
        /// http://localhost:5003/connect/userinfo
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var subjectId = context.Subject.Claims.FirstOrDefault(c => c.Type == "sub").Value;
            var user = await _userManager.FindByIdAsync(subjectId);
            context.IssuedClaims = await GetClaimsFromUserAsync(user);
        }

        /// <summary>
        /// 判断用户是否可用
        /// Identity Server会确定用户是否有效
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task IsActiveAsync(IsActiveContext context)
        {
            var subjectId = context.Subject.Claims.FirstOrDefault(c => c.Type == "sub").Value;
            var user = await _userManager.FindByIdAsync(subjectId);
            context.IsActive = user != null; //该用户是否已经激活，可用，否则不能接受token

            /*
             这样还应该判断用户是否已经锁定，那么应该IsActive=false
             */
        }
    }
}
