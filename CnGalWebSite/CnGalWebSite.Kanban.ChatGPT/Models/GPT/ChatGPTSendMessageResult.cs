using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.Kanban.ChatGPT.Models.GPT
{
    public class ChatGPTSendMessageResult
    {
        public bool Success { get; set; }

        public string? Message { get; set; }
    }
}
