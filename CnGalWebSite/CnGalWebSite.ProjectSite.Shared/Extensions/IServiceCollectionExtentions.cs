using Blazored.LocalStorage;
using Blazored.SessionStorage;
using CnGalWebSite.Components.Extensions;
using CnGalWebSite.ProjectSite.Shared.Services.Events;
using CnGalWebSite.ProjectSite.Shared.Services.Home;
using CnGalWebSite.ProjectSite.Shared.Services.Projects;
using CnGalWebSite.ProjectSite.Shared.Services.Stalls;
using CnGalWebSite.ProjectSite.Shared.Services.Themes;
using CnGalWebSite.ProjectSite.Shared.Services.Users;
using Masa.Blazor;
using Masa.Blazor.Presets;
using Microsoft.Extensions.DependencyInjection;

namespace CnGalWebSite.ProjectSite.Shared.Extensions
{
    public static class IServiceCollectionExtentions
    {
        public static IServiceCollection AddProjectSite(this IServiceCollection services)
        {
            //Masa Blazor 组件库
            services.AddMasaBlazor(options =>
            {
                //主题
                options.ConfigureTheme(theme =>
                {
                    theme.DefaultTheme = "pink-light";

                    // Pink Light Theme
                    theme.Themes.Add("pink-light", false, custom =>
                    {
                        custom.Primary = "#bc004b";
                        custom.OnPrimary = "#ffffff";
                        custom.Secondary = "#75565b";
                        custom.OnSecondary = "#ffffff";
                        custom.Accent = "#795831";
                        custom.OnAccent = "#ffffff";
                        custom.Error = "#ba1a1a";
                        custom.OnError = "#ffffff";
                        custom.Surface = "#fffbff";
                        custom.OnSurface = "#201a1b";
                        custom.SurfaceDim = "#ece0e0";
                        custom.SurfaceBright = "#fffbff";
                        custom.SurfaceContainer = "#fbeeee";
                        custom.SurfaceContainerLow = "#fff8f7";
                        custom.SurfaceContainerLowest = "#ffffff";
                        custom.SurfaceContainerHigh = "#ece0e0";
                        custom.SurfaceContainerHighest = "#cfc4c4";
                        custom.InversePrimary = "#ffb2be";
                        custom.InverseSurface = "#362f2f";
                        custom.InverseOnSurface = "#fbeeee";
                    });

                    // Pink  Theme
                    theme.Themes.Add("pink-dark", true, custom =>
                    {
                        custom.Primary = "#ffb2be";
                        custom.OnPrimary = "#660025";
                        custom.Secondary = "#e5bdc2";
                        custom.OnSecondary = "#43292d";
                        custom.Accent = "#ebbf90";
                        custom.OnAccent = "#452b08";
                        custom.Error = "#ffb4ab";
                        custom.OnError = "#690005";
                        custom.Surface = "#201a1b";
                        custom.OnSurface = "#ece0e0";
                        custom.SurfaceDim = "#201a1b";
                        custom.SurfaceBright = "#4d4546";
                        custom.SurfaceContainer = "#362f2f";
                        custom.SurfaceContainerLow = "#201a1b";
                        custom.SurfaceContainerLowest = "#000000";
                        custom.SurfaceContainerHigh = "#4d4546";
                        custom.SurfaceContainerHighest = "#595051";
                        custom.InversePrimary = "#bc004b";
                        custom.InverseSurface = "#ece0e0";
                        custom.InverseOnSurface = "#201a1b";
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
