using CnGalWebSite.IdentityServer.Admin.Shared.Options;
using CnGalWebSite.IdentityServer.Admin.Shared.Services;
using CnGalWebSite.IdentityServer.Admin.SSR.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
//自定义服务
builder.Services.AddScoped<IDataCacheService, DataCacheService>();
builder.Services.AddScoped<IHttpService, HttpService>();
//设置主题
builder.Services.AddMasaBlazor(s => s.ConfigureTheme(s =>
{
    s.Themes.Light.Primary = "#f06292";
    s.Themes.Dark.Primary = "#2196F3";
}));
//Http
builder.Services.AddScoped(sp => new HttpClient());

//默认采用cookie认证方案，添加oidc认证方案
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "cookies";
    options.DefaultChallengeScheme = "oidc";
})
    //配置cookie认证
    .AddCookie("cookies")
    .AddOpenIdConnect("oidc", options =>
    {
        //id4服务的地址
        options.Authority =builder.Configuration["Authority"];

        //id4配置的ClientId以及ClientSecrets
        options.ClientId = builder.Configuration["ClientId"];
        options.ClientSecret = builder.Configuration["ClientSecret"];

        //开发模式下关闭http
        if (builder.Configuration["Authority"].Contains("https") == false)
        {
            options.RequireHttpsMetadata = false;
        }

        //认证模式
        options.ResponseType = "code";

        //保存token到本地
        options.SaveTokens = true;

        //很重要，指定从Identity Server的UserInfo地址来取Claim
        options.GetClaimsFromUserInfoEndpoint = true;
        //指定要取哪些资料（除Profile之外，Profile是默认包含的）
        options.Scope.Add("role");
        options.Scope.Add("IdentityServerApi");
        //这里是个ClaimType的转换，Identity Server的ClaimType和Blazor中间件使用的名称有区别，需要统一。
        options.TokenValidationParameters.RoleClaimType = "role";
        options.TokenValidationParameters.NameClaimType = "name";
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

//设置Cookies
static void CheckSameSite(HttpContext httpContext, CookieOptions options)
{
    if (options.SameSite == SameSiteMode.None)
    {
        var userAgent = httpContext.Request.Headers["User-Agent"].ToString();

            options.SameSite = SameSiteMode.Unspecified;
        
    }
}
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
    options.OnAppendCookie = cookieContext =>
        CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
    options.OnDeleteCookie = cookieContext =>
        CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
});
//添加状态检查
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}
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
