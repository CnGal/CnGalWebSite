using System;
using System.ComponentModel.DataAnnotations;

namespace CnGalWebSite.DataModel.Model
{
    public class RobotEvent
    {
        public long Id { get; set; }

        /// <summary>
        /// 特指时间 即每日执行的时间
        /// </summary>
        public DateTime Time { get; set; }

        public long DelaySecond { get; set; }

        /// <summary>
        /// 每分钟被触发的概率
        /// </summary>
        public double Probability { get; set; }

        public RobotEventType Type { get; set; }

        public string Note { get; set; }

        public string Text { get; set; }

        public bool IsHidden { get; set; }
    }

    public enum RobotEventType
    {
        [Display(Name = "固定时间")]
        FixedTime,
        [Display(Name = "概率触发")]
        PreTime
    }
}
