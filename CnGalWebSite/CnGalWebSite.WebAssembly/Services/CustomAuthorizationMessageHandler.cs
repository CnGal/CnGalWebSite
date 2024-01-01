using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CnGalWebSite.DataModel.Helper;

namespace CnGalWebSite.WebAssembly.Services
{
    public class CustomAuthorizationMessageHandler : AuthorizationMessageHandler
    {
        public CustomAuthorizationMessageHandler(IAccessTokenProvider provider,
            NavigationManager navigationManager)
            : base(provider, navigationManager)
        {
            ConfigureHandler(authorizedUrls: new[] { ToolHelper.WebApiPath, ToolHelper.ImageApiPath, ToolHelper.TaskApiPath });
        }
    }
}
