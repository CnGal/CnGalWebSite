using CnGalWebSite.SDK.MainSite.Models;
using CnGalWebSite.SDK.MainSite.Models.Kanban;

namespace CnGalWebSite.SDK.MainSite.Abstractions;

public interface IKanbanQueryService
{
    /// <summary>
    /// 获取看板娘聊天回复
    /// </summary>
    Task<SdkResult<KanbanChatReply>> GetChatReplyAsync(KanbanChatRequest request, CancellationToken cancellationToken = default);
}
