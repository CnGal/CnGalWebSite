using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.Helper.Helper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CnGalWebSite.RobotClient.Services.SensitiveWords
{
    public class SensitiveWordService:ISensitiveWordService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SensitiveWordService> _logger;

        public List<string> SensitiveWords { get; set; } = new List<string>();

        public SensitiveWordService(IConfiguration configuration, ILogger<SensitiveWordService> logger)
        {
            _configuration = configuration;

            Load();
            _logger = logger;
        }


        public  void Load()
        {
            var path = _configuration["SensitiveWordsPath"] ?? "";
            try
            {
               
                //创建文件夹
                Directory.CreateDirectory(path);
                var root = new DirectoryInfo(path);

                foreach (var item in root.GetFiles())
                {
                    using var fs = new FileStream(Path.Combine(path, item.FullName), FileMode.Open, FileAccess.Read);
                    using var sr = new StreamReader(fs);
                    while (sr.EndOfStream == false)
                    {
                        SensitiveWords.Add( sr.ReadLine());
                    }

                    fs.Close();
                    fs.Dispose();
                    sr.Close();
                    sr.Dispose();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"未找到敏感词列表文件或读取文件失败，请将敏感词放置在{path}文件夹下", path);
            }

        }

        public List<string> Check(List<string> texts)
        {
            var words = new List<string>();
            foreach (var item in texts)
            {
                words.AddRange(ToolHelper.FindStringListInText(item, SensitiveWords));
            }

            return words;
        }

        public List<string> Check(string text)
        {
            return ToolHelper.FindStringListInText(text, SensitiveWords);
        }
    }
}
