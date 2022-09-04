using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.OperationRecords;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CnGalWebSite.DataModel.ViewModel.Admin
{
    public class ListUserCertificationAloneModel
    {
        [Display(Name = "Id")]
        public long Id { get; set; }

        [Display(Name = "用户名")]
        public string UserName { get; set; }
        [Display(Name = "用户Id")]
        public string UserId { get; set; }
        [Display(Name = "词条名称")]
        public string EntryName { get; set; }
        [Display(Name = "词条Id")]
        public int EntryId { get; set; }
        [Display(Name = "认证时间")]
        public DateTime CertificationTime { get; set; }
    }

    public class UserCertificationsPagesInfor
    {
        public Search.QueryPageOptions Options { get; set; }
        public ListUserCertificationAloneModel SearchModel { get; set; }

        public string Text { get; set; }
    }
}
