using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace CnGalWebSite.DataModel.Model
{
    public class FavoriteFolder
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public bool IsDefault { get; set; }

        public string MainImage { get; set; }

        public string BriefIntroduction { get; set; }

        public DateTime CreateTime { get; set; }

        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        public long Count { get; set; }

        public ICollection<FavoriteObject> FavoriteObjects { get; set; }
    }

    public class FavoriteObject
    {
        public long Id { get; set; }

        public FavoriteObjectType Type { get; set; }

        public DateTime CreateTime { get; set; }

        public long FavoriteFolderId { get; set; }
        public FavoriteFolder FavoriteFolder { get; set; }

        public int? EntryId { get; set; }
        public Entry Entry { get; set; }

        public long? PeripheryId { get; set; }
        public Periphery Periphery { get; set; }

        public long? ArticleId { get; set; }
        public Article Article { get; set; }
    }

    public enum FavoriteObjectType
    {
        [Display(Name = "词条")]
        Entry,
        [Display(Name = "文章")]
        Article,
        [Display(Name = "周边")]
        Periphery,

    }
}
