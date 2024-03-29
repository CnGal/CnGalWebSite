﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
#if NET5_0_OR_GREATER
#else
using CnGalWebSite.HistoryData.Model;
#endif
namespace CnGalWebSite.DataModel.Model
{
    public class Comment
    {
        public long Id { get; set; }

        public bool IsHidden { get; set; }

        public DateTime CommentTime { get; set; }

        public string Text { get; set; }



        public CommentType Type { get; set; }

        /// <summary>
        /// 发表评论的用户
        /// </summary>
        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        public int? EntryId { get; set; }
        public Entry Entry { get; set; }

        public long? ArticleId { get; set; }
        public Article Article { get; set; }

        public long? UserSpaceCommentManagerId { get; set; }
        public UserSpaceCommentManager UserSpaceCommentManager { get; set; }

        /// <summary>
        /// 父评论
        /// </summary>
        public Comment ParentCodeNavigation { get; set; }

        /// <summary>
        /// 子评论
        /// </summary>
        public ICollection<Comment> InverseParentCodeNavigation { get; set; }
    }
    public enum CommentType
    {
        [Display(Name = "无")]
        None,
        [Display(Name = "评论文章")]
        CommentArticle,
        [Display(Name = "回复评论")]
        ReplyComment,
        [Display(Name = "评论词条")]
        CommentEntries,
        [Display(Name = "用户留言")]
        CommentUser
    }
}
