
using CnGalWebSite.RobotClientX.DataModels;
using CnGalWebSite.RobotClientX.Models.Messages;
using CnGalWebSite.RobotClientX.Models.Robots;
using UnifyBot.Message.Chain;

namespace CnGalWebSite.RobotClientX.Services.Messages
{
    public interface IMessageService
    {
        Task<RobotReply> GetAutoReply(string message, RobotReplyRange range);

        Task<SendMessageModel> ProcMessageAsync(RobotReplyRange range, string reply, string message, string regex, long qq, string name, long sendto);

        MessageChain ProcMessageToOneBot(string vaule);
    }
}
