using CnGalWebSite.Core.Services;
using CnGalWebSite.ProjectSite.Shared;
using CnGalWebSite.ProjectSite.Shared.Components.Themes;
using CnGalWebSite.ProjectSite.Shared.Extensions;
using CnGalWebSite.ProjectSite.WASM;
using CnGalWebSite.ProjectSite.WASM.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.RootComponents.Add<StyleCard>("#global-style");

builder.Services.AddProjectSite();

//日志
builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));

//注册身份验证的HttpClient
builder.Services.AddScoped<CustomAuthorizationMessageHandler>();
builder.Services.AddHttpClient("AuthAPI")
    .AddHttpMessageHandler<CustomAuthorizationMessageHandler>();
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>()
    .CreateClient("AuthAPI"));

//注册匿名的HttpClient
builder.Services.AddHttpClient("AnonymousAPI");

//注入自定义服务
builder.Services.AddScoped<IHttpService, HttpService>();

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
