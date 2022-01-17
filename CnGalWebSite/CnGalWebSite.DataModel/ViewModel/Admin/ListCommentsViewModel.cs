
using CnGalWebSite.DataModel.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace CnGalWebSite.DataModel.ViewModel.Admin
{
    public class ListCommentsInforViewModel
    {
        public long EntryComments { get; set; }

        public long ArticleComments { get; set; }

        public long SpaceComments { get; set; }

        public long ParentComments { get; set; }

        public long ChildComments { get; set; }

        public long Hiddens { get; set; }
        public long All { get; set; }
    }

    public class ListCommentsViewModel
    {
        public List<ListCommentAloneModel> Comments { get; set; }
    }
    public class ListCommentAloneModel
    {
        [Display(Name = "Id")]
        public long Id { get; set; }
        [Display(Name = "类型")]
        public CommentType? Type { get; set; } = null;
        [Display(Name = "评论时间")]
        public DateTime CommentTime { get; set; }
        [Display(Name = "内容")]
        public string Text { get; set; }
        [Display(Name = "优先级")]
        public int Priority { get; set; }

        [Display(Name = "发表评论的用户Id")]
        public string ApplicationUserId { get; set; }
        [Display(Name = "父评论Id")]
        public long? ParentCodeNavigationId { get; set; }
        [Display(Name = "关联词条Id")]
        public int? EntryId { get; set; }
        [Display(Name = "关联文章Id")]
        public long? ArticleId { get; set; }
        [Display(Name = "关联用户留言板Id")]
        public long? UserSpaceCommentManagerId { get; set; }

        [Display(Name = "是否隐藏")]
        public bool IsHidden { get; set; }
    }

    public class CommentsPagesInfor
    {
        public CnGalWebSite.DataModel.ViewModel.Search.QueryPageOptions Options { get; set; }
        public ListCommentAloneModel SearchModel { get; set; }

        /// <summary>
        /// 声明获取的评论列表类型
        /// </summary>
        public CommentType Type { get; set; }
        public string ObjectId { get; set; }
    }
}
