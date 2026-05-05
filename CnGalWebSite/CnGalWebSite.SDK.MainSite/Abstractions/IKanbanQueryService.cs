using CnGalWebSite.SDK.MainSite.Models;
using CnGalWebSite.SDK.MainSite.Models.Kanban;

namespace CnGalWebSite.SDK.MainSite.Abstractions;

public interface IKanbanQueryService
{
    /// <summary>
    /// 获取看板娘聊天回复
    /// </summary>
    Task<SdkResult<KanbanChatReply>> GetChatReplyAsync(KanbanChatRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取看板娘显示权限（控制看板娘是否可见）
    /// </summary>
    Task<SdkResult<KanbanPermissionsReply>> GetPermissionsAsync(CancellationToken cancellationToken = default);
}
