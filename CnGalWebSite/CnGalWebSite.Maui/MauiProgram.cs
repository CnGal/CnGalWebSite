using Blazored.LocalStorage;
using Blazored.SessionStorage;
using CnGalWebSite.Core.Services;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.ViewModel.Files.Images;
using CnGalWebSite.DrawingBed.Helper.Services;
using CnGalWebSite.Maui.Services;
using CnGalWebSite.PublicToolbox.DataRepositories;
using CnGalWebSite.PublicToolbox.PostTools;
using CnGalWebSite.Shared.DataRepositories;
using CnGalWebSite.Shared.Extentions;
using CnGalWebSite.Shared.Service;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using MauiService = CnGalWebSite.Maui.Services.MauiService;

namespace CnGalWebSite.Maui
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            //设置模式
            ToolHelper.IsMaui = true;
            ToolHelper.IsSSR = false;

            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            //添加本地配置文件
            var a = Assembly.GetExecutingAssembly();
            using var stream = a.GetManifestResourceStream("CnGalWebSite.Maui.appsettings.json");
            var config = new ConfigurationBuilder()
                .AddJsonStream(stream)
                .Build();
            builder.Configuration.AddConfiguration(config);


            builder.Services.AddMauiBlazorWebView();

            builder.Services.AddBootstrapBlazor();

            //依赖注入主页
            builder.Services.AddSingleton<MainPage>();
            //本地储存服务
            builder.Services.AddBlazoredLocalStorage()
                .AddBlazoredSessionStorage();

            //身份验证
            builder.Services.AddAuthorizationCore();

            //缓存
            builder.Services.AddScoped(typeof(IPageModelCatche<>), typeof(PageModelCatche<>));
            builder.Services.AddScoped<IDataCacheService, DataCatcheService>();

            builder.Services.AddScoped(x => new ImagesLargeViewService());

            //添加工具箱
            builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            builder.Services.AddScoped<IEntryService, EntryService>();
            builder.Services.AddScoped<IArticleService, ArticleService>();
            builder.Services.AddScoped<IImageService, ImageService>();
            //services.AddScoped<IEventBase, EventBase>();

            //添加预渲染状态记录
            builder.Services.AddScoped<IApplicationStateService, ApplicationStateService>();

            //添加结构化数据
            builder.Services.AddScoped<IStructuredDataService, StructuredDataService>();

            //MASA组件库
            builder.Services.AddMasaBlazor(s => s.ConfigureTheme(s =>
             {
                 if (DateTime.Now.ToCstTime().Day == 1 && DateTime.Now.ToCstTime().Month == 4)
                 {
                     s.Themes.Light.Primary = "#4CAF50";
                 }
                 else
                 {
                     s.Themes.Light.Primary = "#f06292";
                 }
                 s.Themes.Dark.Primary = "#0078BF";
             }));

            //弹窗服务类
            builder.Services.AddSingleton(typeof(IAlertService), typeof(AlertService));
            builder.Services.AddSingleton(typeof(IOverviewService), typeof(OverviewService));

            //设置Json格式化配置
            ToolHelper.options.Converters.Add(new DateTimeConverterUsingDateTimeParse());
            ToolHelper.options.Converters.Add(new DateTimeConverterUsingDateTimeNullableParse());

            //添加MAUI与Blazor通信服务
            builder.Services.AddScoped<IMauiService, MauiService>();

            builder.Services.AddScoped<IFileUploadService, FileUploadService>();
            builder.Services.AddScoped<IEventService, EventService>();

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


            return builder.Build();
        }
    }
}
