using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Infrastructure;
using CnGalWebSite.SDK.MainSite.Models;
using Microsoft.Extensions.Logging;

namespace CnGalWebSite.SDK.MainSite.Queries;

public sealed class ArticlePageQueryService(IArticleQueryService articles, IExamineQueryService records,
    ICurrentUserAccessor user, ILogger<ArticlePageQueryService> logger) : IArticlePageQueryService
{
    public Task<SdkResult<ArticlePageModel>> GetAsync(long id, CancellationToken cancellationToken = default)
        => DetailPageRequests.LoadAsync(token => articles.GetArticleDetailAsync(id, token), token => records.GetArticleEditRecordsAsync(id, token),
            (article, edits) => new ArticlePageModel(article, edits), user, logger, cancellationToken);
}

public sealed class VideoPageQueryService(IVideoQueryService videos, IExamineQueryService records,
    ICurrentUserAccessor user, ILogger<VideoPageQueryService> logger) : IVideoPageQueryService
{
    public Task<SdkResult<VideoPageModel>> GetAsync(long id, CancellationToken cancellationToken = default)
        => DetailPageRequests.LoadAsync(token => videos.GetVideoDetailAsync(id, token), token => records.GetVideoEditRecordsAsync(id, token),
            (video, edits) => new VideoPageModel(video, edits), user, logger, cancellationToken);
}

public sealed class TagPageQueryService(ITagQueryService tags, IExamineQueryService records,
    ICurrentUserAccessor user, ILogger<TagPageQueryService> logger) : ITagPageQueryService
{
    public Task<SdkResult<TagPageModel>> GetAsync(int id, CancellationToken cancellationToken = default)
        => DetailPageRequests.LoadAsync(token => tags.GetTagDetailAsync(id, token), token => records.GetTagEditRecordsAsync(id, token),
            (tag, edits) => new TagPageModel(tag, edits), user, logger, cancellationToken);
}

public sealed class PeripheryPageQueryService(IPeripheryQueryService peripheries, IExamineQueryService records,
    ICurrentUserAccessor user, ILogger<PeripheryPageQueryService> logger) : IPeripheryPageQueryService
{
    public Task<SdkResult<PeripheryPageModel>> GetAsync(long id, CancellationToken cancellationToken = default)
        => DetailPageRequests.LoadAsync(token => peripheries.GetPeripheryDetailAsync(id, token), token => records.GetPeripheryEditRecordsAsync(id, token),
            (periphery, edits) => new PeripheryPageModel(periphery, edits), user, logger, cancellationToken);
}

// Shared mechanics for this specific pair of requests, without a generic page model or component base.
internal static class DetailPageRequests
{
    public static async Task<SdkResult<TPage>> LoadAsync<T, TPage>(
        Func<CancellationToken, Task<SdkResult<T>>> loadMain,
        Func<CancellationToken, Task<SdkResult<IReadOnlyList<ExaminedNormalListModel>>>> loadRecords,
        Func<T, SdkResult<IReadOnlyList<ExaminedNormalListModel>>, TPage> create,
        ICurrentUserAccessor user, ILogger logger, CancellationToken cancellationToken)
    {
        await user.GetIdentityStateAsync(cancellationToken);
        var identityVersion = user.IdentityVersion;
        using var lifetime = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var tasks = new List<Task>(2);
        try
        {
            var main = loadMain(lifetime.Token); tasks.Add(main);
            var records = loadRecords(lifetime.Token); tasks.Add(records);
            var result = await main;
            if (!result.Success || result.Data is null)
                return SdkResult<TPage>.Fail(result.Error?.Code ?? "PAGE_MAIN_FAILED", result.Error?.Message ?? "页面加载失败", result.Error?.StatusCode);
            await Task.WhenAll(tasks);
            if (identityVersion != user.IdentityVersion) throw new SdkIdentityChangedException();
            return SdkResult<TPage>.Ok(create(result.Data, await records));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch (Exception ex)
        {
            logger.LogError(ex, "Detail page aggregation failed. Page={Page}", typeof(TPage).Name);
            return SdkFailure.FromException<TPage>(ex, "PAGE_EXCEPTION", "页面加载失败");
        }
        finally
        {
            await lifetime.CancelAsync();
            try { await Task.WhenAll(tasks); }
            catch (OperationCanceledException) when (lifetime.IsCancellationRequested) { }
            catch (Exception ex) { logger.LogWarning(ex, "Detail page request cleanup. Page={Page}", typeof(TPage).Name); }
            cancellationToken.ThrowIfCancellationRequested();
        }
    }
}
