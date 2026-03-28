using CnGalWebSite.DataModel.Model;
using CnGalWebSite.SDK.MainSite.Models;

namespace CnGalWebSite.SDK.MainSite.Abstractions;

/// <summary>
/// 评论写操作服务接口。
/// </summary>
public interface ICommentCommandService
{
    /// <summary>
    /// 发表评论。
    /// </summary>
    Task<SdkResult<bool>> PublishCommentAsync(CommentType type, string objectId, string text, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除评论（用户自己的评论）。
    /// </summary>
    Task<SdkResult<bool>> DeleteCommentAsync(long commentId, CancellationToken cancellationToken = default);
}
