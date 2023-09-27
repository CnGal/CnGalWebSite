using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.Kanban.Models
{
    public class DialogBoxModel
    {
        public string Content { get; set; }

        public string Expression { get; set; }

        public string MotionGroup { get; set; }

        public int Motion { get; set; } = -1;

        /// <summary>
        /// 清空队列 显示当前消息
        /// </summary>
        public int Priority { get; set; }

        public DialogBoxType Type { get; set; }
    }

    public enum DialogBoxType
    {
        Text,
        Success,
        Info,
        Warning,
        Error
    }
}
