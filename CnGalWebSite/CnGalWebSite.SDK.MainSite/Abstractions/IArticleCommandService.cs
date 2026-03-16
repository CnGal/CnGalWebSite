using CnGalWebSite.SDK.MainSite.Models;
using CnGalWebSite.SDK.MainSite.Models.ArticleEdit;

namespace CnGalWebSite.SDK.MainSite.Abstractions;

public interface IArticleCommandService
{
    Task<SdkResult<ArticleEditViewModel>> GetArticleCreateTemplateAsync(CancellationToken cancellationToken = default);
    Task<SdkResult<ArticleEditViewModel>> GetArticleEditAsync(long id, CancellationToken cancellationToken = default);
    Task<SdkResult<ArticleEditMetaOptions>> GetArticleEditMetaOptionsAsync(CancellationToken cancellationToken = default);
    Task<SdkResult<long>> SubmitEditAsync(ArticleEditRequest request, CancellationToken cancellationToken = default);
}
