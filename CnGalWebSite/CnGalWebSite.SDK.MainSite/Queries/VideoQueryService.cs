using CnGalWebSite.DataModel.ViewModel.Videos;
using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Infrastructure;
using CnGalWebSite.SDK.MainSite.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CnGalWebSite.SDK.MainSite.Queries;

public sealed class VideoQueryService(
    HttpClient httpClient,
    IMemoryCache memoryCache,
    ILogger<VideoQueryService> logger) : QueryServiceBase(httpClient), IVideoQueryService
{
    private static readonly TimeSpan VideoCacheDuration = TimeSpan.FromMinutes(5);
    private const string VideoDetailPathTemplate = "api/videos/GetView/{0}";

    protected override ILogger Logger => logger;

    public async Task<SdkResult<VideoDetailViewModel>> GetVideoDetailAsync(long id, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"main-site:video-detail:{id}";
        if (memoryCache.TryGetValue(cacheKey, out VideoDetailViewModel? cached) && cached is not null)
        {
            return SdkResult<VideoDetailViewModel>.Ok(cached);
        }

        var path = string.Format(VideoDetailPathTemplate, id);
        var result = await GetSingleAsync<VideoViewModel, VideoDetailViewModel>(
            path,
            MapToViewModel,
            "VIDEO",
            "视频",
            id,
            cancellationToken);

        if (result.Success && result.Data is not null)
        {
            memoryCache.Set(cacheKey, result.Data, VideoCacheDuration);
        }

        return result;
    }

    private static VideoDetailViewModel MapToViewModel(VideoViewModel video)
    {
        return new VideoDetailViewModel
        {
            Id = video.Id,
            Name = video.Name ?? string.Empty,
            DisplayName = video.DisplayName ?? string.Empty,
            BriefIntroduction = video.BriefIntroduction ?? string.Empty,
            MainPicture = video.MainPicture ?? string.Empty,
            BackgroundPicture = video.BackgroundPicture ?? string.Empty,
            SmallBackgroundPicture = video.SmallBackgroundPicture ?? string.Empty,
            MainPage = video.MainPage ?? string.Empty,
            Type = video.Type ?? string.Empty,
            Copyright = video.Copyright,
            Duration = video.Duration,
            IsInteractive = video.IsInteractive,
            IsCreatedByCurrentUser = video.IsCreatedByCurrentUser,
            CreateTime = video.CreateTime,
            LastEditTime = video.LastEditTime,
            PubishTime = video.PubishTime,
            ReaderCount = video.ReaderCount,
            CommentCount = video.CommentCount,
            OriginalAuthor = video.OriginalAuthor ?? string.Empty,
            IsHidden = video.IsHidden,
            CanComment = video.CanComment,
            AuthorName = video.UserInfor?.Name ?? string.Empty,
            AuthorAvatar = video.UserInfor?.PhotoPath ?? string.Empty,
            AuthorId = video.UserInfor?.Id ?? string.Empty,
            IsAuthorOriginal = video.IsCreatedByCurrentUser,
            RelatedEntries = video.RelatedEntries?.ToList() ?? [],
            RelatedArticles = video.RelatedArticles?.ToList() ?? [],
            RelatedVideos = video.RelatedVideos?.ToList() ?? [],
            RelatedOutlinks = video.RelatedOutlinks?.ToList() ?? [],
            Pictures = video.Pictures?
                .SelectMany(p => p.Pictures ?? [])
                .Where(p => string.IsNullOrWhiteSpace(p.Url) is false)
                .OrderBy(p => p.Priority)
                .ToList() ?? [],
        };
    }
}
