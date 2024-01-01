
using CnGalWebSite.TimedTask.Models.DataModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.TimedTask.Models.ViewModels
{
    public class TimedTaskOverviewModel
    {
        public int Id { get; set; }
        /// <summary>
        /// 任务类型
        /// </summary>
        public TimedTaskType Type { get; set; }
        /// <summary>
        /// 执行时间类型
        /// </summary>
        public TimedTaskExecuteType ExecuteType { get; set; }

        /// <summary>
        /// 间隔时间(分钟)
        /// </summary>
        public long IntervalTime { get; set; }
        /// <summary>
        /// 固定时间
        /// </summary>
        public DateTime? EveryTime { get; set; }
        /// <summary>
        /// 上次执行时间
        /// </summary>
        public DateTime? LastExecutedTime { get; set; }
        /// <summary>
        /// 参数
        /// </summary>
        public string Parameter { get; set; }
        /// <summary>
        /// 是否暂停执行
        /// </summary>
        public bool IsPause { get; set; } = false;
        /// <summary>
        /// 是否正在执行
        /// </summary>
        public bool IsRuning { get; set; } = false;
        /// <summary>
        /// 上次是否失败
        /// </summary>
        public bool IsLastFail { get; set; } = false;
    }
    public class TimedTaskEditModel : TimedTaskOverviewModel
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
    }
}
