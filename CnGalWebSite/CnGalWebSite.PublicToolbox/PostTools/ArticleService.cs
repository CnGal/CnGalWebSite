using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Articles;
using CnGalWebSite.DataModel.ViewModel.HistoryData;
using CnGalWebSite.DataModel.ViewModel.PostTools;
using CnGalWebSite.Helper.Extensions;
using CnGalWebSite.Helper.Helper;
using CnGalWebSite.PublicToolbox.DataRepositories;
using CnGalWebSite.PublicToolbox.Helpers;
using HtmlAgilityPack;
using Markdig;
using Markdig.Extensions.Figures;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.PublicToolbox.PostTools
{
    public class ArticleService:IArticleService
    {
        private readonly HttpClient _httpClient;
        private readonly IImageService _imageService;
        private readonly IRepository<RepostArticleModel> _outlinkArticleRepository;
        private readonly ILogger<ArticleService> _logger;

        public event Action<KeyValuePair<OutputLevel, string>> ProgressUpdate;

        public ArticleService(HttpClient httpClient, IRepository<RepostArticleModel> outlinkArticleRepository, IImageService imageService, ILogger<ArticleService> logger)
        {
            _httpClient = httpClient;
            _outlinkArticleRepository = outlinkArticleRepository;
            _imageService = imageService;
            _logger = logger;
        }

        public async Task ProcArticle(RepostArticleModel model, IEnumerable<string> articles, IEnumerable<string> games)
        {
            OnProgressUpdate(model, OutputLevel.Infor, $"开始处理链接 {model.Url}");

            var tempArticle = _outlinkArticleRepository.GetAll().FirstOrDefault(s => s.Url == model.Url);
            if (tempArticle != null)
            {
                if (tempArticle.PostTime != null)
                {
                    OnProgressUpdate(model, OutputLevel.Dager, $"-> 链接 {model.Url} 已于 {tempArticle.PostTime} 提交审核[Id:{tempArticle.Id}]");
                    return;
                }
            }
            else
            {
                try
                {
                    OnProgressUpdate(model, OutputLevel.Infor, $"获取文章内容");
                     await GetArticleContext( model);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "获取链接内容失败 {Link}", model.Url);
                    OnProgressUpdate(model, OutputLevel.Dager, "获取文章失败，请检查链接格式或联系管理员");
                    return;
                }

                await _outlinkArticleRepository.InsertAsync(model);
            }
            if (articles.Any(s => s == model.Title))
            {
                OnProgressUpdate(model, OutputLevel.Dager, $"{model.Url} 与现有文章《{model.Title}》重名，请手动上传");
                return;
            }

            model.CompleteTaskCount++;
            OnProgressUpdate(model, OutputLevel.Infor, $"裁剪主图");

            await CutImage(model);

            model.CompleteTaskCount++;
            OnProgressUpdate(model, OutputLevel.Infor, $"生成文章");

            var article = GenerateArticle(model, games);

            model.CompleteTaskCount++;
            OnProgressUpdate(model, OutputLevel.Infor, $"提交审核");

            model.Id = await SubmitArticle(article);
            model.PostTime = DateTime.Now.ToCstTime();
            await _outlinkArticleRepository.SaveAsync();

            model.CompleteTaskCount++;
            OnProgressUpdate(model, OutputLevel.Infor, $"成功处理 {model.Url}");

        }

        public async Task CutImage(RepostArticleModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Image) == false && model.IsCutImage == false)
            {
                model.Image = await _imageService.GetImage(model.Image, 460, 215);
                model.IsCutImage = model.Image.Contains("image.cngal");
                if (model.IsCutImage)
                {
                    OnProgressUpdate(model, OutputLevel.Infor, "成功裁剪主图");
                }
                else
                {
                    OnProgressUpdate(model, OutputLevel.Warning, "裁剪主图失败，请在提交完成后手动修正");
                }
            }
        }



        protected void OnProgressUpdate(RepostArticleModel model, OutputLevel level, string infor)
        {
            if (level == OutputLevel.Dager)
            {
                model.Error = infor;
            }
            ProgressUpdate?.Invoke(new KeyValuePair<OutputLevel, string>(level, infor));
        }


        private async Task ProcZhiHuArticleFromHtmlAsync(RepostArticleModel model, string html)
        { 

            var document = new HtmlDocument();
            document.LoadHtml(html);

            var node = document.GetElementbyId("root");
            var htmlStr = "";
            var name = "";
            string image = null;
            string author = null;
            var mainNode = node.FirstChild.ChildNodes.FirstOrDefault(s => s.HasClass("App-main")).FirstChild;
            //正文
            try
            {
                htmlStr = mainNode.ChildNodes.MaxBy(s => s.InnerText.Length).ChildNodes.MaxBy(s => s.InnerText.Length).LastChild.LastChild.InnerHtml;
            }
            catch(Exception ex)
            {
                throw new Exception("无法获取知乎文章内容，请联系管理员",ex);
            }

            //检查是否获取到正文
            if (string.IsNullOrWhiteSpace(htmlStr))
            {
                throw new Exception("无法获取知乎文章内容，请联系管理员");
            }

            //主图
            try
            {
                var tempNode = mainNode.ChildNodes.FirstOrDefault(s => s.Name == "img");
                if (tempNode == null)
                {
                    tempNode = mainNode.ChildNodes.FirstOrDefault(s => s.OuterHtml.Contains("background-image"));
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
                name = mainNode.ChildNodes.MaxBy(s => s.InnerText.Length).ChildNodes[0].FirstChild.InnerText;
            }
            catch
            {

            }
            //作者
            try
            {
                author = mainNode.ChildNodes.MaxBy(s => s.InnerText.Length).ChildNodes[0].ChildNodes[1].FirstChild.FirstChild.LastChild.FirstChild.InnerText;
            }
            catch
            {

            }
            //时间
            try
            {
                var times = mainNode.ChildNodes.MaxBy(s => s.InnerText.Length).ChildNodes.Where(s => s.InnerText.Contains("发布于") || s.InnerText.Contains("编辑于")).Select(s => s.InnerText.Replace("发布于 ", "").Replace("编辑于 ", ""));
                foreach (var item in times)
                {
                    try
                    {
                        model.PublishTime = DateTime.ParseExact(item, "yyyy-MM-dd HH:mm", null);
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


            if(string.IsNullOrWhiteSpace(htmlStr))
            {
                throw new Exception("无法获取知乎文章内容，请联系管理员");
            }

            model.MainPage = converter.Convert(htmlStr);
            model.MainPage = await ProgressImage(model, model.MainPage, RepostArticleType.ZhiHu);
            model.Title = name;
            model.OriginalAuthor = ToolHelper.MidStrEx(model.MainPage, "原作者：", "\r").Replace("*", "").Split("丨").FirstOrDefault()?.Trim();
            if (string.IsNullOrWhiteSpace(model.OriginalAuthor) && author != null)
            {
                model.OriginalAuthor = author;
            }
            model.Image = image ?? ToolHelper.GetImageLinks(model.MainPage).FirstOrDefault();
           
        }

        private async Task ProcXiaoHeiHeArticleFromHtmlAsync(RepostArticleModel model, string html)
        {

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
                model.OriginalAuthor = node.ChildNodes.FirstOrDefault(s => s.HasClass("user-bar")).ChildNodes.FirstOrDefault(s => s.HasClass("row-1")).ChildNodes.FirstOrDefault(s => s.HasClass("info")).ChildNodes.FirstOrDefault(s => s.HasClass("top")).InnerText;
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
            model.MainPage = converter.Convert(htmlStr.Replace("data-original=", "src="));
            model.MainPage = await ProgressImage(model, model.MainPage, RepostArticleType.XiaoHeiHe);
            model.Title = name;
            model.OriginalAuthor = ToolHelper.MidStrEx(model.MainPage, "本文作者 @", "**");
            if (string.IsNullOrWhiteSpace(model.OriginalAuthor) && author != null)
            {
                model.OriginalAuthor = author;
            }
            model.Image = image ?? ToolHelper.GetImageLinks(model.MainPage).FirstOrDefault();
  
        }

        public async Task<string> ProgressImage(RepostArticleModel model,string text, RepostArticleType type)
        {
            if (type == RepostArticleType.ZhiHu)
            {

                var figures = text.Split("</figure>");

                model.TotalTaskCount += figures.Count();

                foreach (var item in figures)
                {
                    

                    var textTemp = item.Split("<figure");
                    if (textTemp.Length > 1)
                    {
                        var image = ToolHelper.MidStrEx(textTemp[1], "data-original=\"", "\">");

                        OnProgressUpdate(model, OutputLevel.Infor, $"获取图片 {image}");

                        var newImage = await _imageService.GetImage(image);

                        text = text.Replace("<figure" + ToolHelper.MidStrEx(text, "<figure", "</figure>") + "</figure>", "\n![image](" + newImage + ")\n");
                    }

                    model.CompleteTaskCount++;
                }

            }
            else if (type == RepostArticleType.XiaoHeiHe)
            {

                var images = ToolHelper.GetImageLinks(text);
                model.TotalTaskCount += images.Count;
                foreach (var temp in images)
                {
                    var image = temp.Replace("/thumb", "");
                    OnProgressUpdate(model, OutputLevel.Infor, $"获取图片 {image}");

                    var infor = await _imageService.GetImage(image);

                    //替换图片
                    text = text.Replace(temp, infor);

                    model.CompleteTaskCount++;
                }
            }
            return text;
        }


        private async Task GetArticleContext(RepostArticleModel model)
        {
            //替换链接
           var url = model.Url.Replace("https://api.xiaoheihe.cn/maxnews/app/share/detail/", "https://api.xiaoheihe.cn/v3/bbs/app/api/web/share?link_id=");

            if (url.Contains("zhuanlan.zhihu.com"))
            {
                var html = HtmlHelper.GetHtml(url);

                await ProcZhiHuArticleFromHtmlAsync(model, html);

            }
            else if (url.Contains("api.xiaoheihe.cn"))
            {
                var html = HtmlHelper.GetHtml(url);
                await ProcXiaoHeiHeArticleFromHtmlAsync(model, html);
            }
            else
            {
                throw new Exception("链接格式不正确或不支持该平台");
            }
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

        private CreateArticleViewModel GenerateArticle(RepostArticleModel model,IEnumerable<string> games)
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
                    OriginalAuthor = string.IsNullOrWhiteSpace(model.OriginalAuthor) ? model.UserName : model.OriginalAuthor,
                    Type = model.Type,
                },
                MainPage = new EditArticleMainPageViewModel
                {
                    Context = model.MainPage,
                }
            };

        

            //查找关联词条
            foreach (var item in games.Where(s => s != "幻觉" && s != "画师"))
            {
                if (model.MainPage.Contains(item))
                {
                    article.Relevances.Games.Add(new DataModel.ViewModel.RelevancesModel
                    {
                        DisplayName = item,
                    });
                }

            }
            foreach (var item in games)
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
}
