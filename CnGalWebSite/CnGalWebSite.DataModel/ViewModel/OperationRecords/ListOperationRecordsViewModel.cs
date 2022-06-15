using CnGalWebSite.DataModel.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CnGalWebSite.DataModel.ViewModel.OperationRecords
{
    public class ListOperationRecordAloneModel
    {
        [Display(Name = "Id")]
        public long Id { get; set; }
        [Display(Name = "类型")]
        public OperationRecordType? Type { get; set; }

        [Display(Name = "用户名")]
        public string UserName { get; set; }
        [Display(Name = "用户Id")]
        public string UserId { get; set; }
        [Display(Name = "目标Id")]
        public string ObjectId { get; set; }
        [Display(Name = "Ip")]
        public string Ip { get; set; }
        [Display(Name = "Cookie")]
        public string Cookie { get; set; }
        [Display(Name = "操作时间")]
        public DateTime OperationTime { get; set; }
    }

    public class OperationRecordsPagesInfor
    {
        public Search.QueryPageOptions Options { get; set; }
        public ListOperationRecordAloneModel SearchModel { get; set; }

        public string Text { get; set; }
    }
}
