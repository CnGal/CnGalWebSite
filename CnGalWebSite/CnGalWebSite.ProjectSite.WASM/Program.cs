using CnGalWebSite.ProjectSite.Shared;
using CnGalWebSite.ProjectSite.Shared.Extensions;
using CnGalWebSite.ProjectSite.WASM;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddProjectSite();

await builder.Build().RunAsync();
