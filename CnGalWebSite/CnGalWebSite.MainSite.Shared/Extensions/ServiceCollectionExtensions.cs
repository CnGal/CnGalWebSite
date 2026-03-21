using CnGalWebSite.MainSite.Shared.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CnGalWebSite.MainSite.Shared;

/// <summary>
/// MainSite.Shared 层的 DI 注册扩展
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 注册 MainSite.Shared 提供的前端服务（Toast 等）
    /// </summary>
    public static IServiceCollection AddMainSiteSharedServices(this IServiceCollection services)
    {
        services.AddScoped<ICgToastService, CgToastService>();
        services.AddScoped<ICgDeviceIdentificationService, CgDeviceIdentificationService>();
        return services;
    }
}
