using CnGalWebSite.HealthCheck.Models;
using CnGalWebSite.MainSite.Components;
using CnGalWebSite.MainSite.Shared;
using CnGalWebSite.SDK.MainSite.Extensions;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Components.Server.Circuits;
using CnGalWebSite.MainSite.Services;

var builder = WebApplication.CreateBuilder(args);
var apiBaseAddress = builder.Configuration["MainSiteApi:BaseAddress"];
var imageApiBaseAddress = builder.Configuration["MainSiteApi:ImageApiPath"];
var taskApiBaseAddress = builder.Configuration["MainSiteApi:TaskApiPath"];

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddMainSiteOidcAuthentication(builder.Configuration);
builder.Services.AddMainSiteSdk(apiBaseAddress!, imageApiBaseAddress!, taskApiBaseAddress!);
builder.Services.AddMainSiteSharedServices();
//添加状态检查
builder.Services.AddHealthChecks();
builder.Services.AddScoped<CircuitHandler, IdleCircuitHandler>();

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
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.

}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

//转发Ip
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});


//添加状态检查终结点
app.UseHealthChecks("/healthz", ServiceStatus.Options);

app.UseAuthentication();
app.UseAuthorization();

// 覆写 SSR 页面的 Cache-Control，按登录状态区分缓存策略
app.Use(async (context, next) =>
{
    context.Response.OnStarting(() =>
    {
        var contentType = context.Response.ContentType;
        if (contentType != null
            && contentType.StartsWith("text/html", StringComparison.OrdinalIgnoreCase)
            && context.Response.StatusCode == 200)
        {
            if (context.User.Identity?.IsAuthenticated != true)
            {
                // 未登录：CDN 可缓存 5 分钟，浏览器不缓存（避免登录后看到旧页面）
                context.Response.Headers.CacheControl = "public, s-maxage=300, max-age=0, must-revalidate";
            }
            else
            {
                // 已登录：任何缓存层都不得缓存
                context.Response.Headers.CacheControl = "private, no-cache, no-store";
            }
            context.Response.Headers.Remove("Pragma");
        }
        return Task.CompletedTask;
    });
    await next();
});

app.MapStaticAssets();
app.MapMainSiteAuthenticationEndpoints();
app.MapRazorComponents<App>()
    .AddAdditionalAssemblies(typeof(AssemblyMarker).Assembly)
    .AddInteractiveServerRenderMode()
    .DisableAntiforgery();

app.Run();
