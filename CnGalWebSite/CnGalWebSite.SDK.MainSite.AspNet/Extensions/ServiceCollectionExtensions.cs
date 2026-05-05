using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.AspNet.Auth;
using CnGalWebSite.SDK.MainSite.AspNet.Services.Toolbox;
using CnGalWebSite.SDK.MainSite.Auth;
using CnGalWebSite.SDK.MainSite.Commands;
using CnGalWebSite.SDK.MainSite.Extensions;
using CnGalWebSite.SDK.MainSite.Models;
using CnGalWebSite.SDK.MainSite.Queries;
using CnGalWebSite.SDK.MainSite.Services.Toolbox;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;

namespace CnGalWebSite.SDK.MainSite.AspNet.Extensions;

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
            options.ResponseType = "code";
            options.ResponseMode = "query";
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

    public static IServiceCollection AddMainSiteSdk(this IServiceCollection services, string apiBaseAddress, string imageApiBaseAddress, string taskApiBaseAddress)
    {
        services.AddMainSiteSdkCore(apiBaseAddress, imageApiBaseAddress, taskApiBaseAddress);
        services.AddSingleton<ICurrentUserAccessor, HttpContextCurrentUserAccessor>();
        services.AddScoped<CircuitTokenStore>();
        services.AddTransient<AccessTokenHandler>();
        services.AddScoped(typeof(IToolboxLocalRepository<>), typeof(ToolboxLocalRepository<>));

        var apiBase = new Uri(EnsureTrailingSlash(apiBaseAddress));
        var imageApiBase = new Uri(EnsureTrailingSlash(imageApiBaseAddress));
        var taskApiBase = new Uri(EnsureTrailingSlash(taskApiBaseAddress));

        RegisterSdkHttpClient<IHomeQueryService, HomeQueryService>(services, apiBase);
        RegisterSdkHttpClient<IEntryQueryService, EntryQueryService>(services, apiBase);
        RegisterSdkHttpClient<ISpaceQueryService, SpaceQueryService>(services, apiBase);
        RegisterSdkHttpClient<ITagQueryService, TagQueryService>(services, apiBase);
        RegisterSdkHttpClient<IArticleQueryService, ArticleQueryService>(services, apiBase);
        RegisterSdkHttpClient<IVideoQueryService, VideoQueryService>(services, apiBase);
        RegisterSdkHttpClient<IPeripheryQueryService, PeripheryQueryService>(services, apiBase);
        RegisterSdkHttpClient<ILotteryQueryService, LotteryQueryService>(services, apiBase);
        RegisterSdkHttpClient<IPlayedGameQueryService, PlayedGameQueryService>(services, apiBase);
        RegisterSdkHttpClient<ICommentQueryService, CommentQueryService>(services, apiBase);
        RegisterSdkHttpClient<IFavoriteFolderQueryService, FavoriteFolderQueryService>(services, apiBase);
        RegisterSdkHttpClient<ICommodityQueryService, CommodityQueryService>(services, apiBase);

        RegisterSdkHttpClient<IEntryCommandService, EntryCommandService>(services, apiBase);
        RegisterSdkHttpClient<IArticleCommandService, ArticleCommandService>(services, apiBase);
        RegisterSdkHttpClient<ITagCommandService, TagCommandService>(services, apiBase);
        RegisterSdkHttpClient<IVideoCommandService, VideoCommandService>(services, apiBase);
        RegisterSdkHttpClient<ISpaceCommandService, SpaceCommandService>(services, apiBase);
        RegisterSdkHttpClient<ILotteryCommandService, LotteryCommandService>(services, apiBase);
        RegisterSdkHttpClient<IPeripheryCommandService, PeripheryCommandService>(services, apiBase);
        RegisterSdkHttpClient<IPlayedGameCommandService, PlayedGameCommandService>(services, apiBase);
        RegisterSdkHttpClient<ICommentCommandService, CommentCommandService>(services, apiBase);
        RegisterSdkHttpClient<IFavoriteFolderCommandService, FavoriteFolderCommandService>(services, apiBase);
        RegisterSdkHttpClient<IToolboxCommandService, ToolboxCommandService>(services, apiBase);

        RegisterSdkHttpClient<IAdminQueryService, AdminQueryService>(services, apiBase);
        RegisterSdkHttpClient<IAdminCommandService, AdminCommandService>(services, apiBase);

        RegisterSdkHttpClient<IExamineQueryService, ExamineQueryService>(services, apiBase);
        RegisterSdkHttpClient<IExamineCommandService, ExamineCommandService>(services, apiBase);

        RegisterSdkHttpClient<IFileQueryService, FileQueryService>(services, imageApiBase);
        RegisterSdkHttpClient<ITimedTaskService, TimedTaskService>(services, taskApiBase);

        return services;
    }

    private static void RegisterSdkHttpClient<TInterface, TImpl>(
        IServiceCollection services,
        Uri baseAddress)
        where TInterface : class
        where TImpl : class, TInterface
    {
        var builder = services.AddHttpClient<TInterface, TImpl>(client =>
        {
            client.BaseAddress = baseAddress;
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(5),
            PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),
            MaxConnectionsPerServer = 20,
            EnableMultipleHttp2Connections = true,
        });

        builder.AddHttpMessageHandler<AccessTokenHandler>();

        builder.AddStandardResilienceHandler(options =>
        {
            options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(12);
            options.CircuitBreaker.MinimumThroughput = 4;
        });
    }

    private static string EnsureTrailingSlash(string address)
    {
        return address.EndsWith('/') ? address : $"{address}/";
    }
}
