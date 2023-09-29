using CnGalWebSite.Components.Service;
using CnGalWebSite.Components.Services;
using CnGalWebSite.Core.Services.Query;
using Microsoft.Extensions.DependencyInjection;

namespace CnGalWebSite.Components.Extensions
{
    public static class IServiceCollectionExtentions
    {
        public static IServiceCollection AddCnGalComponents(this IServiceCollection services)
        {
            services.AddScoped(typeof(IPageModelCatche<>), typeof(PageModelCatche<>));
            services.AddScoped(typeof(ISettingService), typeof(SettingService));
            services.AddScoped(typeof(IMiniModeService), typeof(MiniModeService));

            return services;
        }
    }
}
