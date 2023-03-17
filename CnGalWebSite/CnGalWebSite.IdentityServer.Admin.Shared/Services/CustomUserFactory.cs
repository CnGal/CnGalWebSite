using Microsoft.AspNetCore.Components.WebAssembly.Authentication.Internal;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CnGalWebSite.IdentityServer.Admin.Shared.Services
{
    public class CustomUserFactory
         : AccountClaimsPrincipalFactory<RemoteUserAccount>
    {
        public CustomUserFactory(IAccessTokenProviderAccessor accessor)
        : base(accessor)
        {
        }

        public async override ValueTask<ClaimsPrincipal> CreateUserAsync(
            RemoteUserAccount account,
            RemoteAuthenticationUserOptions options)
        {
            var user = await base.CreateUserAsync(account, options);

            if (user.Identity.IsAuthenticated)
            {
                var identity = (ClaimsIdentity)user.Identity;
                var roleClaims = identity.FindAll(identity.RoleClaimType);

                if (roleClaims != null && roleClaims.Any())
                {
                    var temp = new List<Claim>();
                    foreach (var existingClaim in roleClaims)
                    {
                        temp.Add(existingClaim);
                    }
                    foreach (var existingClaim in temp)
                    {
                        identity.RemoveClaim(existingClaim);
                    }

                    var rolesElem = account.AdditionalProperties[identity.RoleClaimType];

                    if (rolesElem is JsonElement roles)
                    {
                        if (roles.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var role in roles.EnumerateArray())
                            {
                                identity.AddClaim(new Claim(options.RoleClaim, role.GetString()));
                            }
                        }
                        else
                        {
                            identity.AddClaim(new Claim(options.RoleClaim, roles.GetString()));
                        }
                    }
                }
            }

            return user;
        }
    }
}
