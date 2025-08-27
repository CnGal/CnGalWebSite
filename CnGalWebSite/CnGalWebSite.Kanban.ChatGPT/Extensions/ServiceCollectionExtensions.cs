using CnGalWebSite.Kanban.ChatGPT.Services.Storage;
using CnGalWebSite.Kanban.ChatGPT.Services.UserProfileService;
using Microsoft.Extensions.DependencyInjection;

namespace CnGalWebSite.Kanban.ChatGPT.Extensions
{
    /// <summary>
    /// 服务集合扩展方法
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 添加用户个性化服务
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
                public static IServiceCollection AddUserProfileServices(this IServiceCollection services)
        {
            // 注册持久化存储服务
            services.AddSingleton<IPersistentStorage, JsonFileStorage>();

            // 注册用户个性化服务
            services.AddScoped<IUserProfileService, UserProfileService>();

            // 注册看板娘自我记忆服务
            services.AddSingleton<ISelfMemoryService, SelfMemoryService>();

            return services;
        }
    }
}
