
using CnGalWebSite.DataModel.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace CnGalWebSite.DataModel.ViewModel.Admin
{
    public class ListExaminesInforViewModel
    {
        public int Examining { get; set; }

        public int Passed { get; set; }

        public int Unpassed { get; set; }

        public int All { get; set; }
    }
    public class ListExaminesViewModel
    {
        public List<ListExamineAloneModel> Examines { get; set; }
    }
    public class ListExamineAloneModel
    {
        [Display(Name = "Id")]
        public long Id { get; set; }
        /// <summary>
        /// 对数据的操作 0无 1用户主页
        /// </summary>
        [Display(Name = "操作")]
        public Operation? Operation { get; set; }

        [Display(Name = "是否通过审核")]
        public bool? IsPassed { get; set; }
        [Display(Name = "审核时间")]
        public DateTime? PassedTime { get; set; }
        [Display(Name = "创建时间")]
        public DateTime? ApplyTime { get; set; }
        [Display(Name = "批注")]
        public string Comments { get; set; }
        [Display(Name = "申请审核用户Id")]
        public string ApplicationUserId { get; set; }
        [Display(Name = "申请审核的用户")]
        public string UserName { get; set; }
        [Display(Name = "处理此审核的用户")]
        public string PassedAdminName { get; set; }
        [Display(Name = "关联词条Id")]
        public int? EntryId { get; set; }
        [Display(Name = "关联标签Id")]
        public int? TagId { get; set; }
        [Display(Name = "关联文章Id")]
        public long? ArticleId { get; set; }
        [Display(Name = "关联评论Id")]
        public long? CommentId { get; set; }
        [Display(Name = "附加贡献值")]
        public int ContributionValue { get; set; }

    }

    public class ExaminesPagesInfor
    {
        public CnGalWebSite.DataModel.ViewModel.Search.QueryPageOptions Options { get; set; }
        public ListExamineAloneModel SearchModel { get; set; }

        public string Text { get; set; }
    }
}
