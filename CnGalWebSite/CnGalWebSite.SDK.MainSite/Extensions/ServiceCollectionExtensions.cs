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

        var apiBase = new Uri(EnsureTrailingSlash(apiBaseAddress));
        var imageApiBase = new Uri(EnsureTrailingSlash(imageApiBaseAddress));

        // Query 服务（均需认证）
        RegisterSdkHttpClient<IHomeQueryService, HomeQueryService>(services, apiBase, withAuth: true);
        RegisterSdkHttpClient<IEntryQueryService, EntryQueryService>(services, apiBase, withAuth: true);
        RegisterSdkHttpClient<ISpaceQueryService, SpaceQueryService>(services, apiBase, withAuth: true);
        RegisterSdkHttpClient<ITagQueryService, TagQueryService>(services, apiBase, withAuth: true);
        RegisterSdkHttpClient<IArticleQueryService, ArticleQueryService>(services, apiBase, withAuth: true);
        RegisterSdkHttpClient<IVideoQueryService, VideoQueryService>(services, apiBase, withAuth: true);
        RegisterSdkHttpClient<ILotteryQueryService, LotteryQueryService>(services, apiBase, withAuth: true);

        // Command 服务
        RegisterSdkHttpClient<IEntryCommandService, EntryCommandService>(services, apiBase, withAuth: true);
        RegisterSdkHttpClient<IArticleCommandService, ArticleCommandService>(services, apiBase, withAuth: true);
        RegisterSdkHttpClient<ITagCommandService, TagCommandService>(services, apiBase, withAuth: true);
        RegisterSdkHttpClient<IVideoCommandService, VideoCommandService>(services, apiBase, withAuth: true);
        RegisterSdkHttpClient<ISpaceCommandService, SpaceCommandService>(services, apiBase, withAuth: true);
        RegisterSdkHttpClient<IFileCommandService, FileCommandService>(services, imageApiBase, withAuth: false);

        return services;
    }

    private static void RegisterSdkHttpClient<TInterface, TImpl>(
        IServiceCollection services,
        Uri baseAddress,
        bool withAuth)
        where TInterface : class
        where TImpl : class, TInterface
    {
        var builder = services.AddHttpClient<TInterface, TImpl>(client =>
        {
            client.BaseAddress = baseAddress;
        });

        if (withAuth)
        {
            builder.AddHttpMessageHandler<AccessTokenHandler>();
        }
    }

    private static string EnsureTrailingSlash(string address)
    {
        return address.EndsWith('/') ? address : $"{address}/";
    }
}
