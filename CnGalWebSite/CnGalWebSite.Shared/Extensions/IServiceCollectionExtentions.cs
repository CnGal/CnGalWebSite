using Blazored.LocalStorage;
using Blazored.SessionStorage;
using CnGalWebSite.Core.Services.Query;
using CnGalWebSite.DrawingBed.Helper.Services;
using CnGalWebSite.PublicToolbox.DataRepositories;
using CnGalWebSite.PublicToolbox.PostTools;
using CnGalWebSite.Shared.DataRepositories;
using CnGalWebSite.Shared.Service;
using Microsoft.Extensions.DependencyInjection;

namespace CnGalWebSite.Shared.Extentions
{
    public static class IServiceCollectionExtentions
    {
        public static IServiceCollection AddMainSite(this IServiceCollection services)
        {
            //Masa Blazor 组件库
            services.AddMasaBlazor(options => {
                //主题
                options.ConfigureTheme(s =>
                {
                    s.Themes.Light.Primary = "var(--md-sys-color-primary)";
                    s.Themes.Light.Secondary = "var(--md-sys-color-secondary)";
                    s.Themes.Light.Error = "var(--md-sys-color-error)";
                    s.Themes.Dark.Primary = "var(--md-sys-color-primary)";
                    s.Themes.Dark.Secondary = "var(--md-sys-color-secondary)";
                    s.Themes.Dark.Error = "var(--md-sys-color-error)";
                });
            });

            //本地储存
            services.AddBlazoredLocalStorage().AddBlazoredSessionStorage();
            //按类型缓存数据
            services.AddScoped(typeof(IPageModelCatche<>), typeof(PageModelCatche<>));
            //全局缓存数据
            services.AddScoped<IDataCacheService, DataCatcheService>();
            //查看大图
            services.AddScoped(x => new ImagesLargeViewService());


            //工具箱
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>))
                .AddScoped<IEntryService, EntryService>()
                .AddScoped<IArticleService, ArticleService>()
                .AddScoped<IImageService, ImageService>()
                .AddScoped<IVideoService, VideoService>();

            //空白MAUI服务 
            services.AddScoped<IMauiService, MauiService>();
            //文件上传
            services.AddScoped<IFileUploadService, FileUploadService>();
            //事件
            services.AddScoped<IEventService, EventService>();
            //结构化数据
            _ = services.AddScoped<IStructuredDataService, StructuredDataService>();
            //Query
            services.AddScoped<IQueryService, QueryService>();
            


            return services;
        }
    }
}
