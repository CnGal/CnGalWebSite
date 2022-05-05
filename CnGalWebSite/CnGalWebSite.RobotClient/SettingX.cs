using Newtonsoft.Json;

namespace CnGalWebSite.RobotClient
{
    public class SettingX
    {
        public Setting BasicSetting { get; set; } = new Setting();
        public List<MessageArg> MessageArgs { get; set; } = new List<MessageArg>
        {
            new MessageArg
            {
                Name="name",
                Value="看板娘",
            },
            new MessageArg
            {
                Name="weather",
                Value="今天天气真不错~",
            },
        };

        public void Init()
        {
            Console.ResetColor();
            Directory.CreateDirectory(BasicSetting.RootPath);
            LoadMessageArgs();
            LoadBasicSetting();
            Verify();
        }

        public void LoadBasicSetting()
        {
            try
            {
                var path = Path.Combine(BasicSetting.RootPath, "Setting.json");

                using (var file = File.OpenText(path))
                {
                    var serializer = new JsonSerializer();
                    BasicSetting = (Setting)serializer.Deserialize(file, typeof(Setting));
                    file.Close();
                    file.Dispose();
                }
            }
            catch
            {
                SaveBasicSetting();
            }

        }

        public void LoadMessageArgs()
        {
            try
            {
                var path = Path.Combine(BasicSetting.RootPath, "MessageArgs.json");

                using (var file = File.OpenText(path))
                {
                    var serializer = new JsonSerializer();
                    MessageArgs = (List<MessageArg>)serializer.Deserialize(file, typeof(List<MessageArg>));
                    file.Close();
                    file.Dispose();
                }
            }
            catch
            {
                SaveMessageArgs();
            }

        }

        public void Verify()
        {
            if (string.IsNullOrWhiteSpace(BasicSetting.VerifyKey))
            {
                Console.WriteLine("请输入 VerifyKey");
                BasicSetting.VerifyKey = Console.ReadLine();
            }

            if (string.IsNullOrWhiteSpace(BasicSetting.QQ))
            {
                Console.WriteLine("请输入 QQ号");
                BasicSetting.QQ = Console.ReadLine();
            }

            SaveBasicSetting();
        }

        public void SaveBasicSetting()
        {
            var path = Path.Combine(BasicSetting.RootPath, "Setting.json");

            using (var file = File.CreateText(path))
            {
                var serializer = new JsonSerializer();
                serializer.Serialize(file, BasicSetting);
                file.Close();
                file.Dispose();
            }
        }

        public void SaveMessageArgs()
        {
            var path = Path.Combine(BasicSetting.RootPath, "MessageArgs.json");

            using (var file = File.CreateText(path))
            {
                var serializer = new JsonSerializer();
                serializer.Serialize(file, MessageArgs);
                file.Close();
                file.Dispose();
            }
        }
    }


    public class Setting
    {
        public string MiraiUrl { get; set; } = "localhost:8080";

        public string VerifyKey { get; set; }

        public string RootPath { get; set; } = "Data";

        public string SensitiveWordsPath { get; set; } = "SensitiveWords";

        public long WarningQQGroup { get; set; } = 991706063;

        public string QQ { get; set; }

        /// <summary>
        /// 数据刷新间隔时间 单位 秒
        /// </summary>
        public long IntervalTime { get; set; } = 60 * 5;
        /// <summary>
        /// 每分钟最多回复次数
        /// </summary>
        public long TotalLimit { get; set; } = 15;
        /// <summary>
        /// 单用户每分钟最多回复次数 （会警告
        /// </summary>
        public long SingleLimit { get; set; } = 10;
    }

    public class MessageArg
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
