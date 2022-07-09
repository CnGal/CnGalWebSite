using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Robots;
using CnGalWebSite.Helper.Extensions;
using CnGalWebSite.Helper.Helper;
using Masuda.Net.HelpMessage;
using MeowMiraiLib.Msg.Sender;
using MeowMiraiLib.Msg.Type;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using Message = MeowMiraiLib.Msg.Type.Message;

namespace CnGalWebSite.RobotClient
{
    public class MessageX
    {
        private readonly Setting _setting;
        private readonly SensitiveWordX _SensitiveWordX;
        private readonly IDictionary<string, string> _cache;
        private readonly HttpClient _httpClient;
        private readonly List<RobotReply> _replies;
        private readonly List<MessageArg> _messageArgs;
        private readonly List<RobotFace> _robotFaces;


        public MessageX(Setting setting, HttpClient client, List<RobotReply> replies, IDictionary<string, string> cache, List<MessageArg> messageArgs, List<RobotFace> robotFaces, SensitiveWordX SensitiveWordX)
        {
            _setting = setting;
            _httpClient = client;
            _replies = replies;
            _cache = cache;
            _messageArgs = messageArgs;
            _robotFaces = robotFaces;
            _SensitiveWordX = SensitiveWordX;
        }

        /// <summary>
        /// 获取可能的回复
        /// </summary>
        /// <param name="message"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public RobotReply GetAutoReply(string message, RobotReplyRange range)
        {

            var now = DateTime.Now.ToCstTime();
            var replies = _replies.Where(s => s.IsHidden == false && (s.Range == RobotReplyRange.All || s.Range == range) && now.TimeOfDay <= s.BeforeTime.TimeOfDay && now.TimeOfDay >= s.AfterTime.TimeOfDay && Regex.IsMatch(message, s.Key))
                .GroupBy(s => s.Priority)
                .OrderByDescending(s => s.Key)
                .ToList();

            if (replies.Count == 0)
            {
                return null;
            }

            var index = new Random().Next(0, replies.FirstOrDefault().Count());
            var reply = replies.FirstOrDefault().ToList()[index];

            //检查是否含有变量替换 如果有 则检查输入是否包含敏感词
            if (reply.Value.Contains('$'))
            {
                var words = _SensitiveWordX.Check(message);

                if (words.Count != 0)
                {
                    return new RobotReply
                    {
                        Value = _messageArgs.FirstOrDefault(s => s.Name == "SensitiveReply")?.Value ?? "看板娘不知道哦~",
                    };
                }
            }

            return reply;
        }

        /// <summary>
        /// 处理消息回复
        /// </summary>
        /// <param name="reply"></param>
        /// <param name="message"></param>
        /// <param name="regex"></param>
        /// <param name="qq"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="ArgError"></exception>
        public async Task<SendMessageModel> ProcMessageAsync(RobotReplyRange range,string reply, string message, string regex,long qq,string name)
        {

            var args = new List<KeyValuePair<string, string>>();
            try
            {
                await ProcMessageArgument(reply, message, qq,name, args);
                ProcMessageReplaceInput(reply, message, regex, args);
                ProcMessageFace(reply, args);
            }
            catch (ArgError arg)
            {
                reply = arg.Error;
            }
            catch (Exception ex)
            {
                OutputHelper.PressError(ex, "获取变量值失败");
                reply = "呜呜呜~";
            }


            //检测敏感词
            var words = _SensitiveWordX.Check(args.Where(s => s.Key == "sender" || (s.Key.Contains('[') && s.Key.Contains(']'))).Select(s => s.Value).ToList());

            if (words.Count != 0)
            {
                var msg = $"对{name}({qq})的消息回复中包含敏感词\n消息：{message}\n回复：{reply}\n\n参数替换列表：\n";
                foreach (var item in args)
                {
                    msg += $"{item.Key} -> {item.Value}\n";

                }
                msg += $"\n触发的敏感词：\n";
                foreach (var item in words)
                {
                    msg += $"{item}\n";
                }

                OutputHelper.Write(OutputLevel.Dager, msg);

                throw new ArgError(msg);
            }

            //替换参数
            foreach (var item in args)
            {
                reply = reply.Replace(item.Key, item.Value);
            }

            if(range== RobotReplyRange.Channel)
            {
                return new SendMessageModel
                {
                    MasudaMessage = ProcMessageToMasuda(reply),
                    Range = range
                };
            }
            else
            {

                return new SendMessageModel
                {
                    MiraiMessage = ProcMessageToMirai(reply),
                    Range = range
                };
            }
        }

