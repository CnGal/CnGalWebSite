using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Articles;
using CnGalWebSite.DataModel.ViewModel.Robots;
using CnGalWebSite.Helper.Extensions;
using CnGalWebSite.Helper.Helper;
using MeowMiraiLib.Msg.Sender;
using MeowMiraiLib.Msg.Type;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Message = MeowMiraiLib.Msg.Type.Message;

namespace CnGalWebSite.RobotClient
{
    public class MessageX
    {
        private readonly Setting _setting;
        private readonly IDictionary<string, string> _cache;
        private readonly HttpClient _httpClient;
        private readonly List<RobotReply> _replies;
        private readonly List<MessageArg> _messageArgs;
        private readonly List<RobotFace> _robotFaces;


        public MessageX(Setting setting, HttpClient client, List<RobotReply> replies, IDictionary<string, string> cache, List<MessageArg> messageArgs, List<RobotFace> robotFaces)
        {
            _setting = setting;
            _httpClient = client;
            _replies = replies;
            _cache = cache;
            _messageArgs = messageArgs;
            _robotFaces = robotFaces;
        }

        public RobotReply GetAutoReply(string message, GroupMessageSender sender)
        {

            var now = DateTime.Now.ToCstTime();
            var replies = _replies.Where(s => s.IsHidden == false && now.TimeOfDay <= s.BeforeTime.TimeOfDay && now.TimeOfDay >= s.AfterTime.TimeOfDay && Regex.IsMatch(message, s.Key))
                .GroupBy(s=>s.Priority)
                .OrderByDescending(s=>s.Key)
                .ToList();

            if (replies.Count == 0)
            {
                return null;
            }

            int index = new Random().Next(0, replies.FirstOrDefault().Count());

            return replies.FirstOrDefault().ToList()[index];
        }

        public async Task<Message[]> ProcMessageAsync(string reply, string message, string regex,GroupMessageSender sender)
        {
            return ProcMessageArray(ProcMessageFace(ProcMessageReplaceInput(await ProcMessageArgument(reply, message, sender),message,regex), sender), sender);
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

         
            if(string.IsNullOrWhiteSpace(vaule)==false)
            {
                messages.Add(new Plain(vaule));
            }

            if(string.IsNullOrWhiteSpace(vaule)&&messages.Count==0)
            {
                return null;
            }
            else
            {
                return messages.ToArray();
            }
        }

        public string ProcMessageReplaceInput(string reply, string message, string regex)
        {
            if(string.IsNullOrWhiteSpace(regex))
            {
                return message;
            }

            var splits = Regex.Split(message,regex).Where(s=>string.IsNullOrWhiteSpace(s)==false).ToList();


            for (int i = 0; i < splits.Count; i++)
            {
                reply = reply.Replace($"[{i+1}]", splits[i].ToString());
            }

            return reply;
        }

        public async Task<string> ProcMessageArgument(string reply,string message, GroupMessageSender sender)
        {
            try
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
                }
            }
            catch (ArgError arg)
            {
                reply = arg.Error;
            }
            catch (Exception ex)
            {
                OutputHelper.PressError(ex, "获取变量值失败");
                return "呜呜呜~";

            }

            return reply;
        }

        public string ProcMessageFace(string message, GroupMessageSender sender)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return null;
            }

            foreach (var item in _robotFaces.Where(s => s.IsHidden == false))
            {
                message = message.Replace($"[{item.Key}]", item.Value);
            }
            return message;
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


    public class ArgError:Exception
    {
        public string Error { get; set; }

        public ArgError(string error)
        {
            Error = error;
        }
    }
}
