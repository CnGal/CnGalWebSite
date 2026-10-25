using CnGalWebSite.DataModel.ViewModel.Articles;
using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Infrastructure;
using CnGalWebSite.SDK.MainSite.Models;
using Microsoft.Extensions.Logging;

namespace CnGalWebSite.SDK.MainSite.Queries;

public sealed class ArticleQueryService(
    HttpClient httpClient,
    PublicDataCache publicCache,
    ILogger<ArticleQueryService> logger) : QueryServiceBase(httpClient), IArticleQueryService
{
    private readonly PublicDataCache memoryCache = publicCache.ForSource(httpClient.BaseAddress);

    private static readonly TimeSpan ArticleCacheDuration = TimeSpan.FromMinutes(5);
    private const string ArticleDetailPathTemplate = "api/articles/GetArticleView/{0}";

    protected override ILogger Logger => logger;

    public async Task<SdkResult<ArticleDetailViewModel>> GetArticleDetailAsync(long id, CancellationToken cancellationToken = default)
    {
        var cacheRead = await memoryCache.BeginQueryAsync(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        var cacheKey = PublicCacheKeys.Detail("article", id);
        if (memoryCache.TryGetValue(cacheKey, out ArticleDetailViewModel? cached, cacheRead) && cached is not null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return memoryCache.Complete<ArticleDetailViewModel>(cached, cacheRead);
        }

        var path = string.Format(ArticleDetailPathTemplate, id);
        var result = await GetSingleAsync<ArticleViewModel, ArticleDetailViewModel>(
            path,
            MapToViewModel,
            "ARTICLE",
            "文章",
            id,
            cancellationToken);

        if (result.Success && result.Data is not null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            memoryCache.Set(cacheKey, result.Data, ArticleCacheDuration, cacheRead);
        }

        cancellationToken.ThrowIfCancellationRequested();
        return memoryCache.CompleteResult(result, cacheRead);
    }

    private static ArticleDetailViewModel MapToViewModel(ArticleViewModel article)
    {
        return new ArticleDetailViewModel
        {
            Id = article.Id,
            Name = article.Name ?? string.Empty,
            DisplayName = article.DisplayName ?? string.Empty,
            BriefIntroduction = article.BriefIntroduction ?? string.Empty,
            MainPicture = article.MainPicture ?? string.Empty,
            BackgroundPicture = article.BackgroundPicture ?? string.Empty,
            SmallBackgroundPicture = article.SmallBackgroundPicture ?? string.Empty,
            MainPage = article.MainPage ?? string.Empty,
            Type = article.Type,
            CreateTime = article.CreateTime,
            LastEditTime = article.LastEditTime,
            OriginalAuthor = article.OriginalAuthor ?? string.Empty,
            OriginalLink = article.OriginalLink ?? string.Empty,
            PubishTime = article.PubishTime,
            ReaderCount = article.ReaderCount,
            ThumbsUpCount = article.ThumbsUpCount,
            CommentCount = article.CommentCount,
            IsHidden = article.IsHidden,
            CanComment = article.CanComment,
            AuthorName = article.UserInfor?.Name ?? string.Empty,
            AuthorAvatar = article.UserInfor?.PhotoPath ?? string.Empty,
            AuthorId = article.UserInfor?.Id ?? string.Empty,
            AuthorRanks = article.UserInfor?.Ranks?.ToList() ?? [],
            RelatedEntries = article.RelatedEntries?.ToList() ?? [],
            RelatedArticles = article.RelatedArticles?.ToList() ?? [],
            RelatedVideos = article.RelatedVideos?.ToList() ?? [],
            RelatedOutlinks = article.RelatedOutlinks?.ToList() ?? [],
        };
    }
}