        /// <summary>
        /// 将纯文本回复转换成可发送的消息数组
        /// </summary>
        /// <param name="vaule"></param>
        /// <returns></returns>
        public Message[] ProcMessageToMirai(string vaule)
        {
            if (string.IsNullOrWhiteSpace(vaule))
            {
                return null;
            }

            var messages = new List<Message>();

            while (true)
            {
                if (vaule.Contains("[image="))
                {
                    var imageStr = vaule.MidStrEx("[image=", "]");

                    if (string.IsNullOrWhiteSpace(imageStr) == false)
                    {
                        vaule = vaule.Replace("[image=" + imageStr + "]", "");
                        //修正一部分图片链接缺省协议
                        if(imageStr.Contains("http")==false)
                        {
                            imageStr = "https:" + imageStr;
                        }
                        messages.Add(new Image(url: imageStr.Replace("http://image.cngal.org/", "https://image.cngal.org/")));
                    }
                }
                else if (vaule.Contains("[声音="))
                {
                    var voiceStr = vaule.MidStrEx("[声音=", "]");

                    if (string.IsNullOrWhiteSpace(voiceStr) == false)
                    {
                        vaule = vaule.Replace("[声音=" + voiceStr + "]", "");
                        messages.Add(new Voice(url: voiceStr.Replace("http://res.cngal.org/", "https://res.cngal.org/")));

                    }
                }

                else if (vaule.Contains("[@"))
                {
                    var idStr = vaule.MidStrEx("[@", "]");
                    if (long.TryParse(idStr, out var id))
                    {

                        vaule = vaule.Replace("[@" + idStr + "]", "");
                        messages.Add(new At(id, idStr));
                    }
                }
                else
                {
                    break;
                }


            }


            if (string.IsNullOrWhiteSpace(vaule) == false)
            {
                messages.Add(new Plain(vaule));
            }

            if (string.IsNullOrWhiteSpace(vaule) && messages.Count == 0)
            {
                return null;
            }
            else
            {
                return messages.ToArray();
            }
        }
        /// <summary>
        /// 将纯文本回复转换成可发送的消息数组
        /// </summary>
        /// <param name="vaule"></param>
        /// <returns></returns>
        public MessageBase[] ProcMessageToMasuda(string vaule)
        {
            if (string.IsNullOrWhiteSpace(vaule))
            {
                return null;
            }

            var messages = new List<MessageBase>();

            while (true)
            {
                if (vaule.Contains("[image="))
                {
                    var imageStr = vaule.MidStrEx("[image=", "]");

                    if (string.IsNullOrWhiteSpace(imageStr) == false)
                    {
                        vaule = vaule.Replace("[image=" + imageStr + "]", "");
                        //修正一部分图片链接缺省协议
                        if (imageStr.Contains("http") == false)
                        {
                            imageStr = "https:" + imageStr;
                        }
                        messages.Add(new ImageMessage(url: imageStr.Replace("http://image.cngal.org/", "https://image.cngal.org/")));
                    }
                }
                else if (vaule.Contains("[声音="))
                {
                    //var voiceStr = vaule.MidStrEx("[声音=", "]");

                    //if (string.IsNullOrWhiteSpace(voiceStr) == false)
                    //{
                    //    vaule = vaule.Replace("[声音=" + voiceStr + "]", "");
                    //    messages.Add(new Voice(url: voiceStr.Replace("http://res.cngal.org/", "https://res.cngal.org/")));

                    //}

                    return null;
                }

                else if (vaule.Contains("[@"))
                {
                    var idStr = vaule.MidStrEx("[@", "]");
                    if (long.TryParse(idStr, out var id))
                    {

                        vaule = vaule.Replace("[@" + idStr + "]", "");
                        messages.Add(new AtMessage(idStr));
                    }
                }
                else
                {
                    break;
                }


            }


            if (string.IsNullOrWhiteSpace(vaule) == false)
            {
                messages.Add(new PlainMessage(vaule));
            }

            if (string.IsNullOrWhiteSpace(vaule) && messages.Count == 0)
            {
                return null;
            }
            else
            {
                return messages.ToArray();
            }
        }


