using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CnGalWebSite.DataModel.Model
{
    public class GameNews
    {
        public long Id { get; set; }

        public string Title { get; set; }
        /// <summary>
        /// 主图
        /// </summary>
        public string MainPicture { get; set; }
        /// <summary>
        /// 简介
        /// </summary>
        public string BriefIntroduction { get; set; }
        /// <summary>
        /// 主页
        /// </summary>
        [StringLength(10000000)]
        public string MainPage { get; set; }
        /// <summary>
        /// 类型
        /// </summary>
        public ArticleType Type { get; set; }

        public string NewsType { get; set; }

        public DateTime PublishTime { get; set; }

        public string Author { get; set; }

        public string Link { get; set; }

        public GameNewsState State { get; set; }

        public bool IsOriginal { get; set; }

        public OriginalRSS RSS { get; set; }

        public long? ArticleId { get; set; }

        /// <summary>
        /// 周报生成不以关联文章的内容为模板 单向同步
        /// </summary>
        public Article Article { get; set; }

        /// <summary>
        /// 关联词条
        /// </summary>
        public List<GameNewsRelatedEntry> Entries { get; set; } = new List<GameNewsRelatedEntry>();
    }

    public class GameNewsRelatedEntry
    {
        public long Id { get; set; }

        public string EntryName { get; set; }
    }

    public enum GameNewsState
    {
        [Display(Name = "待处理")]
        Edit,
        [Display(Name = "已发表")]
        Publish,
        [Display(Name = "已忽略")]
        Ignore
    }

    public class OriginalRSS
    {
        public long Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public DateTime PublishTime { get; set; }

        public string Link { get; set; }

        public string Author { get; set; }

        public OriginalRSSType Type { get; set; }
    }

    public enum OriginalRSSType
    {
        [Display(Name = "微博")]
        Weibo,
        [Display(Name = "自定义")]
        Custom,
        [Display(Name = "B站")]
        Bilibili,
    }

    public class WeeklyNews
    {
        public long Id { get; set; }

        public string Title { get; set; }
        /// <summary>
        /// 简介
        /// </summary>
        public string BriefIntroduction { get; set; }
        /// <summary>
        /// 主页
        /// </summary>
        [StringLength(10000000)]
        public string MainPage { get; set; }
        /// <summary>
        /// 类型
        /// </summary>
        public ArticleType Type { get; set; }
        /// <summary>
        /// 主图
        /// </summary>
        public string MainPicture { get; set; }

        public DateTime PublishTime { get; set; }
        public DateTime CreateTime { get; set; }

        public GameNewsState State { get; set; }

        public long? ArticleId { get; set; }
        public Article Article { get; set; }

        public List<GameNews> News { get; set; }
    }
}
