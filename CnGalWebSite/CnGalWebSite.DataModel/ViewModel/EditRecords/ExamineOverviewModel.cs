
using CnGalWebSite.DataModel.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace CnGalWebSite.DataModel.ViewModel.Admin
{

    public class ExamineOverviewModel
    {
        public long Id { get; set; }
        /// <summary>
        /// 操作
        /// </summary>
        public Operation Operation { get; set; }

        /// <summary>
        /// 是否通过审核
        /// </summary>
        public bool? IsPassed { get; set; }
        /// <summary>
        /// 审核时间
        /// </summary>
        public DateTime? PassedTime { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime ApplyTime { get; set; }
        /// <summary>
        /// 批注
        /// </summary>
        public string Comments { get; set; }
        /// <summary>
        /// 申请审核用户Id
        /// </summary>
        public string ApplicationUserId { get; set; }
        /// <summary>
        /// 申请审核的用户
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 处理此审核的用户
        /// </summary>
        public string PassedAdminName { get; set; }
    }
}
