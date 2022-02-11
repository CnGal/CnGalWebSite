using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace CnGalWebSite.DataModel.Model
{
    public class Comment
    {
        public long Id { get; set; }

        public bool IsHidden { get; set; }

        public DateTime CommentTime { get; set; }

        [StringLength(10000000)]
        public string Text { get; set; }

        public CommentType Type { get; set; }
        /// <summary>
        /// 优先级
        /// </summary>
        public int Priority { get; set; } = 0;

        /// <summary>
        /// 发表评论的用户
        /// </summary>
        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        public int? EntryId { get; set; }
        public Entry Entry { get; set; }

        public long? ArticleId { get; set; }
        public Article Article { get; set; }

        public long? PeripheryId { get; set; }
        public Periphery Periphery { get; set; }

        public long? VoteId { get; set; }
        public Vote Vote { get; set; }

        public long? LotteryId { get; set; }
        public Lottery Lottery { get; set; }

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
        CommentUser,
        [Display(Name = "评论周边")]
        CommentPeriphery,
        [Display(Name = "评论投票")]
        CommentVote,
        [Display(Name = "评论抽奖")]
        CommentLottery,
    }
}
