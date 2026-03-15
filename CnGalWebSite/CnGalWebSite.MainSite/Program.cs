using CnGalWebSite.HealthCheck.Models;
using CnGalWebSite.MainSite.Components;
using CnGalWebSite.MainSite.Shared;
using CnGalWebSite.SDK.MainSite.Extensions;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);
var apiBaseAddress = builder.Configuration["MainSiteApi:BaseAddress"];
var imageApiBaseAddress = builder.Configuration["MainSiteApi:ImageApiPath"];

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddMainSiteOidcAuthentication(builder.Configuration);
builder.Services.AddMainSiteSdk(apiBaseAddress!, imageApiBaseAddress!);
builder.Services.AddMainSiteSharedServices();
//添加状态检查
builder.Services.AddHealthChecks();
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

app.UseAntiforgery();

app.MapStaticAssets();
app.MapMainSiteAuthenticationEndpoints();
app.MapRazorComponents<App>()
    .AddAdditionalAssemblies(typeof(AssemblyMarker).Assembly)
    .AddInteractiveServerRenderMode();

app.Run();
