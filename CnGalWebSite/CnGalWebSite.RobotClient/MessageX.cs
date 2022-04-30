using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Robots;
using CnGalWebSite.Helper.Extensions;
using CnGalWebSite.Helper.Helper;
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

        public RobotReply GetAutoReply(string message, GroupMessageSender sender)
        {

            var now = DateTime.Now.ToCstTime();
            var replies = _replies.Where(s => s.IsHidden == false && now.TimeOfDay <= s.BeforeTime.TimeOfDay && now.TimeOfDay >= s.AfterTime.TimeOfDay && Regex.IsMatch(message, s.Key))
                .GroupBy(s => s.Priority)
                .OrderByDescending(s => s.Key)
                .ToList();

            if (replies.Count == 0)
            {
                return null;
            }

            int index = new Random().Next(0, replies.FirstOrDefault().Count());

            return replies.FirstOrDefault().ToList()[index];
        }

        public async Task<Message[]> ProcMessageAsync(string reply, string message, string regex, GroupMessageSender sender)
        {
            var args = new List<KeyValuePair<string, string>>();
            try
            {
                await ProcMessageArgument(reply, message, sender, args);
                ProcMessageReplaceInput(reply, message, regex, args);
                ProcMessageFace(reply, sender, args);
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
                string msg = $"对{sender.memberName}({sender.id})的消息回复中包含敏感词\n消息：{message}\n回复：{reply}\n\n参数替换列表：\n";
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
            return ProcMessageArray(reply, sender);
        }

        public Message[] ProcMessageArray(string vaule, GroupMessageSender sender)
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
                    long id = 0;
                    if (long.TryParse(idStr, out id))
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

        public void ProcMessageReplaceInput(string reply, string message, string regex, List<KeyValuePair<string, string>> args)
        {
            if (string.IsNullOrWhiteSpace(regex))
            {
                return;
            }



            var splits = Regex.Split(message, regex).Where(s => string.IsNullOrWhiteSpace(s) == false).ToList();


            for (int i = 0; i < splits.Count; i++)
            {
                if (reply.Contains($"[{i + 1}]"))
                {
                    args.Add(new KeyValuePair<string, string>($"[{i + 1}]", splits[i].ToString()));
                }
            }
        }

        public async Task ProcMessageArgument(string reply, string message, GroupMessageSender sender, List<KeyValuePair<string, string>> args)
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
                    "qq" => sender.id.ToString(),
                    "weather" => _messageArgs.FirstOrDefault(s => s.Name == "weather")?.Value,
                    "sender" => sender.memberName,
                    "auth" => await GetArgValue(argument, sender.id.ToString()),
                    "n" => "\n",
                    "r" => "\r",
                    "facelist" => "该功能暂未实装",
                    "introduce" => await GetArgValue(argument, message),
                    "website" => await GetArgValue(argument, message),
                    _ => await GetArgValue(argument, null)
                };

                reply = reply.Replace("$(" + argument + ")", value);

                args.Add(new KeyValuePair<string, string>("$(" + argument + ")", value));
            }
        }

        public void ProcMessageFace(string reply, GroupMessageSender sender, List<KeyValuePair<string, string>> args)
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

            for (int i = 1; i < 4; i++)
            {
                if (reply.Contains($"[{i}]") && args.Any(s => s.Key == $"[{i}]") == false)
                {
                    args.Add(new KeyValuePair<string, string>($"[{i}]", ""));
                }
            }
        }

        public async Task<string> GetArgValue(string name, string infor)
        {
            var result = await _httpClient.PostAsJsonAsync<GetArgValueModel>(ToolHelper.WebApiPath + "api/robot/GetArgValue", new GetArgValueModel
            {
                Infor = infor,
                Name = name,
            });

            string jsonContent = result.Content.ReadAsStringAsync().Result;
            Result obj = JsonSerializer.Deserialize<Result>(jsonContent, ToolHelper.options);
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
}
