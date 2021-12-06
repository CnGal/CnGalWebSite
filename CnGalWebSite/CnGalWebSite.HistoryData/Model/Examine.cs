using System;
using System.ComponentModel.DataAnnotations;
#if NET5_0_OR_GREATER
#else
using CnGalWebSite.HistoryData.Model;
#endif
namespace CnGalWebSite.DataModel.Model
{
    public class Examine
    {
        public int Id { get; set; }
        [StringLength(4000)]

        public string Context { get; set; }

        /// <summary>
        /// 对数据的操作 0无 1用户主页
        /// </summary>
        public Operation? Operation { get; set; }

        public bool? IsPassed { get; set; }

        public DateTime? PassedTime { get; set; }

        public DateTime? ApplyTime { get; set; }

        public string Comments { get; set; }
        /// <summary>
        /// 处理审核请求的管理员名称
        /// </summary>
        public string PassedAdminName { get; set; }

        public string ApplicationUserId { get; set; }

        public int? EntryId { get; set; }

        public int? TagId { get; set; }

        public long? ArticleId { get; set; }

        public long? CommentId { get; set; }

        /// <summary>
        /// 前置审核Id
        /// </summary>
        public int? PrepositionExamineId { get; set; }

        public ApplicationUser ApplicationUser { get; set; }

        public Entry Entry { get; set; }

        public Article Article { get; set; }

        public Comment Comment { get; set; }

        public Tag Tag { get; set; }
    }
    public enum Operation
    {
        [Display(Name = "缺省")]
        None,
        [Display(Name = "修改用户主页")]
        UserMainPage,
        [Display(Name = "编辑词条主要信息")]
        EstablishMain,
        [Display(Name = "编辑词条附加信息")]
        EstablishAddInfor,
        [Display(Name = "编辑词条主页")]
        EstablishMainPage,
        [Display(Name = "编辑词条图片")]
        EstablishImages,
        [Display(Name = "编辑词条相关链接")]
        EstablishRelevances,
        [Display(Name = "编辑词条标签")]
        EstablishTags,
        [Display(Name = "编辑文章主要信息")]
        EditArticleMain,
        [Display(Name = "编辑文章关联词条")]
        EditArticleRelevanes,
        [Display(Name = "编辑文章内容")]
        EditArticleMainPage,
        [Display(Name = "编辑标签")]
        EditTag,
        [Display(Name = "发表评论")]
        PubulishComment
    }
}
