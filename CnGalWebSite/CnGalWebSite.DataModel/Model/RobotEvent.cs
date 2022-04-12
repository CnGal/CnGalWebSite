using System;
using System.Collections.Generic;
using System.Text;

namespace CnGalWebSite.DataModel.Model
{
    public class RobotEvent
    {
        public long Id { get; set; }

        /// <summary>
        /// 特指时间 即每日执行的时间
        /// </summary>
        public DateTime Time { get; set; }

        public string Text { get; set; }

        public bool IsHidden { get; set; }
    }
}
