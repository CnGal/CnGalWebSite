using CnGalWebSite.DataModel.ViewModel.Tags;
using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Infrastructure;
using CnGalWebSite.SDK.MainSite.Models;
using Microsoft.Extensions.Logging;

namespace CnGalWebSite.SDK.MainSite.Queries;

public sealed class TagQueryService(
    HttpClient httpClient,
    ILogger<TagQueryService> logger) : QueryServiceBase(httpClient), ITagQueryService
{
    protected override ILogger Logger => logger;

    public Task<SdkResult<List<TagTreeModel>>> GetTagTreeAsync(CancellationToken cancellationToken = default)
    {
        return GetAsync<List<TagTreeModel>>(
            "api/tags/GetTagsTreeView",
            "TAG_TREE",
            "标签树",
            cancellationToken);
    }
}
