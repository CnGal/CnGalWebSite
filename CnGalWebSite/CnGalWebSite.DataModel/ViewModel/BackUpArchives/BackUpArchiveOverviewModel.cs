
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace CnGalWebSite.DataModel.ViewModel.BackUpArchives
{

    public class BackUpArchiveOverviewModel
    {
        public long Id { get; set; }
        /// <summary>
        /// 上次备份时间
        /// </summary>
        public DateTime LastBackUpTime { get; set; }

        /// <summary>
        /// 上次备份用时
        /// </summary>
        public double LastTimeUsed { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        public BackUpArchiveType Type { get; set; }

        /// <summary>
        /// 目标Id
        /// </summary>
        public long ObjectId { get; set; }
        /// <summary>
        /// 目标名称
        /// </summary>
        public string ObjectName { get; set; }

        /// <summary>
        /// 上次是否失败
        /// </summary>
        public bool IsLastFail { get; set; }
    }

    public enum BackUpArchiveType
    {
        [Display(Name ="词条")]
        Entry,
        [Display(Name = "文章")]
        Article
    }
}
