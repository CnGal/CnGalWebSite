using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Auth;
using CnGalWebSite.SDK.MainSite.Commands;
using CnGalWebSite.SDK.MainSite.Models;
using CnGalWebSite.SDK.MainSite.Queries;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Security.Claims;
using System;

namespace CnGalWebSite.SDK.MainSite.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMainSiteOidcAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var oidcOptions = configuration.GetSection(MainSiteOidcOptions.SectionName).Get<MainSiteOidcOptions>() ?? new MainSiteOidcOptions();
        var scopes = oidcOptions.Scopes
            .Where(scope => string.IsNullOrWhiteSpace(scope) is false)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (scopes.Contains("offline_access", StringComparer.OrdinalIgnoreCase) is false)
        {
            scopes = [.. scopes, "offline_access"];
        }
        var persistentLoginDuration = TimeSpan.FromDays(Math.Max(1, oidcOptions.PersistentLoginDays));

        services.AddHttpContextAccessor();
        services.AddSingleton<IMainSiteAuthRequestService, MainSiteAuthRequestService>();
        services.AddSingleton<CookieOidcRefresher>();
        services.Configure<MainSiteOidcOptions>(configuration.GetSection(MainSiteOidcOptions.SectionName));

        services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = MainSiteOidcOptions.OidcScheme;
            options.DefaultSignOutScheme = MainSiteOidcOptions.OidcScheme;
        })
        .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
        {
            options.Cookie.Name = "__Host-cngal-main-site";
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.ExpireTimeSpan = persistentLoginDuration;
            options.SlidingExpiration = true;
        })
        .AddOpenIdConnect(MainSiteOidcOptions.OidcScheme, options =>
        {
            options.Authority = oidcOptions.Authority;

            if (string.IsNullOrWhiteSpace(oidcOptions.MetadataAddress) is false)
            {
                options.MetadataAddress = oidcOptions.MetadataAddress;
            }

            options.ClientId = oidcOptions.ClientId;
            options.ClientSecret = oidcOptions.ClientSecret;
            options.RequireHttpsMetadata = oidcOptions.RequireHttpsMetadata;
            options.ResponseType = OpenIdConnectResponseType.Code;
            options.ResponseMode = OpenIdConnectResponseMode.Query;
            options.MapInboundClaims = false;
            options.SaveTokens = true;
            options.GetClaimsFromUserInfoEndpoint = true;

            options.Scope.Clear();
            foreach (var scope in scopes)
            {
                options.Scope.Add(scope);
            }

            options.TokenValidationParameters.NameClaimType = JwtRegisteredClaimNames.Name;
            options.TokenValidationParameters.RoleClaimType = "role";

            options.Events.OnUserInformationReceived = context =>
            {
                if (context.Principal?.Identity is not ClaimsIdentity claimsIdentity)
                {
                    return Task.CompletedTask;
                }

                if (context.User.RootElement.TryGetProperty("role", out var roleElement) is false)
                {
                    return Task.CompletedTask;
                }

                if (roleElement.ValueKind == System.Text.Json.JsonValueKind.Array)
                {
                    foreach (var role in roleElement.EnumerateArray().Select(element => element.ToString()))
                    {
                        if (string.IsNullOrWhiteSpace(role))
                        {
                            continue;
                        }

                        claimsIdentity.AddClaim(new Claim("role", role));
                    }
                }
                else
                {
                    var role = roleElement.ToString();
                    if (string.IsNullOrWhiteSpace(role) is false)
                    {
                        claimsIdentity.AddClaim(new Claim("role", role));
                    }
                }

                return Task.CompletedTask;
            };
        });

        services.AddOptions<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme)
            .Configure<CookieOidcRefresher>((cookieOptions, refresher) =>
            {
                cookieOptions.Events.OnValidatePrincipal = context =>
                    refresher.ValidateOrRefreshCookieAsync(context, MainSiteOidcOptions.OidcScheme);
            });

        services.AddAuthorization();
        services.AddCascadingAuthenticationState();
        return services;
    }

    public static IServiceCollection AddMainSiteSdk(this IServiceCollection services, string apiBaseAddress, string imageApiBaseAddress)
    {
        services.AddMemoryCache();
        services.AddHttpContextAccessor();
        services.AddTransient<AccessTokenHandler>();
        services.AddHttpClient<IHomeQueryService, HomeQueryService>(client =>
        {
            client.BaseAddress = new Uri(EnsureTrailingSlash(apiBaseAddress));
        })
        .AddHttpMessageHandler<AccessTokenHandler>();
        services.AddHttpClient<IEntryQueryService, EntryQueryService>(client =>
        {
            client.BaseAddress = new Uri(EnsureTrailingSlash(apiBaseAddress));
        })
        .AddHttpMessageHandler<AccessTokenHandler>();
        services.AddHttpClient<ISpaceQueryService, SpaceQueryService>(client =>
        {
            client.BaseAddress = new Uri(EnsureTrailingSlash(apiBaseAddress));
        })
        .AddHttpMessageHandler<AccessTokenHandler>();
        services.AddHttpClient<IEntryCommandService, EntryCommandService>(client =>
        {
            client.BaseAddress = new Uri(EnsureTrailingSlash(apiBaseAddress));
        })
        .AddHttpMessageHandler<AccessTokenHandler>();
        services.AddHttpClient<IFileCommandService, FileCommandService>(client =>
        {
            client.BaseAddress = new Uri(EnsureTrailingSlash(imageApiBaseAddress));
        });
        return services;
    }

    private static string EnsureTrailingSlash(string address)
    {
        return address.EndsWith('/') ? address : $"{address}/";
    }
}
