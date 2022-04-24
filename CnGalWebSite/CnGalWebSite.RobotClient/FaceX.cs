using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.Helper.Helper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.RobotClient
{
    public class FaceX
    {
        public List<RobotFace> Faces { get; set; } = new List<RobotFace>();
        private readonly Setting _setting;
        private readonly List<MessageArg> _messageArgs;
        private readonly HttpClient _httpClient;


        public FaceX(Setting setting, HttpClient client, List<MessageArg> messageArgs)
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
                var path = Path.Combine(_setting.RootPath, "Faces.json");

                using (StreamReader file = File.OpenText(path))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    Faces = (List<RobotFace>)serializer.Deserialize(file, typeof(List<RobotFace>));
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
            var path = Path.Combine(_setting.RootPath, "Faces.json");

            using (StreamWriter file = File.CreateText(path))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, Faces);
                file.Close();
                file.Dispose();
            }
        }

        public async Task RefreshAsync()
        {
            try
            {
                var model = await _httpClient.GetFromJsonAsync<List<RobotFace>>(ToolHelper.WebApiPath + "api/robot/getrobotFaces");

                Faces.Clear();
                Faces.AddRange(model);
                Save();
            }
            catch (Exception ex)
            {
                OutputHelper.PressError(ex, "无法获取表情列表");
            }
        }
    }
}
