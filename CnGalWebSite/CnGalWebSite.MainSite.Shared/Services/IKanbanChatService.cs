using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CnGalWebSite.MainSite.Shared.Services.KanbanModels;

namespace CnGalWebSite.MainSite.Shared.Services;

/// <summary>
/// 看板娘聊天服务接口
/// </summary>
public interface IKanbanChatService
{
    /// <summary>
    /// 聊天消息列表
    /// </summary>
    IReadOnlyList<KanbanChatModel> Messages { get; }

    /// <summary>
    /// 聊天面板是否打开
    /// </summary>
    bool IsOpen { get; }

    /// <summary>
    /// 发送消息并获取回复
    /// </summary>
    /// <param name="message">用户消息</param>
    /// <param name="isFirst">是否首次对话</param>
    Task SendMessageAsync(string message, bool isFirst);

    /// <summary>
    /// 打开聊天面板
    /// </summary>
    void Show();

    /// <summary>
    /// 关闭聊天面板
    /// </summary>
    void Close();

    /// <summary>
    /// 清空聊天记录
    /// </summary>
    void Clear();
}
