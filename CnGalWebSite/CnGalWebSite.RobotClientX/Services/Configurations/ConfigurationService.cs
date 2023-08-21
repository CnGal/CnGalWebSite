using CnGalWebSite.RobotClientX.Models.Configurations;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace CnGalWebSite.RobotClientX.Services.Configurations
{
    public class ConfigurationService:IConfigurationService
    {
        private ConfigurationModel _configurationModel;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ConfigurationService(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            _configuration = configuration;
            _webHostEnvironment = webHostEnvironment;

            Load();
        }

        public void Load()
        {
            var path = Path.Combine(_webHostEnvironment.WebRootPath, "Data", "Setting.json");
            string jsonString = File.ReadAllText(path);
            _configurationModel = JsonSerializer.Deserialize<ConfigurationModel>(jsonString)!;

            if(string.IsNullOrWhiteSpace( _configurationModel.ChatGPT_SystemMessageTemplate))
            {
                _configurationModel.ChatGPT_SystemMessageTemplate = _configuration["ChatGPT_SystemMessageTemplate"];
            }
            if (string.IsNullOrWhiteSpace(_configurationModel.ChatGPT_UserMessageTemplate))
            {
                _configurationModel.ChatGPT_UserMessageTemplate = _configuration["ChatGPT_UserMessageTemplate"];
            }

            Save();
        }

        public void Save()
        {
            var path = Path.Combine(_webHostEnvironment.WebRootPath, "Data", "Setting.json");
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.All)
            };
            string jsonString = JsonSerializer.Serialize(_configurationModel, options);
            File.WriteAllText(path, jsonString);
        }

        public ConfigurationModel GetConfiguration()
        {
            return _configurationModel;
        }
    }
}
