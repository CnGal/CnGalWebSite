using CnGalWebSite.MainSite.Components;
using CnGalWebSite.MainSite.Shared;
using CnGalWebSite.SDK.MainSite.Extensions;

var builder = WebApplication.CreateBuilder(args);
var apiBaseAddress = builder.Configuration["MainSiteApi:BaseAddress"] ?? "https://api.cngal.org/";

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddMainSiteSdk(apiBaseAddress);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddAdditionalAssemblies(typeof(AssemblyMarker).Assembly)
    .AddInteractiveServerRenderMode();

app.Run();
