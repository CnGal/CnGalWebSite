using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CnGalWebSite.DataModel.Model
{
    public class Video
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
        public string Type { get; set; }

        /// <summary>
        /// 版权
        /// </summary>
        public CopyrightType Copyright { get; set; }
        /// <summary>
        /// 时长
        /// </summary>
        public TimeSpan Duration { get; set; }
        /// <summary>
        /// 是否为互动视频
        /// </summary>
        public bool IsInteractive { get; set; }
        /// <summary>
        /// 是否为用户本人创作
        /// </summary>
        public bool IsCreatedByCurrentUser { get; set; }

        /// <summary>
        /// 优先级
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 最后编辑时间
        /// </summary>
        public DateTime LastEditTime { get; set; }

        /// <summary>
        /// 创建视频的用户
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

        /// <summary>
        /// 原作者名称
        /// </summary>
        public string OriginalAuthor { get; set; }

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
        /// 图片列表
        /// </summary>
        public ICollection<EntryPicture> Pictures { get; set; } = new List<EntryPicture>();

        /// <summary>
        /// 关联词条
        /// </summary>
        public ICollection<Entry> Entries { get; set; } = new List<Entry>();
        /// <summary>
        /// 关联文章
        /// </summary>
        public ICollection<Article> Articles { get; set; } = new List<Article>();
        /// <summary>
        /// 关联外部链接
        /// </summary>
        public ICollection<Outlink> Outlinks { get; set; } = new List<Outlink>();
        /// <summary>
        /// 审核记录 也是编辑记录
        /// </summary>
        public ICollection<Examine> Examines { get; set; } = new List<Examine>();
        /// <summary>
        /// 关联视频列表
        /// </summary>
        public virtual ICollection<VideoRelation> VideoRelationFromVideoNavigation { get; set; } = new List<VideoRelation>();

        /// <summary>
        /// To 指当前词条被关联的其他词条关联 呈现编辑视图时不使用
        /// </summary>
        public virtual ICollection<VideoRelation> VideoRelationToVideoNavigation { get; set; } = new List<VideoRelation>();
    }


    public partial class VideoRelation
    {
        public long VideoRelationId { get; set; }

        public long? FromVideo { get; set; }
        public long? ToVideo { get; set; }

        public virtual Video FromVideoNavigation { get; set; } = new Video();
        public virtual Video ToVideoNavigation { get; set; } = new Video();
    }

    public enum CopyrightType
    {
        [Display(Name = "原创")]
        Original,
        [Display(Name = "转载")]
        Transshipment
    }
}
