using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.RobotClientX.Models.GPT
{
    public class ChatCompletionModel
    {
        public string Model { get; set; } = "gpt-3.5-turbo";
        public List<ChatCompletionMessage> Messages { get; set; }=new List<ChatCompletionMessage>();
    }

    public class ChatCompletionMessage
    {
        public string Role { get; set; }
        public string Content { get; set; }
    }
}
