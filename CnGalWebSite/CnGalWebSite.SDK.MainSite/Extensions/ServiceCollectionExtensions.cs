using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Queries;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace CnGalWebSite.SDK.MainSite.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMainSiteSdk(this IServiceCollection services, string apiBaseAddress)
    {
        services.AddMemoryCache();
        services.AddHttpClient<IHomeQueryService, HomeQueryService>(client =>
        {
            client.BaseAddress = new Uri(EnsureTrailingSlash(apiBaseAddress));
        });
        services.AddHttpClient<IEntryQueryService, EntryQueryService>(client =>
        {
            client.BaseAddress = new Uri(EnsureTrailingSlash(apiBaseAddress));
        });
        return services;
    }

    private static string EnsureTrailingSlash(string apiBaseAddress)
    {
        if (string.IsNullOrWhiteSpace(apiBaseAddress))
        {
            return "https://api.cngal.org/";
        }

        return apiBaseAddress.EndsWith('/') ? apiBaseAddress : $"{apiBaseAddress}/";
    }
}
