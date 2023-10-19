using CnGalWebSite.ProjectSite.Models.DataModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CnGalWebSite.ProjectSite.Models.DataModels
{
    public class OperationRecord
    {
        public long Id { get; set; }

        public OperationRecordType Type { get; set; }

        public PageType PageType { get; set; }

        public long PageId { get; set; }

        public long ObjectId;

        public string IP { get; set; }

        public string Cookie { get; set; }

        public string UA { get; set; }

        public DateTime Time { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

    }

    public enum OperationRecordType
    {
        [Display(Name = "编辑企划")]
        EditProject,
        [Display(Name = "编辑橱窗")]
        EditStall,
        [Display(Name = "编辑个人信息")]
        EditUser,
        [Display(Name = "企划征应")]
        ApplyProject,
        [Display(Name = "橱窗邀请")]
        ApplyStall,
        [Display(Name = "处理企划应征")]
        ProcProject,
        [Display(Name = "处理橱窗邀请")]
        ProcStall,
    }
}
