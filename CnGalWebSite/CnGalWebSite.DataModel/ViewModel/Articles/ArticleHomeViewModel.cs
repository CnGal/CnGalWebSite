namespace CnGalWebSite.DataModel.ViewModel.Articles
{
    public class ArticleHomeViewModel
    {
        public List<ArticleHomeAloneViewModel> Toughts { get; set; }
        public List<ArticleHomeAloneViewModel> Strategies { get; set; }
        public List<ArticleHomeAloneViewModel> Interviews { get; set; }
        public List<ArticleHomeAloneViewModel> News { get; set; }
    }
    public class ArticleHomeAloneViewModel
    {
        public string Image { get; set; }

        public string DisPlayName { get; set; }
        public string DisPlayValue { get; set; }


        public int ReadCount { get; set; }

        public int CommentCount { get; set; }

        public long Id { get; set; }
    }
}
