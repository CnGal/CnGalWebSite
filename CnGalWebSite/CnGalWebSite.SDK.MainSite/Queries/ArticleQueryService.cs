using CnGalWebSite.DataModel.ViewModel.Articles;
using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Infrastructure;
using CnGalWebSite.SDK.MainSite.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CnGalWebSite.SDK.MainSite.Queries;

public sealed class ArticleQueryService(
    HttpClient httpClient,
    IMemoryCache memoryCache,
    ILogger<ArticleQueryService> logger) : QueryServiceBase(httpClient), IArticleQueryService
{
    private static readonly TimeSpan ArticleCacheDuration = TimeSpan.FromMinutes(5);
    private const string ArticleDetailPathTemplate = "api/articles/GetArticleView/{0}";

    protected override ILogger Logger => logger;

    public async Task<SdkResult<ArticleDetailViewModel>> GetArticleDetailAsync(long id, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"main-site:article-detail:{id}";
        if (memoryCache.TryGetValue(cacheKey, out ArticleDetailViewModel? cached) && cached is not null)
        {
            return SdkResult<ArticleDetailViewModel>.Ok(cached);
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
            memoryCache.Set(cacheKey, result.Data, ArticleCacheDuration);
        }

        return result;
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
