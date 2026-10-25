using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Commands;
using CnGalWebSite.SDK.MainSite.Infrastructure;
using CnGalWebSite.SDK.MainSite.Queries;
using CnGalWebSite.SDK.MainSite.Services.Toolbox;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http.Resilience;
using Polly;

namespace CnGalWebSite.SDK.MainSite.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMainSiteSdkCore(this IServiceCollection services, string apiBaseAddress, string imageApiBaseAddress, string taskApiBaseAddress,
        Func<IServiceProvider, string, HttpClient>? createClient = null)
    {
        services.AddMemoryCache();
        services.AddScoped<SdkHttpClientScope>();
        services.AddScoped<IEntryPageQueryService, EntryPageQueryService>();
        services.AddScoped<IArticlePageQueryService, ArticlePageQueryService>();
        services.AddScoped<IVideoPageQueryService, VideoPageQueryService>();
        services.AddScoped<ITagPageQueryService, TagPageQueryService>();
        services.AddScoped<IPeripheryPageQueryService, PeripheryPageQueryService>();
        services.TryAddSingleton<ExamineUserCache>();
        services.TryAddScoped<ICurrentUserAccessor, NullCurrentUserAccessor>();
        services.AddTransient<PublicDataCache>();

        var apiBase = new Uri(EnsureTrailingSlash(apiBaseAddress));
        var imageApiBase = new Uri(EnsureTrailingSlash(imageApiBaseAddress));
        var taskApiBase = new Uri(EnsureTrailingSlash(taskApiBaseAddress));

        RegisterSdkHttpClient<IAccountQueryService, AccountQueryService>(services, apiBase, createClient);
        RegisterSdkHttpClient<IHomeQueryService, HomeQueryService>(services, apiBase, createClient);
        RegisterSdkHttpClient<IEntryQueryService, EntryQueryService>(services, apiBase, createClient);
        RegisterSdkHttpClient<ISpaceQueryService, SpaceQueryService>(services, apiBase, createClient);
        RegisterSdkHttpClient<ITagQueryService, TagQueryService>(services, apiBase, createClient);
        RegisterSdkHttpClient<IArticleQueryService, ArticleQueryService>(services, apiBase, createClient);
        RegisterSdkHttpClient<IVideoQueryService, VideoQueryService>(services, apiBase, createClient);
        RegisterSdkHttpClient<IPeripheryQueryService, PeripheryQueryService>(services, apiBase, createClient);
        RegisterSdkHttpClient<ILotteryQueryService, LotteryQueryService>(services, apiBase, createClient);
        RegisterSdkHttpClient<IPlayedGameQueryService, PlayedGameQueryService>(services, apiBase, createClient);
        RegisterSdkHttpClient<IStoreInfoQueryService, StoreInfoQueryService>(services, apiBase, createClient);
        RegisterSdkHttpClient<ICommentQueryService, CommentQueryService>(services, apiBase, createClient);
        RegisterSdkHttpClient<ITableQueryService, TableQueryService>(services, apiBase, createClient);
        RegisterSdkHttpClient<IVoteQueryService, VoteQueryService>(services, apiBase, createClient);
        RegisterSdkHttpClient<IFavoriteFolderQueryService, FavoriteFolderQueryService>(services, apiBase, createClient);
        RegisterSdkHttpClient<IKanbanQueryService, KanbanQueryService>(services, apiBase, createClient);
        RegisterSdkHttpClient<ICommodityQueryService, CommodityQueryService>(services, apiBase, createClient);

        RegisterSdkHttpClient<IEntryCommandService, EntryCommandService>(services, apiBase, createClient);
        RegisterSdkHttpClient<IArticleCommandService, ArticleCommandService>(services, apiBase, createClient);
        RegisterSdkHttpClient<ITagCommandService, TagCommandService>(services, apiBase, createClient);
        RegisterSdkHttpClient<IVideoCommandService, VideoCommandService>(services, apiBase, createClient);
        RegisterSdkHttpClient<ISpaceCommandService, SpaceCommandService>(services, apiBase, createClient);
        RegisterSdkHttpClient<ILotteryCommandService, LotteryCommandService>(services, apiBase, createClient);
        RegisterSdkHttpClient<IPeripheryCommandService, PeripheryCommandService>(services, apiBase, createClient);
        RegisterSdkHttpClient<IPlayedGameCommandService, PlayedGameCommandService>(services, apiBase, createClient);
        RegisterSdkHttpClient<ICommentCommandService, CommentCommandService>(services, apiBase, createClient);
        RegisterSdkHttpClient<IFavoriteFolderCommandService, FavoriteFolderCommandService>(services, apiBase, createClient);
        RegisterSdkHttpClient<IToolboxCommandService, ToolboxCommandService>(services, apiBase, createClient);
        RegisterSdkHttpClient<IFileCommandService, FileCommandService>(services, imageApiBase, createClient);

        services.AddScoped<ToolboxImageService>();
        services.AddScoped<IToolboxVideoRepostService, ToolboxVideoRepostService>();
        services.AddScoped<IToolboxEntryMergeService, ToolboxEntryMergeService>();
        services.AddHttpClient<IToolboxArticleRepostService, ToolboxArticleRepostService>();

        RegisterSdkHttpClient<IAdminQueryService, AdminQueryService>(services, apiBase, createClient);
        RegisterSdkHttpClient<IAdminCommandService, AdminCommandService>(services, apiBase, createClient);

        RegisterSdkHttpClient<IPerfectionQueryService, PerfectionQueryService>(services, apiBase, createClient);

        RegisterSdkHttpClient<IExamineQueryService, ExamineQueryService>(services, apiBase, createClient);
        RegisterSdkHttpClient<IExamineCommandService, ExamineCommandService>(services, apiBase, createClient);

        RegisterSdkHttpClient<IFileQueryService, FileQueryService>(services, imageApiBase, createClient);
        RegisterSdkHttpClient<ITimedTaskService, TimedTaskService>(services, taskApiBase, createClient);

        RegisterSdkHttpClient<IExpoQueryService, ExpoQueryService>(services, apiBase, createClient);
        RegisterSdkHttpClient<IExpoCommandService, ExpoCommandService>(services, apiBase, createClient);

        return services;
    }

    private static void RegisterSdkHttpClient<TInterface, TImpl>(
        IServiceCollection services,
        Uri baseAddress,
        Func<IServiceProvider, string, HttpClient>? createClient)
        where TInterface : class
        where TImpl : class, TInterface
    {
        var clientName = typeof(TInterface).FullName!;
        var builder = services.AddHttpClient(clientName, client =>
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

        builder.AddStandardResilienceHandler(options =>
        {
            options.Retry.MaxRetryAttempts = 1;
            options.Retry.UseJitter = true;
            var shouldRetry = options.Retry.ShouldHandle;
            options.Retry.ShouldHandle = args => SdkRequestPolicy.CanReplay(args.Context.GetRequestMessage())
                ? shouldRetry(args) : ValueTask.FromResult(false);
            options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(12);
            options.CircuitBreaker.MinimumThroughput = 4;
        });
        services.AddScoped<TInterface>(provider => ActivatorUtilities.CreateInstance<TImpl>(provider,
            createClient?.Invoke(provider, clientName)
                ?? provider.GetRequiredService<SdkHttpClientScope>().Create(clientName)));
    }

    private static string EnsureTrailingSlash(string address)
    {
        return address.EndsWith('/') ? address : $"{address}/";
    }

    private sealed class SdkHttpClientScope(IHttpClientFactory factory) : IDisposable
    {
        private readonly List<HttpClient> _clients = [];
        public HttpClient Create(string name) { var client = factory.CreateClient(name); _clients.Add(client); return client; }
        public void Dispose() { foreach (var client in _clients) client.Dispose(); _clients.Clear(); }
    }

    private sealed class NullCurrentUserAccessor : ICurrentUserAccessor
    {
        public string? GetCurrentUserId() => null;
    }
}
