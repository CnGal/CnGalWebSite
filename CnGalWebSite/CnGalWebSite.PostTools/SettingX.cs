using CnGalWebSite.DataModel.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.PostTools
{
    public class SettingX
    {
        public string Token { get; set; }

        public string ArticlesFileName { get; set; } = "Links.txt";

        public string MergeEntriesFileName { get; set; } = "MergeEntries.txt";

        public string ImportPeripheriesFileName { get; set; } = "ImportPeripheries.json";

        public string TempPath { get; set; } = "Data";

        public string TransferDepositFileAPI { get; set; } = "https://api.cngal.top/";

        public string UserName { get; set; }

        public List<ArticleTypeString> ArticleTypeStrings { get; set; } = new List<ArticleTypeString>();

        public void Init()
        {
            Console.ResetColor();
            Directory.CreateDirectory(TempPath);
        }

        public void Load()
        {
            try
            {
                var path = Path.Combine(TempPath, "setting.json");

                using (var file = File.OpenText(path))
                {
                    var serializer = new JsonSerializer();
                    var temp = (SettingX)serializer.Deserialize(file, typeof(SettingX));

                    Token = temp.Token;
                    ArticlesFileName = temp.ArticlesFileName;
                    TempPath = temp.TempPath;
                    TransferDepositFileAPI = temp.TransferDepositFileAPI;
                    UserName = temp.UserName;
                    ArticleTypeStrings.Clear();
                    ArticleTypeStrings = temp.ArticleTypeStrings;
                }
            }
            catch
            {
                ArticleTypeStrings = new List<ArticleTypeString>
                                        {
                                            new ArticleTypeString
                                            {
                                                Type=ArticleType.News,
                                                Texts=new List<string>
                                                {
                                                    "国G七日报",
                                                    "2021中文恋爱游戏大赛参赛制作组",
                                                    "Steam",
                                                }
                                            },
                                            new ArticleTypeString
                                            {
                                                Type=ArticleType.Interview,
                                                Texts=new List<string>
                                                {
                                                    "访谈",
                                                    "专访"
                                                }
                                            },
                                            new ArticleTypeString
                                            {
                                                Type=ArticleType.Evaluation,
                                                Texts=new List<string>
                                                {
                                                    "评测",
                                                    "测评",
                                                    "浅评"
                                                }
                                            },
                                            new ArticleTypeString
                                            {
                                                Type=ArticleType.None,
                                                Texts=new List<string>
                                                {
                                                    "杂谈"
                                                }
                                            },
                                            new ArticleTypeString
                                            {
                                                Type=ArticleType.Fan,
                                                Texts=new List<string>
                                                {
                                                    "二创"
                                                }
                                            },
                                            new ArticleTypeString
                                            {
                                                Type=ArticleType.Peripheral,
                                                Texts=new List<string>
                                                {
                                                    "周边"
                                                }
                                            },
                                            new ArticleTypeString
                                            {
                                                Type=ArticleType.Strategy,
                                                Texts=new List<string>
                                                {
                                                    "攻略"
                                                }
                                            },
                                        };
                Save();
            }

        }

        public void Save()
        {
            var path = Path.Combine(TempPath, "setting.json");

            using (var file = File.CreateText(path))
            {
                var serializer = new JsonSerializer();
                serializer.Serialize(file, this);
            }
        }
    }

    public class ArticleTypeString
    {
        public ArticleType Type { get; set; }

        public List<string> Texts { get; set; } = new List<string>();
    }
}
