using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.Helper.Helper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using File = System.IO.File;

namespace CnGalWebSite.RobotClient
{
    public class ReplyX
    {
        public List<RobotReply> Replies { get; set; } = new List<RobotReply>();
        private readonly Setting _setting;
        private readonly List<MessageArg> _messageArgs;
        private readonly HttpClient _httpClient;


        public ReplyX(Setting setting, HttpClient client, List<MessageArg> messageArgs)
        {
            _setting = setting;
            _httpClient = client;
            _messageArgs = messageArgs;
        }


        public void Init()
        {
            Load();

        }

        public void Load()
        {
            try
            {
                var path = Path.Combine(_setting.RootPath, "Replies.json");

                using (StreamReader file = File.OpenText(path))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    Replies = (List<RobotReply>)serializer.Deserialize(file, typeof(List<RobotReply>));
                    file.Close();
                    file.Dispose();
                }
            }
            catch
            {
                Save();
            }

        }

        public void Save()
        {
            var path = Path.Combine(_setting.RootPath, "Replies.json");

            using (StreamWriter file = File.CreateText(path))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, Replies);
                file.Close();
                file.Dispose();
            }
        }

        public async Task RefreshAsync()
        {
            try
            {
                var model = await _httpClient.GetFromJsonAsync<List<RobotReply>>(ToolHelper.WebApiPath + "api/robot/getrobotreplies");
                var name = _messageArgs.FirstOrDefault(s => s.Name == "name")?.Value;
                foreach (var reply in model)
                {
                    reply.Key = reply.Key.Replace("$(name)", name);
                    reply.Value = reply.Value.Replace("$(name)", name);
                }

                Replies.Clear();
                Replies.AddRange(model);
                Save();
            }
            catch (Exception ex)
            {
                OutputHelper.PressError(ex, "无法获取自动回复列表");
            }
        }
    }
}
