using Blazored.LocalStorage;
using Blazored.SessionStorage;
using Microsoft.Extensions.DependencyInjection;
using CnGalWebSite.Components.Extensions;
using CnGalWebSite.ProjectSite.Shared.Services.Themes;
using CnGalWebSite.ProjectSite.Shared.Services.Events;
using CnGalWebSite.ProjectSite.Shared.Services.Users;
using CnGalWebSite.ProjectSite.Shared.Services.Projects;
using CnGalWebSite.ProjectSite.Shared.Services.Stalls;
using CnGalWebSite.ProjectSite.Shared.Services.Home;

namespace CnGalWebSite.ProjectSite.Shared.Extensions
{
    public static class IServiceCollectionExtentions
    {
        public static IServiceCollection AddProjectSite(this IServiceCollection services)
        {
            //Masa Blazor 组件库
            services.AddMasaBlazor(options => {
                //主题
                options.ConfigureTheme(s =>
                {
                    s.Themes.Light.Primary = "rgba(var(--m-theme-primary))";
                    s.Themes.Light.Secondary = "var(--md-sys-color-secondary)";
                    s.Themes.Light.Error = "var(--md-sys-color-error)";
                    s.Themes.Dark.Primary = "rgba(var(--m-theme-primary))";
                    s.Themes.Dark.Secondary = "var(--md-sys-color-secondary)";
                    s.Themes.Dark.Error = "var(--md-sys-color-error)";
                });
            });

            //本地储存
            services.AddBlazoredLocalStorage().AddBlazoredSessionStorage();
            //公共组件
            services.AddCnGalComponents();
            //设置
            services.AddScoped<ISettingService, SettingService>();
            //事件
            services.AddScoped<IEventService, EventService>();
            //用户
            services.AddScoped<IUserService, Userservice>();
            //企划
            services.AddScoped<IProjectService, ProjectService>();
            //企划职位
            services.AddScoped<IProjectPositionService, ProjectPositionService>();
            //橱窗
            services.AddScoped<IStallService, StallService>();
            //主页
            services.AddScoped<IHomeService, HomeService>();
            return services;
        }
    }
}
