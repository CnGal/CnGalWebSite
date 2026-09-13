using CnGalWebSite.Core.Configuration;
using Meilisearch;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NETCore.MailKit.Core;
using NETCore.MailKit.Infrastructure.Internal;

namespace CnGalWebSite.APIServer.Configuration;

public static class ExternalServiceRegistration
{
    public static IServiceCollection AddConfiguredMeilisearch(this IServiceCollection services)
    {
        return services.AddSingleton<MeilisearchClient>(provider =>
        {
            var settings = provider.GetRequiredService<IOptions<MeilisearchOptions>>()
                .GetOptional(MeilisearchOptions.SectionName);
            return new MeilisearchClient(settings.BaseAddress, settings.ApiKey);
        });
    }

    public static IServiceCollection AddConfiguredMailKit(this IServiceCollection services)
    {
        return services.AddScoped<IEmailService>(provider =>
        {
            var settings = provider.GetRequiredService<IOptions<MailOptions>>()
                .GetOptional(MailOptions.SectionName);
            return new EmailService(new NETCore.MailKit.MailKitProvider(new MailKitOptions
            {
                Server = settings.Server,
                Port = settings.Port,
                SenderName = settings.SenderName,
                SenderEmail = settings.SenderEmail,
                Account = settings.Account,
                Password = settings.Password,
                Security = true
            }));
        });
    }
}
