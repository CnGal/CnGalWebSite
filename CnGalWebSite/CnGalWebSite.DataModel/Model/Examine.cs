﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace CnGalWebSite.DataModel.Model
{
    public class Examine
    {
        public long Id { get; set; }
        [StringLength(4000000)]
        public string Context { get; set; }

        /// <summary>
        /// 对数据的操作 0无 1用户主页
        /// </summary>
        public Operation Operation { get; set; }
        /// <summary>
        /// 审核版本
        /// </summary>
        public ExamineVersion Version { get; set; }

        public bool? IsPassed { get; set; }

        public DateTime? PassedTime { get; set; }

        public DateTime ApplyTime { get; set; }
        /// <summary>
        /// 附加贡献值
        /// </summary>
        public int ContributionValue { get; set; } = 0;
        /// <summary>
        /// 提交审核时的附加说明
        /// </summary>
        public string Note { get; set; }
        /// <summary>
        /// 批注
        /// </summary>
        public string Comments { get; set; }
        /// <summary>
        /// 处理审核请求的管理员名称
        /// </summary>
        public string PassedAdminName { get; set; }

        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        public int? EntryId { get; set; }
        public Entry Entry { get; set; }

        public int? TagId { get; set; }
        public Tag Tag { get; set; }

        public long? ArticleId { get; set; }
        public Article Article { get; set; }

        public long? CommentId { get; set; }
        public Comment Comment { get; set; }

        public int? DisambigId { get; set; }
        public Disambig Disambig { get; set; }

        public long? PeripheryId { get; set; }
        public Periphery Periphery { get; set; }

        public long? PlayedGameId { get; set; }
        public PlayedGame PlayedGame { get; set; }

        public long? VideoId { get; set; }
        public Video Video { get; set; }

        public long? FavoriteFolderId { get; set; }
        public FavoriteFolder FavoriteFolder { get; set; }
        /// <summary>
        /// 前置审核Id
        /// </summary>
        public long? PrepositionExamineId { get; set; }

        public virtual ICollection<UserReviewEditRecord> ReviewUsers { get; set; }

    }

    public class UserReviewEditRecord
    {
        public long Id { get; set; }

        public EditRecordReviewState State { get; set; }

        /// <summary>
        /// 审阅时间
        /// </summary>
        public DateTime? ReviewedTime { get; set; }

        public ApplicationUser ApplicationUser { get; set; }
        public string ApplicationUserId { get; set; }

        public long? ExamineId { get; set; }
        public Examine Examine { get; set; }
    }


    public enum EditRecordReviewState
    {
        [Display(Name ="未读")]
        UnRead,
        [Display(Name = "已读")]
        Reviewed,
        [Display(Name = "忽略")]
        Ignored
    }

    public enum ExamineVersion
    {
        V1_0,
        V1_1,
        V1_2,
    }

    public enum Operation
    {
        [Display(Name = "缺省")]
        None,
        [Display(Name = "修改个人主页")]
        UserMainPage,
        [Display(Name = "编辑词条主要信息")]
        EstablishMain,
        [Display(Name = "编辑词条附加信息")]
        EstablishAddInfor,
        [Display(Name = "编辑词条主页")]
        EstablishMainPage,
        [Display(Name = "编辑词条图片")]
        EstablishImages,
        [Display(Name = "编辑词条关联信息")]
        EstablishRelevances,
        [Display(Name = "编辑词条标签")]
        EstablishTags,
        [Display(Name = "编辑文章主要信息")]
        EditArticleMain,
        [Display(Name = "编辑文章关联信息")]
        EditArticleRelevanes,
        [Display(Name = "编辑文章内容")]
        EditArticleMainPage,
        [Display(Name = "编辑标签")]
        EditTag,
        [Display(Name = "发表评论")]
        PubulishComment,
        [Display(Name = "编辑消歧义页主要信息")]
        DisambigMain,
        [Display(Name = "编辑消歧义页关联信息")]
        DisambigRelevances,
        [Display(Name = "编辑个人信息")]
        EditUserMain,
        [Display(Name = "编辑周边主要信息")]
        EditPeripheryMain,
        [Display(Name = "编辑周边图片")]
        EditPeripheryImages,
        [Display(Name = "编辑周边关联词条")]
        EditPeripheryRelatedEntries,
        [Display(Name = "编辑标签主要信息")]
        EditTagMain,
        [Display(Name = "编辑标签子标签")]
        EditTagChildTags,
        [Display(Name = "编辑标签子词条")]
        EditTagChildEntries,
        [Display(Name = "编辑周边关联周边")]
        EditPeripheryRelatedPeripheries,
        [Display(Name = "编辑游玩记录")]
        EditPlayedGameMain,
        [Display(Name = "编辑词条音频")]
        EstablishAudio,
        [Display(Name = "申请用户认证")]
        RequestUserCertification,
        [Display(Name = "编辑视频主要信息")]
        EditVideoMain,
        [Display(Name = "编辑视频图片")]
        EditVideoImages,
        [Display(Name = "编辑视频内容")]
        EditVideoMainPage,
        [Display(Name = "编辑视频关联信息")]
        EditVideoRelevanes,
        [Display(Name = "编辑收藏夹")]
        EditFavoriteFolderMain,
        [Display(Name = "编辑词条官网模板补充信息")]
        EstablishWebsite,
    }
}
