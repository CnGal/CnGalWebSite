using CnGalWebSite.DataModel.ImportModel;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.HistoryData.Model;
using Newtonsoft.Json;
using Spire.Xls;
using Spire.Xls.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CnGalWebSite.HistoryData
{
    public static class Helper
    {
        public static string GetStringValue(this IWorksheet sheet, string index)
        {
            if (sheet.Range[index].Value2 != null && !string.IsNullOrWhiteSpace(sheet.Range[index].Value2.ToString()))
            {
                return sheet.Range[index].Value2.ToString();
            }
            else
            {
                return null;
            }
        }
    }
    public class Program
    {
        private static readonly string path = Environment.CurrentDirectory;
        private static IWorksheet biaoge_jichu;
        private static IWorksheet biaoge_staff;
        private static IWorksheet biaoge_zhizuoren;
        private static IWorksheet biaoge_juese;
        private static IWorksheet PeripheriesSheet;
        private static readonly List<Entry> Entry_s = new List<Entry>();
        private static readonly List<Article> Article_s = new List<Article>();
        private static List<Article_Database> articles = new List<Article_Database>();
        private static List<Guide> guides = new List<Guide>();
        private static List<Game> games = new List<Game>();
        private static List<NameIdPairModel> CurrentGames = new List<NameIdPairModel>();
        private static List<NameIdPairModel> CurrentArticles = new List<NameIdPairModel>();
        private static readonly List<OldAndNewUrlModel> Urls = new List<OldAndNewUrlModel>();
        private static List<HistoryUser> historyUsers = new List<HistoryUser>();
        private static readonly List<ImportUserModel> users = new List<ImportUserModel>();
        private static readonly List<Periphery> peripheries = new List<Periphery>();

        private static void Main()
        {
            //加载当前数据
            LoadCurrentDatabaseData();
            //处理表格
            ImprotPeripheries();
        }

        private static void MatchOldAndNew()
        {
            //加载历史数据
            LoadHistoryDatabaseData();
            //加载当前数据
            LoadCurrentDatabaseData();

            //匹配数据
            foreach (Article_Database item in articles)
            {
                NameIdPairModel temp = CurrentArticles.Find(s => s.Name == item.Title);

                Urls.Add(new OldAndNewUrlModel
                {
                    NewUrl = temp == null ? "" : ("/articles/index/" + temp.Id),
                    OldUrl = "/article/" + item.Id + ".html",
                    Title = item.Title
                });
            }

            //匹配数据
            foreach (Guide item in guides)
            {
                NameIdPairModel temp = CurrentArticles.Find(s => s.Name == item.Title);

                Urls.Add(new OldAndNewUrlModel
                {
                    NewUrl = temp == null ? "" : ("/articles/index/" + temp.Id),
                    OldUrl = "/guide/" + item.Id + ".html",
                    Title = item.Title
                });
            }


            //匹配数据
            foreach (Game item in games)
            {
                NameIdPairModel temp = CurrentGames.Find(s => s.Name == item.Title);

                if (temp == null)
                {
                    Entry entry = new Entry
                    {
                        Name = item.Title,
                        DisplayName = item.Title,
                        AnotherName = item.Bgm,
                        Information = new List<BasicEntryInformation>(),
                        Relevances = new List<EntryRelevance>(),
                        Pictures = new List<EntryPicture>(),
                    };
                    if (string.IsNullOrWhiteSpace(item.ReleaseTime) == false)
                    {
                        try
                        {
                            item.ReleaseTime = item.ReleaseTime.Replace(" ", "");
                            entry.PubulishTime = DateTime.ParseExact(item.ReleaseTime, "yyyy-MM-dd", null);
                        }
                        catch
                        {
                            try
                            {
                                item.ReleaseTime = item.ReleaseTime.Replace(" ", "");
                                entry.PubulishTime = DateTime.ParseExact(item.ReleaseTime, "yyyy-M-d", null);
                            }
                            catch
                            {
                                try
                                {
                                    entry.PubulishTime = DateTime.ParseExact(item.ReleaseTime, "yyyy年MM月dd日", null);
                                }
                                catch
                                {
                                    try
                                    {
                                        entry.PubulishTime = DateTime.ParseExact(item.ReleaseTime, "yyyy年M月d日", null);
                                    }
                                    catch
                                    {
                                        try
                                        {
                                            entry.PubulishTime = DateTime.ParseExact(item.ReleaseTime, "yyyy.MM.dd", null);
                                        }
                                        catch
                                        {
                                            entry.Information.Add(new BasicEntryInformation
                                            {
                                                Modifier = "基本信息",
                                                DisplayName = "发行时间备注",
                                                DisplayValue = item.ReleaseTime
                                            });
                                        }

                                    }


                                }
                            }

                        }
                    }

                    if (entry.PubulishTime != null)
                    {
                        entry.Information.Add(new BasicEntryInformation
                        {
                            Modifier = "基本信息",
                            DisplayName = "发行时间",
                            DisplayValue = ((DateTime)entry.PubulishTime).ToString("yyyy年M月d日")
                        });
                    }
                    if (string.IsNullOrWhiteSpace(item.SellLink) == false && item.SellLink.Contains("store.steampowered.com"))
                    {
                        if (item.SellLink.Contains("store.steampowered.com"))
                        {
                            try
                            {
                                entry.SteamId = item.SellLink.Replace("http://", "https://").Replace("https://store.steampowered.com/app/", "").Split('/')[0];
                                entry.Information.Add(new BasicEntryInformation
                                {
                                    Modifier = "基本信息",
                                    DisplayName = "Steam平台Id",
                                    DisplayValue = entry.SteamId
                                });
                            }
                            catch
                            {

                            }
                        }
                        else
                        {
                            entry.Information.Add(new BasicEntryInformation
                            {
                                Modifier = "基本信息",
                                DisplayName = "官网",
                                DisplayValue = item.SellLink
                            });
                        }

                    }
                    entry.MainPicture = string.IsNullOrWhiteSpace(entry.SteamId) ? item.CoverImg : "https://media.st.dl.pinyuncloud.com/steam/apps/" + entry.SteamId + "/header.jpg";// linshi[linshi.Length - 1];

                    entry.MainPage = item.Content.Replace("_ueditor_page_break_tag_", "");
                    string sss2 = Regex.Replace(item.Content, "[a-zA-Z0-9]", "");
                    string tempStr = sss2.Replace("<", "").Replace(">", "").Replace("/", "").Replace("\\", "").Replace(";", "").Replace(":", "").Replace("\"", "").Replace(" ", "").Replace(".", "").Replace("=", "").Replace("-", "").Replace("_", "");
                    entry.BriefIntroduction = tempStr.Substring(0, tempStr.Length > 120 ? 120 : tempStr.Length) + "......";

                    Entry_s.Add(entry);
                }


                Urls.Add(new OldAndNewUrlModel
                {
                    NewUrl = temp == null ? "" : ("/entries/index/" + temp.Id),
                    OldUrl = "/game/" + item.Id + ".html",
                    Title = item.Title
                });
            }

            //输出
            using (StreamWriter file = File.CreateText($"{path}\\输出文件夹\\OldUrl_NewUrl.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, Urls);
            }

            using (StreamWriter file = File.CreateText($"{path}\\输出文件夹\\NewEntries.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, Entry_s);
            }

            //自定义输出
            // 创建文件
            FileStream fs = new FileStream($"{path}\\输出文件夹\\oldurls.conf", FileMode.OpenOrCreate, FileAccess.ReadWrite); //可以指定盘符，也可以指定任意文件名，还可以为word等文件
            StreamWriter sw = new StreamWriter(fs); // 创建写入流

            string tempStr1 = "";
            foreach (OldAndNewUrlModel item in Urls)
            {
                tempStr1 += ("location " + item.OldUrl + " {\n");
                tempStr1 += ("    return 301 " + item.NewUrl + ";\n");
                tempStr1 += ("}\n");
            }
            sw.WriteLine(tempStr1);
            sw.Close();
            fs.Close();

            //自定义输出
            // 创建文件
            FileStream fs1 = new FileStream($"{path}\\输出文件夹\\baidu.text", FileMode.OpenOrCreate, FileAccess.ReadWrite); //可以指定盘符，也可以指定任意文件名，还可以为word等文件
            StreamWriter sw1 = new StreamWriter(fs1); // 创建写入流
            tempStr1 = "";
            foreach (OldAndNewUrlModel item in Urls)
            {
                tempStr1 += ("https://www.cngal.org" + item.OldUrl + " https://www.cngal.org" + item.NewUrl + "\n");
            }
            sw1.WriteLine(tempStr1);
            sw1.Close();
            fs1.Close();
            sw1.Dispose();
            fs1.Dispose();
        }

        public static void LoadCurrentDatabaseData()
        {
            using (StreamReader file = File.OpenText(path + "\\数据源文件夹\\CurrentArticles.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                CurrentArticles = (List<NameIdPairModel>)serializer.Deserialize(file, typeof(List<NameIdPairModel>));
            }



            using (StreamReader file = File.OpenText(path + "\\数据源文件夹\\CurrentGames.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                CurrentGames = (List<NameIdPairModel>)serializer.Deserialize(file, typeof(List<NameIdPairModel>));
            }


        }

        /// <summary>
        /// 处理周边导入表格
        /// </summary>
        private static void ImprotPeripheries()
        {
            Directory.CreateDirectory(path + "\\输出文件夹");
            Directory.CreateDirectory(path + "\\输出文件夹\\图片");
            //第一步 加载表格
            Workbook linshi_ = new Workbook();
            linshi_.LoadFromFile(path + "\\数据源文件夹\\周边.xlsx");
            PeripheriesSheet = linshi_.Worksheets[0];


            //第二步 从基本信息表中创建周边
            for (int i = 3; i <= 5; i++)
            {
                peripheries.Add(CreatePeripheriy(PeripheriesSheet, i));
                Console.WriteLine("[第二步]   <" + (i-2) + ">");
            }

            //第二步 添加关联词条
            for (int i = 3; i <= 5; i++)
            {
                GetRelatedEntry(PeripheriesSheet, i);
                Console.WriteLine("[第三步]   <" + (i - 2) + ">");
            }

            //第三步 添加关联周边
            for (int i = 3; i <= 5; i++)
            {
                GetRelatedPeriphery(PeripheriesSheet, i);
                Console.WriteLine("[第四步]   <" + (i - 2) + ">");
            }


            //输出
            using (StreamWriter file = File.CreateText($"{path}\\输出文件夹\\Peripheries.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, peripheries);
            }
            Console.WriteLine("=====================================");
            Console.WriteLine("             处理完成                ");
            Console.WriteLine("=====================================");
            Console.ReadKey();
        }

        public static Periphery CreatePeripheriy(IWorksheet sheet, int index)
        {
            Periphery model = new Periphery();
            //查找是否同名
            var name = sheet.GetStringValue("A" + index);
            if (name == null)
            {
                return null;
            }

            if (peripheries.Any(s => s.Name == name))
            {
                throw new Exception("周边重名");
            }

            model.Name= name;

            //导入基本信息
            model.DisplayName = sheet.GetStringValue("B" + index) ?? model.Name;
            model.BriefIntroduction = sheet.GetStringValue("C" + index);
            model.MainPicture = sheet.GetStringValue("D" + index);
            model.SaleLink = sheet.GetStringValue("E" + index);
            model.Price = sheet.GetStringValue("F" + index);

            var type = sheet.GetStringValue("H" + index);
            if(type.Contains("画册"))
            {
                model.Type = PeripheryType.SetorAlbumEtc;
            }
            if (type.Contains("设定集"))
            {
                model.Type = PeripheryType.SetorAlbumEtc;
            }
            if (type.Contains("原声集"))
            {
                model.Type = PeripheryType.Ost;
            }
            if (type.Contains("套装"))
            {
                model.Type = PeripheryType.Set;
            }

            model.Size = sheet.GetStringValue("I" + index);
            model.Category = sheet.GetStringValue("J" + index);
            model.Brand = sheet.GetStringValue("K" + index);
            model.Author = sheet.GetStringValue("L" + index);
            model.Material = sheet.GetStringValue("M" + index);
            model.IndividualParts = sheet.GetStringValue("N" + index);
            model.IsReprint = sheet.GetStringValue("O" + index)=="是";
            model.IsAvailableItem = sheet.GetStringValue("P" + index) == "是";
            int count = 0;
            int.TryParse(sheet.GetStringValue("Q" + index), out count);
            model.PageCount = count;
            int.TryParse(sheet.GetStringValue("R" + index), out count);
            model.SongCount = count;
            model.LastEditTime = DateTime.Now;
            //读取相关词条

            return model;
        }

        public static void GetRelatedEntry(IWorksheet sheet, int index)
        {
            Periphery model = peripheries.FirstOrDefault(s => s.Name == sheet.GetStringValue("A" + index));
            if(model == null)
            {
                return;
            }

            var entriesString = sheet.GetStringValue("S" + index);
            if(entriesString==null)
            {
                return;
            }

            var entryNames = entriesString.Replace("，", ",").Replace("、", ",").Split(',');
            foreach (var entryName in entryNames)
            {
                var entry = CurrentGames.FirstOrDefault(s => s.Name == entryName);
                if (entry != null)
                {
                    model.Entries.Add(entry.Name);
                }
            }
        }

        public static void GetRelatedPeriphery(IWorksheet sheet, int index)
        {
            Periphery model = peripheries.FirstOrDefault(s => s.Name == sheet.GetStringValue("A" + index));
            if (model == null)
            {
                return;
            }

            var entriesString = sheet.GetStringValue("G" + index);
            if (entriesString == null)
            {
                return;
            }

            var entryNames = entriesString.Replace("，", ",").Replace("、", ",").Split(',');
            foreach (var entryName in entryNames)
            {
                var entry = peripheries.FirstOrDefault(s => s.Name == entryName);
                if (entry != null)
                {
                    model.Peripheries.Add(entry.Name);
                }
            }
        }


        /// <summary>
        /// 在获取网页信息之前准备数据模型
        /// </summary>
        private static void Chushihua()
        {
            Directory.CreateDirectory(path + "\\输出文件夹");
            Directory.CreateDirectory(path + "\\输出文件夹\\图片");
            //第一步 加载表格
            Workbook linshi_ = new Workbook();
            linshi_.LoadFromFile(path + "\\数据源文件夹\\基础.xlsx");
            biaoge_jichu = linshi_.Worksheets[0];

            linshi_ = new Workbook();
            linshi_.LoadFromFile(path + "\\数据源文件夹\\STAFF.xlsx");
            biaoge_staff = linshi_.Worksheets[0];

            linshi_ = new Workbook();
            linshi_.LoadFromFile(path + "\\数据源文件夹\\制作人.xlsx");
            biaoge_zhizuoren = linshi_.Worksheets[0];

            linshi_ = new Workbook();
            linshi_.LoadFromFile(path + "\\数据源文件夹\\角色.xlsx");
            biaoge_juese = linshi_.Worksheets[0];

            //第二步 从基本信息表中创建词条
            for (int i = 2; i <= 282; i++)
            {
                if (biaoge_jichu.Range["C" + i].Value2 != null && biaoge_jichu.Range["C" + i].Value2.ToString() != "")
                {
                    Entry_s.Add(CreateEntryGame(i, biaoge_jichu));
                    Console.WriteLine("[第二步]   <" + i + ">");
                }
            }
            //第三步 添加Staff列表到基本信息和关联信息中
            for (int i = 2; i <= 2272; i++)
            {
                if (biaoge_staff.Range["D" + i].Value2 != null && biaoge_staff.Range["D" + i].Value2.ToString() != "")
                {
                    CreateEntryStaffOut(i, biaoge_staff);
                    Console.WriteLine("[第三步]   <" + i + ">");

                }
            }
            //第四步 创建制作组词条 并添加游戏到自己的关联词条中
            for (int i = 2; i <= 282; i++)
            {
                if (biaoge_jichu.Range["C" + i].Value2 != null && biaoge_jichu.Range["B" + i].Value2 != null && !string.IsNullOrWhiteSpace(biaoge_jichu.Range["B" + i].Value2.ToString()) && !string.IsNullOrWhiteSpace(biaoge_jichu.Range["C" + i].Value2.ToString()))
                {
                    string groupNames = biaoge_jichu.Range["B" + i].Value2.ToString();
                    string[] groupName = groupNames.Replace(", ", ",").Replace("，", ",").Replace("、", ",").Split(',');
                    foreach (string item in groupName)
                    {
                        CreateEntryGroup(i, biaoge_jichu, item);
                    }
                    Console.WriteLine("[第四步]   <" + i + ">");

                }
            }
            //第五步 添加Staff词条自己的基本信息
            for (int i = 2; i <= 146; i++)
            {
                if (biaoge_zhizuoren.Range["A" + i].Value2 != null && !string.IsNullOrWhiteSpace(biaoge_zhizuoren.Range["A" + i].Value2.ToString()))
                {
                    AddEntrStaffOther(i, biaoge_zhizuoren);
                    Console.WriteLine("[第五步]   <" + i + ">");

                }
            }
            //第六步 创建角色词条 并为它自己添加基本信息和关联词条
            for (int i = 2; i <= 79; i++)
            {
                if (biaoge_juese.Range["A" + i].Value2 != null && !string.IsNullOrWhiteSpace(biaoge_juese.Range["A" + i].Value2.ToString()))
                {
                    CreateEntryRole(i, biaoge_juese);
                    Console.WriteLine("[第六步]   <" + i + ">");

                }
            }


            //webview1.Navigate(new Uri("https://www.cngal.org/games"));

            //第八步 根据数据库信息修正
            LoadHistoryDatabaseData();

            GetArticleDateFrom();
            GetGuideDataFrom();
            GetGameDataFromAsync();


            //第十步 遍历添加显示名称
            Console.WriteLine("[第十步]   <复制显示名称>");
            foreach (Entry item in Entry_s)
            {
                item.DisplayName = item.Name;
            }
            foreach (Article item in Article_s)
            {
                item.DisplayName = item.Name;
            }
            //第十一步 根据文章信息补全用户
            Console.WriteLine("[第十一步]   <根据文章信息补全用户>");
            GetArticleCreateUser();



            SaveArticle();
            SaveEntry();

            using (StreamWriter file = File.CreateText(path + "\\输出文件夹\\Users.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, users);
            }
            Console.WriteLine("导出数据成功！");
            Console.ReadLine();
        }

        public static void SaveArticle()
        {
            int MaxCount = 2000;
            int temp = 1;
            int i = 0;
            for (; i + MaxCount < Article_s.Count; temp++)
            {
                List<Article> list = Article_s.GetRange(i, MaxCount);
                using (StreamWriter file = File.CreateText($"{path}\\输出文件夹\\Articles_{temp}.json"))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(file, list);
                }
                Console.WriteLine("[第十二步]   <" + temp + ">");
                i += MaxCount;
            }
            if (i == 0)
            {
                i = MaxCount;
            }
            if (i >= Article_s.Count)
            {
                List<Article> list = Article_s.GetRange(i - MaxCount, Article_s.Count - 1);
                using (StreamWriter file = File.CreateText($"{path}\\输出文件夹\\Articles_{temp}.json"))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(file, list);
                }
                Console.WriteLine("[第十二步]   <" + temp + ">");
            }
        }

        public static void SaveEntry()
        {
            int MaxCount = 5000;
            int temp = 1;
            int i = 0;
            for (; i + MaxCount < Entry_s.Count; temp++)
            {
                List<Entry> list = Entry_s.GetRange(i, MaxCount);
                using (StreamWriter file = File.CreateText($"{path}\\输出文件夹\\Entries_{temp}.json"))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(file, list);
                }
                Console.WriteLine("[第十三步]   <" + temp + ">");
                i += MaxCount;

            }
            if (i == 0)
            {
                i = MaxCount;
            }
            if (i >= Entry_s.Count)
            {
                List<Entry> list = Entry_s.GetRange(i - MaxCount, Entry_s.Count - 1);
                using (StreamWriter file = File.CreateText($"{path}\\输出文件夹\\Entries_{temp}.json"))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(file, list);
                }
                Console.WriteLine("[第十二步]   <" + temp + ">");
            }
        }

        public static void GetGameDataFromAsync()
        {
            int jishu = 0;
            foreach (Game item in games)
            {
                jishu++;
                Console.WriteLine("[第九步]   <" + jishu + ">");
                //找到对应词条

                Entry entry_ = Entry_s.Find(s => s.Type == EntryType.Game && s.Name == item.Title);
                if (entry_ != null)
                {
                    //赋值

                    if (string.IsNullOrWhiteSpace(item.CoverImg) == false && item.CoverImg.Contains("https://") == false && item.CoverImg.Contains("http://") == false)
                    {
                        item.CoverImg = item.CoverImg.Replace("//", "https://");
                    }
                    entry_.MainPicture = string.IsNullOrWhiteSpace(entry_.SteamId) ? item.CoverImg : "https://media.st.dl.pinyuncloud.com/steam/apps/" + entry_.SteamId + "/header.jpg";// linshi[linshi.Length - 1];


                    entry_.MainPage = item.Content.Replace("_ueditor_page_break_tag_", "");
                    string sss2 = Regex.Replace(item.Content, "[a-zA-Z0-9]", "");
                    string tempStr = sss2.Replace("<", "").Replace(">", "").Replace("/", "").Replace("\\", "").Replace(";", "").Replace(":", "").Replace("\"", "").Replace(" ", "").Replace(".", "").Replace("=", "").Replace("-", "").Replace("_", "");
                    entry_.BriefIntroduction = tempStr.Substring(0, tempStr.Length > 120 ? 120 : tempStr.Length) + "......";
                }

            }
        }

        public static void GetGuideDataFrom()
        {
            int jishu = 0;
            foreach (Guide item in guides)
            {
                if (item.Title.Contains("deleted"))
                {
                    continue;
                }
                jishu++;
                Console.WriteLine("[第八步]   <" + jishu + ">");
                Article article_ = new Article
                {
                    Type = ArticleType.Strategy,
                    MainPage = item.Content.Replace("_ueditor_page_break_tag_", ""),
                    CreateUserId = item.UserId.ToString(),
                    OriginalAuthor = item.Author,
                    OriginalLink = " https://www.cngal.org/guide/" + item.Id + ".html",
                    PubishTime = item.CreatedAt,
                    Name = item.Title,
                    CreateTime = item.CreatedAt,
                    Relevances = new List<ArticleRelevance>()
                };
                Article_s.Add(article_);
                //寻找关联词条
                string[] linshi = item.GameIds.Replace(", ", ",").Split(',');
                foreach (string infor in linshi)
                {
                    Game game_ = games.Find(s => s.Id.ToString() == infor);
                    if (game_ != null)
                    {
                        Entry entry_ = Entry_s.Find(s => s.Name == game_.Title && s.Type == EntryType.Game);
                        if (entry_ != null)
                        {
                            entry_.Relevances.Add(new EntryRelevance { Modifier = "文章", DisplayName = item.Title });
                            article_.Relevances.Add(new ArticleRelevance { Modifier = "游戏", DisplayName = entry_.Name });
                        }


                    }
                }
                string sss2 = Regex.Replace(item.Content, "[a-zA-Z0-9]", "");
                string tempStr = sss2.Replace("<", "").Replace(">", "").Replace("/", "").Replace("\\", "").Replace(";", "").Replace(":", "").Replace("\"", "").Replace(" ", "").Replace(".", "").Replace("=", "").Replace("-", "").Replace("_", "");
                article_.BriefIntroduction = tempStr.Substring(0, tempStr.Length > 120 ? 120 : tempStr.Length) + "......";
            }
        }

        public static void GetArticleDateFrom()
        {
            int jishu = 0;
            foreach (Article_Database item in articles)
            {
                if (item.Title.Contains("deleted"))
                {
                    continue;
                }
                jishu++;
                Console.WriteLine("[第七步]   <" + jishu + ">");

                Article article_ = new Article
                {
                    Type = ArticleType.Tought,
                    MainPage = item.Content.Replace("_ueditor_page_break_tag_", ""),
                    CreateUserId = item.UserId.ToString(),
                    OriginalAuthor = item.Author,
                    OriginalLink = " https://www.cngal.org/article/" + item.Id + ".html",
                    PubishTime = item.CreatedAt,
                    Name = item.Title,
                    CreateTime = item.CreatedAt,
                    Relevances = new List<ArticleRelevance>()
                };

                //判断文章类别
                if (article_.Name.Contains("2020CNGAL") || article_.Name.Contains("评测") || article_.Name.Contains("测评") || article_.Name.Contains("简评") || article_.Name.Contains("鉴赏"))
                {
                    article_.Type = ArticleType.Evaluation;
                }
                else if (article_.Name.Contains("周边") || article_.Name.Contains("贩售介绍") || article_.Name.Contains("开箱"))
                {
                    article_.Type = ArticleType.Peripheral;
                }
                else if (article_.Name.Contains("专访") || article_.Name.Contains("访谈") || article_.Name.Contains("背后"))
                {
                    article_.Type = ArticleType.Interview;
                }
                //寻找关联词条
                string[] linshi = item.GameIds.Replace(", ", ",").Split(',');
                foreach (string infor in linshi)
                {
                    Game game_ = games.Find(s => s.Id.ToString() == infor);
                    if (game_ != null)
                    {
                        Entry entry_ = Entry_s.Find(s => s.Name == game_.Title && s.Type == EntryType.Game);
                        if (entry_ != null)
                        {
                            entry_.Relevances.Add(new EntryRelevance { Modifier = "动态", DisplayName = item.Title });

                            article_.Relevances.Add(new ArticleRelevance { Modifier = "游戏", DisplayName = entry_.Name });
                        }

                    }
                    if (article_.Type == ArticleType.Tought)
                    {
                        article_.Type = ArticleType.News;
                    }
                    string sss2 = Regex.Replace(item.Content, "[a-zA-Z0-9]", "");
                    string tempStr = sss2.Replace("<", "").Replace(">", "").Replace("/", "").Replace("\\", "").Replace(";", "").Replace(":", "").Replace("\"", "").Replace(" ", "").Replace(".", "").Replace("=", "").Replace("-", "").Replace("_", "");
                    article_.BriefIntroduction = tempStr.Substring(0, tempStr.Length > 120 ? 120 : tempStr.Length) + "......";
                }
                Article_s.Add(article_);
            }
        }

        public static void LoadHistoryDatabaseData()
        {
            using (StreamReader file = File.OpenText(path + "\\数据源文件夹\\Articles.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                articles = (List<Article_Database>)serializer.Deserialize(file, typeof(List<Article_Database>));
            }
            //筛选
            articles = articles.Where(s => s.State == 1).ToList();
            using (StreamReader file = File.OpenText(path + "\\数据源文件夹\\Guides.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                guides = (List<Guide>)serializer.Deserialize(file, typeof(List<Guide>));
            }
            guides = guides.Where(s => s.State == 1).ToList();
            using (StreamReader file = File.OpenText(path + "\\数据源文件夹\\Games.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                games = (List<Game>)serializer.Deserialize(file, typeof(List<Game>));
            }
            games = games.Where(s => s.State == 1).ToList();
            using (StreamReader file = File.OpenText(path + "\\数据源文件夹\\cngal_users.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                historyUsers = (List<HistoryUser>)serializer.Deserialize(file, typeof(List<HistoryUser>));
            }
            historyUsers = historyUsers.Where(s => s.state == 1).ToList();
        }

        public static void GetArticleCreateUser()
        {
            foreach (HistoryUser item in historyUsers)
            {
                users.Add(new ImportUserModel
                {
                    UserIdentity = item.id.ToString(),
                    LoginName = item.login_name,
                    UserName = item.nickname,
                    Email = item.email,
                    PasswordSalt = item.pwd_salt,
                    Password = Guid.NewGuid().ToString(),
                    // Integral = (int)(item.points*1.5),
                    //ContributionValue=item.points,
                    RegistTime = DateTime.ParseExact(item.created_at, "yyyy-MM-dd HH:mm:ss", null),
                    LastOnlineTime = DateTime.ParseExact(item.updated_at, "yyyy-MM-dd HH:mm:ss", null)
                });
            }
        }

        public static void AddUser(string name, string id)
        {
            if (users.Find(s => s.UserIdentity == id) == null)
            {
                users.Add(new ImportUserModel
                {
                    UserName = name,
                    Password = "123",
                    PasswordSalt = "123456",
                    Email = new Random().Next().ToString() + "@qq.com",
                    UserIdentity = id.ToString()
                });
            }

        }

        /// <summary>
        /// Downs the load file.
        /// </summary>
        /// <param name="url">api地址</param>
        /// <param name="savePath">文件保存路径</param>
        /// <returns></returns>
        public static async Task<string> DownLoadFile(string url, string savePath)
        {
            string newFileName = savePath;
            using (HttpClient httpClient = new HttpClient())
            {
                try
                {
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage response = httpClient.GetAsync(url).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        Stream ms = await response.Content.ReadAsStreamAsync();
                        using (BinaryReader br = new BinaryReader(ms))
                        {
                            byte[] data = br.ReadBytes((int)ms.Length);
                            File.WriteAllBytes(newFileName, data);

                        }
                    }
                }
                catch
                {
                    if (url[0] == 'h')
                    {
                        return "";
                    }
                    else
                    {
                        await DownLoadFile("https:" + url, savePath);
                    }


                    // Logger.Instance.Log($"http get error,url={url},exception:{e}", Category.Error);
                }
            }
            return newFileName;
        }

        private static GenderType GetGenderType_(string str)
        {
            if (str == "男")
            {
                return GenderType.Man;
            }
            else if (str == "女")
            {
                return GenderType.Women;
            }
            else if (str == "")
            {
                return GenderType.None;
            }
            else
            {
                return GenderType.Other;
            }
        }

        private static Entry CreateEntryRole(int hang, IWorksheet worksheet)
        {
            //第一步 定位到该行的标题
            if (worksheet.Range["A" + hang].Value2 == null && !string.IsNullOrWhiteSpace(worksheet.Range["A" + hang].Value2.ToString()))
            {
                throw new Exception("数据不能为空");
            }
            //删除重名的角色
            Entry entry = Entry_s.Find(s => s.Name == worksheet.Range["A" + hang].Value2.ToString());
            if (entry == null)
            {
                //第二步 读取标题和简介
                entry = new Entry()
                {
                    Information = new List<BasicEntryInformation>(),
                    Relevances = new List<EntryRelevance>(),
                    //Tags = new List<string>(),
                    Pictures = new List<EntryPicture>(),
                    Type = EntryType.Role
                };
                Entry_s.Add(entry);
            }


            entry.Name = worksheet.Range["A" + hang].Value2.ToString();
            if (worksheet.Range["N" + hang].Value2 != null && !string.IsNullOrWhiteSpace(worksheet.Range["N" + hang].Value2.ToString()))
            {
                entry.BriefIntroduction = worksheet.Range["N" + hang].Value2.ToString();
            }
            //第三步读取附加信息
            if (worksheet.Range["B" + hang].Value2 != null && !string.IsNullOrWhiteSpace(worksheet.Range["B" + hang].Value2.ToString()))
            {
                //这一步有关联词条可以添加
                entry.Information.Add(new BasicEntryInformation
                {
                    Modifier = "基本信息",
                    DisplayName = "声优",
                    DisplayValue = worksheet.Range["B" + hang].Value2.ToString()
                });
                entry.Relevances.Add(new EntryRelevance
                {
                    DisplayName = worksheet.Range["B" + hang].Value2.ToString(),
                    Modifier = "STAFF"
                });
            }
            if (worksheet.Range["C" + hang].Value2 != null && !string.IsNullOrWhiteSpace(worksheet.Range["C" + hang].Value2.ToString()))
            {
                entry.AnotherName = worksheet.Range["C" + hang].Value2.ToString();
            }
            if (worksheet.Range["D" + hang].Value2 != null && !string.IsNullOrWhiteSpace(worksheet.Range["D" + hang].Value2.ToString()))
            {
                entry.Information.Add(new BasicEntryInformation
                {
                    Modifier = "基本信息",
                    DisplayName = "性别",
                    DisplayValue = GetGenderType_(worksheet.Range["D" + hang].Value2.ToString()).ToString()
                });
            }
            if (worksheet.Range["E" + hang].Value2 != null && !string.IsNullOrWhiteSpace(worksheet.Range["E" + hang].Value2.ToString()))
            {
                entry.Information.Add(new BasicEntryInformation
                {
                    Modifier = "基本信息",
                    DisplayName = "身材数据",
                    DisplayValue = worksheet.Range["E" + hang].Value2.ToString()
                });
            }
            if (worksheet.Range["F" + hang].Value2 != null && !string.IsNullOrWhiteSpace(worksheet.Range["F" + hang].Value2.ToString()))
            {
                entry.Information.Add(new BasicEntryInformation
                {
                    Modifier = "基本信息",
                    DisplayName = "身材(主观)",
                    DisplayValue = worksheet.Range["F" + hang].Value2.ToString()
                });
            }
            try
            {
                if (worksheet.Range["G" + hang].Value2 != null && !string.IsNullOrWhiteSpace(worksheet.Range["G" + hang].Value2.ToString()))
                {
                    entry.Information.Add(new BasicEntryInformation
                    {
                        Modifier = "基本信息",
                        DisplayName = "生日",
                        DisplayValue = worksheet.Range["G" + hang].Value2.ToString()
                    });
                }

            }
            catch
            {

            }
            if (worksheet.Range["H" + hang].Value2 != null && !string.IsNullOrWhiteSpace(worksheet.Range["H" + hang].Value2.ToString()))
            {
                entry.Information.Add(new BasicEntryInformation
                {
                    Modifier = "基本信息",
                    DisplayName = "发色",
                    DisplayValue = worksheet.Range["H" + hang].Value2.ToString()
                });
            }
            if (worksheet.Range["I" + hang].Value2 != null && !string.IsNullOrWhiteSpace(worksheet.Range["I" + hang].Value2.ToString()))
            {
                entry.Information.Add(new BasicEntryInformation
                {
                    Modifier = "基本信息",
                    DisplayName = "瞳色",
                    DisplayValue = worksheet.Range["I" + hang].Value2.ToString()
                });
            }
            if (worksheet.Range["J" + hang].Value2 != null && !string.IsNullOrWhiteSpace(worksheet.Range["J" + hang].Value2.ToString()))
            {
                entry.Information.Add(new BasicEntryInformation
                {
                    Modifier = "基本信息",
                    DisplayName = "服饰",
                    DisplayValue = worksheet.Range["J" + hang].Value2.ToString()
                });

            }
            if (worksheet.Range["K" + hang].Value2 != null && !string.IsNullOrWhiteSpace(worksheet.Range["K" + hang].Value2.ToString()))
            {
                entry.Information.Add(new BasicEntryInformation
                {
                    Modifier = "基本信息",
                    DisplayName = "性格",
                    DisplayValue = worksheet.Range["K" + hang].Value2.ToString()
                });

            }
            if (worksheet.Range["L" + hang].Value2 != null && !string.IsNullOrWhiteSpace(worksheet.Range["L" + hang].Value2.ToString()))
            {
                entry.Information.Add(new BasicEntryInformation
                {
                    Modifier = "基本信息",
                    DisplayName = "角色身份",
                    DisplayValue = worksheet.Range["L" + hang].Value2.ToString()
                });

            }
            if (worksheet.Range["M" + hang].Value2 != null && !string.IsNullOrWhiteSpace(worksheet.Range["M" + hang].Value2.ToString()))
            {
                entry.Relevances.Add(new EntryRelevance
                {
                    DisplayName = worksheet.Range["M" + hang].Value2.ToString(),
                    Modifier = "游戏"
                });
                //查找关联作品 添加
                // 寻找对应的游戏词条 添加自己关联信息
                foreach (Entry item in Entry_s)
                {
                    if (item.Name == worksheet.Range["M" + hang].Value2.ToString())
                    {
                        //STAFF表会在外层添加 这里只注意关联词条

                        //关联词条
                        item.Relevances.Add(new EntryRelevance
                        {
                            DisplayName = entry.Name,
                            Modifier = "角色"
                        });
                    }
                }
            }
            return entry;
        }

        private static void AddEntrStaffOther(int hang, IWorksheet staff)
        {
            if (staff.Range["A" + hang].Value2 == null && !string.IsNullOrWhiteSpace(staff.Range["A" + hang].Value2.ToString()))
            {
                throw new Exception("数据不能为空");
            }
            //第一步 寻找或创建词条
            Entry entry = null;
            string name = staff.Range["A" + hang].Value2.ToString();
            foreach (Entry item in Entry_s)
            {
                if (item.Name == name)
                {
                    entry = item;
                }
            }
            if (entry == null)
            {
                entry = new Entry
                {
                    Type = EntryType.Staff,
                    Information = new List<BasicEntryInformation>(),
                    Relevances = new List<EntryRelevance>(),
                    // Tags = new List<string>(),
                    Pictures = new List<EntryPicture>()
                };
                Entry_s.Add(entry);
            }
            //第二步 添加基本信息
            if (staff.Range["A" + hang].Value2 != null && !string.IsNullOrWhiteSpace(staff.Range["A" + hang].Value2.ToString()))
            {
                entry.Information.Add(new BasicEntryInformation
                {
                    Modifier = "基本信息",
                    DisplayName = "昵称（官方称呼）",
                    DisplayValue = staff.Range["A" + hang].Value2.ToString()
                });
            }
            if (staff.Range["D" + hang].Value2 != null && !string.IsNullOrWhiteSpace(staff.Range["D" + hang].Value2.ToString()))
            {
                AddStaffWeb(entry, staff.Range["D" + hang].Value2.ToString());
            }

        }

        private static void AddStaffWeb(Entry entry, string str)
        {
            string[] linshi1 = str.Replace(", ", ",").Split(',');
            foreach (string item in linshi1)
            {
                string[] linshi2 = item.Split('|');
                if (linshi2.Length == 2)
                {
                    entry.Information.Add(new BasicEntryInformation
                    {
                        Modifier = "相关网站",
                        DisplayName = linshi2[0],
                        DisplayValue = linshi2[1]
                    });
                }
            }
        }

        private static Entry CreateEntryGroup(int hang, IWorksheet game, string name)
        {
            //第一步创建制作组词条并赋值
            //查找这个制作组书否存在
            Entry entry = Entry_s.Find(s => s.Name == name);
            if (entry == null)
            {
                entry = new Entry
                {
                    Type = EntryType.ProductionGroup,
                    Name = name,
                    Information = new List<BasicEntryInformation>(),
                    Relevances = new List<EntryRelevance>(),
                    //Tags = new List<string>(),
                    Pictures = new List<EntryPicture>()
                };
                Entry_s.Add(entry);
            }
            if (game.Range["D" + hang].Value2 != null && game.Range["D" + hang].Value2.ToString() != "")
            {
                entry.AnotherName = game.Range["D" + hang].Value2.ToString();
            }
            //把游戏添加到关联词条中
            if (game.Range["C" + hang].Value2 != null && !string.IsNullOrWhiteSpace(game.Range["C" + hang].Value2.ToString()))
            {
                string gameName = game.Range["C" + hang].Value2.ToString();
                bool isAdded = false;
                foreach (EntryRelevance item in entry.Relevances)
                {
                    if (item.DisplayValue == gameName)
                    {
                        isAdded = true;
                        break;
                    }

                }
                if (isAdded == false)
                {
                    //这一行数据还要添加关联词条
                    entry.Relevances.Add(new EntryRelevance
                    {
                        DisplayName = gameName,
                        Modifier = "游戏"
                    });
                }
            }
            return entry;
        }

        private static void CreateEntryStaffOut(int hang, IWorksheet staff)
        {
            //第一步定位到昵称列 分割名字
            if (staff.Range["D" + hang].Value2 == null && !string.IsNullOrWhiteSpace(staff.Range["D" + hang].Value2.ToString()))
            {
                throw new Exception("数据不能为空");
            }
            string[] linshi1 = staff.Range["D" + hang].Value2.ToString().Replace(", ", ",").Split(',');
            foreach (string item in linshi1)
            {
                CreateEntryStaffIn(hang, item, staff);
            }
            // 第二步 添加STAFF表关联
            foreach (Entry item in Entry_s)
            {
                if (item.Name == staff.Range["A" + hang].Value2.ToString())
                {
                    string str1 = staff.Range["C" + hang].Value2.ToString().Replace("、", ",");
                    string str2 = staff.Range["D" + hang].Value2.ToString();
                    string 子项目 = staff.Range["B" + hang].Value2.ToString();
                    string 角色 = staff.Range["E" + hang].Value2.ToString();
                    string 组织 = staff.Range["F" + hang].Value2.ToString();
                    string str3 = staff.Range["G" + hang].Value2.ToString();


                    string[] linshi = str2.Replace(", ", ",").Split(',');
                    foreach (string temp in linshi)
                    {
                        if (string.IsNullOrWhiteSpace(temp))
                        {
                            continue;
                        }
                        if (string.IsNullOrWhiteSpace(str3))
                        {
                            str3 = temp;
                        }
                        string 职位通用 = GetPositionGeneralType_(str3).ToString();
                        //STAFF表
                        List<BasicEntryInformationAdditional> linshi3 = new List<BasicEntryInformationAdditional>();
                        if (子项目 != null && !string.IsNullOrWhiteSpace(子项目))
                        {

                            linshi3.Add(new BasicEntryInformationAdditional
                            {
                                DisplayName = "子项目",
                                DisplayValue = 子项目
                            });
                        }
                        if (str1 != null && !string.IsNullOrWhiteSpace(str1))
                        {
                            linshi3.Add(new BasicEntryInformationAdditional
                            {
                                DisplayName = "职位（官方称呼）",
                                DisplayValue = str1
                            });
                        }
                        if (temp != null && !string.IsNullOrWhiteSpace(temp))
                        {
                            linshi3.Add(new BasicEntryInformationAdditional
                            {
                                DisplayName = "昵称（官方称呼）",
                                DisplayValue = temp
                            });
                        }
                        if (角色 != null && !string.IsNullOrWhiteSpace(角色))
                        {
                            linshi3.Add(new BasicEntryInformationAdditional
                            {
                                DisplayName = "角色",
                                DisplayValue = 角色
                            });
                        }
                        if (组织 != null && !string.IsNullOrWhiteSpace(组织))
                        {
                            linshi3.Add(new BasicEntryInformationAdditional
                            {
                                DisplayName = "组织",
                                DisplayValue = 组织
                            });
                        }
                        if (职位通用 != null && !string.IsNullOrWhiteSpace(职位通用))
                        {
                            linshi3.Add(new BasicEntryInformationAdditional
                            {
                                DisplayName = "职位（通用）",
                                DisplayValue = 职位通用
                            });
                        }
                        item.Information.Add(new BasicEntryInformation
                        {
                            DisplayName = str1,
                            DisplayValue = temp,
                            Modifier = "STAFF",
                            Additional = linshi3
                        });
                    }


                }
            }
        }

        public static PositionGeneralType GetPositionGeneralType_(string str)
        {

            if (str == "视频")
            {
                return PositionGeneralType.Video;
            }
            else if (str == "设计")
            {
                return PositionGeneralType.Design;
            }
            else if (str == "音乐")
            {
                return PositionGeneralType.Music;
            }
            else if (str == "作词")
            {
                return PositionGeneralType.ComposingWords;
            }
            else if (str == "演唱")
            {
                return PositionGeneralType.Singing;
            }
            else if (str == "剧本")
            {
                return PositionGeneralType.Script;
            }
            else if (str == "配音")
            {
                return PositionGeneralType.CV;
            }
            else if (str == "演出")
            {
                return PositionGeneralType.Show;
            }
            else if (str == "美术")
            {
                return PositionGeneralType.FineArts;
            }
            else if (str == "程序")
            {
                return PositionGeneralType.Program;
            }
            else if (str == "运营")
            {
                return PositionGeneralType.Operate;
            }
            else if (str == "发行")
            {
                return PositionGeneralType.Issue;
            }
            else if (str == "制作")
            {
                return PositionGeneralType.Make;
            }
            else if (str == "PV")
            {
                return PositionGeneralType.PV;
            }
            else if (str == "后期")
            {
                return PositionGeneralType.LaterStage;
            }
            else if (str == "主催")
            {
                return PositionGeneralType.MainUrge;
            }
            else if (str == "策划")
            {
                return PositionGeneralType.Plan;
            }
            else
            {
                return PositionGeneralType.Other;
            }

        }

        private static void CreateEntryStaffIn(int hang, string name, IWorksheet staff)
        {
            //第一步 检查该词条是否被创建
            Entry entry = Entry_s.Find(s => s.Name == name);
            if (entry == null)
            {
                entry = new Entry
                {
                    Name = name,
                    Type = EntryType.Staff,
                    Information = new List<BasicEntryInformation>(),
                    Relevances = new List<EntryRelevance>(),
                    //  Tags = new List<string>(),
                    Pictures = new List<EntryPicture>()
                };
                Entry_s.Add(entry);
            }
            //第二步 在staff词条中添加关联作品
            if (staff.Range["A" + hang].Value2 != null && staff.Range["A" + hang].Value2.ToString() != "")
            {
                bool isAdd = false;
                string temp = staff.Range["A" + hang].Value2.ToString();
                foreach (EntryRelevance item in entry.Relevances)
                {
                    if (item.DisplayName == temp)
                    {
                        isAdd = true;
                        break;
                    }
                }
                if (isAdd == false)
                {
                    entry.Relevances.Add(new EntryRelevance
                    {
                        DisplayName = temp,
                        Modifier = "游戏"
                    });
                }

            }
            //第二步 在staff词条中添加关联角色
            if (staff.Range["E" + hang].Value2 != null && staff.Range["E" + hang].Value2.ToString() != "")
            {

                string temp_ = staff.Range["E" + hang].Value2.ToString();
                string[] temps = temp_.Replace(", ", ",").Split(',');
                foreach (string temp in temps)
                {
                    bool isAdd = false;
                    foreach (EntryRelevance item in entry.Relevances)
                    {
                        if (item.DisplayName == temp)
                        {
                            isAdd = true;
                            break;
                        }
                    }
                    if (isAdd == false)
                    {
                        entry.Relevances.Add(new EntryRelevance
                        {
                            DisplayName = temp,
                            Modifier = "角色"
                        });

                        //创建角色
                        //删除重名的角色
                        Entry entry_role = Entry_s.Find(s => s.Name == temp);
                        if (entry_role == null)
                        {
                            entry_role = new Entry()
                            {
                                Name = temp,
                                Information = new List<BasicEntryInformation>(),
                                Relevances = new List<EntryRelevance>(),
                                // Tags = new List<string>(),
                                Pictures = new List<EntryPicture>(),
                                Type = EntryType.Role
                            };
                            //将创建的角色添加到列表中
                            Entry_s.Add(entry_role);
                        }
                        isAdd = false;
                        foreach (EntryRelevance item in entry_role.Relevances)
                        {
                            if (item.DisplayName == name)
                            {
                                isAdd = true;
                                break;
                            }
                        }
                        if (isAdd == false)
                        {
                            if (entry_role.Relevances.Any(s => s.DisplayName == name && s.Modifier == "STAFF") == false)
                            {
                                entry_role.Relevances.Add(new EntryRelevance
                                {
                                    DisplayName = name,
                                    Modifier = "STAFF"
                                });
                            }
                            if (entry_role.Information.Any(s => s.DisplayName == "声优" && s.Modifier == "基本信息" && s.DisplayValue == name) == false)
                            {
                                entry_role.Information.Add(new BasicEntryInformation
                                {
                                    DisplayName = "声优",
                                    Modifier = "基本信息",
                                    DisplayValue = name
                                });
                            }

                            if (staff.Range["A" + hang].Value2 != null && staff.Range["A" + hang].Value2.ToString() != "")
                            {

                                entry_role.Relevances.Add(new EntryRelevance
                                {
                                    DisplayName = staff.Range["A" + hang].Value2.ToString(),
                                    Modifier = "游戏"
                                });
                            }
                        }

                    }

                }
            }
            //第三步 寻找对应的游戏词条 添加自己关联信息
            /*   foreach (var item in Entry_s)
               {
                   if (item.Name == staff.Range["A" + hang].Value2.ToString())
                   {
                       //STAFF表会在外层添加 这里只注意关联词条

                       //关联词条
                       item.Relevances.Add(new EntryRelevance_
                       {
                           DisplayName = entry.Name,
                           Modifier = "STAFF"
                       });

                       return;
                   }
               }*/
        }

        private static Entry CreateEntryGame(int hang, IWorksheet worksheet)
        {
            //第一步 定位到该行的标题
            if (worksheet.Range["C" + hang].Value2 == null && worksheet.Range["C" + hang].Value2.ToString() != "")
            {
                throw new Exception("数据不能为空");
            }

            //第二步 读取标题和简介
            Entry entry = new Entry()
            {
                Information = new List<BasicEntryInformation>(),
                Relevances = new List<EntryRelevance>(),
                //   Tags = new List<string>(),
                Pictures = new List<EntryPicture>(),
                Type = EntryType.Game
            };
            entry.Name = worksheet.Range["C" + hang].Value2.ToString();
            if (worksheet.Range["P" + hang].Value2 != null && worksheet.Range["P" + hang].Value2.ToString() != "")
            {
                entry.BriefIntroduction = worksheet.Range["P" + hang].Value2.ToString();

            }

            //第三步读取附加信息
            if (worksheet.Range["A" + hang].Value2 != null && worksheet.Range["A" + hang].Value2.ToString() != "")
            {
                string timeString = worksheet.Range["A" + hang].Value2.ToString();
                try
                {
                    entry.PubulishTime = DateTime.ParseExact(timeString, "G", null);
                }
                catch
                {
                    try
                    {
                        entry.PubulishTime = DateTime.ParseExact(timeString, "D", null);
                    }
                    catch
                    {

                    }

                }
                if (entry.PubulishTime != null)
                {
                    entry.Information.Add(new BasicEntryInformation
                    {
                        Modifier = "基本信息",
                        DisplayName = "发行时间",
                        DisplayValue = ((DateTime)entry.PubulishTime).ToString("yyyy年M月d日")
                    });
                }
            }
            if (worksheet.Range["B" + hang].Value2 != null && worksheet.Range["B" + hang].Value2.ToString() != "")
            {
                string groupNames = worksheet.Range["B" + hang].Value2.ToString();
                string[] groupName = groupNames.Replace("，", ",").Replace("、", ",").Replace(", ", ",").Split(',');
                foreach (string item in groupName)
                {

                    entry.Relevances.Add(new EntryRelevance
                    {
                        DisplayName = item,
                        Modifier = "制作组"
                    });
                }
                //这一行数据还要添加关联词条
                entry.Information.Add(new BasicEntryInformation
                {
                    Modifier = "基本信息",
                    DisplayName = "制作组",
                    DisplayValue = groupNames
                });
            }

            if (worksheet.Range["E" + hang].Value2 != null && worksheet.Range["E" + hang].Value2.ToString() != "")
            {
                entry.AnotherName = worksheet.Range["E" + hang].Value2.ToString();
            }
            if (worksheet.Range["F" + hang].Value2 != null && worksheet.Range["F" + hang].Value2.ToString() != "")
            {
                entry.Information.Add(new BasicEntryInformation
                {
                    Modifier = "基本信息",
                    DisplayName = "发行商",
                    DisplayValue = worksheet.Range["F" + hang].Value2.ToString()
                });
            }
            if (worksheet.Range["G" + hang].Value2 != null && worksheet.Range["G" + hang].Value2.ToString() != "")
            {
                entry.Information.Add(new BasicEntryInformation
                {
                    Modifier = "基本信息",
                    DisplayName = "原作",
                    DisplayValue = worksheet.Range["G" + hang].Value2.ToString()
                });
            }
            if (worksheet.Range["H" + hang].Value2 != null && worksheet.Range["H" + hang].Value2.ToString() != "")
            {
                entry.Information.Add(new BasicEntryInformation
                {
                    Modifier = "基本信息",
                    DisplayName = "类型",
                    DisplayValue = worksheet.Range["H" + hang].Value2.ToString()
                });
            }
            if (worksheet.Range["I" + hang].Value2 != null && worksheet.Range["I" + hang].Value2.ToString() != "")
            {
                entry.Information.Add(new BasicEntryInformation
                {
                    Modifier = "基本信息",
                    DisplayName = "引擎",
                    DisplayValue = worksheet.Range["I" + hang].Value2.ToString()
                });
            }
            if (worksheet.Range["J" + hang].Value2 != null && worksheet.Range["J" + hang].Value2.ToString() != "")
            {
                entry.Information.Add(new BasicEntryInformation
                {
                    Modifier = "基本信息",
                    DisplayName = "游戏平台",
                    DisplayValue = worksheet.Range["J" + hang].Value2.ToString().Replace(",", "、")
                });
            }
            if (worksheet.Range["K" + hang].Value2 != null && worksheet.Range["K" + hang].Value2.ToString() != "")
            {
                entry.Information.Add(new BasicEntryInformation
                {
                    Modifier = "基本信息",
                    DisplayName = "Steam平台Id",
                    DisplayValue = worksheet.Range["K" + hang].Value2.ToString()
                });
                entry.SteamId = worksheet.Range["K" + hang].Value2.ToString();
            }

            if (worksheet.Range["L" + hang].Value2 != null && worksheet.Range["L" + hang].Value2.ToString() != "")
            {
                GetOtherLink(worksheet.Range["L" + hang].Value2.ToString(), entry);
            }
            if (worksheet.Range["M" + hang].Value2 != null && worksheet.Range["M" + hang].Value2.ToString() != "")
            {
                GetWebString(worksheet.Range["M" + hang].Value2.ToString(), entry);
            }
            if (worksheet.Range["N" + hang].Value2 != null && worksheet.Range["N" + hang].Value2.ToString() != "")
            {
                HuoquGuanlianZuoping_In_Jiben(worksheet.Range["N" + hang].Value2.ToString(), entry);
            }
            if (worksheet.Range["O" + hang].Value2 != null && worksheet.Range["O" + hang].Value2.ToString() != "")
            {
                entry.Information.Add(new BasicEntryInformation
                {
                    Modifier = "基本信息",
                    DisplayName = "QQ群",
                    DisplayValue = worksheet.Range["O" + hang].Value2.ToString()
                });
            }
            return entry;
        }

        private static void HuoquGuanlianZuoping_In_Jiben(string guan, Entry entry)
        {
            string[] linshi1 = guan.Replace(", ", ",").Split(',');
            foreach (string item in linshi1)
            {
                entry.Relevances.Add(new EntryRelevance
                {
                    DisplayName = item,
                    Modifier = "游戏"
                });
            }
        }


        private static void GetWebString(string web, Entry entry)
        {
            if (web.Contains("|") == false)
            {
                entry.Information.Add(new BasicEntryInformation
                {
                    Modifier = "基本信息",
                    DisplayName = "官网",
                    DisplayValue = web
                });
                return;
            }
            else
            {
                bool isFirst = true;
                string[] linshi1 = web.Replace(", ", ",").Split(',');
                foreach (string item in linshi1)
                {
                    string[] linshi2 = item.Split('|');
                    if (linshi2.Length == 2)
                    {
                        entry.Information.Add(new BasicEntryInformation
                        {
                            Modifier = "相关网站",
                            DisplayName = linshi2[0],
                            DisplayValue = linshi2[1]
                        });
                        if (isFirst)
                        {
                            entry.Information.Add(new BasicEntryInformation
                            {
                                Modifier = "基本信息",
                                DisplayName = "官网",
                                DisplayValue = linshi2[1]
                            });
                            isFirst = false;
                        }
                    }
                }
                return;
            }
        }

        private static void GetOtherLink(string links, Entry entry)
        {
            string[] temps = links.Split(',');
            foreach (string temp in temps)
            {
                string[] infors = temp.Split('|');
                if (infors.Length == 2)
                {
                    if (infors[0] != "Steam")
                    {
                        string[] infor = infors[1].Split('(');
                        string link;
                        if (infor.Length == 0)
                        {
                            continue;
                        }
                        else if (infor.Length == 1)
                        {
                            link = infor[0];
                        }
                        else
                        {
                            link = infor[1].Replace(")", "");
                        }
                        entry.Relevances.Add(new EntryRelevance
                        {
                            DisplayName = infors[0],
                            DisplayValue = link,
                            Link = link,
                            Modifier = "其他"
                        });
                    }
                }
            }
        }

    }

}
