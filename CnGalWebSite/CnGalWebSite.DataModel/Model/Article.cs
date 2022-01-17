using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace CnGalWebSite.DataModel.Model
{
    public class Article
    {
        public long Id { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 显示名称
        /// </summary>
        public string DisplayName { get; set; }
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
        /// 类型
        /// </summary>
        public ArticleType Type { get; set; }

        /// <summary>
        /// 动态类别 仅对动态有效
        /// </summary>
        public string NewsType { get; set; }

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
        /// 真实发生动态的时间 当类型是文章时 有效
        /// </summary>
        public DateTime? RealNewsTime { get; set; }

        /// <summary>
        /// 创建文章的用户
        /// </summary>
        public ApplicationUser CreateUser { get; set; }
        public string CreateUserId { get; set; }

        /// <summary>
        /// 阅读数
        /// </summary>
        public int ReaderCount { get; set; }
        /// <summary>
        /// 点赞数
        /// </summary>
        public int ThumbsUpCount { get; set; }
        /// <summary>
        /// 评论数
        /// </summary>
        public int CommentCount { get; set; }
        /// <summary>
        /// 点赞列表
        /// </summary>
        public List<ThumbsUp> ThumbsUps { get; set; }

        /// <summary>
        /// 是否隐藏
        /// </summary>
        public bool IsHidden { get; set; } = false;

        /// <summary>
        /// 是否可以评论
        /// </summary>
        public bool? CanComment { get; set; } = true;


        public string OriginalAuthor { get; set; }
        public string OriginalLink { get; set; }
        /// <summary>
        /// 发布时间 指转载前发布时间
        /// </summary>
        public DateTime PubishTime { get; set; }

        /// <summary>
        /// 主页
        /// </summary>
        [StringLength(10000000)]
        public string MainPage { get; set; }

        /// <summary>
        /// 消歧义页
        /// </summary>
        public int? DisambigId { get; set; }
        public Disambig Disambig { get; set; }

        /// <summary>
        /// 备份管理
        /// </summary>
        public long? BackUpArchiveId { get; set; }
        public BackUpArchive BackUpArchive { get; set; }

        /// <summary>
        /// 相关性列表
        /// </summary>
        public ICollection<ArticleRelevance> Relevances { get; set; } = new List<ArticleRelevance>();

        public virtual ICollection<Vote> Votes { get; set; } = new List<Vote>();
        /// <summary>
        /// 关联文章
        /// </summary>
        public ICollection<Entry> Entries { get; set; } = new List<Entry>();
        /// <summary>
        /// 关联外部链接
        /// </summary>
        public ICollection<Outlink> Outlinks { get; set; } = new List<Outlink>();
        /// <summary>
        /// 审核记录 也是编辑记录
        /// </summary>
        public ICollection<Examine> Examines { get; set; } = new List<Examine>();
        /// <summary>
        /// 关联词条列表
        /// </summary>
        public virtual ICollection<ArticleRelation> ArticleRelationFromArticleNavigation { get; set; } = new List<ArticleRelation>();

        /// <summary>
        /// To 指当前词条被关联的其他词条关联 呈现编辑视图时不使用
        /// </summary>
        public virtual ICollection<ArticleRelation> ArticleRelationToArticleNavigation { get; set; } = new List<ArticleRelation>();
    }
    public partial class ArticleRelation
    {
        public long ArticleRelationId { get; set; }

        public long? FromArticle { get; set; }
        public long? ToArticle { get; set; }

        public virtual Article FromArticleNavigation { get; set; } = new Article();
        public virtual Article ToArticleNavigation { get; set; } = new Article();
    }

    public enum ArticleType
    {
        [Display(Name = "感想")]
        Tought,
        [Display(Name = "攻略")]
        Strategy,
        [Display(Name = "访谈")]
        Interview,
        [Display(Name = "动态")]
        News,
        [Display(Name = "评测")]
        Evaluation,
        [Display(Name = "周边")]
        Peripheral,
        [Display(Name = "公告")]
        Notice,
        [Display(Name = "杂谈")]
        None,
        [Display(Name = "二创")]
        Fan,
    }

    public class ArticleRelevance
    {
        public long Id { get; set; }

        public string Modifier { get; set; }
        public string DisplayName { get; set; }
        public string DisplayValue { get; set; }
        /// <summary>
        /// 当 类别 不是可识别时 使用下方链接
        /// </summary>
        public string Link { get; set; }
    }
}
