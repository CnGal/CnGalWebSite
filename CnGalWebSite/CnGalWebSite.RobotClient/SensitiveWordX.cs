using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.Helper.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.RobotClient
{
    public class SensitiveWordX
    {
        public List<string> SensitiveWords { get; set; } = new List<string>();
        private readonly Setting _setting;



        public SensitiveWordX(Setting setting)
        {
            _setting = setting;
        }


        public async Task InitAsync()
        {
            Directory.CreateDirectory(_setting.SensitiveWordsPath);
            await LoadAsync();

        }

        public async Task LoadAsync()
        {
            try
            {
                DirectoryInfo root = new DirectoryInfo(_setting.SensitiveWordsPath);

                foreach(var item in root.GetFiles())
                {
                    using var fs = new FileStream(Path.Combine(_setting.SensitiveWordsPath, item.FullName), FileMode.Open, FileAccess.Read);
                    using var sr = new StreamReader(fs);
                    while (sr.EndOfStream == false)
                    {
                        SensitiveWords.Add(await sr.ReadLineAsync());
                    }

                    fs.Close();
                    fs.Dispose();
                    sr.Close();
                    sr.Dispose();
                }
            }
            catch(Exception ex)
            {
                OutputHelper.PressError(ex, "获取敏感词列表失败","未找到敏感词列表文件或读取文件失败",$"请将敏感词放置在{_setting.SensitiveWordsPath}文件夹下");
            }

        }


        public List<string> Check(List<string> texts)
        {
            var words = new List<string>();
            foreach(var item in texts)
            {
                words.AddRange(ToolHelper.FindStringListInText(item, SensitiveWords));
            }

            return words;
        }
    }
}