        /// <summary>
        /// 执行参数替换
        /// </summary>
        /// <param name="reply"></param>
        /// <param name="message"></param>
        /// <param name="regex"></param>
        /// <param name="args"></param>
        public void ProcMessageReplaceInput(string reply, string message, string regex, List<KeyValuePair<string, string>> args)
        {
            if (string.IsNullOrWhiteSpace(regex))
            {
                return;
            }



            var splits = Regex.Split(message, regex).Where(s => string.IsNullOrWhiteSpace(s) == false).ToList();


            for (var i = 0; i < splits.Count; i++)
            {
                if (reply.Contains($"[{i + 1}]"))
                {
                    args.Add(new KeyValuePair<string, string>($"[{i + 1}]", splits[i].ToString()));
                }
            }
        }

        /// <summary>
        /// 获取参数替换列表
        /// </summary>
        /// <param name="reply"></param>
        /// <param name="message"></param>
        /// <param name="qq"></param>
        /// <param name="name"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public async Task ProcMessageArgument(string reply, string message, long qq, string name, List<KeyValuePair<string, string>> args)
        {
            while (true)
            {
                var argument = reply.MidStrEx("$(", ")");

                if (string.IsNullOrWhiteSpace(argument))
                {
                    break;
                }

                var value = argument switch
                {
                    "time" => DateTime.Now.ToCstTime().ToString("HH:mm"),
                    "qq" => qq.ToString(),
                    "weather" => _messageArgs.FirstOrDefault(s => s.Name == "weather")?.Value,
                    "sender" => name,
                    "n" => "\n",
                    "r" => "\r",
                    "facelist" => "该功能暂未实装",
                    _ => await GetArgValue(argument, message,qq)
                };

                reply = reply.Replace("$(" + argument + ")", value);

                args.Add(new KeyValuePair<string, string>("$(" + argument + ")", value));
            }
        }

        /// <summary>
        /// 处理表情
        /// </summary>
        /// <param name="reply"></param>
        /// <param name="args"></param>
        public void ProcMessageFace(string reply, List<KeyValuePair<string, string>> args)
        {
            if (string.IsNullOrWhiteSpace(reply))
            {
                return;
            }

            foreach (var item in _robotFaces.Where(s => s.IsHidden == false))
            {
                if (reply.Contains($"[{item.Key}]"))
                {
                    args.Add(new KeyValuePair<string, string>($"[{item.Key}]", item.Value));
                }
            }

            for (var i = 1; i < 4; i++)
            {
                if (reply.Contains($"[{i}]") && args.Any(s => s.Key == $"[{i}]") == false)
                {
                    args.Add(new KeyValuePair<string, string>($"[{i}]", ""));
                }
            }
        }

        /// <summary>
        /// 获取参数值
        /// </summary>
        /// <param name="name"></param>
        /// <param name="infor"></param>
        /// <param name="qq"></param>
        /// <returns></returns>
        public async Task<string> GetArgValue(string name, string infor, long qq)
        {
            return await GetArgValue(name, infor,qq, new Dictionary<string, string>());
        }

        public async Task<string> GetArgValue(string name, string infor, long qq, Dictionary<string,string> adds)
        {
            //优先查找本地
            var argVaule= _messageArgs.FirstOrDefault(s => s.Name == name);
            if(argVaule!=null)
            {
                return argVaule.Value;
            }

            //若本地没有 则请求服务器
            var result = await _httpClient.PostAsJsonAsync<GetArgValueModel>(ToolHelper.WebApiPath + "api/robot/GetArgValue", new GetArgValueModel
            {
                Infor = infor,
                Name = name,
                AdditionalInformations = adds,
                SenderId=qq,
                
            });

            var jsonContent = result.Content.ReadAsStringAsync().Result;
            var obj = JsonSerializer.Deserialize<Result>(jsonContent, ToolHelper.options);
            //判断结果
            if (obj.Successful == false)
            {
                throw new ArgError(obj.Error);
            }
            else
            {
                return obj.Error;
            }
        }
    }


    public class ArgError : Exception
    {
        public string Error { get; set; }

        public ArgError(string error)
        {
            Error = error;
        }
    }

    public class SendMessageModel
    {
        public RobotReplyRange Range { get; set; }

        public long SendTo { get; set; }

        public Message[] MiraiMessage { get; set; } = Array.Empty<Message>();

        public MessageBase[] MasudaMessage { get;set; }
    }
}
