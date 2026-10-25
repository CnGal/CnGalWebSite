using CnGalWebSite.DataModel.ViewModel.Tags;
using CnGalWebSite.DataModel.ViewModel.ThematicPages;
using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Infrastructure;
using CnGalWebSite.SDK.MainSite.Models;
using Microsoft.Extensions.Logging;

namespace CnGalWebSite.SDK.MainSite.Queries;

public sealed class TagQueryService(
    HttpClient httpClient,
    PublicDataCache publicCache,
    ILogger<TagQueryService> logger) : QueryServiceBase(httpClient), ITagQueryService
{
    private readonly PublicDataCache memoryCache = publicCache.ForSource(httpClient.BaseAddress);

    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    protected override ILogger Logger => logger;

    public async Task<SdkResult<IReadOnlyList<TagTreeModel>>> GetTagTreeAsync(CancellationToken cancellationToken = default)
    {
        var cacheRead = await memoryCache.BeginQueryAsync(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        const string cacheKey = "main-site:tag-tree";
        if (memoryCache.TryGetValue(cacheKey, out List<TagTreeModel>? cached, cacheRead) && cached is not null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return memoryCache.Complete<IReadOnlyList<TagTreeModel>>(cached, cacheRead);
        }

        var result = await GetAsync<List<TagTreeModel>>(
            "api/tags/GetTagsTreeView",
            "TAG_TREE",
            "标签树",
            cancellationToken);

        if (result.Success && result.Data is not null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            memoryCache.Set(cacheKey, result.Data, CacheDuration, cacheRead);
        }

        return result.Success
            ? SdkResult<IReadOnlyList<TagTreeModel>>.Ok(result.Data!)
            : SdkResult<IReadOnlyList<TagTreeModel>>.Fail(result.Error!.Code, result.Error!.Message, result.Error!.StatusCode);
    }

    public async Task<SdkResult<TagIndexViewModel>> GetTagDetailAsync(int id, CancellationToken cancellationToken = default)
    {
        var cacheRead = await memoryCache.BeginQueryAsync(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        var cacheKey = PublicCacheKeys.Detail("tag", id);
        if (memoryCache.TryGetValue(cacheKey, out TagIndexViewModel? cached, cacheRead) && cached is not null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return memoryCache.Complete<TagIndexViewModel>(cached, cacheRead);
        }

        var result = await GetSingleAsync<TagIndexViewModel, TagIndexViewModel>(
            $"api/tags/gettag/{id}",
            dto => dto,
            "TAG",
            "标签",
            id,
            cancellationToken);

        if (result.Success && result.Data is not null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            memoryCache.Set(cacheKey, result.Data, CacheDuration, cacheRead);
        }

        cancellationToken.ThrowIfCancellationRequested();
        return memoryCache.CompleteResult(result, cacheRead);
    }

    public async Task<SdkResult<CVThematicPageViewModel>> GetCVThematicPageAsync(CancellationToken cancellationToken = default)
    {
        var cacheRead = await memoryCache.BeginQueryAsync(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        const string cacheKey = "main-site:cv-thematic-page";
        if (memoryCache.TryGetValue(cacheKey, out CVThematicPageViewModel? cached, cacheRead) && cached is not null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return memoryCache.Complete<CVThematicPageViewModel>(cached, cacheRead);
        }

        var result = await GetAsync<CVThematicPageViewModel>(
            "api/tags/GetCVThematicPageViewModel",
            "CV_THEMATIC_PAGE",
            "CV专题页",
            cancellationToken);

        if (!result.Success || result.Data is null)
        {
            return SdkResult<CVThematicPageViewModel>.Fail(
                result.Error?.Code ?? "CV_THEMATIC_PAGE_FAILED",
                result.Error?.Message ?? "获取CV专题页数据失败",
                result.Error?.StatusCode);
        }

        cancellationToken.ThrowIfCancellationRequested();

        memoryCache.Set(cacheKey, result.Data, CacheDuration, cacheRead);

        cancellationToken.ThrowIfCancellationRequested();
        return memoryCache.Complete<CVThematicPageViewModel>(result.Data, cacheRead);
    }
}
