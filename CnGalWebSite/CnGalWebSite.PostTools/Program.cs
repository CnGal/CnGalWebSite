// See https://aka.ms/new-console-template for more information

using System;
using System.Net.Http.Headers;
using System.Net.Http;
using CnGalWebSite.DataModel.Model;
using System.Net.Http.Json;
using CnGalWebSite.DataModel.Helper;
using System.IO;
using CnGalWebSite.DataModel.ViewModel.HistoryData;
using System.Text.Json;
using CnGalWebSite.DataModel.ViewModel.Entries;
using CnGalWebSite.DataModel.ViewModel.Files;
using Newtonsoft.Json;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;
using HtmlAgilityPack;
using System.Xml.Linq;
using CnGalWebSite.DataModel.ViewModel.Space;
using CnGalWebSite.DataModel.ViewModel.Articles;
using CnGalWebSite.Helper.Extensions;
using Markdig;
using System.Net;
using System.Text;
using System.Security.Policy;

namespace CnGalWebSite.PostTools // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        private static readonly HttpClient client = new HttpClient();
        private static Setting setting = new Setting();
        private static ImageHelper imageHelper = new ImageHelper(client, setting);
        private static PostData postData = new PostData(client, setting);
        private static List<string> links = new List<string>();
        private static CurrentData currentData = new CurrentData(client, setting);

        //因为要用到HttpRequest请求页面，所以这里设置了请求的头部信息useragents，可以让服务器以为这是浏览器发起的请求。
        private readonly static string[] usersagents = new string[] {
            "Mozilla/5.0 (Linux; U; Android 2.3.7; en-us; Nexus One Build/FRF91) AppleWebKit/533.1 (KHTML, like Gecko) Version/4.0 Mobile Safari/533.1",
            "MQQBrowser/26 Mozilla/5.0 (Linux; U; Android 2.3.7; zh-cn; MB200 Build/GRJ22; CyanogenMod-7) AppleWebKit/533.1 (KHTML, like Gecko) Version/4.0 Mobile Safari/533.1",
            "JUC (Linux; U; 2.3.7; zh-cn; MB200; 320*480) UCWEB7.9.3.103/139/999",
            "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:7.0a1) Gecko/20110623 Firefox/7.0a1 Fennec/7.0a1",
            "Opera/9.80 (Android 2.3.4; Linux; Opera Mobi/build-1107180945; U; en-GB) Presto/2.8.149 Version/11.10",
            "Mozilla/5.0 (Linux; U; Android 3.0; en-us; Xoom Build/HRI39) AppleWebKit/534.13 (KHTML, like Gecko) Version/4.0 Safari/534.13",
            "Mozilla/5.0 (iPhone; U; CPU iPhone OS 3_0 like Mac OS X; en-us) AppleWebKit/420.1 (KHTML, like Gecko) Version/3.0 Mobile/1A542a Safari/419.3",
            "Mozilla/5.0 (iPhone; U; CPU iPhone OS 4_0 like Mac OS X; en-us) AppleWebKit/532.9 (KHTML, like Gecko) Version/4.0.5 Mobile/8A293 Safari/6531.22.7",
            "Mozilla/5.0 (iPad; U; CPU OS 3_2 like Mac OS X; en-us) AppleWebKit/531.21.10 (KHTML, like Gecko) Version/4.0.4 Mobile/7B334b Safari/531.21.10",
            "Mozilla/5.0 (BlackBerry; U; BlackBerry 9800; en) AppleWebKit/534.1+ (KHTML, like Gecko) Version/6.0.0.337 Mobile Safari/534.1+",
            "Mozilla/5.0 (hp-tablet; Linux; hpwOS/3.0.0; U; en-US) AppleWebKit/534.6 (KHTML, like Gecko) wOSBrowser/233.70 Safari/534.6 TouchPad/1.0",
            "Mozilla/5.0 (SymbianOS/9.4; Series60/5.0 NokiaN97-1/20.0.019; Profile/MIDP-2.1 Configuration/CLDC-1.1) AppleWebKit/525 (KHTML, like Gecko) BrowserNG/7.1.18124",
            "Mozilla/5.0 (compatible; MSIE 9.0; Windows Phone OS 7.5; Trident/5.0; IEMobile/9.0; HTC; Titan)",
            "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36",
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_10_1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2227.1 Safari/537.36",
            "Mozilla/5.0 (X11; U; Linux x86_64; zh-CN; rv:1.9.2.10) Gecko/20100922 Ubuntu/10.10 (maverick) Firefox/3.6.10",
            "Mozilla/5.0 (Windows NT 5.1; U; en; rv:1.8.1) Gecko/20061208 Firefox/2.0.0 Opera 9.50",
            "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/534.57.2 (KHTML, like Gecko) Version/5.1.7 Safari/534.57.2",
            "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/30.0.1599.101 Safari/537.36",
            "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; WOW64; Trident/5.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; .NET4.0C; .NET4.0E; LBBROWSER) ",
            "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; WOW64; Trident/5.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; .NET4.0C; .NET4.0E; QQBrowser/7.0.3698.400)",
            "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/38.0.2125.122 UBrowser/4.0.3214.0 Safari/537.36",
            "Mozilla/5.0 (Linux; U; Android 2.2.1; zh-cn; HTC_Wildfire_A3333 Build/FRG83D) AppleWebKit/533.1 (KHTML, like Gecko) Version/4.0 Mobile Safari/533.1",
            "Mozilla/5.0 (BlackBerry; U; BlackBerry 9800; en) AppleWebKit/534.1+ (KHTML, like Gecko) Version/6.0.0.337 Mobile Safari/534.1+",
            "Mozilla/5.0 (compatible; MSIE 9.0; Windows Phone OS 7.5; Trident/5.0; IEMobile/9.0; HTC; Titan)",
            "Mozilla/4.0 (compatible; MSIE 6.0; ) Opera/UCWEB7.0.2.37/28/999",
            "Openwave/ UCWEB7.0.2.37/28/999",
            "NOKIA5700/ UCWEB7.0.2.37/28/999",
            "UCWEB7.0.2.37/28/999",
            "Mozilla/5.0 (hp-tablet; Linux; hpwOS/3.0.0; U; en-US) AppleWebKit/534.6 (KHTML, like Gecko) wOSBrowser/233.70 Safari/534.6 TouchPad/1.0",
            "Mozilla/5.0 (Linux; U; Android 3.0; en-us; Xoom Build/HRI39) AppleWebKit/534.13 (KHTML, like Gecko) Version/4.0 Safari/534.13",
            "Opera/9.80 (Android 2.3.4; Linux; Opera Mobi/build-1107180945; U; en-GB) Presto/2.8.149 Version/11.10",
            "Mozilla/5.0 (iPad; U; CPU OS 4_3_3 like Mac OS X; en-us) AppleWebKit/533.17.9 (KHTML, like Gecko) Version/5.0.2 Mobile/8J2 Safari/6533.18.5",
         };

        static async Task Main(string[] args)
        {
            await Proc();
        }

        static async Task Proc()
        {
            Console.WriteLine("=================================================");
            Console.WriteLine("           CnGal资料站 投稿工具 v0.3");
            Console.WriteLine("=================================================");

            Console.WriteLine("[1] 读取配置文件");
            setting.Init();
            setting.Load();
            Console.WriteLine("[2] 读取图片缓存");
            imageHelper.Load();

            Console.WriteLine("[3] 读取已处理的文章");
            postData.Load();

            Console.WriteLine("[4] 获取现有的词条和文章");
            await currentData.LoadAsync();

            Console.WriteLine("[5] 登入账户");

            await LoginAsync();


            //尝试读取数据文件
            bool isFirst = true;
            Console.WriteLine("[6] 读取链接");
            while (links.Count == 0)
            {
                try
                {
                    using var fs1 = new FileStream(setting.DataPath, FileMode.Open, FileAccess.Read);
                    using var sr1 = new StreamReader(fs1);
                    while (sr1.EndOfStream == false)
                    {
                        var str = await sr1.ReadLineAsync();
                        if (string.IsNullOrWhiteSpace(str) == false)
                        {
                            links.Add(str);
                        }
                    }
                }
                catch (Exception ex)
                {
                    File.Create(setting.DataPath).Close();
                }

                if (isFirst && links.Any() == false)
                {
                    Console.WriteLine($"请在{setting.DataPath}文件中输入要导入的链接，每行填写一个链接，目前只支持知乎链接，填写完毕后按下回车");
                    Console.ReadKey();

                }
                isFirst = false;
            }
            Console.WriteLine($"[6] 已读取{links.Count}条链接");

            //循环读取链接并生成审核模型
            Console.WriteLine("[7] 开始处理链接");

            foreach (var item in links)
            {
                var zhihuArticle = postData.articles.FirstOrDefault(s => s.Url == item);
                if (zhihuArticle != null)
                {
                    if (zhihuArticle.PostTime != null)
                    {
                        OutputHelper.Write(OutputLevel.Warning, $"[7] 链接 {item} 已于 {zhihuArticle.PostTime} 提交审核[Id:{zhihuArticle.Id}]");
                        continue;
                    }
                }
                else
                {
                    zhihuArticle = await GetArticleContext(item);
                    postData.articles.Add(zhihuArticle);
                    postData.Save();
                }

                if (currentData.articles.Any(s => s == zhihuArticle.Title))
                {
                    OutputHelper.Write(OutputLevel.Dager, $"[7] 链接 {item} 与现有文章《{zhihuArticle.Title}》重名，请手动上传");
                    continue;
                }

                await imageHelper.CutImage(zhihuArticle);

                var article = GenerateArticle(zhihuArticle);

                zhihuArticle.Id = await SubmitArticle(article);
                zhihuArticle.PostTime = DateTime.Now.ToCstTime();
                postData.Save();

                OutputHelper.Write( OutputLevel.Infor,$"[7] 处理完成第 {links.IndexOf(item) + 1} 条链接");
            }

            //成功
            Console.WriteLine("=================================================");
            Console.WriteLine($"[8] 总计处理{links.Count}条链接，感谢您使用本投稿工具");
            Console.WriteLine("=================================================");
            Console.WriteLine("按任意键关闭");
            Console.ReadKey();
        }

        static async Task<long> SubmitArticle(CreateArticleViewModel model)
        {
            try
            {

                var result = await client.PostAsJsonAsync<CreateArticleViewModel>(ToolHelper.WebApiPath + "api/articles/createarticle", model);
                string jsonContent = result.Content.ReadAsStringAsync().Result;
                Result obj = System.Text.Json.JsonSerializer.Deserialize<Result>(jsonContent, ToolHelper.options);
                //判断结果
                if (obj.Successful == false)
                {
                    throw new Exception(obj.Error);
                }
                Console.WriteLine($"[7] 已提交{model.Main.Type.GetDisplayName()}《{model.Main.Name}》审核[Id:{obj.Error}]");
                return long.Parse(obj.Error);

            }
            catch (Exception ex)
            {
                OutputHelper.PressError(ex, "提交文章审核失败");
                return 0;
            }
        }


        static async Task LoginAsync()
        {
            while (true)
            {
                if (string.IsNullOrWhiteSpace(setting.Token))
                {
                    Console.WriteLine("请输入登入令牌，可以在个人空间->编辑个人资料->获取登入令牌中获取，输入完毕后请按下回车");
                    setting.Token = Console.ReadLine();
                }


                try
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", setting.Token);

                    //调用刷新接口
                    var result = await client.GetFromJsonAsync<LoginResult>(ToolHelper.WebApiPath + "api/account/RefreshJWToken");
                    if (result.Code == LoginResultCode.OK)
                    {
                        setting.Token = result.Token;
              
                        break;
                    }
                    else
                    {
                        setting.Token = null;
           

                    }
                }
                catch (Exception ex)
                {
                    setting.Token = null;
    
                    OutputHelper.PressError(ex, "登入失败", "令牌无效");
                }
            }

            try
            {
                var model = await client.GetFromJsonAsync<PersonalSpaceViewModel>(ToolHelper.WebApiPath + "api/space/GetUserView/");
                setting.UserName = model.BasicInfor.Name;
            }
            catch (Exception ex)
            {
                setting.Token = null;
        
                OutputHelper.PressError(ex, "获取用户信息失败");
            }

            setting.Save();
        }

        static CreateArticleViewModel GenerateArticle(ZhiHuArticleModel model)
        {
            CreateArticleViewModel article = new CreateArticleViewModel
            {
                Name = model.Title,
                Main = new EditArticleMainViewModel
                {
                    Name = model.Title,
                    DisplayName = model.Title,
                    MainPicture = model.Image,
                    PubishTime = model.PublishTime,
                    RealNewsTime = model.PublishTime,
                    OriginalLink = model.Url,
                    BriefIntroduction = GetArticleBriefIntroduction(model.MainPage, 50),
                    OriginalAuthor=string.IsNullOrWhiteSpace( model.OriginalAuthor)? setting.UserName : model.OriginalAuthor,
                },
                MainPage = new EditArticleMainPageViewModel
                {
                    Context = model.MainPage,
                }
            };

            //判断类型
            foreach (var item in setting.ArticleTypeStrings)
            {
                foreach (var temp in item.Texts)
                {
                    if (article.Name.Contains(temp))
                    {
                        article.Main.Type = item.Type;
                        break;
                    }
                }
                if (article.Main.Type != ArticleType.Tought)
                {
                    break;
                }
            }

            //默认值为评测
            if(article.Main.Type == ArticleType.Tought)
            {
                article.Main.Type = ArticleType.Evaluation;
            }

            //查找关联词条
            foreach (var item in currentData.games.Where(s => s != "幻觉" && s != "画师"))
            {
                if (model.MainPage.Contains(item))
                {
                    article.Relevances.Games.Add(new DataModel.ViewModel.RelevancesModel
                    {
                        DisplayName = item,
                    });
                }

            }
            foreach (var item in currentData.games)
            {

                if (model.Title.Contains(item))
                {
                    article.Relevances.Games.Add(new DataModel.ViewModel.RelevancesModel
                    {
                        DisplayName = item
                    });
                }
            }
            return article;
        }

        static string GetArticleBriefIntroduction(string mainPage, int maxLength)
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

            return title.Abbreviate(maxLength).Replace("\n", "").Replace("\r", "").Replace("image", "");
        }

        static async Task<ZhiHuArticleModel> GetArticleContext(string url)
        {
            try
            {
                var article = new ZhiHuArticleModel();

                HttpWebRequest request = null;
                HttpWebResponse response = null;
                Stream stream = null;
                StreamReader reader = null;
                string str = string.Empty;
                Encoding encoding = Encoding.Default;

                //请求地址
                request = (HttpWebRequest)WebRequest.Create(url);
                //随机使用一个useragents
                var randomNumber =new Random().Next(0, usersagents.Length);
                request.UserAgent = usersagents[randomNumber];
                request.Timeout = 30000;
                request.ServicePoint.Expect100Continue = false;
                request.KeepAlive = false;
                response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    using (stream = response.GetResponseStream())
                    {
                        reader = new StreamReader(stream);
                        str = reader.ReadToEnd();
                        stream.Close();
                    }
                }


                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(str);

                var node = document.GetElementbyId("root");
                string htmlStr = "";
                string name = "";
                string image = null;
                string author = null;
                //正文
                try
                {
                    htmlStr = node.FirstChild.ChildNodes[2].FirstChild.ChildNodes.MaxBy(s => s.InnerText.Length).ChildNodes.MaxBy(s=>s.InnerText.Length).LastChild.LastChild.InnerHtml;
                }
                catch
                {

                }
                //主图
                try
                {
                    var tempNode = node.FirstChild.ChildNodes[2].FirstChild.ChildNodes.FirstOrDefault(s => s.Name == "img");
                    if (tempNode == null)
                    {
                        tempNode = node.FirstChild.ChildNodes[2].FirstChild.ChildNodes.FirstOrDefault(s => s.OuterHtml.Contains("background-image"));
                        if(tempNode!=null)
                        {
                            image= ToolHelper.MidStrEx(tempNode.OuterHtml, "url(", "?source");
                        }
                    }
                    else
                    {
                        image = ToolHelper.MidStrEx(tempNode.OuterHtml, "src=\"", "\"");
                    }
                }
                catch
                {

                }
                //标题
                try
                {
                    name = node.FirstChild.ChildNodes[2].FirstChild.ChildNodes.MaxBy(s => s.InnerText.Length).ChildNodes[0].FirstChild.InnerText;
                }
                catch
                {

                }
                //作者
                try
                {
                    author = node.FirstChild.ChildNodes[2].FirstChild.ChildNodes.MaxBy(s => s.InnerText.Length).ChildNodes[0].ChildNodes[1].FirstChild.FirstChild.LastChild.FirstChild.InnerText;
                }
                catch
                {

                }
                //时间
                try
                {
                    var times = node.FirstChild.ChildNodes[2].FirstChild.ChildNodes.MaxBy(s => s.InnerText.Length).ChildNodes.Where(s => s.InnerText.Contains("发布于") || s.InnerText.Contains("编辑于")).Select(s => s.InnerText.Replace("发布于 ","").Replace("编辑于 ", ""));
                    foreach (var item in times)
                    {
                        try
                        {
                            article.PublishTime = DateTime.ParseExact(item, "yyyy-MM-dd HH:mm", null);
                            break;
                        }
                        catch
                        {

                        }

                    }
                }
                catch
                {

                }

                var converter = new ReverseMarkdown.Converter();



                article.MainPage = converter.Convert(htmlStr);
                article.MainPage = await imageHelper.ProgressImage(article.MainPage);
                article.Title = name;
                article.Url = url;
                article.OriginalAuthor = ToolHelper.MidStrEx(article.MainPage, "原作者：", "\r");
                if (string.IsNullOrWhiteSpace(article.OriginalAuthor) && author != null)
                {
                    article.OriginalAuthor = author;
                }
                article.Image = image ?? ToolHelper.GetImageLinks(article.MainPage).FirstOrDefault();
                return article;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }



    public class PostData
    {
        public readonly List<ZhiHuArticleModel> articles = new List<ZhiHuArticleModel>();
        private readonly Setting setting;
        private readonly HttpClient client;

        public PostData(HttpClient _client, Setting _setting)
        {
            client = _client;
            setting = _setting;
        }

        public void Load()
        {
            try
            {
                var path = Path.Combine(setting.TempPath, "articles.json");

                using (StreamReader file = File.OpenText(path))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    articles.AddRange((List<ZhiHuArticleModel>)serializer.Deserialize(file, typeof(List<ZhiHuArticleModel>)));
                }
            }
            catch
            {
                Save();
            }

        }
        public void Save()
        {
            var path = Path.Combine(setting.TempPath, "articles.json");

            using (StreamWriter file = File.CreateText(path))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, articles);
            }
        }
    }

    public class CurrentData
    {
        public readonly List<string> games = new List<string>();
        public readonly List<string> articles = new List<string>();
        private readonly Setting setting;
        private readonly HttpClient client;

        public CurrentData(HttpClient _client, Setting _setting)
        {
            client = _client;
            setting = _setting;
        }

        public async Task LoadAsync()
        {
            try
            {
                games.AddRange(await client.GetFromJsonAsync<List<string>>(ToolHelper.WebApiPath + "api/entries/GetAllEntries/0"));
                //获取所有文章
                articles.AddRange(await client.GetFromJsonAsync<List<string>>(ToolHelper.WebApiPath + "api/articles/GetAllArticles"));

            }
            catch (Exception ex)
            {
                OutputHelper.PressError(ex, "获取所有词条文章名称失败");
            }

        }
    }

    public class ImageHelper
    {
        private readonly HttpClient client;
        private readonly Setting setting;
        public readonly List<OriginalImageToDrawingBedUrl> images = new List<OriginalImageToDrawingBedUrl>();


        public ImageHelper(HttpClient _client, Setting _setting)
        {
            client = _client;
            setting = _setting;
        }

        public void Load()
        {
            try
            {
                var path = Path.Combine(setting.TempPath, "images.json");

                using (StreamReader file = File.OpenText(path))
                {
                    Newtonsoft.Json.JsonSerializer serializer = new JsonSerializer();
                    images.AddRange((List<OriginalImageToDrawingBedUrl>)serializer.Deserialize(file, typeof(List<OriginalImageToDrawingBedUrl>)));
                }
            }
            catch
            {
                Save();
            }

        }
        public void Save()
        {
            var path = Path.Combine(setting.TempPath, "images.json");

            using (StreamWriter file = File.CreateText(path))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, images);
            }
        }

        public async Task<string> UploadImageAsync(string url, double x = 0, double y = 0)
        {
            try
            {
                var result = await client.PostAsJsonAsync(setting.TransferDepositFileAPI + "api/files/TransferDepositFile", new TransferDepositFileModel
                {
                    Url = url,
                    X = x,
                    Y = y
                });

                var jsonContent = result.Content.ReadAsStringAsync().Result;
                var obj = System.Text.Json.JsonSerializer.Deserialize<Result>(jsonContent, ToolHelper.options);

                if (obj.Successful)
                {
                    var temp= obj.Error.Replace("http://local.host/", "https://pic.cngal.top/");
                    Console.WriteLine("上传完成：" + temp);

                    return temp;
                }
                else
                {
                    OutputHelper.Write(OutputLevel.Dager, "上传失败：" + url);
                    return url;
                }
            }
            catch (Exception)
            {
                return url;
            }

        }

        public async Task CutImage(ZhiHuArticleModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Image) == false && model.IsCutImage == false)
            {
                model.Image = await UploadImageAsync(model.Image, 460, 215);
                model.IsCutImage = model.Image.Contains("pic.cngal");
                if (model.IsCutImage)
                {
                    Console.WriteLine("成功裁剪主图");
                }
                else
                {
                    OutputHelper.Write(OutputLevel.Dager, "裁剪主图失败，请在提交完成后手动修正");
                }
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
                    var newImage = await GetImage(image);

                    text = text.Replace("<figure" + ToolHelper.MidStrEx(text, "<figure", "</figure>") + "</figure>", "\n![image](" + newImage + ")\n");
                }
            }

            return text;
        }

        public async Task<string> GetImage(string url)
        {
            var image = images.FirstOrDefault(s => s.OldUrl == url);
            if (image == null)
            {
                var newUrl = await UploadImageAsync(url);
                images.Add(new OriginalImageToDrawingBedUrl
                {
                    NewUrl = newUrl,
                    OldUrl = url
                });
                Save();
                //await Task.Delay(1500);
                return newUrl;
            }
            else
            {
                return image.NewUrl;
            }
        }
    }

    public static class OutputHelper
    {
        public static void PressError(Exception ex, string message = "", string reason = "服务器网络异常", string resolvent = "检查网络是否正常，加群761794704反馈")
        {
            Write( OutputLevel.Dager,$"> {message}\n> 报错：{ex.Message}\n> 原因：{reason}\n> 解决方法：{resolvent}");
        }

        public static void Write(OutputLevel level, string text)
        {
            switch (level)
            {
                case OutputLevel.Infor:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case OutputLevel.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case OutputLevel.Dager:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
            }
            Console.WriteLine(text);
            Console.ForegroundColor = ConsoleColor.White;
        }
    }

    public enum OutputLevel
    {
        Infor,
        Warning,
        Dager
    }

    public class Setting
    {
        public string Token { get; set; }

        public string DataPath { get; set; } = "Links.txt";

        public string TempPath { get; set; } =  "Data";

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

                using (StreamReader file = File.OpenText(path))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    var temp = (Setting)serializer.Deserialize(file, typeof(Setting));

                    Token = temp.Token;
                    DataPath = temp.DataPath;
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

            using (StreamWriter file = File.CreateText(path))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, this);
            }
        }
    }

    public class ArticleTypeString
    {
        public ArticleType Type { get; set; }

        public List<string> Texts { get; set; } = new List<string>();
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



