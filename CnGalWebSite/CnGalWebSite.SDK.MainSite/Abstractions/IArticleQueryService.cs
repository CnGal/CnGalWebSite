using CnGalWebSite.SDK.MainSite.Models;

namespace CnGalWebSite.SDK.MainSite.Abstractions;

public interface IArticleQueryService
{
    Task<SdkResult<ArticleDetailViewModel>> GetArticleDetailAsync(long id, CancellationToken cancellationToken = default);
}
