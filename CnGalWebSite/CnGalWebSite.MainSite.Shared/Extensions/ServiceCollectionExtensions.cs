using CnGalWebSite.MainSite.Shared.Services;
using CnGalWebSite.MainSite.Shared.Services.Toolbox;
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
        services.AddScoped<IMiniModeService, MiniModeService>();
        services.AddScoped<ICgThemeService, CgThemeService>();
        services.AddSingleton<ICircuitHandlerService, CircuitHandlerService>();
        services.AddScoped(typeof(IToolboxLocalRepository<>), typeof(ToolboxLocalRepository<>));
        services.AddScoped<ToolboxImageService>();
        services.AddScoped<IToolboxVideoRepostService, ToolboxVideoRepostService>();
        services.AddScoped<IToolboxEntryMergeService, ToolboxEntryMergeService>();
        services.AddHttpClient<IToolboxArticleRepostService, ToolboxArticleRepostService>();
        return services;
    }
}
