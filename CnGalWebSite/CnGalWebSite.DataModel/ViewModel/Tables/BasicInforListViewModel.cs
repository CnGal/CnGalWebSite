using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace CnGalWebSite.DataModel.ViewModel.Tables
{
    public class BasicInforTableModel
    {
        public long Id { get; set; }
        [Display(Name = "真实Id")]
        public long RealId { get; set; }
        [Display(Name = "名称")]
        public string Name { get; set; }
        [Display(Name = "发行时间")]
        public DateTime? IssueTime { get; set; }
        [Display(Name = "原作")]
        public string Original { get; set; }
        [Display(Name = "制作组")]
        public string ProductionGroup { get; set; }
        [Display(Name = "游戏平台")]
        public string GamePlatforms { get; set; }
        [Display(Name = "引擎")]
        public string Engine { get; set; }
        [Display(Name = "发行商")]
        public string Publisher { get; set; }
        [Display(Name = "游戏别称")]
        public string GameNickname { get; set; }
        [Display(Name = "标签")]
        public string Tags { get; set; }
        [Display(Name = "发行方式")]
        public string IssueMethod { get; set; }
        [Display(Name = "官网")]
        public string OfficialWebsite { get; set; }
        [Display(Name = "Steam平台Id")]
        public string SteamId { get; set; }
        [Display(Name = "QQ群")]
        public string QQgroupGame { get; set; }
    }
}
