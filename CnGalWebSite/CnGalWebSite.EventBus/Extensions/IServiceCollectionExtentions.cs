
using CnGalWebSite.EventBus.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CnGalWebSite.EventBus.Extensions
{
    public static class IServiceCollectionExtentions
    {
        public static IServiceCollection AddEventBus(this IServiceCollection services)
        {
            services.AddSingleton<IEventBusService, EventBusService>();
            services.AddSingleton<IEventBus, EventBusRabbitMQ>();

            return services;
        }
    }
}
