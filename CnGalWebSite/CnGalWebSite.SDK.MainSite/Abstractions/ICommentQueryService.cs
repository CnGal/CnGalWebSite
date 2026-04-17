using CnGalWebSite.DataModel.Model;
using CnGalWebSite.SDK.MainSite.Models;
using CnGalWebSite.SDK.MainSite.Models.Comments;

namespace CnGalWebSite.SDK.MainSite.Abstractions;

/// <summary>
/// 评论查询服务接口（只读）。
/// </summary>
public interface ICommentQueryService
{
    /// <summary>
    /// 获取指定对象的评论列表。
    /// </summary>
    Task<SdkResult<CommentListViewModel>> GetCommentsAsync(CommentType type, string objectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 清除指定对象的评论列表缓存。
    /// </summary>
    void InvalidateCache(CommentType type, string objectId);
}
