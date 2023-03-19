using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CnGalWebSite.IdentityServer.Admin.Shared.Options;

namespace CnGalWebSite.IdentityServer.Admin.WASM.Services
{
    public class CustomAuthorizationMessageHandler : AuthorizationMessageHandler
    {
        public CustomAuthorizationMessageHandler(IAccessTokenProvider provider,
            NavigationManager navigationManager)
            : base(provider, navigationManager)
        {
            ConfigureHandler(authorizedUrls: new[] { "https://localhost:5001/" });
        }
    }
}
