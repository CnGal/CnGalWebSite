using CnGalWebSite.DataModel.Model;

namespace CnGalWebSite.SDK.MainSite.Models;

/// <summary>
/// 消息中心页面 ViewModel。
/// </summary>
public sealed class MessageListViewModel
{
    /// <summary>
    /// 全部消息（按时间倒序）。
    /// </summary>
    public IReadOnlyList<MessageItemViewModel> All { get; init; } = [];

    /// <summary>
    /// 回复我的消息。
    /// </summary>
    public IReadOnlyList<MessageItemViewModel> Replies { get; init; } = [];

    /// <summary>
    /// 审核通过提醒。
    /// </summary>
    public IReadOnlyList<MessageItemViewModel> Passed { get; init; } = [];

    /// <summary>
    /// 审核驳回提醒。
    /// </summary>
    public IReadOnlyList<MessageItemViewModel> Rejected { get; init; } = [];

    /// <summary>
    /// 是否有未读的"回复我的"消息。
    /// </summary>
    public bool HasUnreadReplies { get; init; }

    /// <summary>
    /// 是否有未读的"审核通过"消息。
    /// </summary>
    public bool HasUnreadPassed { get; init; }

    /// <summary>
    /// 是否有未读的"审核驳回"消息。
    /// </summary>
    public bool HasUnreadRejected { get; init; }
}

/// <summary>
/// 单条消息 ViewModel。
/// </summary>
public sealed class MessageItemViewModel
{
    public long Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Text { get; init; } = string.Empty;

    public string Image { get; init; } = string.Empty;

    public string Link { get; init; } = string.Empty;

    public string LinkTitle { get; init; } = string.Empty;

    public DateTime PostTime { get; init; }

    public bool IsReaded { get; init; }

    public MessageType Type { get; init; }

    /// <summary>
    /// 关联的评论 ID（用于回复场景），0 表示无关联评论。
    /// </summary>
    public long CommentId { get; init; }
}
