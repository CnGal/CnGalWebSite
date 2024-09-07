using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CnGalWebSite.EamineService.Services.SensitiveWords
{
    public class SensitiveWordService : ISensitiveWordService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SensitiveWordService> _logger;

        public List<string> SensitiveWords { get; set; } = new List<string>();

        public SensitiveWordService(IConfiguration configuration, ILogger<SensitiveWordService> logger)
        {
            _configuration = configuration;
            _logger = logger;

            Load();
        }


        public void Load()
        {
            var path = string.IsNullOrWhiteSpace(_configuration["SensitiveWords_Path"]) ? Path.Combine("Data", "SensitiveWords") : _configuration["SensitiveWords_Path"]!;
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
                        var line = sr.ReadLine();
                        if (string.IsNullOrWhiteSpace(line) == false)
                        {
                            SensitiveWords.Add(line);
                        }
                    }
                }

                if (SensitiveWords.Count == 0)
                {
                    throw new Exception("没有敏感词");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "未找到敏感词列表文件或读取文件失败，请将敏感词放置在{path}文件夹下", path);
            }

        }

        public List<string> Check(List<string> texts)
        {
            var words = new List<string>();
            foreach (var item in texts)
            {
                words.AddRange(Check(item));
            }

            return words;
        }

        public List<string> Check(string text)
        {
            var words = text.FindStringListInText(SensitiveWords);

            if (words.Count == 0)
            {
                _logger.LogInformation("未发现敏感词：{}", text.Length >= 30 ? ($"{text[..30].Replace("\n", "\n      ")}......") : text);

            }
            else
            {
                _logger.LogInformation("发现敏感词：\n      {}", string.Join("\n      ", words));
            }

            return words;
        }

        public int Count()
        {
            return SensitiveWords.Count;
        }
    }
}
