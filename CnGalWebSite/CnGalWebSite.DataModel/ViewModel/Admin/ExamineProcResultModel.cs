using CnGalWebSite.DataModel.Model;

namespace CnGalWebSite.DataModel.ViewModel.Admin
{
    public class ExamineProcResultModel : Result
    {
        public Operation Operation { get; set; }

        public int? EntryId { get; set; }

        public long? ArticleId { get; set; }

        public int? TagId { get; set; }

        public long? VideoId { get; set; }

        public long? PeripheryId { get; set; }

        public long? CommentId { get; set; }

        public int? DisambigId { get; set; }

        public long? PlayedGameId { get; set; }

        public long? FavoriteFolderId { get; set; }

        public string? ApplicationUserId { get; set; }
    }
}
