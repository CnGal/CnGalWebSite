using CnGalWebSite.DataModel.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Xml.Linq;

namespace CnGalWebSite.DataModel.ViewModel.EditRecords
{
    public class ListUserReviewEditRecordsViewModel
    {
        public List<ListUserReviewEditRecordAloneModel> UserReviewEditRecords { get; set; } = new List<ListUserReviewEditRecordAloneModel> { };
    }
    public class ListUserReviewEditRecordAloneModel
    {
        [Display(Name = "Id")]
        public long Id { get; set; }
        [Display(Name = "操作")]
        public Operation Operation { get; set; }
        [Display(Name = "名称")]
        public string EntryName { get; set; }
        [Display(Name = "词条Id")]
        public int EntryId { get; set; }
        [Display(Name = "状态")]
        public EditRecordReviewState State { get; set; }
        [Display(Name = "编辑者Id")]
        public string UserId { get; set; }
        [Display(Name = "编辑者")]
        public string UserName { get; set; }
    }

    public class UserReviewEditRecordsPagesInfor
    {
        public CnGalWebSite.DataModel.ViewModel.Search.QueryPageOptions Options { get; set; }
        public ListUserReviewEditRecordAloneModel SearchModel { get; set; }
    }

}
