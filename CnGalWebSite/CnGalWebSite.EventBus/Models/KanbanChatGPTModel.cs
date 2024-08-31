using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.EventBus.Models
{
    public class KanbanChatGPTSendModel
    {
        public string Message { get; set; }

        public bool IsFirst { get; set; }

        public string UserId { get; set; }

        /// <summary>
        /// 单次对话上限
        /// </summary>
        public int MessageMax { get; set; }
    }

    public class KanbanChatGPTReceiveModel
    {
        public string Message { get; set; }

        public bool Success {  get; set; }
    }
}
