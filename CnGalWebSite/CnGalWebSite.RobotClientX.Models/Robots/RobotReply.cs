﻿using System;
using System.ComponentModel.DataAnnotations;

namespace CnGalWebSite.RobotClientX.Models.Robots
{
    public class RobotReply
    {
        public long Id { get; set; }

        public string Key { get; set; }

        public string Value { get; set; }

        public DateTime UpdateTime { get; set; }

        /// <summary>
        /// 在这个时间之后才回复
        /// </summary>
        public DateTime AfterTime { get; set; }
        /// <summary>
        /// 在这个时间之前才回复
        /// </summary>
        public DateTime BeforeTime { get; set; }

        /// <summary>
        /// 优先级
        /// </summary>
        public int Priority { get; set; }

        public RobotReplyRange Range { get; set; }

        public bool IsHidden { get; set; }
    }

    public enum RobotReplyRange
    {
        [Display(Name ="全部")]
        All,
        [Display(Name = "群聊")]
        Group,
        [Display(Name = "私聊")]
        Friend,
        [Display(Name = "频道")]
        Channel,
    }
}
