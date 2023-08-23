
namespace CnGalWebSite.Shared.Models.Articles
{
    public class ArticleCatalogModel
    {
        public string Text { get; set; }

        public string Href { get; set; }

        public bool IsActive { get; set; }

        public List<ArticleCatalogModel> Nodes { get; set; } = new List<ArticleCatalogModel>();
    }


}
