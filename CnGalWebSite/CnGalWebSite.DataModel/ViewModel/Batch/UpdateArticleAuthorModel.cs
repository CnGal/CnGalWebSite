namespace CnGalWebSite.DataModel.ViewModel.Batch
{
    public class UpdateAuthorModel
    {
        public long[] ArticleIds { get; set; }

        public long[] VideoIds { get; set; }

        public string UserId { get; set; }
    }
}
