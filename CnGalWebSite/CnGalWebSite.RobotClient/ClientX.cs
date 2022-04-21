using CnGalWebSite.DataModel.Helper;
using MeowMiraiLib.Msg;
using MeowMiraiLib.Msg.Sender;
using MeowMiraiLib.Msg.Type;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.RobotClient
{
    public class ClientX
    {
        private readonly Setting _setting;
        private readonly MessageX _messageX;
        private readonly GroupX _groupX;
        public MeowMiraiLib.Client MiraiClient { get; set; }
        private readonly List<PostLog> _post = new List<PostLog>();

        public ClientX(Setting setting, MessageX messageX, GroupX groupX)
        {
            _setting = setting;
            _messageX = messageX;
            _groupX = groupX;
        }

        public void Init()
        {
            MiraiClient = new($"ws://{_setting.MiraiUrl}/all?verifyKey={_setting.VerifyKey}&qq={_setting.QQ}", true, true, -1);
        }

        public async Task ReplyFromGroupAsync(GroupMessageSender s, Message[] e)
        {
            var message = e.MGetPlainString();
            var sendto = s.group.id;
            var at = e.FirstOrDefault(s => s.type == "At");
            if (at != null)
            {
                long atTarget = (at as At).target;
                message += "[@" + atTarget.ToString() + "]";
            }

            //忽略未关注的群聊
            if (_groupX.Groups.Any(s => s.GroupId == sendto) == false)
            {
                return;
            }



            //尝试找出所有匹配的回复
            var reply = _messageX.GetAutoReply(message, s);

            var result = await _messageX.ProcMessageAsync(reply.Value, message,reply.Key, s);

            if (result != null)
            {

                //判断该用户是否连续10次互动 1分钟内
                var singleCount = _post.Count(x => (DateTime.Now.ToCstTime() - x.PostTime).TotalMinutes <= 1 && x.QQ == s.id);
                var totalCount = _post.Count(x => (DateTime.Now.ToCstTime() - x.PostTime).TotalMinutes <= 1);

                if (singleCount == _setting.SingleLimit)
                {
                    result = await _messageX.ProcMessageAsync($"[黑化微笑][@{s.id}]如果恶意骚扰人家的话，我会请你离开哦…",null, null, s);
                }
                else if (singleCount > _setting.SingleLimit)
                {
                    return;
                }

                if (singleCount == _setting.TotalLimit)
                {
                    result = await _messageX.ProcMessageAsync($"核心温度过高，正在冷却......",null, null, s);
                }
                else if (singleCount > _setting.TotalLimit)
                {
                    return;
                }

                _post.Add(new PostLog
                {
                    Message = message,
                    PostTime = DateTime.Now.ToCstTime(),
                    QQ = s.id,
                    Reply = reply.Value
                });

                var j =await new GroupMessage(sendto, result).SendAsync(MiraiClient);
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
