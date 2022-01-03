using HtmlAgilityPack;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using CnGalWebSite.APIServer.Application.Files;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using TencentCloud.Ocr.V20181119.Models;
using Newtonsoft.Json;
using CnGalWebSite.DataModel.ViewModel.Entries;
using System.Security.Policy;
using CnGalWebSite.DataModel.ViewModel.HistoryData;
using TencentCloud.Mrs.V20200910.Models;
using CnGalWebSite.DataModel.Helper;
using System.Linq;
using CnGalWebSite.DataModel.Model;
using Markdig;
using CnGalWebSite.APIServer.Application.Helper;

namespace CnGalWebSite.APIServer.Application.HistoryData
{
    public class HistoryDataService : IHistoryDataService
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IConfiguration _configuration;
        private readonly IFileService _fileService;
        private readonly IAppHelper _appHelper;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private List<OriginalImageToDrawingBedUrl> _images;

        public HistoryDataService(IHttpClientFactory clientFactory, IConfiguration configuration, IFileService fileService, IWebHostEnvironment webHostEnvironment, IAppHelper appHelper)
        {
            _clientFactory = clientFactory;
            _configuration = configuration;
            _fileService = fileService;
            _webHostEnvironment = webHostEnvironment;
            _appHelper = appHelper;
            InitImages();
        }

