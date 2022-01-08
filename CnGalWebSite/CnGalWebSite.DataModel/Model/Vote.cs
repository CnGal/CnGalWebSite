using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CnGalWebSite.DataModel.Model
{
    public class Vote
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string   DisplayName { get; set; }
        /// <summary>
        /// 简介
        /// </summary>
        public string BriefIntroduction { get; set; }
        /// <summary>
        /// 主图
        /// </summary>
        public string MainPicture { get; set; }
        /// <summary>
        /// 背景图
        /// </summary>
        public string BackgroundPicture { get; set; }
        /// <summary>
        /// 小背景图
        /// </summary>
        public string SmallBackgroundPicture { get; set; }
        /// <summary>
        /// 缩略图
        /// </summary>
        public string Thumbnail { get; set; }
        /// <summary>
        /// 主页
        /// </summary>
        [StringLength(10000000)]
        public string MainPage { get; set; }
        /// <summary>
        /// 类型
        /// </summary>
        public VoteType Type { get; set; }

        /// <summary>
        /// 优先级
        /// </summary>
        public int Priority { get; set; } = 0;

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 最后编辑时间
        /// </summary>
        public DateTime LastEditTime { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime BeginTime { get; set; }

        /// <summary>
        ///截止时间
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// 阅读数
        /// </summary>
        public int ReaderCount { get; set; }

        /// <summary>
        /// 评论数
        /// </summary>
        public int CommentCount { get; set; }

        /// <summary>
        /// 是否隐藏
        /// </summary>
        public bool IsHidden { get; set; } = false;

        /// <summary>
        /// 是否允许修改
        /// </summary>
        public bool IsAllowModification { get; set; }

        /// <summary>
        /// 是否可以评论
        /// </summary>
        public bool? CanComment { get; set; } = true;

        public long MinimumSelectionCount { get; set; }

        public long MaximumSelectionCount { get; set; }


        public virtual ICollection<VoteUser> VoteUsers { get; set; } = new List<VoteUser>();

        public virtual ICollection<VoteOption> VoteOptions { get; set; } = new List<VoteOption>();


        public virtual ICollection<Entry> Entries { get; set; } = new List<Entry>();

        public virtual ICollection<Article> Articles { get; set; } = new List<Article>();

        public virtual ICollection<Periphery> Peripheries { get; set; } = new List<Periphery>();
    }

    public class VoteOption
    {
        public long Id { get; set; }

        public string Text { get; set; }

        public VoteOptionType Type { get; set; }


        public int? EntryId { get; set; }
        public Entry Entry { get; set; }

        public long? ArticleId { get; set; }
        public Article Article { get; set; }

        public long? PeripheryId { get; set; }
        public Periphery Periphery { get; set; }

        public virtual ICollection<VoteUser> VoteUsers { get; set; } = new List<VoteUser>();
    }

    public enum VoteOptionType
    {
        [Display(Name = "文本")]
        Text,
        [Display(Name = "词条")]
        Entry,
        [Display(Name = "文章")]
        Article,
        [Display(Name = "周边")]
        Periphery
    }

    public class VoteUser
    {
        public long Id { get; set; }

        public virtual ICollection<VoteOption> SeletedOptions { get; set; } = new List<VoteOption>();

        public ApplicationUser ApplicationUser { get; set; }
        public string ApplicationUserId { get; set; }

        public long? VoteId { get; set; }
        public Vote Vote { get; set; }

        public DateTime VotedTime { get; set; }

        /// <summary>
        /// 是否匿名
        /// </summary>
        public bool IsAnonymous { get; set; }
    }

    public enum VoteType
    {
        [Display(Name ="单选")]
        SingleChoice,
        [Display(Name = "多选")]
        MultipleChoice
    }
}
