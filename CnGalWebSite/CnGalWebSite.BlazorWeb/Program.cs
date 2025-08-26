using CnGalWebSite.BlazorWeb.Components;
using CnGalWebSite.DataModel.Helper;
using System.Text.Json.Serialization;
using CnGalWebSite.Shared.Extentions;
using Microsoft.AspNetCore.HttpOverrides;
using System.Text;
using CnGalWebSite.BlazorWeb.Services;
using CnGalWebSite.Core.Services;
using IdentityModel.AspNetCore.AccessTokenManagement;
using System.Reflection.PortableExecutable;
using CnGalWebSite.BlazorWeb.Plumbing;
using System.Security.Claims;
using CnGalWebSite.HealthCheck.Models;
using Microsoft.AspNetCore.Components.Server.Circuits;
using CnGalWebSite.Shared.Service;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddServerSideBlazor().AddCircuitOptions(option =>
    {
        option.DisconnectedCircuitMaxRetained = 30; // default is 100
        option.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(2); // default is 3 minutes
        option.DetailedErrors = true;
    });
builder.Services.AddRazorComponents(options => options.DetailedErrors = true).AddInteractiveWebAssemblyComponents()
    .AddInteractiveServerComponents().AddHubOptions(options =>
    {
        options.MaximumReceiveMessageSize = int.MaxValue;
        options.EnableDetailedErrors = true;
    });
//添加mvc控制器
builder.Services.AddControllers();
//判断是否 SSR
ToolHelper.IsSSR = ToolHelper.PreSetIsSSR == null ? true : ToolHelper.PreSetIsSSR.Value;
//覆盖默认api地址
if (string.IsNullOrWhiteSpace(builder.Configuration["WebApiPath"]) == false)
{
    ToolHelper.WebApiPath = builder.Configuration["WebApiPath"];
}
if (string.IsNullOrWhiteSpace(builder.Configuration["ImageApiPath"]) == false)
{
    ToolHelper.ImageApiPath = builder.Configuration["ImageApiPath"];
}
if (string.IsNullOrWhiteSpace(builder.Configuration["TaskApiPath"]) == false)
{
    ToolHelper.TaskApiPath = builder.Configuration["TaskApiPath"];
}

//设置Json格式化配置
ToolHelper.options.Converters.Add(new DateTimeConverterUsingDateTimeParse());
ToolHelper.options.Converters.Add(new DateTimeConverterUsingDateTimeNullableParse());
ToolHelper.options.Converters.Add(new JsonStringEnumConverter());


//主站
builder.Services.AddMainSite();

//本地化
builder.Services.AddLocalization();
//身份验证
builder.Services.AddAuthorizationCore();

//添加状态检查
builder.Services.AddHealthChecks();

//添加真实IP
_ = builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

//自定义服务
//services.AddScoped<IHttpService, HttpService>();
builder.Services.AddSingleton<ITokenStoreService, TokenStoreService>();

//使用 IdentityModel 管理刷新令牌
builder.Services.AddAccessTokenManagement();

// not allowed to programmatically use HttpContext in Blazor Server.
// that's why tokens cannot be managed in the login session
builder.Services.AddScoped<IUserAccessTokenStore, ServerSideTokenStore>();

// registers HTTP client that uses the managed user access token
builder.Services.AddHttpClient<IHttpService, HttpService>(client =>
{
    client.BaseAddress = new Uri(ToolHelper.WebApiPath);
});

// 添加空闲线路检查
builder.Services.AddSingleton<CircuitHandler, IdleCircuitHandler>();
builder.Services.AddSingleton<ICircuitHandlerService, CircuitHandlerService>();

//添加认身份证
//services.AddAuthorization(options =>
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
        //不检查Https
        options.RequireHttpsMetadata = false;

        //认证模式
        options.ResponseType = "code";
        options.ResponseMode = "query";

        options.MapInboundClaims = false;
        //保存token到本地
        options.SaveTokens = true;
        //很重要，指定从Identity Server的UserInfo地址来取Claim
        options.GetClaimsFromUserInfoEndpoint = true;
        //指定要取哪些资料（除Profile之外，Profile是默认包含的）
        options.Scope.Add("role");
        options.Scope.Add("openid");
        options.Scope.Add("CnGalAPI");
        options.Scope.Add("FileAPI");
        options.Scope.Add("TaskAPI");
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


var app = builder.Build();
//设置请求来源
if (!app.Environment.IsDevelopment())
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
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}
else
{
    app.UseDeveloperExceptionPage();
}
//转发Ip
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

//设置Cookies
app.UseCookiePolicy();

app.MapStaticAssets();

//添加状态检查终结点
app.UseHealthChecks("/healthz", ServiceStatus.Options);

app.UseRouting();

//添加认证与授权中间件
app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();


app.MapDefaultControllerRoute();
app.MapRazorComponents<App>()
     .AddAdditionalAssemblies(typeof(CnGalWebSite.Shared.App).Assembly)
 .AddInteractiveServerRenderMode()
 .AddInteractiveWebAssemblyRenderMode();


app.Run();
