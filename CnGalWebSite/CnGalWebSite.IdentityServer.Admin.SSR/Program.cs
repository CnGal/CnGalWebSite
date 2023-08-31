using BlazorComponent;
using CnGalWebSite.Core.Services;
using CnGalWebSite.Core.Services.Query;
using CnGalWebSite.IdentityServer.Admin.Shared.Options;
using CnGalWebSite.IdentityServer.Admin.Shared.Services;
using CnGalWebSite.IdentityServer.Admin.SSR.Plumbing;
using CnGalWebSite.IdentityServer.Admin.SSR.Services;
using IdentityModel.AspNetCore.AccessTokenManagement;
using IdentityModel.Client;
using Masa.Blazor;
using Masa.Blazor.Presets;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.HttpOverrides;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);
//判断是否 SSR
StaticOptions.IsSSR = StaticOptions.PreSetIsSSR == null ? true : StaticOptions.PreSetIsSSR.Value;
//覆盖默认api地址
if (string.IsNullOrWhiteSpace(builder.Configuration["IdsApiUrl"]) == false)
{
    StaticOptions.IdsApiUrl = builder.Configuration["IdsApiUrl"];
}


// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
//自定义服务
builder.Services.AddScoped<IHttpService, HttpService>();
builder.Services.AddSingleton<ITokenStoreService, TokenStoreService>();
builder.Services.AddScoped<INavigationService, NavigationService> ();
//添加Query
builder.Services.AddScoped<IQueryService, QueryService>();
//设置主题
builder.Services.AddMasaBlazor(options =>
{
    //主题
    options.ConfigureTheme(s =>
    {
        if (DateTime.Now.Day == 1 && DateTime.Now.Month == 4)
        {
            s.Themes.Light.Primary = "#4CAF50";
        }
        else
        {
            s.Themes.Light.Primary = "#f06292";
        }
        s.Themes.Dark.Primary = "#0078BF";
    });
});
//使用 IdentityModel 管理刷新令牌
builder.Services.AddAccessTokenManagement();

// not allowed to programmatically use HttpContext in Blazor Server.
// that's why tokens cannot be managed in the login session
builder.Services.AddScoped<IUserAccessTokenStore, ServerSideTokenStore>();

// registers HTTP client that uses the managed user access token
builder.Services.AddHttpClient<IHttpService, HttpService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["IdsApiUrl"]);
});

//添加认身份证
//builder.Services.AddAuthorization(options =>
//{
//    // By default, all incoming requests will be authorized according to the default policy
//    // comment out if you want to drive the login/logout workflow from the UI
//    options.FallbackPolicy = options.DefaultPolicy;
//});

//注册Cookie服务
builder.Services.AddTransient<CookieEvents>();
builder.Services.AddTransient<OidcEvents>();

//默认采用cookie认证方案，添加oidc认证方案
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "cookies";
    options.DefaultChallengeScheme = "cngal";
    options.DefaultSignOutScheme = "cngal";
})
    .AddCookie("cookies", options =>
    {
        options.Cookie.Name = "__Host-blazor";
        options.Cookie.SameSite = SameSiteMode.Lax;

        options.EventsType = typeof(CookieEvents);
    })
    .AddOpenIdConnect("cngal", options =>
    {
        //id4服务的地址
        options.Authority = builder.Configuration["Authority"];

        //id4配置的ClientId以及ClientSecrets
        options.ClientId = builder.Configuration["ClientId"];
        options.ClientSecret = builder.Configuration["ClientSecret"];

        //认证模式
        options.ResponseType = "code";
        options.ResponseMode = "query";

        options.MapInboundClaims = false;
        //保存token到本地
        options.SaveTokens = true;
        //不检查Https
        options.RequireHttpsMetadata = false;
        //很重要，指定从Identity Server的UserInfo地址来取Claim
        options.GetClaimsFromUserInfoEndpoint = true;
        //指定要取哪些资料（除Profile之外，Profile是默认包含的）
        options.Scope.Add("openid");
        options.Scope.Add("role");
        options.Scope.Add("IdentityServerApi");
        options.Scope.Add("offline_access");
        //这里是个ClaimType的转换，Identity Server的ClaimType和Blazor中间件使用的名称有区别，需要统一。
        options.TokenValidationParameters.RoleClaimType = "role";
        options.TokenValidationParameters.NameClaimType = "name";
        //注册事件
        options.EventsType = typeof(OidcEvents);


        options.Events.OnUserInformationReceived = (context) =>
        {
            //回顾之前关于WebAssembly的例子，涉及到数组的转换，这里也一样要处理

            ClaimsIdentity claimsId = context.Principal.Identity as ClaimsIdentity;

            var roleElement = context.User.RootElement.GetProperty("role");
            if (roleElement.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
                var roles = context.User.RootElement.GetProperty("role").EnumerateArray().Select(e =>
                {
                    return e.ToString();
                });
                claimsId.AddClaims(roles.Select(r => new Claim("role", r)));
            }
            else
            {
                claimsId.AddClaim(new Claim("role", roleElement.ToString()));
            }


            return Task.CompletedTask;
        };
    });

//添加状态检查
builder.Services.AddHealthChecks();
//添加真实IP
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

var app = builder.Build();
//设置请求来源
if (!builder.Environment.IsDevelopment())
{
    app.Use((context, next) =>
    {
        context.Request.Scheme = "https";
        return next();
    });
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}
//转发Ip
_ = app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});
//设置Cookies
app.UseCookiePolicy();

app.UseStaticFiles();

//添加状态检查终结点
app.UseHealthChecks("/healthz");

app.UseRouting();

//添加认证与授权中间件
app.UseAuthentication();
app.UseAuthorization();

//配置路由
app.MapDefaultControllerRoute();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
