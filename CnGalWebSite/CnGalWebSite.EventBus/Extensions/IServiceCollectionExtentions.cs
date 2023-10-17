
using CnGalWebSite.EventBus.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CnGalWebSite.EventBus.Extensions
{
    public static class IServiceCollectionExtentions
    {
        public static IServiceCollection AddEventBus(this IServiceCollection services)
        {
            services.AddScoped<IEventBusService, EventBusService>();
            services.AddScoped<IEventBus, EventBusRabbitMQ>();

            return services;
        }
    }
}
