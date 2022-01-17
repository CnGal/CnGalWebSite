using System;
namespace CnGalWebSite.DataModel.Model
{
    public class ThumbsUp
    {
        public long Id { get; set; }

        public DateTime ThumbsUpTime { get; set; }

        public long ArticleId { get; set; }
        public Article Article { get; set; }

        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
    }
}
