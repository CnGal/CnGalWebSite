using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Auth;
using CnGalWebSite.SDK.MainSite.Commands;
using CnGalWebSite.SDK.MainSite.Models;
using CnGalWebSite.SDK.MainSite.Queries;
using CnGalWebSite.SDK.MainSite.Services.Toolbox;
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

    public static IServiceCollection AddMainSiteSdk(this IServiceCollection services, string apiBaseAddress, string imageApiBaseAddress, string taskApiBaseAddress)
    {
        services.AddMemoryCache();
        services.AddHttpContextAccessor();
        services.AddTransient<AccessTokenHandler>();

        var apiBase = new Uri(EnsureTrailingSlash(apiBaseAddress));
        var imageApiBase = new Uri(EnsureTrailingSlash(imageApiBaseAddress));
        var taskApiBase = new Uri(EnsureTrailingSlash(taskApiBaseAddress));

        // Query 服务（均需认证）
        RegisterSdkHttpClient<IHomeQueryService, HomeQueryService>(services, apiBase, withAuth: true);
        RegisterSdkHttpClient<IEntryQueryService, EntryQueryService>(services, apiBase, withAuth: true);
        RegisterSdkHttpClient<ISpaceQueryService, SpaceQueryService>(services, apiBase, withAuth: true);
        RegisterSdkHttpClient<ITagQueryService, TagQueryService>(services, apiBase, withAuth: true);
        RegisterSdkHttpClient<IArticleQueryService, ArticleQueryService>(services, apiBase, withAuth: true);
        RegisterSdkHttpClient<IVideoQueryService, VideoQueryService>(services, apiBase, withAuth: true);
        RegisterSdkHttpClient<IPeripheryQueryService, PeripheryQueryService>(services, apiBase, withAuth: true);
        RegisterSdkHttpClient<ILotteryQueryService, LotteryQueryService>(services, apiBase, withAuth: true);
        RegisterSdkHttpClient<IPlayedGameQueryService, PlayedGameQueryService>(services, apiBase, withAuth: true);
        RegisterSdkHttpClient<IStoreInfoQueryService, StoreInfoQueryService>(services, apiBase, withAuth: false);
        RegisterSdkHttpClient<ICommentQueryService, CommentQueryService>(services, apiBase, withAuth: true);
        RegisterSdkHttpClient<ITableQueryService, TableQueryService>(services, apiBase, withAuth: false);
        RegisterSdkHttpClient<IVoteQueryService, VoteQueryService>(services, apiBase, withAuth: false);
        RegisterSdkHttpClient<IFavoriteFolderQueryService, FavoriteFolderQueryService>(services, apiBase, withAuth: true);

        // Command 服务
        RegisterSdkHttpClient<IEntryCommandService, EntryCommandService>(services, apiBase, withAuth: true);
        RegisterSdkHttpClient<IArticleCommandService, ArticleCommandService>(services, apiBase, withAuth: true);
        RegisterSdkHttpClient<ITagCommandService, TagCommandService>(services, apiBase, withAuth: true);
        RegisterSdkHttpClient<IVideoCommandService, VideoCommandService>(services, apiBase, withAuth: true);
        RegisterSdkHttpClient<ISpaceCommandService, SpaceCommandService>(services, apiBase, withAuth: true);
        RegisterSdkHttpClient<ILotteryCommandService, LotteryCommandService>(services, apiBase, withAuth: true);
        RegisterSdkHttpClient<IPeripheryCommandService, PeripheryCommandService>(services, apiBase, withAuth: true);
        RegisterSdkHttpClient<IPlayedGameCommandService, PlayedGameCommandService>(services, apiBase, withAuth: true);
        RegisterSdkHttpClient<ICommentCommandService, CommentCommandService>(services, apiBase, withAuth: true);
        RegisterSdkHttpClient<IFavoriteFolderCommandService, FavoriteFolderCommandService>(services, apiBase, withAuth: true);
        RegisterSdkHttpClient<IToolboxCommandService, ToolboxCommandService>(services, apiBase, withAuth: true);
        RegisterSdkHttpClient<IFileCommandService, FileCommandService>(services, imageApiBase, withAuth: false);

        // Toolbox 编排服务
        services.AddScoped(typeof(IToolboxLocalRepository<>), typeof(ToolboxLocalRepository<>));
        services.AddScoped<ToolboxImageService>();
        services.AddScoped<IToolboxVideoRepostService, ToolboxVideoRepostService>();
        services.AddScoped<IToolboxEntryMergeService, ToolboxEntryMergeService>();
        services.AddHttpClient<IToolboxArticleRepostService, ToolboxArticleRepostService>();

        // Admin 服务（需认证）
        RegisterSdkHttpClient<IAdminQueryService, AdminQueryService>(services, apiBase, withAuth: true);
        RegisterSdkHttpClient<IAdminCommandService, AdminCommandService>(services, apiBase, withAuth: true);

        // Examine 服务（需认证，审核/审阅/监控）
        RegisterSdkHttpClient<IExamineQueryService, ExamineQueryService>(services, apiBase, withAuth: true);
        RegisterSdkHttpClient<IExamineCommandService, ExamineCommandService>(services, apiBase, withAuth: true);

        // 外部服务（DrawingBed / TimedTask，认证）
        RegisterSdkHttpClient<IFileQueryService, FileQueryService>(services, imageApiBase, withAuth: true);
        RegisterSdkHttpClient<ITimedTaskService, TimedTaskService>(services, taskApiBase, withAuth: true);

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
            // 总超时 30 秒（远低于 .NET 默认的 100 秒），配合弹性策略后实际最多 2 次重试
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
        {
            // 连接池生命周期：定期回收连接，避免陈旧连接拖死请求
            PooledConnectionLifetime = TimeSpan.FromMinutes(5),
            // 空闲连接 2 分钟后自动关闭
            PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),
            // 限制对同一 API 服务器的最大并发连接数
            MaxConnectionsPerServer = 20,
            // 允许 HTTP/2 多路复用（减少连接数，提升吞吐）
            EnableMultipleHttp2Connections = true,
        });

        if (withAuth)
        {
            builder.AddHttpMessageHandler<AccessTokenHandler>();
        }

        // 标准弹性策略：自动重试 + 熔断 + 单次尝试超时
        builder.AddStandardResilienceHandler(options =>
        {
            // 单次 HTTP 尝试的超时（不是总超时，总超时由 HttpClient.Timeout 控制）
            options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(12);
            // 断路器：降低最小吞吐量阈值，使冷启动时也能触发保护
            options.CircuitBreaker.MinimumThroughput = 4;
        });
    }

    private static string EnsureTrailingSlash(string address)
    {
        return address.EndsWith('/') ? address : $"{address}/";
    }
}
