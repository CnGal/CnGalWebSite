
using CnGalWebSite.DataModel.Model;
using Masuda.Net.HelpMessage;
using MeowMiraiLib.Msg.Type;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Message = MeowMiraiLib.Msg.Type.Message;

namespace CnGalWebSite.RobotClient.DataModels.Messages
{
    public class SendMessageModel
    {
        public string Text;

        public RobotReplyRange Range { get; set; }

        public long SendTo { get; set; }

        public Message[] MiraiMessage { get; set; } = Array.Empty<Message>();

        public MessageBase[] MasudaMessage { get; set; }
    }
}
