using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Commands;
using CnGalWebSite.SDK.MainSite.Queries;
using CnGalWebSite.SDK.MainSite.Services.Toolbox;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace CnGalWebSite.SDK.MainSite.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMainSiteSdkCore(this IServiceCollection services, string apiBaseAddress, string imageApiBaseAddress, string taskApiBaseAddress)
    {
        services.AddMemoryCache();
        services.AddSingleton<ICurrentUserAccessor, NullCurrentUserAccessor>();

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
        RegisterSdkHttpClient<IStoreInfoQueryService, StoreInfoQueryService>(services, apiBase);
        RegisterSdkHttpClient<ICommentQueryService, CommentQueryService>(services, apiBase);
        RegisterSdkHttpClient<ITableQueryService, TableQueryService>(services, apiBase);
        RegisterSdkHttpClient<IVoteQueryService, VoteQueryService>(services, apiBase);
        RegisterSdkHttpClient<IFavoriteFolderQueryService, FavoriteFolderQueryService>(services, apiBase);
        RegisterSdkHttpClient<IKanbanQueryService, KanbanQueryService>(services, apiBase);
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
        RegisterSdkHttpClient<IFileCommandService, FileCommandService>(services, imageApiBase);

        services.AddScoped<ToolboxImageService>();
        services.AddScoped<IToolboxVideoRepostService, ToolboxVideoRepostService>();
        services.AddScoped<IToolboxEntryMergeService, ToolboxEntryMergeService>();
        services.AddHttpClient<IToolboxArticleRepostService, ToolboxArticleRepostService>();

        RegisterSdkHttpClient<IAdminQueryService, AdminQueryService>(services, apiBase);
        RegisterSdkHttpClient<IAdminCommandService, AdminCommandService>(services, apiBase);

        RegisterSdkHttpClient<IPerfectionQueryService, PerfectionQueryService>(services, apiBase);

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

    private sealed class NullCurrentUserAccessor : ICurrentUserAccessor
    {
        public string? GetCurrentUserId() => null;
    }
}
