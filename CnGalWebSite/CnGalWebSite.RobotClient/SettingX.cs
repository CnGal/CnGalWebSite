using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

                using (StreamReader file = File.OpenText(path))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    BasicSetting = (Setting)serializer.Deserialize(file, typeof(Setting));
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

                using (StreamReader file = File.OpenText(path))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    MessageArgs = (List<MessageArg>)serializer.Deserialize(file, typeof(List<MessageArg>));
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

            using (StreamWriter file = File.CreateText(path))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, BasicSetting);
            }
        }

        public void SaveMessageArgs()
        {
            var path = Path.Combine(BasicSetting.RootPath, "MessageArgs.json");

            using (StreamWriter file = File.CreateText(path))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, MessageArgs);
            }
        }
    }


    public class Setting
    {
        public string MiraiUrl { get; set; } = "localhost:8080";

        public string VerifyKey { get; set; }

        public string RootPath { get; set; } = "Data";

        public string QQ { get; set; }

        /// <summary>
        /// 数据刷新间隔时间 单位 秒
        /// </summary>
        public long IntervalTime { get; set; } = 60 * 5;
    }

    public class MessageArg
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
