using System;
using System.Collections.Generic;
using System.Text;

namespace CnGalWebSite.PublicToolbox.Models
{
    public class ToolTaskBase
    {
        public int TotalTaskCount { get; set; } 
        public int CompleteTaskCount { get; set; }

        public string Error { get; set; }

        /// <summary>
        /// 任务处理时间
        /// </summary>
        public DateTime? PostTime { get; set; }
    }
}
