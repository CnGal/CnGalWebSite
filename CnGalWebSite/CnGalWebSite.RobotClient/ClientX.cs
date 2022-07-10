using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using MeowMiraiLib.Msg;
using MeowMiraiLib.Msg.Sender;
using MeowMiraiLib.Msg.Type;
using Masuda.Net;
using Masuda.Net.HelpMessage;

namespace CnGalWebSite.RobotClient
{
    public class ClientX
    {
        private readonly Setting _setting;
        private readonly MessageX _messageX;
        private readonly GroupX _groupX;
        public MeowMiraiLib.Client MiraiClient { get; set; }
        public MasudaBot MasudaClient { get; set; }
        private readonly List<PostLog> _post = new();
        private readonly List<MessageArg> _messageArgs;

        public ClientX(Setting setting, MessageX messageX, GroupX groupX, List<MessageArg> messageArgs)
        {
            _setting = setting;
            _messageX = messageX;
            _groupX = groupX;
            _messageArgs = messageArgs;
        }

        public void InitMirai()
        {
            MiraiClient = new($"ws://{_setting.MiraiUrl}/all?verifyKey={_setting.NormalVerifyKey}&qq={_setting.QQ}", true, true, -1);
        }
        public void InitMasuda()
        {
            MasudaClient = new(_setting.ChannelAppId, _setting.ChannelAppKey, _setting.ChannelToken, BotType.Private);
        }
        /// <summary>
        /// 回复好友消息
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public async Task ReplyFromFriendAsync(FriendMessageSender s, MeowMiraiLib.Msg.Type.Message[] e)
        {


            await ReplyMessageAsync(RobotReplyRange.Friend, ConversionMeaasge(e), s.id, s.id, s.nickname);
        }

        public async Task ReplyFromChannelAsync(Masuda.Net.Models.Message e)
        {
            ////过滤非水区的发言
            //if ((await MasudaClient.GetChannelAsync( e.ChannelId)).Name.Contains("水区"))
            //{

            //}
            var messgae = MessageHelper.GetPureMessage(e.Content);
            if(string.IsNullOrWhiteSpace(messgae))
            {
                return;
            }
            await ReplyMessageAsync(RobotReplyRange.Channel, MessageHelper.GetPureMessage(e.Content),0,0,e.Author.Username,e);

        }

        public async Task ReplyFromGroupAsync(GroupMessageSender s, MeowMiraiLib.Msg.Type.Message[] e)
        {
            //忽略未关注的群聊
            var group = _groupX.Groups.FirstOrDefault(x => x.GroupId == s.group.id);
            if (group == null)
            {
                return;
            }
            //检查是否符合强制匹配
            var message = ConversionMeaasge(e);
            if (group.ForceMatch)
            {
                var name = _messageArgs.FirstOrDefault();
                if (name != null && message.Contains(name.Value) == false)
                {
                    return;
                }
            }
            await ReplyMessageAsync(RobotReplyRange.Group, message, s.group.id, s.id, s.memberName);
        }

        private string ConversionMeaasge(MeowMiraiLib.Msg.Type.Message[] e)
        {
            var message = e.MGetPlainString();
            var at = e.FirstOrDefault(s => s.type == "At");
            if (at != null)
            {
                var atTarget = (at as At).target;
                message += "[@" + atTarget.ToString() + "]";
            }
            return message;
        }

        /// <summary>
        /// 回复消息
        /// </summary>
        private async Task ReplyMessageAsync(RobotReplyRange range, string message, long sendto, long memberId, string memberName, Masuda.Net.Models.Message e=null)
        {
            //尝试找出所有匹配的回复
            var reply = _messageX.GetAutoReply(message, range);

            if (reply == null)
            {
                return;
            }
            var result = new SendMessageModel();


                try
                {
                    //处理消息
                    result = await _messageX.ProcMessageAsync(range, reply.Value, message, reply.Key, memberId, memberName);
                }
                catch (ArgError ae)
                {
                    if (_setting.WarningQQGroup > 0)
                    {
                        //发送警告
                        var j = await new GroupMessage(_setting.WarningQQGroup, new MeowMiraiLib.Msg.Type.Message[] { new Plain(ae.Error) }).SendAsync(MiraiClient);
                        Console.WriteLine(j);
                    }
                }


            if (result != null)
            {
                //检查上限
                if (await CheckLimit(range, memberId, memberName) == false)
                {
                    //发送消息
                    result.SendTo = sendto;
                   await SendMessage(result, e);
                }
            }
            //添加发送记录
            _post.Add(new PostLog
            {
                Message = message,
                PostTime = DateTime.Now.ToCstTime(),
                QQ = memberId,
                Reply = reply.Value
            });
        }

        /// <summary>
        /// 检查是否超过限制
        /// </summary>
        /// <param name="sendto"></param>
        /// <param name="memberId"></param>
        /// <param name="memberName"></param>
        /// <returns>是否超过限制</returns>
        private async Task<bool> CheckLimit(RobotReplyRange range,long memberId, string memberName, Masuda.Net.Models.Message msg = null)
        {
            //判断该用户是否连续10次互动 1分钟内
            var singleCount = _post.Count(x => (DateTime.Now.ToCstTime() - x.PostTime).TotalMinutes <= 1 && x.QQ == memberId);
            var totalCount = _post.Count(x => (DateTime.Now.ToCstTime() - x.PostTime).TotalMinutes <= 1);


            if (singleCount == _setting.SingleLimit)
            {
                var result = await _messageX.ProcMessageAsync(range, $"[黑化微笑][@{memberId}]如果恶意骚扰人家的话，我会请你离开哦…", null, null, memberId, memberName);
                await SendMessage(result, msg);
                return true;
            }
            else if (singleCount > _setting.SingleLimit)
            {
                return true;
            }

            if (singleCount == _setting.TotalLimit)
            {
                var result = await _messageX.ProcMessageAsync(range, $"核心温度过高，正在冷却......", null, null, memberId, memberName);
                await SendMessage(result, msg);
                return true;
            }
            else if (singleCount > _setting.TotalLimit)
            {
                return true;
            }

            return false;
        }

        public async Task SendMessage(SendMessageModel model, Masuda.Net.Models.Message msg = null)
        {
            if (model.Range == RobotReplyRange.Channel)
            {
                await MasudaClient.ReplyMessageAsync(msg, model.MasudaMessage);
            }
            else if (model.Range == RobotReplyRange.Friend)
            {
                var j = model.MiraiMessage.SendToFriend(model.SendTo, MiraiClient);
                Console.WriteLine(j);
            }
            else
            {
                var j = model.MiraiMessage.SendToGroup(model.SendTo, MiraiClient);
                Console.WriteLine(j);
            }
        }
    }



    public class PostLog
    {
        public long QQ { get; set; }

        public string Message { get; set; }

        public string Reply { get; set; }

        public DateTime PostTime { get; set; }
    }
}
