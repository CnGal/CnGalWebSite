using CnGalWebSite.DataModel.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CnGalWebSite.DataModel.ViewModel.Robots
{
    public class ListRobotEventsViewModel
    {
        public List<ListRobotEventAloneModel> RobotEvents { get; set; }
    }
    public class ListRobotEventAloneModel
    {
        [Display(Name = "Id")]
        public long Id { get; set; }
        [Display(Name = "消息")]
        public string Text { get; set; }
        [Display(Name = "备注")]
        public string Note { get; set; }
        [Display(Name = "类型")]
        public RobotEventType Type { get; set; }
        [Display(Name = "定时")]
        public DateTime Time { get; set; }
        [Display(Name = "有效期间 秒")]
        public long DelaySecond { get; set; }
        [Display(Name = "概率")]
        public double Probability { get; set; }
        [Display(Name = "是否隐藏")]
        public bool IsHidden { get; set; }
    }

    public class RobotEventsPagesInfor
    {
        public Search.QueryPageOptions Options { get; set; }
        public ListRobotEventAloneModel SearchModel { get; set; }

        public string Text { get; set; }
    }
}
