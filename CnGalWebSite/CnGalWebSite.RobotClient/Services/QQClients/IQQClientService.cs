using CnGalWebSite.DataModel.Model;
using CnGalWebSite.RobotClient.DataModels.Messages;
using MeowMiraiLib;
using MeowMiraiLib.Msg.Sender;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.RobotClient.Services.QQClients
{
    public interface IQQClientService
    {

         Task SendMessage(SendMessageModel model, Masuda.Net.Models.Message msg = null);

        Task ReplyFromGroupAsync(GroupMessageSender s, MeowMiraiLib.Msg.Type.Message[] e);

        Task ReplyFromFriendAsync(FriendMessageSender s, MeowMiraiLib.Msg.Type.Message[] e);

        Task SendMessage(RobotReplyRange range, long id, string text, Masuda.Net.Models.Message msg = null);
    }
}
