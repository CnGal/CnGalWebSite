using CnGalWebSite.Kanban.ChatGPT.Models.GPT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.Kanban.ChatGPT.Services.KanbanService
{
    public interface IKanbanService
    {
        Task<ChatGPTSendMessageResult> GetReply(string message, string userId, bool first, int messageMax);
    }
}
