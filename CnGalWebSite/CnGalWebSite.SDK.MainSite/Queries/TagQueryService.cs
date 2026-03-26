using CnGalWebSite.DataModel.ViewModel.Tags;
using CnGalWebSite.DataModel.ViewModel.ThematicPages;
using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Infrastructure;
using CnGalWebSite.SDK.MainSite.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CnGalWebSite.SDK.MainSite.Queries;

public sealed class TagQueryService(
    HttpClient httpClient,
    IMemoryCache memoryCache,
    ILogger<TagQueryService> logger) : QueryServiceBase(httpClient), ITagQueryService
{
    private static readonly TimeSpan CvPageCacheDuration = TimeSpan.FromMinutes(5);

    protected override ILogger Logger => logger;

    public Task<SdkResult<List<TagTreeModel>>> GetTagTreeAsync(CancellationToken cancellationToken = default)
    {
        return GetAsync<List<TagTreeModel>>(
            "api/tags/GetTagsTreeView",
            "TAG_TREE",
            "标签树",
            cancellationToken);
    }

    public Task<SdkResult<TagIndexViewModel>> GetTagDetailAsync(int id, CancellationToken cancellationToken = default)
    {
        return GetSingleAsync<TagIndexViewModel, TagIndexViewModel>(
            $"api/tags/gettag/{id}",
            dto => dto,
            "TAG",
            "标签",
            id,
            cancellationToken);
    }

    public async Task<SdkResult<CVThematicPageViewModel>> GetCVThematicPageAsync(CancellationToken cancellationToken = default)
    {
        const string cacheKey = "main-site:cv-thematic-page";
        if (memoryCache.TryGetValue(cacheKey, out CVThematicPageViewModel? cached) && cached is not null)
        {
            return SdkResult<CVThematicPageViewModel>.Ok(cached);
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

        memoryCache.Set(cacheKey, result.Data, CvPageCacheDuration);

        return SdkResult<CVThematicPageViewModel>.Ok(result.Data);
    }
}
