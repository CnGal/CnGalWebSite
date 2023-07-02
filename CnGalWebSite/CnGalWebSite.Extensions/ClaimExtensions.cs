using IdentityModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.Extensions
{
     public static class ClaimExtensions
    {
        /// <summary>
        /// 获取外部身份验证提供的用户信息
        /// </summary>
        /// <param name="claims"></param>
        /// <returns></returns>
        public static (string email, string name) GetExternalUserInfor(this IEnumerable<Claim> claims)
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

        public static string GetUserId(this IEnumerable<Claim> claims)
        {
            return claims?.FirstOrDefault(s => s.Type == JwtClaimTypes.Subject || s.Type == ClaimTypes.NameIdentifier)?.Value;
        }

        public static string GetUserSteamId(this IEnumerable<Claim> claims)
        {
            return claims?.FirstOrDefault(s => s.Type == "SteamId")?.Value;
        }
    }
}
