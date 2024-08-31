using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.Kanban.ChatGPT.Models.GPT
{
    public class ChatResult
    {
        public string? Id { get; set; }
        public string? Object { get; set; }
        public long Created { get; set; }
        public string? Model { get; set; }
        public ChatResultUsage Usage { get; set; } = new ChatResultUsage();
        public List<ChatResultChoice> Choices { get; set; } = new List<ChatResultChoice>();
    }

    public class ChatResultUsage
    {
        public long Prompt_tokens { get; set; }
        public long Completion_tokens { get; set; }
        public long Total_tokens { get; set; }
    }
    public class ChatResultChoice
    {
        public ChatResultChoiceMessage Message { get; set; } = new ChatResultChoiceMessage();
        public string? Finish_reason { get; set; }
        public int Index { get; set; }
    }
    public class ChatResultChoiceMessage
    {
        public string? Role { get; set; }
        public string? Content { get; set; }
    }
}
