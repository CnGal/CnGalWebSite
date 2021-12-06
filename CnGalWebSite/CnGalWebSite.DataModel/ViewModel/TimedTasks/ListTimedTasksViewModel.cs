using BootstrapBlazor.Components;
using CnGalWebSite.DataModel.Model;
using System.ComponentModel.DataAnnotations;

namespace CnGalWebSite.DataModel.ViewModel.TimedTasks
{
    public class ListTimedTasksInforViewModel
    {
        public int IsRuning { get; set; }
        public int IsPasue { get; set; }
        public int IsLastFail { get; set; }
        public int All { get; set; }
    }

    public class ListTimedTasksViewModel
    {
        public List<ListTimedTaskAloneModel> TimedTasks { get; set; }
    }
    public class ListTimedTaskAloneModel
    {
        [Display(Name = "Id")]
        public int Id { get; set; }
        [Display(Name = "备注")]
        public string Name { get; set; }
        [Display(Name = "任务类型")]
        public TimedTaskType? Type { get; set; }
        [Display(Name = "执行时间类型")]
        public TimedTaskExecuteType? ExecuteType { get; set; }

        [Display(Name = "间隔时间(分钟)")]
        public long IntervalTime { get; set; }
        [Display(Name = "固定时间")]
        public string EveryTime { get; set; }
        [Display(Name = "上次执行时间")]
        public DateTime? LastExecutedTime { get; set; }
        [Display(Name = "参数")]
        public string Parameter { get; set; }
        [Display(Name = "是否暂停执行")]
        public bool IsPause { get; set; } = false;
        [Display(Name = "是否正在执行")]
        public bool IsRuning { get; set; } = false;
        [Display(Name = "上次是否失败")]
        public bool IsLastFail { get; set; } = false;
    }

    public class TimedTasksPagesInfor
    {
        public QueryPageOptions Options { get; set; }
        public ListTimedTaskAloneModel SearchModel { get; set; }

        public string Text { get; set; }
    }
}
