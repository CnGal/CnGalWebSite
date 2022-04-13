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


        public MessageX(Setting setting, HttpClient client, List<RobotReply> replies, IDictionary<string, string> cache, List<MessageArg> messageArgs)
        {
            _setting = setting;
            _httpClient = client;
            _replies = replies;
            _cache = cache;
            _messageArgs = messageArgs;
        }

        public async Task<Message[]> ProcMessage(string message, GroupMessageSender sender)
        {
            //var cache = _cache.FirstOrDefault(s => s.Key == message);
            //if (string.IsNullOrWhiteSpace(cache.Value) == false)
            //{
            //    return cache.Value;
            //}

            var now = DateTime.Now.ToCstTime();
            var replies = _replies.Where(s => s.IsHidden == false && now.TimeOfDay <= s.BeforeTime.TimeOfDay && now.TimeOfDay >= s.AfterTime.TimeOfDay && Regex.IsMatch(message, s.Key)).ToList();

            if (replies.Count == 0)
            {
                return null;
            }

            int index = new Random().Next(0, replies.Count);

            var vaule = await ReplayArgument(replies[index].Value, sender);

            //if (string.IsNullOrWhiteSpace(vaule) == false)
            //{
            //    _cache.Add(new KeyValuePair<string, string>(message, vaule));
            //}
            return StringToMessageArray(vaule, sender);
        }

        public Message[] StringToMessageArray(string vaule, GroupMessageSender sender)
        {
            if (string.IsNullOrWhiteSpace(vaule))
            {
                return null;
            }

            if (vaule.Contains("[image="))
            {
                var imageStr = vaule.MidStrEx("[image=", "]");

                if (string.IsNullOrWhiteSpace(imageStr)==false)
                {
                    var text = vaule.Replace("[image=" + imageStr + "]", "");
                    var messages= new List<Message>
                                 {
                                    new Image(url: vaule.MidStrEx("[image=", "]").Replace("http://image.cngal.org/", "https://image.cngal.org/"))
                                 };
                    if(string.IsNullOrWhiteSpace(text)==false)
                    {
                        messages.Add(new Plain(text));
                    }
                    return messages.ToArray();
                }
                else
                {
                    return null;
                }
            }
            else if (vaule.Contains("[声音="))
            {
                return new Message[] { new Voice(url: vaule.MidStrEx("[声音=", "]").Replace("http://res.cngal.org/", "https://res.cngal.org/")) };
            }

            else if (vaule.Contains("[@"))
            {
                var idStr = vaule.MidStrEx("[@", "]");
                long id = 0;
                if (long.TryParse(idStr, out id))
                {
                    return new Message[]
                                 {
                                    new At(id,idStr),
                                    new Plain(vaule.Replace("[@"+idStr+ "]",""))
                                 };
                }
                else
                {
                    return new Message[] { new Plain(vaule.Replace("[@" + idStr + "]", "")) };
                }

            }
            else
            {
                return new Message[] { new Plain(vaule) };
            }
        }


        public async Task<string> ReplayArgument(string message, GroupMessageSender sender)
        {
            while (true)
            {
                var argument = message.MidStrEx("$(", ")");

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
                    "auth" =>await GetArgValue(argument, sender.id.ToString()),
                    "n" => "\n",
                    "r" => "\r",
                    "facelist" => "该功能暂未实装",
                    _ => await GetArgValue(argument, null)
                };

                message = message.Replace("$(" + argument + ")", value);
            }

            return message;
        }

        public async Task<string> GetArgValue(string name, string infor)
        {
            try
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
            catch (Exception ex)
            {
                OutputHelper.PressError(ex, "获取变量值失败");
                return "呜呜呜~";
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
