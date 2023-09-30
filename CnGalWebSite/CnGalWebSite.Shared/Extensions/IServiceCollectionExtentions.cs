using Blazored.LocalStorage;
using Blazored.SessionStorage;
using CnGalWebSite.Components.Extensions;
using CnGalWebSite.Components.Service;
using CnGalWebSite.Core.Services.Query;
using CnGalWebSite.DrawingBed.Helper.Services;
using CnGalWebSite.Kanban.Extensions;
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
            //添加公共组件
            services.AddCnGalComponents();
            //全局缓存数据
            services.AddScoped<IDataCacheService, DataCatcheService>();
            //用户
            services.AddScoped<IUserService, Userservice>();


            //工具箱
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>))
                .AddScoped<IEntryService, EntryService>()
                .AddScoped<IArticleService, ArticleService>()
                .AddScoped<IImageService, ImageService>()
                .AddScoped<IVideoService, VideoService>();

            //空白MAUI服务 
            services.AddScoped<IMauiService, MauiService>();

            //事件
            services.AddScoped<IEventService, EventService>();
            //结构化数据
            _ = services.AddScoped<IStructuredDataService, StructuredDataService>();
            //Query
            services.AddScoped<IQueryService, QueryService>();
            // 看板娘Live2D
            services.AddKanbanLive2D();


            return services;
        }
    }
}
