using CnGalWebSite.Kanban.ChatGPT.Models.GPT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.Kanban.ChatGPT.Services.ChatGPTService
{
    public interface IChatGPTService
    {
        Task<ChatGPTSendMessageResult> SendMessages(List<ChatCompletionMessage> messages);
    }
}
