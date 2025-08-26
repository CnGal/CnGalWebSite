using CnGalWebSite.BlazorWebAuto.Client.Services;
using CnGalWebSite.Core.Services;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.Shared.Extentions;
using CnGalWebSite.Shared.MasaComponent.Shared.Tips;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthenticationStateDeserialization();


//判断是否 SSR
ToolHelper.IsSSR = ToolHelper.PreSetIsSSR == null ? false : ToolHelper.PreSetIsSSR.Value;


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

builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient<IHttpService, HttpService>(client =>
{
    client.BaseAddress = new Uri(ToolHelper.WebApiPath);
});

//主站
builder.Services.AddMainSite();

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

//设置Json格式化配置
ToolHelper.options.Converters.Add(new DateTimeConverterUsingDateTimeParse());
ToolHelper.options.Converters.Add(new DateTimeConverterUsingDateTimeNullableParse());
ToolHelper.options.Converters.Add(new JsonStringEnumConverter());

//日志
builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));


await builder.Build().RunAsync();
