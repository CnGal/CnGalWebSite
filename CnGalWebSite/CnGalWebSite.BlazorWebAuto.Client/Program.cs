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


//�ж��Ƿ� SSR
ToolHelper.IsSSR = ToolHelper.PreSetIsSSR == null ? false : ToolHelper.PreSetIsSSR.Value;


//����Ĭ��api��ַ
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

//��վ
builder.Services.AddMainSite();

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

//����Json��ʽ������
ToolHelper.options.Converters.Add(new DateTimeConverterUsingDateTimeParse());
ToolHelper.options.Converters.Add(new DateTimeConverterUsingDateTimeNullableParse());
ToolHelper.options.Converters.Add(new JsonStringEnumConverter());

//��־
builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));


await builder.Build().RunAsync();
