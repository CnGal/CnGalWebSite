using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Articles;
using CnGalWebSite.Helper.Extensions;
using CnGalWebSite.Helper.Helper;
using HtmlAgilityPack;
using Markdig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.PostTools
{
    public class OutlinkArticleX
    {
        private readonly SettingX _setting;
        private readonly HttpClient _httpClient;
        private readonly ImageX _image;
        private readonly HostDataX _hostData;
        private readonly HtmlX _html;
        private readonly HistoryX _history;

        public OutlinkArticleX(HttpClient httpClient, SettingX setting, ImageX image, HostDataX hostData, HtmlX html, HistoryX history)
        {
            _setting = setting;
            _httpClient = httpClient;
            _image = image;
            _hostData = hostData;
            _history = history;
            _html = html;
        }

        public async Task ProcArticle()
        {
            //尝试读取数据文件
            List<string> links = new List<string>();
            var isFirst = true;
            Console.WriteLine("-> 读取链接");
            while (links.Count == 0)
            {
                try
                {
                    using var fs1 = new FileStream(_setting.ArticlesFileName, FileMode.Open, FileAccess.Read);
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
                catch (Exception)
                {
                    File.Create(_setting.ArticlesFileName).Close();
                }

                if (isFirst)
                {
                    Console.WriteLine($"请在{_setting.ArticlesFileName}文件中输入要导入的链接，每行填写一个链接，目前只支持知乎、小黑盒链接");
                    Console.WriteLine($"例如：");
                    Console.WriteLine($"https://zhuanlan.zhihu.com/p/480692805");
                    Console.WriteLine($"https://api.xiaoheihe.cn/v3/bbs/app/api/web/share?link_id=2494072");
                    Console.WriteLine($"按下回车【Enter】确认，按下【Esc】返回");
                }
                if (links.Any() == false)
                {
                    if (isFirst == false)
                    {
                        Console.WriteLine($"请在{_setting.ArticlesFileName}文件中输入要导入的链接");
                    }
                    var key = Console.ReadKey();
                    if (key.Key == ConsoleKey.Escape)
                    {
                        return;
                    }
                }

                isFirst = false;
            }
            Console.WriteLine($"-> 已读取{links.Count}条链接");

            //循环读取链接并生成审核模型
            Console.WriteLine("-> 开始处理链接");

            foreach (var item in links)
            {
                var tempArticle = _history.articles.FirstOrDefault(s => s.Url == item);
                if (tempArticle != null)
                {
                    if (tempArticle.PostTime != null)
                    {
                        OutputHelper.Write(OutputLevel.Warning, $"-> 链接 {item} 已于 {tempArticle.PostTime} 提交审核[Id:{tempArticle.Id}]");
                        continue;
                    }
                }
                else
                {
                    try
                    {
                        tempArticle = await GetArticleContext(item);
                    }
                    catch (Exception ex)
                    {
                        OutputHelper.PressError(ex, "尝试处理链接失败", "链接格式不正确");
                        continue;
                    }

                    _history.articles.Add(tempArticle);
                    _history.Save();
                }

                if (_hostData.articles.Any(s => s == tempArticle.Title))
                {
                    OutputHelper.Write(OutputLevel.Dager, $"-> 链接 {item} 与现有文章《{tempArticle.Title}》重名，请手动上传");
                    continue;
                }

                await _image.CutImage(tempArticle);

                var article = GenerateArticle(tempArticle);

                tempArticle.Id = await SubmitArticle(article);
                tempArticle.PostTime = DateTime.Now.ToCstTime();
                _history.Save();

                OutputHelper.Write(OutputLevel.Infor, $"-> 处理完成第 {links.IndexOf(item) + 1} 条链接");
            }

            //成功
            OutputHelper.Repeat();
            Console.WriteLine($"总计处理{links.Count}条链接，感谢您使用本投稿工具");
            OutputHelper.Repeat();
            Console.WriteLine("按任意键返回上级菜单");
            Console.ReadKey();
        }

        private async Task<OutlinkArticleModel> ProcZhiHuArticleFromHtmlAsync(string html)
        {
            var article = new OutlinkArticleModel();


            var document = new HtmlDocument();
            document.LoadHtml(html);

            var node = document.GetElementbyId("root");
            var htmlStr = "";
            var name = "";
            string image = null;
            string author = null;
            //正文
            try
            {
                htmlStr = node.FirstChild.ChildNodes[2].FirstChild.ChildNodes.MaxBy(s => s.InnerText.Length).ChildNodes.MaxBy(s => s.InnerText.Length).LastChild.LastChild.InnerHtml;
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
                    if (tempNode != null)
                    {
                        image = ToolHelper.MidStrEx(tempNode.OuterHtml, "url(", "?source");
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
                var times = node.FirstChild.ChildNodes[2].FirstChild.ChildNodes.MaxBy(s => s.InnerText.Length).ChildNodes.Where(s => s.InnerText.Contains("发布于") || s.InnerText.Contains("编辑于")).Select(s => s.InnerText.Replace("发布于 ", "").Replace("编辑于 ", ""));
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
            article.MainPage = await _image.ProgressImage(article.MainPage, OutlinkArticleType.ZhiHu);
            article.Title = name;
            article.OriginalAuthor = ToolHelper.MidStrEx(article.MainPage, "原作者：", "\r").Replace("*","");
            if (string.IsNullOrWhiteSpace(article.OriginalAuthor) && author != null)
            {
                article.OriginalAuthor = author;
            }
            article.Image = image ?? ToolHelper.GetImageLinks(article.MainPage).FirstOrDefault();
            return article;
        }

        private async Task<OutlinkArticleModel> ProcXiaoHeiHeArticleFromHtmlAsync(string html)
        {
            var article = new OutlinkArticleModel();


            var document = new HtmlDocument();
            document.LoadHtml(html);

            var node = document.GetElementbyId("app");
            var htmlStr = "";
            var name = "";
            string image = null;
            string author = null;

            //主图标题
            try
            {
                name = node.ChildNodes.FirstOrDefault(s => s.HasClass("article-header")).ChildNodes.FirstOrDefault(s => s.HasClass("title")).InnerText;
            }
            catch
            {

            }


            //作者
            try
            {
                article.OriginalAuthor = node.ChildNodes.FirstOrDefault(s => s.HasClass("user-bar")).ChildNodes.FirstOrDefault(s => s.HasClass("row-1")).ChildNodes.FirstOrDefault(s => s.HasClass("info")).ChildNodes.FirstOrDefault(s => s.HasClass("top")).InnerText;
            }
            catch
            {

            }

            //正文
            try
            {
                htmlStr = node.ChildNodes.FirstOrDefault(s => s.HasClass("article-content")).FirstChild.InnerHtml;
            }
            catch
            {

            }

            var converter = new ReverseMarkdown.Converter();
            article.MainPage = converter.Convert(htmlStr.Replace("data-original=", "src="));
            article.MainPage = await _image.ProgressImage(article.MainPage, OutlinkArticleType.XiaoHeiHe);
            article.Title = name;
            article.OriginalAuthor = ToolHelper.MidStrEx(article.MainPage, "本文作者 @", "**");
            if (string.IsNullOrWhiteSpace(article.OriginalAuthor) && author != null)
            {
                article.OriginalAuthor = author;
            }
            article.Image = image ?? ToolHelper.GetImageLinks(article.MainPage).FirstOrDefault();
            return article;
        }

        private async Task<OutlinkArticleModel> GetArticleContext(string url)
        {
            //替换链接
            url = url.Replace("https://api.xiaoheihe.cn/maxnews/app/share/detail/", "https://api.xiaoheihe.cn/v3/bbs/app/api/web/share?link_id=");

            OutlinkArticleModel result = null;

            if (url.Contains("zhuanlan.zhihu.com"))
            {
                var html = _html.GetHtml(url);

                result = await ProcZhiHuArticleFromHtmlAsync(html);

            }
            else if (url.Contains("api.xiaoheihe.cn"))
            {
                var html = await _html.GetHtmlJsAsync(url);
                result = await ProcXiaoHeiHeArticleFromHtmlAsync(html);
            }
            else
            {
                throw new Exception("链接格式不正确或不支持该平台");
            }

            result.Url = url;

            return result;
        }

        private async Task<long> SubmitArticle(CreateArticleViewModel model)
        {
            try
            {

                var result = await _httpClient.PostAsJsonAsync<CreateArticleViewModel>(ToolHelper.WebApiPath + "api/articles/createarticle", model);
                var jsonContent = result.Content.ReadAsStringAsync().Result;
                var obj = System.Text.Json.JsonSerializer.Deserialize<Result>(jsonContent, ToolHelper.options);
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

        private CreateArticleViewModel GenerateArticle(OutlinkArticleModel model)
        {
            var article = new CreateArticleViewModel
            {
                Name = model.Title,
                Main = new EditArticleMainViewModel
                {
                    Name = model.Title,
                    DisplayName = model.Title,
                    MainPicture = model.Image,
                    PubishTime = model.PublishTime.Year < 2000 ? DateTime.Now.ToCstTime() : model.PublishTime,
                    RealNewsTime = model.PublishTime,
                    OriginalLink = model.Url,
                    BriefIntroduction = GetArticleBriefIntroduction(model.MainPage, 50),
                    OriginalAuthor = string.IsNullOrWhiteSpace(model.OriginalAuthor) ? _setting.UserName : model.OriginalAuthor,
                },
                MainPage = new EditArticleMainPageViewModel
                {
                    Context = model.MainPage,
                }
            };

            //判断类型
            foreach (var item in _setting.ArticleTypeStrings)
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
            if (article.Main.Type == ArticleType.Tought)
            {
                article.Main.Type = ArticleType.Evaluation;
            }

            //查找关联词条
            foreach (var item in _hostData.games.Where(s => s != "幻觉" && s != "画师"))
            {
                if (model.MainPage.Contains(item))
                {
                    article.Relevances.Games.Add(new DataModel.ViewModel.RelevancesModel
                    {
                        DisplayName = item,
                    });
                }

            }
            foreach (var item in _hostData.games)
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

        private string GetArticleBriefIntroduction(string mainPage, int maxLength)
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
    }

    public class OutlinkArticleModel
    {
        public long Id { get; set; }

        public string Title { get; set; }

        public string OriginalAuthor { get; set; }

        public string Url { get; set; }

        public string MainPage { get; set; }

        public string Image { get; set; }

        public bool IsCutImage { get; set; }

        public DateTime PublishTime { get; set; }

        public DateTime? PostTime { get; set; }
    }

    public enum OutlinkArticleType
    {
        ZhiHu,
        XiaoHeiHe
    }
}
