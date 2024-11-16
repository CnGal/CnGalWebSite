using Blazored.LocalStorage;
using Masa.Blazor;
using Microsoft.Extensions.DependencyInjection;
using Masa.Blazor.Presets;
using BlazorComponent;
using CnGalWebSite.Core.Services.Query;
using CnGalWebSite.Components.Extensions;

namespace CnGalWebSite.IdentityServer.Admin.Shared.Extensions
{
    public static class IServiceCollectionExtentions
    {
        public static IServiceCollection AddIdentityServerAdmin(this IServiceCollection services)
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

                // 提示框
                options.Defaults = new Dictionary<string, IDictionary<string, object>>()
                {
                    {
                        PopupComponents.SNACKBAR, new Dictionary<string, object>()
                        {
                            { nameof(PEnqueuedSnackbars.Closeable), true },
                            { nameof(PEnqueuedSnackbars.Position), SnackPosition.BottomRight },
                            { nameof(PEnqueuedSnackbars.Text), true },
                            { nameof(PEnqueuedSnackbars.Elevation), new StringNumber(2) },
                            { nameof(PEnqueuedSnackbars.MaxCount), 5 },
                            { nameof(PEnqueuedSnackbars.Timeout), 5000 },
                        }
                    }
                };
            });

            //本地储存
            services.AddBlazoredLocalStorage();
            //添加公共组件
            services.AddCnGalComponents();

            return services;
        }
    }
}
