using CnGalWebSite.IdentityServer.Admin.Shared;
using CnGalWebSite.IdentityServer.Admin.Shared.Options;
using CnGalWebSite.IdentityServer.Admin.Shared.Services;
using CnGalWebSite.IdentityServer.Admin.WASM;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
//判断是否 SSR
StaticOptions.IsSSR = StaticOptions.PreSetIsSSR == null ? false : StaticOptions.PreSetIsSSR.Value;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped<CustomAuthorizationMessageHandler>();
builder.Services.AddHttpClient("IdsAdminAPI")
    //声明使用以上自定义的处理程序
    .AddHttpMessageHandler<CustomAuthorizationMessageHandler>();
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>()
    .CreateClient("IdsAdminAPI"));

//注入自定义服务
builder.Services.AddScoped<IDataCacheService, DataCacheService>();
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
}).AddAccountClaimsPrincipalFactory<CustomUserFactory>();
await builder.Build().RunAsync();
