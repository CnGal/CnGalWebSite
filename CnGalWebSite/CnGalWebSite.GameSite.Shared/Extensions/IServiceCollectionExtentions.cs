using Blazored.LocalStorage;
using Blazored.SessionStorage;
using Microsoft.Extensions.DependencyInjection;
using CnGalWebSite.Components.Extensions;
using CnGalWebSite.GameSite.Shared.Services.Themes;
using CnGalWebSite.GameSite.Shared.Services.Events;
using CnGalWebSite.GameSite.Shared.Services.Users;

namespace CnGalWebSite.GameSite.Shared.Extensions
{
    public static class IServiceCollectionExtentions
    {
        public static IServiceCollection AddGameSite(this IServiceCollection services)
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

            return services;
        }
    }
}
