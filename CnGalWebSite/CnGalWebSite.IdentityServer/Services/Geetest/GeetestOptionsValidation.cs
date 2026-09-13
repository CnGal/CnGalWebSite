using CnGalWebSite.IdentityServer.Models.DataModels.Geetest;
using Microsoft.Extensions.DependencyInjection;

namespace CnGalWebSite.IdentityServer.Services.Geetest;

public static class GeetestOptionsValidation
{
    public static IServiceCollection AddGeetestOptions(this IServiceCollection services)
    {
        services.AddOptions<GeetestOptions>()
            .BindConfiguration(GeetestOptions.SectionName)
            .Validate(options =>
                !string.IsNullOrWhiteSpace(options.Id) &&
                !string.IsNullOrWhiteSpace(options.Key));
        return services;
    }
}
