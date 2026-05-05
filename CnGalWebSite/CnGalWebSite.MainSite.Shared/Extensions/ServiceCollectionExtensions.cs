using Blazored.LocalStorage;
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
        services.AddScoped<IMiniModeService, MiniModeService>();
        services.AddScoped<ICgThemeService, CgThemeService>();
        services.AddScoped<IKanbanSettingService, KanbanSettingService>();
        services.AddScoped<IKanbanUserDataService, KanbanUserDataService>();
        services.AddScoped<IKanbanEventService, KanbanEventService>();
        services.AddSingleton<ICircuitHandlerService, CircuitHandlerService>();
        services.AddSingleton<IKanbanRemoteConfigService, KanbanRemoteConfigService>();
        services.AddScoped<IKanbanLive2DService, KanbanLive2DService>();
        services.AddScoped<IKanbanChatService, KanbanChatService>();
        services.AddBlazoredLocalStorage();
        return services;
    }
}
