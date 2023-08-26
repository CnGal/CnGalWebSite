using CnGalWebSite.DataModel.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Xml.Linq;

namespace CnGalWebSite.DataModel.ViewModel.EditRecords
{
    public class UserReviewEditRecordOverviewModel
    {
        
        public long ExamineId { get; set; }
        /// <summary>
        /// 操作
        /// </summary>
        public Operation Operation { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string EntryName { get; set; }
        /// <summary>
        /// 词条Id
        /// </summary>
        public int EntryId { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public EditRecordReviewState State { get; set; }
        /// <summary>
        /// 编辑者Id
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        /// 编辑者
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 审阅时间
        /// </summary>
        public DateTime? ReviewedTime { get; set; }
    }
}
