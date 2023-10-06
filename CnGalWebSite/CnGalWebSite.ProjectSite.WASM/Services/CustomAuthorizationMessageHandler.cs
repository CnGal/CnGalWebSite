using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components;

namespace CnGalWebSite.ProjectSite.WASM.Services
{
    public class CustomAuthorizationMessageHandler : AuthorizationMessageHandler
    {
        public CustomAuthorizationMessageHandler(IAccessTokenProvider provider,
            NavigationManager navigationManager,IConfiguration configuration)
            : base(provider, navigationManager)
        {
            ConfigureHandler(authorizedUrls: new[] { configuration["CnGalAPI"], configuration["WebApiPath"], configuration["ImageApiPath"] });
        }
    }
}
