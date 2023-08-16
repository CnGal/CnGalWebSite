
using CnGalWebSite.RobotClientX.DataModels;
using CnGalWebSite.RobotClientX.Models.Messages;
using CnGalWebSite.RobotClientX.Models.Robots;
using MeowMiraiLib;
using MeowMiraiLib.Msg.Sender;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.RobotClientX.Services.QQClients
{
    public interface IQQClientService
    {
        Task SendMessage(SendMessageModel model, Masuda.Net.Models.Message msg = null);

        Task ReplyFromGroupAsync(GroupMessageSender s, MeowMiraiLib.Msg.Type.Message[] e);

        Task ReplyFromFriendAsync(FriendMessageSender s, MeowMiraiLib.Msg.Type.Message[] e);

        Task SendMessage(RobotReplyRange range, long id, string text, Masuda.Net.Models.Message msg = null);

        Task Init();

        string GetMiraiSession();
    }
}
