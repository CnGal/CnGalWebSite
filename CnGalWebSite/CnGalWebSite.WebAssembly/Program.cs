using Blazored.LocalStorage;
using Blazored.SessionStorage;
using CnGalWebSite.Core.Services;
using CnGalWebSite.Core.Services.Query;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DrawingBed.Helper.Services;
using CnGalWebSite.PublicToolbox.DataRepositories;
using CnGalWebSite.PublicToolbox.PostTools;
using CnGalWebSite.Shared;
using CnGalWebSite.Shared.DataRepositories;
using CnGalWebSite.Shared.Extentions;
using CnGalWebSite.Shared.MasaComponent.Shared.Tips;
using CnGalWebSite.Shared.Service;
using CnGalWebSite.WebAssembly.Services;
using Masa.Blazor.Presets;
using Masa.Blazor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging;

namespace CnGalWebSite.WebAssembly
{
    /// <summary>
    /// 
    /// </summary>
    public class Program
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static async Task Main(string[] args)
        {
            //判断是否 SSR
            ToolHelper.IsSSR = ToolHelper.PreSetIsSSR == null ? false : ToolHelper.PreSetIsSSR.Value;

            var builder = WebAssemblyHostBuilder.CreateDefault(args);
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

            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadX>("head::after");
            builder.RootComponents.Add<StyleTip>("#global-style");

            //主站
            builder.Services.AddMainSite();

            builder.Services.BuildServiceProvider(validateScopes: false);

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            //设置Json格式化配置
            ToolHelper.options.Converters.Add(new DateTimeConverterUsingDateTimeParse());
            ToolHelper.options.Converters.Add(new DateTimeConverterUsingDateTimeNullableParse());
            ToolHelper.options.Converters.Add(new JsonStringEnumConverter());

            //日志
            builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));

            //添加OpenId
            builder.Services.AddOidcAuthentication(options =>
            {
                builder.Configuration.Bind("Local", options.ProviderOptions);
                options.UserOptions.RoleClaim = "role";
                options.UserOptions.NameClaim = "name";
                options.ProviderOptions.ResponseMode = "query";
                options.ProviderOptions.ResponseType = "code";
            }).AddAccountClaimsPrincipalFactory<CustomUserFactory>();
            //添加Http服务
            builder.Services.AddSingleton<IHttpService, HttpService>();
            //注册身份验证的HttpClient
            builder.Services.AddScoped<CustomAuthorizationMessageHandler>();
            builder.Services.AddHttpClient("AuthAPI")
                .AddHttpMessageHandler<CustomAuthorizationMessageHandler>();

            //注册匿名的HttpClient
            builder.Services.AddHttpClient("AnonymousAPI");

            var host = builder.Build();

            await host.RunAsync();
        }
    }
}
