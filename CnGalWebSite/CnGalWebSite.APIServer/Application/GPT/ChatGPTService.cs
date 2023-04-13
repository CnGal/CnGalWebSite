using CnGalWebSite.Core.Services;
using CnGalWebSite.DataModel.Model;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using CnGalWebSite.RobotClient.DataModels.GPT;

namespace CnGalWebSite.APIServer.Application.GPT
{
    public class ChatGPTService:IChatGPTService
    {
        private readonly IHttpService _httpService;
        private readonly IConfiguration _configuration;
        private readonly List<DateTime> _record = new List<DateTime>();
        private readonly HttpClient _httpClient;
        private readonly ILogger<ChatGPTService> _logger;

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        public ChatGPTService(IHttpService httpService, IConfiguration configuration, ILogger<ChatGPTService> logger)
        {
            _httpService = httpService;
            _configuration = configuration;
            _logger = logger;

            _httpClient = _httpService.GetClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + _configuration["ChatGPTApiKey"]);
        }

        public async Task< string > GetReply(string question)
        {
            var datetime= DateTime.Now.ToCstTime();

            //检查上限
            if (_record.Count(s => s > datetime.AddMinutes(-1)) > int.Parse(_configuration["ChatGPTLimit"] ?? "10"))
            {
                return "哀家累了呢~";
            }
            //清理过期的记录
            _record.RemoveAll(s => s < datetime.AddMinutes(-1));

            //读取配置
            var sys = _configuration["ChatGPT_SystemMessageTemplate"];
            var user = _configuration["ChatGPT_UserMessageTemplate"];
            var url = _configuration["ChatGPTApiUrl"];

            if (string.IsNullOrWhiteSpace(sys)|| string.IsNullOrWhiteSpace(user))
            {
                return null;
            }
            //替换文字
            question = question.Replace("看板娘", "").Replace($"[@3257277748]", "");

            //填充消息模板
            sys = sys.Replace("{date}", datetime.ToString("yyyy年MM月dd日"));
            user = user.Replace("{question}", question);

            //日志
            _logger.LogInformation("向ChatGPT发送消息：{question}",question);
            //添加记录
            _record.Add(datetime);

            var response=await _httpClient.PostAsJsonAsync<ChatCompletionModel>(url + "v1/chat/completions", new ChatCompletionModel
            {
                Messages = new List<ChatCompletionMessage>
                {
                    new ChatCompletionMessage
                    {
                        Role="system",
                        Content=sys
                    },
                    new ChatCompletionMessage
                    {
                        Role="user",
                        Content=user
                    }
                }
            });

            if(!response.IsSuccessStatusCode)
            {
                return null;
            }

            string jsonContent = response.Content.ReadAsStringAsync().Result;
            var result= JsonSerializer.Deserialize<ChatResult>(jsonContent, _jsonOptions);

            var reply = result?.Choices?.FirstOrDefault()?.Message?.Content;
            if ( string.IsNullOrWhiteSpace(reply))
            {
                return null;
            }

            _logger.LogInformation("收到ChatGPT的回复：{reply}", reply);

            return reply;
        }
    }
}
