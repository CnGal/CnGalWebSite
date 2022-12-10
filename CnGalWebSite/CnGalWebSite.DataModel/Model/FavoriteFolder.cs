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
        /// <summary>
        /// 最后编辑时间
        /// </summary>
        public DateTime LastEditTime { get; set; }
        /// <summary>
        /// 是否向他人公开
        /// </summary>
        public bool ShowPublicly { get; set; }
        /// <summary>
        /// 阅读数
        /// </summary>
        public int ReaderCount { get; set; }
        /// <summary>
        /// 是否隐藏
        /// </summary>
        public bool IsHidden { get; set; }

        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        public long Count { get; set; }

        public ICollection<FavoriteObject> FavoriteObjects { get; set; }

        /// <summary>
        /// 审核记录 也是编辑记录
        /// </summary>
        public ICollection<Examine> Examines { get; set; } = new List<Examine>();
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

        public int? TagId { get; set; }
        public Tag Tag { get; set; }

        public long? VideoId { get; set; }
        public Video Video { get; set; }
    }

    public enum FavoriteObjectType
    {
        [Display(Name = "词条")]
        Entry,
        [Display(Name = "文章")]
        Article,
        [Display(Name = "周边")]
        Periphery,
        [Display(Name = "视频")]
        Video,
        [Display(Name = "标签")]
        Tag,
    }
}
