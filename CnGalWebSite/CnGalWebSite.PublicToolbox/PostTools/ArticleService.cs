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
                OnProgressUpdate(model, OutputLevel.Dager, $"与现有文章《{model.Title}》重名，请自定义标题");
                return;
            }

            model.CompleteTaskCount++;
            OnProgressUpdate(model, OutputLevel.Infor, $"裁剪主图");

            await CutImage(model);
            model.CompleteTaskCount++;

            CreateArticleViewModel article = null;
            try
            {
                OnProgressUpdate(model, OutputLevel.Infor, $"生成文章");
                 article = GenerateArticle(model, games);
                model.CompleteTaskCount++;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "生成文章失败 {Link}", model.Url);
                OnProgressUpdate(model, OutputLevel.Dager, "生成文章失败，原因：" + ex.Message);
                return;
            }
          

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
                var temp= mainNode.ChildNodes.FirstOrDefault(s => s.Name == "article").ChildNodes.FirstOrDefault(s => s.HasClass("Post-Header")).ChildNodes.FirstOrDefault(s => s.HasClass("Post-Author")).ChildNodes.FirstOrDefault(s => s.HasClass("AuthorInfo")).ChildNodes.FirstOrDefault(s => s.HasClass("AuthorInfo")).ChildNodes.FirstOrDefault(s => s.HasClass("AuthorInfo-content"));
                author = temp.ChildNodes.FirstOrDefault(s => s.HasClass("AuthorInfo-head")).InnerText;

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

            model.MainPage = await ProgressImage(model, converter.Convert(htmlStr));
            model.Title =string.IsNullOrWhiteSpace(model.Title)? name:model.Title;
            model.OriginalAuthor = ToolHelper.MidStrEx(model.MainPage, "原作者：", "\r").Replace("*", "").Split("丨").FirstOrDefault()?.Trim();
            if (string.IsNullOrWhiteSpace(model.OriginalAuthor) && author != null)
            {
                model.OriginalAuthor = author;
            }
            model.Image = image ?? model.MainPage.GetImageLinks().FirstOrDefault();
           
        }

        private async Task ProcBilibiliArticleFromHtmlAsync(RepostArticleModel model, string html)
        {

            var document = new HtmlDocument();
            document.LoadHtml(html);

            var node = document.GetElementbyId("app");
            var htmlStr = "";
            string name = null;
            string image = null;
            string author = null;
            var mainNode = node.ChildNodes.FirstOrDefault(s => s.HasClass("article-detail"));
            //正文
            try
            {
                htmlStr = document.GetElementbyId("read-article-holder").InnerHtml;
            }
            catch (Exception ex)
            {
                throw new Exception("无法获取知乎文章内容，请联系管理员", ex);
            }

            //检查是否获取到正文
            if (string.IsNullOrWhiteSpace(htmlStr))
            {
                throw new Exception("无法获取知乎文章内容，请联系管理员");
            }

            //标题
            try
            {
                var inner = mainNode.ChildNodes.FirstOrDefault(s => s.HasClass("fixed-top-header")).ChildNodes.FirstOrDefault(s => s.HasClass("inner"));
                //标题
                name = inner.ChildNodes.FirstOrDefault(s => s.HasClass("inner-title")).InnerText;
                //作者
                author = inner.ChildNodes.FirstOrDefault(s => s.HasClass("inner-right")).ChildNodes.FirstOrDefault(s => s.HasClass("up-info")).InnerText;
            }
            catch
            {
                OnProgressUpdate(model, OutputLevel.Warning, $"获取标题或作者失败");
            }

            //时间
            try
            {
                var time = mainNode.ChildNodes.FirstOrDefault(s => s.HasClass("article-container")).ChildNodes.FirstOrDefault(s => s.HasClass("article-container__content")).ChildNodes.FirstOrDefault(s => s.HasClass("title-container")).ChildNodes.FirstOrDefault(s => s.HasClass("article-read-panel")).ChildNodes.FirstOrDefault(s => s.HasClass("article-read-info")).ChildNodes.FirstOrDefault(s => s.HasClass("publish-text")).InnerText;

                model.PublishTime = DateTime.Parse(time);
            }
            catch
            {
                OnProgressUpdate(model, OutputLevel.Warning, $"获取时间失败");
            }

            var converter = new ReverseMarkdown.Converter();


            if (string.IsNullOrWhiteSpace(htmlStr))
            {
                throw new Exception("无法获取知乎文章内容，请联系管理员");
            }

            model.MainPage = await ProgressImage(model, converter.Convert(htmlStr));
            model.Title = string.IsNullOrWhiteSpace(model.Title) ? (name ?? model.MainPage.Abbreviate(10) ): model.Title;

            model.OriginalAuthor = author;

            model.Image = image ?? model.MainPage.GetImageLinks().FirstOrDefault();

        }

        public async Task<string> ProgressImage(RepostArticleModel model,string text)
        {

                var figures = text.Split("</figure>");

                model.TotalTaskCount += figures.Count();

                foreach (var item in figures)
                {
                    var textTemp = item.Split("<figure");
                    if (textTemp.Length > 1)
                    {
                        //获取图片链接
                        var image = ToolHelper.MidStrEx(textTemp[1], "data-original=\"", "\">").Split('@').FirstOrDefault();
                        if (string.IsNullOrWhiteSpace(image))
                        {
                            image = ToolHelper.MidStrEx(textTemp[1], "data-src=\"", "\"").Split('@').FirstOrDefault();
                        }
                        if (string.IsNullOrWhiteSpace(image))
                        {
                            image = ToolHelper.MidStrEx(textTemp[1], "<img src=\"", "\"").Split('@').FirstOrDefault();
                        }
                        if (string.IsNullOrWhiteSpace(image))
                        {
                            continue;
                        }

                    //获取注释
                    var figcaption = textTemp[1].MidStrEx("<figcaption", ">");
                        var figure = textTemp[1].MidStrEx($"<figcaption{figcaption}>", "</figcaption>");

                        OnProgressUpdate(model, OutputLevel.Infor, $"获取图片 {image}");

                        var newImage = await _imageService.GetImage(image);

                        //替换
                        text = text.Replace("<figure" + ToolHelper.MidStrEx(text, "<figure", "</figure>") + "</figure>", $"\n![{figure??""}]({newImage})\n");
                    }

                    model.CompleteTaskCount++;
                }

            
           
            return text;
        }


        private async Task GetArticleContext(RepostArticleModel model)
        {

            if (model.Url.Contains("zhuanlan.zhihu.com"))
            {
                var html = HtmlHelper.GetHtml(model.Url);

                await ProcZhiHuArticleFromHtmlAsync(model, html);
            }
            else if (model.Url.Contains("www.bilibili.com"))
            {
                model.Url = model.Url.Split('?').FirstOrDefault();
                var html = HtmlHelper.GetHtml(model.Url);
                await ProcBilibiliArticleFromHtmlAsync(model, html);
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
                _logger.LogInformation($"[7] 已提交{model.Main.Type.GetDisplayName()}《{model.Main.Name}》审核[Id:{obj.Error}]");
                return long.Parse(obj.Error);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "提交文章审核失败");
                return 0;
            }
        }

        private CreateArticleViewModel GenerateArticle(RepostArticleModel model,IEnumerable<string> games)
        {
            if(string.IsNullOrWhiteSpace(model.MainPage))
            {
                throw new Exception("文章内容不能为空");
            }
            if (string.IsNullOrWhiteSpace(model.Title))
            {
                throw new Exception("文章标题不能为空");
            }

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

            var entries = games.ToList();
            entries.RemoveAll(s => s == "幻觉" || s == "画师" || s == "她");

            //查找关联词条
            foreach (var item in entries)
            {
                if (model.MainPage.Contains(item))
                {
                    article.Relevances.Games.Add(new DataModel.ViewModel.RelevancesModel
                    {
                        DisplayName = item,
                    });
                }

            }
            foreach (var item in entries)
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
