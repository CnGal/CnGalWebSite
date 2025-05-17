
using CnGalWebSite.RobotClientX.Models.Robots;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnifyBot.Message.Chain;

namespace CnGalWebSite.RobotClientX.DataModels
{
    public class SendMessageModel
    {
        public string Text;

        public RobotReplyRange Range { get; set; }

        public long SendTo { get; set; }

        public MessageChain Messages { get; set; }
    }
}
