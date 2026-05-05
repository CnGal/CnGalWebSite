using System;
using CnGalWebSite.MainSite.Shared.Services.KanbanModels;

namespace CnGalWebSite.MainSite.Shared.Services;

/// <summary>
/// Live2D 看板娘 JS 互操作服务接口
/// </summary>
public interface IKanbanLive2DService
{
    event Action Live2DInitialized;

    event Action<string> KanbanImageGenerated;

    event Action<string> CustomEventTriggered;

    KanbanSettingModel Kanban { get; }

    DialogBoxSettingModel DialogBox { get; }

    /// <summary>
    /// 初始化 Live2D：加载 ES 模块 JS，创建 DotNetObjectReference，调用 JS initKanbanLive2D
    /// </summary>
    /// <param name="modelDir">模型目录名（逗号分隔）</param>
    /// <param name="modelIndex">默认模型索引</param>
    Task InitAsync(string modelDir, int modelIndex);

    /// <summary>
    /// 切换模型
    /// </summary>
    Task SetModelAsync(int index);

    /// <summary>
    /// 设置表情
    /// </summary>
    Task SetExpression(string name);

    /// <summary>
    /// 清除表情
    /// </summary>
    Task CleanExpression();

    /// <summary>
    /// 设置衣服（保存到 UserData）
    /// </summary>
    Task SetClothes(string name);

    /// <summary>
    /// 设置丝袜（保存到 UserData）
    /// </summary>
    Task SetStockings(string name);

    /// <summary>
    /// 设置鞋子（保存到 UserData）
    /// </summary>
    Task SetShoes(string name);

    /// <summary>
    /// 设置动作
    /// </summary>
    Task SetMotion(string group, int index);

    /// <summary>
    /// 清除动作（恢复 Idle）
    /// </summary>
    Task CleanMotion();

    /// <summary>
    /// 触发 WebGL 看板娘截图生成
    /// </summary>
    Task StartKanbanImageGeneration();

    /// <summary>
    /// 获取看板娘截图（轮询 100 次 × 100ms = 最多 10 秒）
    /// </summary>
    /// <returns>Blob URL，超时返回 null</returns>
    Task<string?> GetKanbanImageGeneration();

    /// <summary>
    /// 释放 Live2D 资源
    /// </summary>
    Task ReleaseLive2D();
}
