using CnGalWebSite.Components.Service;
using CnGalWebSite.Components.Services;
using CnGalWebSite.Core.Services.Query;
using CnGalWebSite.DrawingBed.Helper.Services;
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
            //查看大图
            services.AddScoped(x => new ImagesLargeViewService());
            //文件上传
            services.AddScoped<IFileUploadService, FileUploadService>();
            return services;
        }
    }
}
