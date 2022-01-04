using System.ComponentModel.DataAnnotations;

using System;
 using System.Collections.Generic;
namespace CnGalWebSite.DataModel.Model
{
    public class TimedTask
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public TimedTaskType Type { get; set; }

        public TimedTaskExecuteType ExecuteType { get; set; }

        /// <summary>
        /// 间隔时间 单位 分钟
        /// </summary>
        public long IntervalTime { get; set; }
        /// <summary>
        /// 另一种定时方式 每天的固定时间
        /// </summary>
        public DateTime? EveryTime { get; set; }

        public DateTime? LastExecutedTime { get; set; }

        public string Parameter { get; set; }
        /// <summary>
        /// 是否暂停执行
        /// </summary>
        public bool IsPause { get; set; }
        /// <summary>
        /// 是否正在执行
        /// </summary>
        public bool IsRuning { get; set; }
        /// <summary>
        /// 上次是否失败
        /// </summary>
        public bool IsLastFail { get; set; }

    }

    public enum TimedTaskType
    {
        [Display(Name = "更新所有游戏Steam信息")]
        UpdateGameSteamInfor,
        [Display(Name = "更新所有用户Steam信息")]
        UpdateUserSteamInfor,
        [Display(Name = "备份词条")]
        BackupEntry,
        [Display(Name = "备份文章")]
        BackupArticle,
        [Display(Name = "更新数据汇总")]
        UpdateDataSummary,
        [Display(Name = "更新网站地图")]
        UpdateSitemap,
        [Display(Name = "更新词条完善度检查的全站统计数据")]
        UpdatePerfectionCountAndVictoryPercentage,
        [Display(Name = "更新词条完善度全站概览数据")]
        UpdatePerfectionOverview,
        [Display(Name = "更新词条完善度")]
        UpdatePerfection,
        [Display(Name = "更新Elasticsearch数据缓存")]
        UpdateDataToElasticsearch,
        [Display(Name = "抓取RSS源动态")]
        UpdateGameNews,
        [Display(Name = "更新微博用户信息缓存")]
        UpdateWeiboUserInfor
    }
    public enum TimedTaskExecuteType
    {
        [Display(Name = "间隔固定时间运行")]
        IntervalTime,
        [Display(Name = "每天运行")]
        EveryDay
    }
}
