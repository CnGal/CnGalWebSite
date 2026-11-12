namespace CnGalWebSite.DataModel.ViewModel.Articles
{
    public class ArticleContrastEditRecordViewModel
    {
        public long ContrastId { get; set; }
        public long CurrentId { get; set; }

        public ArticleViewModel ContrastModel { get; set; }
        public ArticleViewModel CurrentModel { get; set; }
    }
}
