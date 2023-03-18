using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CnGalWebSite.DataModel.Helper;

namespace CnGalWebSite.Shared.Provider
{
    public class CustomAuthorizationMessageHandler : AuthorizationMessageHandler
    {
        public CustomAuthorizationMessageHandler(IAccessTokenProvider provider,
            NavigationManager navigationManager)
            : base(provider, navigationManager)
        {
            ConfigureHandler(
                //这是Web Api的根地址
                authorizedUrls: new[] { ToolHelper.WebApiPath },
                //对应Api Scope, 表示请求上面的Web Api之前需要先获取该Scope对应的Access Token，并附在Http头里面
                scopes: new[] { "CnGalAPI" });
        }
    }
}
