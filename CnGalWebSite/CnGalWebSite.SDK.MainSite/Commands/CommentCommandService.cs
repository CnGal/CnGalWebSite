using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Coments;
using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Infrastructure;
using CnGalWebSite.SDK.MainSite.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CnGalWebSite.SDK.MainSite.Commands;

/// <summary>
/// 评论写操作服务实现。
/// </summary>
public sealed class CommentCommandService(HttpClient httpClient, IMemoryCache memoryCache, ILogger<CommentCommandService> logger)
    : CommandServiceBase(httpClient), ICommentCommandService
{
    protected override ILogger Logger { get; } = logger;

    public async Task<SdkResult<bool>> PublishCommentAsync(
        CommentType type,
        string objectId,
        string text,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var model = new PublishCommentModel
            {
                Type = type,
                ObjectId = objectId,
                Text = text
            };

            var result = await PostAsJsonAsync<PublishCommentModel, Result>(
                "api/comments/PublishComment", model, cancellationToken);

            if (result is null)
            {
                return SdkResult<bool>.Fail("COMMENT_PUBLISH_NULL_RESPONSE", "发表评论失败：服务器无响应");
            }

            if (!result.Successful)
            {
                Logger.LogWarning("发表评论失败。Error={Error}", result.Error);
                return SdkResult<bool>.Fail("COMMENT_PUBLISH_FAILED", result.Error ?? "发表评论失败");
            }

            InvalidateCommentCache(type, objectId);
            return SdkResult<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "发表评论异常。Type={Type}; ObjectId={ObjectId}", type, objectId);
            return SdkResult<bool>.Fail("COMMENT_PUBLISH_EXCEPTION", "发表评论时发生异常");
        }
    }

    private void InvalidateCommentCache(CommentType type, string objectId)
    {
        memoryCache.Remove($"main-site:comments:{(int)type}:{objectId}");
    }

    public async Task<SdkResult<bool>> DeleteCommentAsync(
        long commentId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var model = new HiddenCommentModel
            {
                Ids = [commentId],
                IsHidden = true
            };

            var result = await PostAsJsonAsync<HiddenCommentModel, Result>(
                "api/comments/UserHiddenComment", model, cancellationToken);

            if (result is null)
            {
                return SdkResult<bool>.Fail("COMMENT_DELETE_NULL_RESPONSE", "删除评论失败：服务器无响应");
            }

            if (!result.Successful)
            {
                Logger.LogWarning("删除评论失败。CommentId={CommentId}; Error={Error}", commentId, result.Error);
                return SdkResult<bool>.Fail("COMMENT_DELETE_FAILED", result.Error ?? "删除评论失败");
            }

            return SdkResult<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "删除评论异常。CommentId={CommentId}", commentId);
            return SdkResult<bool>.Fail("COMMENT_DELETE_EXCEPTION", "删除评论时发生异常");
        }
    }
}
