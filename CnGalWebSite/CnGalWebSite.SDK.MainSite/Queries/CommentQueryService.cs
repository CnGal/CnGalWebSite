using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Coments;
using CnGalWebSite.Helper.ViewModel.Comments;
using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Infrastructure;
using CnGalWebSite.SDK.MainSite.Models;
using CnGalWebSite.SDK.MainSite.Models.Comments;
using Microsoft.Extensions.Logging;

namespace CnGalWebSite.SDK.MainSite.Queries;

/// <summary>
/// 评论查询服务实现。
/// </summary>
public sealed class CommentQueryService(HttpClient httpClient, PublicDataCache publicCache, ILogger<CommentQueryService> logger)
    : QueryServiceBase(httpClient), ICommentQueryService
{
    private readonly PublicDataCache memoryCache = publicCache.ForSource(httpClient.BaseAddress);

    private static readonly TimeSpan CommentCacheDuration = TimeSpan.FromMinutes(2);

    protected override ILogger Logger { get; } = logger;

    public async Task<SdkResult<CommentListViewModel>> GetCommentsAsync(
        CommentType type,
        string objectId,
        CancellationToken cancellationToken = default)
    {
        var cacheRead = await memoryCache.BeginQueryAsync(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        var cacheKey = $"main-site:comments:{(int)type}:{objectId}";
        if (memoryCache.TryGetValue(cacheKey, out CommentListViewModel? cached, cacheRead) && cached is not null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return memoryCache.Complete<CommentListViewModel>(cached, cacheRead);
        }

        var path = $"api/comments/GetComments/{(int)type}/{objectId}";
        var result = await GetSingleAsync<CommentCacheModel, CommentListViewModel>(
            path,
            MapToViewModel,
            "COMMENT",
            "评论列表",
            $"{type}/{objectId}",
            cancellationToken);

        if (result.Success && result.Data is not null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            memoryCache.Set(cacheKey, result.Data, CommentCacheDuration, cacheRead);
        }

        cancellationToken.ThrowIfCancellationRequested();
        return memoryCache.CompleteResult(result, cacheRead);
    }

    public void InvalidateCache(CommentType type, string objectId)
    {
        var cacheKey = $"main-site:comments:{(int)type}:{objectId}";
        memoryCache.Invalidate();
    }

    private static CommentListViewModel MapToViewModel(CommentCacheModel dto)
    {
        return new CommentListViewModel
        {
            Items = dto.Items?.Select(MapComment).ToList() ?? [],
            TotalCount = dto.Items?.Count ?? 0
        };
    }

    private static CommentItemViewModel MapComment(CommentViewModel comment)
    {
        return new CommentItemViewModel
        {
            Id = comment.Id,
            Text = comment.Text ?? "",
            CommentTime = comment.CommentTime,
            User = new CommentUserViewModel
            {
                Id = comment.UserInfor?.Id ?? "",
                Name = comment.UserInfor?.Name ?? "",
                PhotoPath = comment.UserInfor?.PhotoPath ?? "",
                Ranks = comment.UserInfor?.Ranks?.ToList() ?? []
            },
            Children = comment.InverseParentCodeNavigation?
                .Select(MapComment)
                .ToList() ?? []
        };
    }
}
