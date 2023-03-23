using Blazored.LocalStorage;
using CnGalWebSite.IdentityServer.Admin.Shared;
using CnGalWebSite.IdentityServer.Admin.Shared.Options;
using CnGalWebSite.IdentityServer.Admin.Shared.Services;
using CnGalWebSite.IdentityServer.Admin.WASM;
using CnGalWebSite.IdentityServer.Admin.WASM.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
//判断是否 SSR
StaticOptions.IsSSR = StaticOptions.PreSetIsSSR == null ? false : StaticOptions.PreSetIsSSR.Value;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

//本地存储
builder.Services.AddBlazoredLocalStorage();


//注册身份验证的HttpClient
builder.Services.AddScoped<CustomAuthorizationMessageHandler>();
builder.Services.AddHttpClient("AuthAPI")
    .AddHttpMessageHandler<CustomAuthorizationMessageHandler>();
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>()
    .CreateClient("AuthAPI"));

//注册匿名的HttpClient
builder.Services.AddHttpClient("AnonymousAPI");

//注入自定义服务
builder.Services.AddScoped(typeof(ISingleDataCacheService<>), typeof(SingleDataCacheService<>));
builder.Services.AddScoped<INavigationService, NavigationService>();
builder.Services.AddScoped<IHttpService, HttpService>();
//修改Masa主题
builder.Services.AddMasaBlazor(s => s.ConfigureTheme(s =>
{
    s.Themes.Light.Primary = "#f06292";
    s.Themes.Dark.Primary = "#2196F3";
}));
//添加OpenId服务
builder.Services.AddOidcAuthentication(options =>
{
    builder.Configuration.Bind("Local", options.ProviderOptions);
    options.UserOptions.RoleClaim = "role";
    options.UserOptions.NameClaim = "name";
    options.ProviderOptions.ResponseMode = "query";
    options.ProviderOptions.ResponseType = "code";
}).AddAccountClaimsPrincipalFactory<CustomUserFactory>();
await builder.Build().RunAsync();