        public void InitImages()
        {
            try
            {
                var path = Path.Combine(_webHostEnvironment.WebRootPath, "BackUp", "当前数据", "OriginalImageToDrawingBedUrls.json");

                using (StreamReader file = File.OpenText(path))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    _images = (List<OriginalImageToDrawingBedUrl>)serializer.Deserialize(file, typeof(List<OriginalImageToDrawingBedUrl>));
                }

            }
            catch
            {
                _images = new List<OriginalImageToDrawingBedUrl>();
                SaveImages();
            }

        }
        public void SaveImages()
        {
            var path = Path.Combine(_webHostEnvironment.WebRootPath, "BackUp", "当前数据", "OriginalImageToDrawingBedUrls.json");

            using (StreamWriter file = File.CreateText(path))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, _images);
            }
        }
        public List<ZhiHuArticleModel> InitZhiHuArticles()
        {
            try
            {
                var path = Path.Combine(_webHostEnvironment.WebRootPath, "BackUp", "当前数据", "ZhiHuArticles.json");

                using (StreamReader file = File.OpenText(path))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    return (List<ZhiHuArticleModel>)serializer.Deserialize(file, typeof(List<ZhiHuArticleModel>));
                }

            }
            catch
            {
                SaveZhiHuArticles(new List<ZhiHuArticleModel>());
                return new List<ZhiHuArticleModel>(); 
            }

        }
        public void SaveZhiHuArticles(List<ZhiHuArticleModel> zhiHuArticles)
        {
            var path = Path.Combine(_webHostEnvironment.WebRootPath, "BackUp", "当前数据", "ZhiHuArticles.json");

            using (StreamWriter file = File.CreateText(path))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, zhiHuArticles);
            }
        }

        public List<Article> InitArticles()
        {
            try
            {
                var path = Path.Combine(_webHostEnvironment.WebRootPath, "BackUp", "当前数据", "Articles.json");

                using (StreamReader file = File.OpenText(path))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    return (List<Article>)serializer.Deserialize(file, typeof(List<Article>));
                }

            }
            catch
            {
                SaveArticles(new List<Article>());
                return new List<Article>();
            }

        }
        public void SaveArticles(List<Article> articles)
        {
            var path = Path.Combine(_webHostEnvironment.WebRootPath, "BackUp", "当前数据", "Articles.json");

            using (StreamWriter file = File.CreateText(path))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, articles);
            }
        }

        /// <summary>
        /// 生成知乎文章批量导入的Json文件
        /// </summary>
        public async Task GenerateZhiHuArticleImportJson()
        {
            try
            {
                //获取文章标题和链接
                List<KeyValuePair<string, string>> articles = await GetArticleNameAndUrl();
                var currentArticles = LoadCurrentDatabaseArticleData();
                var currentEntries = LoadCurrentDatabaseEntryData();

                //对比之前的文章 排除相同项目
                var deleteArticles =new List<string>();
                int count = 0;
                foreach(var article in articles)
                {
                    foreach(var item in currentArticles)
                    {
                        if(GetStringSimilarity(item.Name,article.Key)>0.9)
                        {
                            deleteArticles.Add(article.Key);
                        }
                    }
                    count++;
                    Console.WriteLine($"【1】已比较完成第 {count} 篇文章");
                }

                articles.RemoveAll(s=>deleteArticles.Contains(s.Key));
                //获取各个文章并替换图片等内容
                List<ZhiHuArticleModel> zhiHuArticles = InitZhiHuArticles();
                count = 0;
                foreach(var item in articles)
                {
                    if (zhiHuArticles.Any(s => s.Title == item.Key))
                    {
                        
                    }
                    else
                    {
                        zhiHuArticles.Add(await GetArticleContext(item.Value, item.Key));
                        SaveImages();
                        SaveZhiHuArticles(zhiHuArticles);
                    }
                 
                    count++;
                    Console.WriteLine($"【2】已获取完成第 {count} 篇文章");
                }
                //裁剪主图
                foreach (var item in zhiHuArticles)
                {
                    await CutImage(item);
                    SaveZhiHuArticles(zhiHuArticles);
                }
                //生成文章
                List<Article> realArticles = new List<Article>();
                var entryNames = currentEntries.Select(s => s.Name).ToList();
                foreach (var item in zhiHuArticles)
                {
                    realArticles.Add(GenerateArticle(item, entryNames));
                }
                //生成Json
                SaveArticles(realArticles);

            }
            catch (Exception ex)
            {

            }

        }


        public Article GenerateArticle(ZhiHuArticleModel model, List<string> entries)
        {
            Article article = new Article
            {
                Name = model.Title,
                DisplayName = model.Title,
                MainPage = model.MainPage,
                MainPicture = model.Image,
                CreateTime = model.PublishTime,
                PubishTime = model.PublishTime,
                RealNewsTime = model.PublishTime,
                LastEditTime = model.PublishTime,
                CreateUserId = "cb8f735c-1ec4-464c-9e6c-ce7cacad4082",
                OriginalLink = model.Url,
                BriefIntroduction = GetArticleBriefIntroduction(model.MainPage, 50)
            };
            if (model.Title.Contains("国G七日报") || model.Title.Contains("2021中文恋爱游戏大赛参赛制作组") || model.Title.Contains("Steam"))
            {
                article.Type = ArticleType.News;
            }
            else if (model.Title.Contains("国G人物志"))
            {
                article.Type = ArticleType.Tought;
            }
            else if (model.Title.Contains("国G再演绎") || model.Title.Contains("风之起兮") || model.Title.Contains("未来的书函") || model.Title.Contains("邮差总按两次铃"))
            {
                article.Type = ArticleType.Fan;
            }
            else if (model.Title.Contains("国G晒晒晒"))
            {
                article.Type = ArticleType.Peripheral;
            }
            else if (model.Title.Contains("访谈") || model.Title.Contains("专访"))
            {
                article.Type = ArticleType.Interview;
            }
            else if (model.Title.Contains("评测") || model.Title.Contains("测评")||model.Title.Contains("浅评"))
            {
                article.Type = ArticleType.Evaluation;
            }
            else if (model.Title.Contains("国G晒晒晒"))
            {
                article.Type = ArticleType.Peripheral;
            }

            //查找关联词条
            foreach (var item in entries.Where(s => s != "幻觉"&&s!="画师"))
            {
                if (model.MainPage.Contains(item))
                {
                    article.Relevances.Add(new ArticleRelevance
                    {
                        DisplayName = item,
                        Modifier = "游戏"
                    });
                }

            }
            foreach (var item in entries)
            {

                if (model.Title.Contains(item))
                {
                    article.Relevances.Add(new ArticleRelevance
                    {
                        DisplayName = item,
                        Modifier = "游戏"
                    });
                }
            }

            if(model.Title.Contains("风之起兮")|| model.Title.Contains("钟奇物语：不存在的浅色回忆"))
            {
                article.MainPicture = "https://pic.cngal.top/images/2022/01/03/ad270b158661.jpg";
                article.Relevances.Add(new ArticleRelevance
                {
                    DisplayName = "恋爱绮谭～不存在的夏天～",
                    Modifier = "游戏"
                });
            }

            if (model.Title.Contains("彼岸终望"))
            {
                article.MainPicture = "https://pic.cngal.top/images/2022/01/03/516c52a12d01.png";
                article.Relevances.Add(new ArticleRelevance
                {
                    DisplayName = "葬花·暗黑桃花源",
                    Modifier = "游戏"
                });
            }

            if (model.Title.Contains("朝露同"))
            {
                article.MainPicture = "https://pic.cngal.top/images/2022/01/03/26d131caaf36.png";
                article.Relevances.Add(new ArticleRelevance
                {
                    DisplayName = "爱人·Lover",
                    Modifier = "游戏"
                });
            }

            if (model.Title.Contains("好心分手"))
            {
                article.MainPicture = "https://pic.cngal.top/images/2022/01/03/89e63525c92b.jpg";
                article.Relevances.Add(new ArticleRelevance
                {
                    DisplayName = "他人世界末",
                    Modifier = "游戏"
                });
            }
            if (model.Title.Contains("暖寒")||model.Title.Contains("雪霁")||model.Title.Contains("《千面》同人文"))
            {
                article.MainPicture = "https://media.st.dl.pinyuncloud.com/steam/apps/1168470/header.jpg";
                article.Relevances.Add(new ArticleRelevance
                {
                    DisplayName = "千面",
                    Modifier = "游戏"
                });
            }
            if (model.Title.Contains("未来的书函"))
            {
                article.MainPicture = "https://pic.cngal.top/images/2022/01/03/1b058bd55187.jpg";
                article.Relevances.Add(new ArticleRelevance
                {
                    DisplayName = "高考恋爱一百天",
                    Modifier = "游戏"
                });
            }

            if (model.Title.Contains("《他人世界末》沧海月明")||model.Title.Contains("《他人世界末》木竹樱—心中"))
            {
                article.MainPicture = "https://pic.cngal.top/images/2022/01/03/e3694dd5b7dd.png";
                article.Relevances.Add(new ArticleRelevance
                {
                    DisplayName = "他人世界末",
                    Modifier = "游戏"
                });
            }
            if (model.Title.Contains("《夜永》同人文"))
            {
                article.MainPicture = "https://pic.cngal.top/images/2022/01/03/f9f8ed865d52.jpg";
                article.Relevances.Add(new ArticleRelevance
                {
                    DisplayName = "夜永",
                    Modifier = "游戏"
                });
            }
            if (model.Title.Contains("邮差总按两次铃"))
            {
                article.MainPicture = "https://pic.cngal.top/images/2022/01/03/9504370f3dc5.jpg";
                article.Relevances.Add(new ArticleRelevance
                {
                    DisplayName = "他人世界末",
                    Modifier = "游戏"
                });
            }
            if (model.Title.Contains("《恋爱绮谭》同人文"))
            {
                article.MainPicture = "https://pic.cngal.top/images/2022/01/03/c6075d0b18a9.jpg";
                article.Relevances.Add(new ArticleRelevance
                {
                    DisplayName = "恋爱绮谭～不存在的夏天～",
                    Modifier = "游戏"
                });
            }
            return article;
        }
        public string GetArticleBriefIntroduction(string mainPage, int maxLength)
        {
            var title = "";
            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().UseSoftlineBreakAsHardlineBreak().Build();
            title = Markdown.ToPlainText(mainPage, pipeline);
            if (title.Split(": ").Length > 1)
            {
                title = title.Split(": ")[1];
            }

            //去除 图片 
            do
            {
                var midStr = ToolHelper.MidStrEx(title, "[", "]");
                title = title.Replace($"[{midStr}]", "");

            } while (string.IsNullOrWhiteSpace(ToolHelper.MidStrEx(title, "[", "]")) == false);
            //去除 话题
            do
            {
                var midStr = ToolHelper.MidStrEx(title, "#", "#");
                title = title.Replace($"#{midStr}#", "");

            } while (string.IsNullOrWhiteSpace(ToolHelper.MidStrEx(title, "#", "#")) == false);

            return _appHelper.GetStringAbbreviation(title, maxLength).Replace("\n","").Replace("\r", "").Replace("image", "");
        }
        public async Task CutImage(ZhiHuArticleModel model)
        {
            if(string.IsNullOrWhiteSpace(model.Image)==false&&model.IsCutImage==false)
            {
                model.Image = await _fileService.SaveImageAsync(model.Image, _configuration["NewsAdminId"], 460, 215);
                model.IsCutImage = true;
                Console.WriteLine("裁剪完成主图：" + model.Title);
            }
        }

        public async Task<ZhiHuArticleModel> GetArticleContext(string url, string name)
        {
            try
            {
                var article = new ZhiHuArticleModel();

                using var client = _clientFactory.CreateClient();

                var result = await client.GetAsync(url);
                var str = await result.Content.ReadAsStringAsync();

                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(str);

                var node = document.GetElementbyId("root");
                string htmlStr = "";
                try
                {
                    htmlStr = node.FirstChild.ChildNodes[2].FirstChild.ChildNodes[2].ChildNodes[1].LastChild.LastChild.InnerHtml;
                }
                catch
                {
                    htmlStr = node.FirstChild.ChildNodes[2].FirstChild.ChildNodes[1].ChildNodes[1].LastChild.LastChild.InnerHtml;
                }

                try
                {
                    var time = node.FirstChild.ChildNodes[2].FirstChild.ChildNodes[2].ChildNodes[2].InnerText.Replace("发布于 ","").Replace("编辑于 ", "");
                    article.PublishTime = DateTime.ParseExact(time, "yyyy-MM-dd HH:mm", null);
                }
                catch
                {
                    var time = node.FirstChild.ChildNodes[2].FirstChild.ChildNodes[1].ChildNodes[2].InnerText.Replace("发布于 ", "").Replace("编辑于 ", "");
                    article.PublishTime = DateTime.ParseExact(time, "yyyy-MM-dd HH:mm", null);

                }

                var converter = new ReverseMarkdown.Converter();

               
               
                article.MainPage = converter.Convert(htmlStr);
                article.MainPage =await ProgressImage(article.MainPage);
                article.Title = name;
                article.Url = url;
                article.Image = ToolHelper.GetImageLinks(article.MainPage).FirstOrDefault();
                return article;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<string> ProgressImage(string text)
        {

            var figures = text.Split("</figure>");
            foreach (var item in figures)
            {
                var textTemp = item.Split("<figure");
                if (textTemp.Length > 1)
                {
                    var image = ToolHelper.MidStrEx(textTemp[1], "data-original=\"", "\">");
                    var newImage = await UploadImage(image);

                    text = text.Replace("<figure"+ToolHelper.MidStrEx(text, "<figure", "</figure>") + "</figure>", "\n![image](" + newImage + ")\n");
                }
            }

            return text;
        }

        public async Task<string> UploadImage(string url)
        {
            var image = _images.FirstOrDefault(s => s.OldUrl == url);
            if (image == null)
            {
                var newUrl = await _fileService.SaveImageAsync(url, _configuration["NewsAdminId"]);
                _images.Add(new OriginalImageToDrawingBedUrl
                {
                    NewUrl = newUrl,
                    OldUrl = url
                });
                //await Task.Delay(1500);
                Console.WriteLine("上传完成：" + newUrl);
                return newUrl;
            }
            else
            {
                return image.NewUrl;
            }
        }

        public async Task<List<KeyValuePair<string, string>>> GetArticleNameAndUrl()
        {
            try
            {
                List<KeyValuePair<string, string>> articles = new List<KeyValuePair<string, string>>();

                var path = Path.Combine(_webHostEnvironment.WebRootPath, "BackUp", "莫言国G-知乎备份");

                DirectoryInfo directoryInfo = new DirectoryInfo(path);

                var files= directoryInfo.GetFiles();

                foreach (var file in files)
                {
                    var Name = file.Name.Replace(" - 知乎.mhtml", "");
                    using var fs1 = new FileStream(file.FullName, FileMode.Open, FileAccess.Read);
                    using var sr1 = new StreamReader(fs1);

                    await sr1.ReadLineAsync();
                    var Url = (await sr1.ReadLineAsync()).Replace("Snapshot-Content-Location: ", "");
                    articles.Add(new KeyValuePair<string, string>(Name, Url));
                }

                return articles;
            }
            catch (Exception ex)
            {
                return new List<KeyValuePair<string, string>>();
            }

        }


        public  List<NameIdPairModel>  LoadCurrentDatabaseArticleData()
        {
            var path = Path.Combine(_webHostEnvironment.WebRootPath, "BackUp", "当前数据", "CurrentArticles.json");

            using (StreamReader file = File.OpenText(path ))
            {
                JsonSerializer serializer = new JsonSerializer();
               return  (List<NameIdPairModel>)serializer.Deserialize(file, typeof(List<NameIdPairModel>));
            }
        }

        public List<NameIdPairModel> LoadCurrentDatabaseEntryData()
        {
            var path = Path.Combine(_webHostEnvironment.WebRootPath, "BackUp", "当前数据", "CurrentGames.json");


            using (StreamReader file = File.OpenText(path))
            {
                JsonSerializer serializer = new JsonSerializer();
                var temp = (List<string>)serializer.Deserialize(file, typeof(List<string>));
                var model = new List<NameIdPairModel>();
                foreach (var item in temp)
                {
                    model.Add(new NameIdPairModel
                    {
                        Id = 0,
                        Name = item
                    });
                }

                return model;
            }
        }

        public static double GetStringSimilarity(string str1, string str2)
        {
            LevenshteinDistance levenshteinDistance = new LevenshteinDistance();
            return (double)levenshteinDistance.LevenshteinDistancePercent(str1, str2);
        }
    }

    public class LevenshteinDistance
    {

        private static LevenshteinDistance _instance = null;
        public static LevenshteinDistance Instance
        {
            get
            {
                if (_instance == null)
                {
                    return new LevenshteinDistance();
                }
                return _instance;
            }
        }


        /// <summary>
        /// 取最小的一位数
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <param name="third"></param>
        /// <returns></returns>
        public int LowerOfThree(int first, int second, int third)
        {
            int min = first;
            if (second < min)
                min = second;
            if (third < min)
                min = third;
            return min;
        }

        public int Levenshtein_Distance(string str1, string str2)
        {
            int[,] Matrix;
            int n = str1.Length;
            int m = str2.Length;

            int temp = 0;
            char ch1;
            char ch2;
            int i = 0;
            int j = 0;
            if (n == 0)
            {
                return m;
            }
            if (m == 0)
            {

                return n;
            }
            Matrix = new int[n + 1, m + 1];

            for (i = 0; i <= n; i++)
            {
                //初始化第一列
                Matrix[i, 0] = i;
            }

            for (j = 0; j <= m; j++)
            {
                //初始化第一行
                Matrix[0, j] = j;
            }

            for (i = 1; i <= n; i++)
            {
                ch1 = str1[i - 1];
                for (j = 1; j <= m; j++)
                {
                    ch2 = str2[j - 1];
                    if (ch1.Equals(ch2))
                    {
                        temp = 0;
                    }
                    else
                    {
                        temp = 1;
                    }
                    Matrix[i, j] = LowerOfThree(Matrix[i - 1, j] + 1, Matrix[i, j - 1] + 1, Matrix[i - 1, j - 1] + temp);


                }
            }

            for (i = 0; i <= n; i++)
            {
                for (j = 0; j <= m; j++)
                {
                   // Console.Write(" {0} ", Matrix[i, j]);
                }
                //Console.WriteLine("");
            }
            return Matrix[n, m];

        }

        /// <summary>
        /// 计算字符串相似度
        /// </summary>
        /// <param name="str1"></param>
        /// <param name="str2"></param>
        /// <returns></returns>
        public decimal LevenshteinDistancePercent(string str1, string str2)
        {
            if (str1 == "" || str2 == "")
            {
                return 0;
            }
            int maxLenth = str1.Length > str2.Length ? str1.Length : str2.Length;
            int val = Levenshtein_Distance(str1, str2);
            return 1 - (decimal)val / maxLenth;
        }
    }
}
