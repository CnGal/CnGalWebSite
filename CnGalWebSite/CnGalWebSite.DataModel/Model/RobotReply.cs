using System;
using System.Collections.Generic;
using System.Text;

namespace CnGalWebSite.DataModel.Model
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

        public bool IsHidden { get; set; }

    }
}
