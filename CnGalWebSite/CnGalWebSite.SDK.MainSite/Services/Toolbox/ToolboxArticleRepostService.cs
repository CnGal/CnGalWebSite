using System.Net;
using System.Text.RegularExpressions;
using CnGalWebSite.DataModel.ViewModel;
using CnGalWebSite.DataModel.ViewModel.Articles;
using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Models.ArticleEdit;
using CnGalWebSite.SDK.MainSite.Models.Toolbox;
using HtmlAgilityPack;
using Markdig;

namespace CnGalWebSite.SDK.MainSite.Services.Toolbox;

public sealed partial class ToolboxArticleRepostService(
    HttpClient httpClient,
    IArticleCommandService articleCommandService,
    ToolboxImageService imageService,
    IToolboxLocalRepository<RepostArticleTaskModel> articleRepository) : ToolboxServiceBase, IToolboxArticleRepostService
{
    private static readonly string[] UserAgents =
    [
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0 Safari/537.36",
        "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.0 Safari/605.1.15"
    ];

    public async Task ProcessArticleAsync(RepostArticleTaskModel model, CancellationToken cancellationToken = default)
    {
        model.Error = string.Empty;
        Report(model, ToolboxOutputLevel.Info, $"开始处理链接 {model.Url}");

        var history = (await articleRepository.GetAllAsync(cancellationToken)).FirstOrDefault(s => s.Url == model.Url);
        if (history is not null && history.PostTime.HasValue)
        {
            Report(model, ToolboxOutputLevel.Error, $"链接 {model.Url} 已于 {history.PostTime} 提交审核[Id:{history.Id}]");
            return;
        }

        if (string.IsNullOrWhiteSpace(model.MainPage))
        {
            Report(model, ToolboxOutputLevel.Info, "获取文章内容");
            var loaded = await LoadArticleContextAsync(model, cancellationToken);
            if (!loaded)
            {
                return;
            }

            await articleRepository.InsertAsync(model, cancellationToken);
        }

        var metaResult = await articleCommandService.GetArticleEditMetaOptionsAsync(cancellationToken);
        if (!metaResult.Success || metaResult.Data is null)
        {
            Report(model, ToolboxOutputLevel.Error, metaResult.Error?.Message ?? "获取候选列表失败");
            return;
        }

        if (metaResult.Data.ArticleItems.Any(s => s == model.Title))
        {
            Report(model, ToolboxOutputLevel.Error, $"与现有文章《{model.Title}》重名，请自定义标题");
            return;
        }

        model.CompleteTaskCount++;
        Report(model, ToolboxOutputLevel.Info, "裁剪主图");
        await CutImageAsync(model, cancellationToken);
        model.CompleteTaskCount++;

        Report(model, ToolboxOutputLevel.Info, "生成文章");
        CreateArticleViewModel article;
        try
        {
            article = GenerateArticle(model, metaResult.Data.EntryGameItems);
        }
        catch (Exception ex)
        {
            Report(model, ToolboxOutputLevel.Error, $"生成文章失败：{ex.Message}");
            return;
        }
        model.CompleteTaskCount++;

        Report(model, ToolboxOutputLevel.Info, "提交审核");
        var submitResult = await articleCommandService.SubmitEditAsync(new ArticleEditRequest
        {
            IsCreate = true,
            Data = article
        }, cancellationToken);

        if (!submitResult.Success || submitResult.Data == 0)
        {
            Report(model, ToolboxOutputLevel.Error, submitResult.Error?.Message ?? "提交文章审核失败");
            return;
        }

        model.Id = submitResult.Data;
        model.PostTime = DateTime.Now;
        model.CompleteTaskCount++;
        await articleRepository.SaveAsync(cancellationToken);
        Report(model, ToolboxOutputLevel.Info, $"成功处理 {model.Url}");
    }

    private async Task<bool> LoadArticleContextAsync(RepostArticleTaskModel model, CancellationToken cancellationToken)
    {
        try
        {
            var url = model.Url.Contains("www.bilibili.com", StringComparison.OrdinalIgnoreCase)
                ? model.Url.Split('?')[0]
                : model.Url;
            var html = await GetHtmlAsync(url, cancellationToken);

            if (url.Contains("zhuanlan.zhihu.com", StringComparison.OrdinalIgnoreCase))
            {
                await ParseZhihuArticleAsync(model, html, cancellationToken);
                return true;
            }

            if (url.Contains("www.bilibili.com", StringComparison.OrdinalIgnoreCase))
            {
                model.Url = url;
                await ParseBilibiliArticleAsync(model, html, cancellationToken);
                return true;
            }

            Report(model, ToolboxOutputLevel.Error, "链接格式不正确或暂不支持该平台");
            return false;
        }
        catch (Exception ex)
        {
            Report(model, ToolboxOutputLevel.Error, $"获取文章失败：{ex.Message}");
            return false;
        }
    }

    private async Task<string> GetHtmlAsync(string url, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.UserAgent.ParseAdd(UserAgents[Random.Shared.Next(UserAgents.Length)]);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    private async Task ParseZhihuArticleAsync(RepostArticleTaskModel model, string html, CancellationToken cancellationToken)
    {
        var document = new HtmlDocument();
        document.LoadHtml(html);

        var contentNode = document.DocumentNode.SelectSingleNode("//article")
            ?? document.DocumentNode.Descendants("div")
                .Where(s => !string.IsNullOrWhiteSpace(s.InnerText))
                .OrderByDescending(s => s.InnerText.Length)
                .FirstOrDefault();

        if (contentNode is null || string.IsNullOrWhiteSpace(contentNode.InnerHtml))
        {
            throw new InvalidOperationException("无法获取知乎文章内容");
        }

        var title = WebUtility.HtmlDecode(document.DocumentNode.SelectSingleNode("//h1")?.InnerText?.Trim() ?? string.Empty);
        var author = WebUtility.HtmlDecode(document.DocumentNode.SelectSingleNode("//*[contains(@class,'AuthorInfo-head')]")?.InnerText?.Trim() ?? string.Empty);
        var markdown = new ReverseMarkdown.Converter().Convert(contentNode.InnerHtml);

        model.MainPage = await ProcessImagesAsync(model, markdown, cancellationToken);
        model.Title = string.IsNullOrWhiteSpace(model.Title) ? title : model.Title;
        model.OriginalAuthor = string.IsNullOrWhiteSpace(model.OriginalAuthor) ? author : model.OriginalAuthor;
        model.Image = ExtractFirstImage(model.MainPage);
    }

    private async Task ParseBilibiliArticleAsync(RepostArticleTaskModel model, string html, CancellationToken cancellationToken)
    {
        var document = new HtmlDocument();
        document.LoadHtml(html);

        var contentNode = document.GetElementbyId("read-article-holder");
        if (contentNode is null || string.IsNullOrWhiteSpace(contentNode.InnerHtml))
        {
            throw new InvalidOperationException("无法获取 B 站专栏文章内容");
        }

        var title = WebUtility.HtmlDecode(document.DocumentNode.SelectSingleNode("//*[contains(@class,'inner-title')]")?.InnerText?.Trim() ?? string.Empty);
        var author = WebUtility.HtmlDecode(document.DocumentNode.SelectSingleNode("//*[contains(@class,'up-info')]")?.InnerText?.Trim() ?? string.Empty);
        var timeText = document.DocumentNode.SelectSingleNode("//*[contains(@class,'publish-text')]")?.InnerText?.Trim();
        if (DateTime.TryParse(timeText, out var publishTime))
        {
            model.PublishTime = publishTime;
        }

        var markdown = new ReverseMarkdown.Converter().Convert(contentNode.InnerHtml);
        model.MainPage = await ProcessImagesAsync(model, markdown, cancellationToken);
        model.Title = string.IsNullOrWhiteSpace(model.Title) ? title : model.Title;
        model.OriginalAuthor = string.IsNullOrWhiteSpace(model.OriginalAuthor) ? author : model.OriginalAuthor;
        model.Image = ExtractFirstImage(model.MainPage);
    }

    private async Task<string> ProcessImagesAsync(RepostArticleTaskModel model, string markdown, CancellationToken cancellationToken)
    {
        var matches = MarkdownImageRegex().Matches(markdown).Cast<Match>().ToList();
        model.TotalTaskCount += matches.Count;

        foreach (var match in matches)
        {
            var imageUrl = match.Groups["url"].Value;
            Report(model, ToolboxOutputLevel.Info, $"获取图片 {imageUrl}");
            var newImage = await imageService.GetImageAsync(imageUrl, cancellationToken: cancellationToken);
            markdown = markdown.Replace(imageUrl, newImage);
            model.CompleteTaskCount++;
        }

        return markdown;
    }

    private async Task CutImageAsync(RepostArticleTaskModel model, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(model.Image) && !model.IsCutImage)
        {
            model.Image = await imageService.GetImageAsync(model.Image, 460, 215, cancellationToken);
            model.IsCutImage = model.Image.Contains("image.cngal", StringComparison.OrdinalIgnoreCase);
            if (!model.IsCutImage)
            {
                Report(model, ToolboxOutputLevel.Warning, "裁剪主图失败，请在提交完成后手动修正");
            }
        }
    }

    private static CreateArticleViewModel GenerateArticle(RepostArticleTaskModel model, IEnumerable<string> games)
    {
        if (string.IsNullOrWhiteSpace(model.MainPage))
        {
            throw new InvalidOperationException("文章内容不能为空");
        }

        if (string.IsNullOrWhiteSpace(model.Title))
        {
            throw new InvalidOperationException("文章标题不能为空");
        }

        var article = new CreateArticleViewModel
        {
            Name = model.Title,
            Main = new EditArticleMainViewModel
            {
                Name = model.Title,
                DisplayName = model.Title,
                MainPicture = model.Image,
                PubishTime = model.PublishTime.Year < 2000 ? DateTime.Now : model.PublishTime,
                RealNewsTime = model.PublishTime,
                OriginalLink = model.Url,
                BriefIntroduction = GetArticleBriefIntroduction(model.MainPage, 50),
                OriginalAuthor = string.IsNullOrWhiteSpace(model.OriginalAuthor) ? model.UserName : model.OriginalAuthor,
                Type = model.Type
            },
            MainPage = new EditArticleMainPageViewModel
            {
                Context = model.MainPage
            }
        };

        article.Relevances.Games.AddRange(BuildGameRelevances(model.Title, model.MainPage, games));
        return article;
    }

    private static string GetArticleBriefIntroduction(string mainPage, int maxLength)
    {
        var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().UseSoftlineBreakAsHardlineBreak().Build();
        var text = Markdown.ToPlainText(mainPage, pipeline)
            .Replace("\n", string.Empty)
            .Replace("\r", string.Empty)
            .Replace("image", string.Empty);

        text = MarkdownImageRegex().Replace(text, string.Empty);
        text = TopicRegex().Replace(text, string.Empty);
        return text.Length <= maxLength ? text : text[..maxLength];
    }

    private static string ExtractFirstImage(string markdown)
    {
        return MarkdownImageRegex().Matches(markdown).Cast<Match>().FirstOrDefault()?.Groups["url"].Value ?? string.Empty;
    }

    [GeneratedRegex(@"!\[[^\]]*\]\((?<url>[^)]+)\)", RegexOptions.Compiled)]
    private static partial Regex MarkdownImageRegex();

    [GeneratedRegex(@"#.*?#", RegexOptions.Compiled)]
    private static partial Regex TopicRegex();
}
