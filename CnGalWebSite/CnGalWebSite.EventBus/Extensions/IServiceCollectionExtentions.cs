
using CnGalWebSite.EventBus.Services;
using CnGalWebSite.EventBus.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CnGalWebSite.EventBus.Extensions
{
    public static class IServiceCollectionExtentions
    {
        public static IServiceCollection AddEventBus(this IServiceCollection services)
        {
            services.AddOptions<RabbitMqOptions>()
                .BindConfiguration(RabbitMqOptions.SectionName)
                .Validate(RabbitMqOptions.IsValid);
            services.AddSingleton<IEventBusService, EventBusService>();
            services.AddSingleton<IEventBus, EventBusRabbitMQ>();

            return services;
        }
    }
}
