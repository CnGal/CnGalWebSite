using CnGalWebSite.DataModel.ViewModel.Articles;

namespace CnGalWebSite.SDK.MainSite.Models.ArticleEdit;

public sealed class ArticleEditViewModel
{
    public bool IsCreate { get; set; }

    public CreateArticleViewModel Data { get; set; } = new();

    public ArticleEditMetaOptions MetaOptions { get; set; } = new();
}
