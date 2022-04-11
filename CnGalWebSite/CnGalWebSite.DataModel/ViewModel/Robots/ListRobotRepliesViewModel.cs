using CnGalWebSite.DataModel.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Xml.Linq;

namespace CnGalWebSite.DataModel.ViewModel.Robots
{

    public class ListRobotsInforViewModel
    {
        public int Events { get; set; }
        public int Replies { get; set; }
        public int Groups { get; set; }
    }

    public class ListRobotRepliesViewModel
    {
        public List<ListRobotReplyAloneModel> RobotReplies { get; set; }
    }
    public class ListRobotReplyAloneModel
    {
        [Display(Name = "Id")]
        public long Id { get; set; }
        [Display(Name = "匹配表达式")]
        public string Key { get; set; }
        [Display(Name = "回复")]
        public string Value { get; set; }
        /// <summary>
        /// 在这个时间之后才回复
        /// </summary>
        [Display(Name = "时间之后")]
        public DateTime AfterTime { get; set; }
        /// <summary>
        /// 在这个时间之前才回复
        /// </summary>
        [Display(Name = "时间之前")]
        public DateTime BeforeTime { get; set; }

        [Display(Name = "更新时间")]
        public DateTime UpdateTime { get; set; }
        [Display(Name = "是否隐藏")]
        public bool IsHidden { get; set; }
    }

    public class RobotRepliesPagesInfor
    {
        public Search.QueryPageOptions Options { get; set; }
        public ListRobotReplyAloneModel SearchModel { get; set; }

        public string Text { get; set; }
    }
}
