using System.Collections.Generic;
using System.Threading.Tasks;
using CnGalWebSite.MainSite.Shared.Services.KanbanModels;

namespace CnGalWebSite.MainSite.Shared.Services;

/// <summary>
/// CDN 远程配置服务接口 — 从 CDN 拉取看板娘元数据（事件、服装、表情、动作等）
/// </summary>
public interface IKanbanRemoteConfigService
{
    /// <summary>
    /// 获取事件组配置（时间事件、日期事件、空闲事件等）
    /// </summary>
    Task<KanbanEventGroupModel?> GetEventGroupAsync();

    /// <summary>
    /// 获取服装列表
    /// </summary>
    Task<List<KanbanClothesModel>> GetClothesAsync();

    /// <summary>
    /// 获取表情列表
    /// </summary>
    Task<List<KanbanExpressionModel>> GetExpressionsAsync();

    /// <summary>
    /// 获取动作组列表（含子动作）
    /// </summary>
    Task<List<KanbanMotionGroupModel>> GetMotionGroupsAsync();

    /// <summary>
    /// 获取袜子列表
    /// </summary>
    Task<List<KanbanStockingsModel>> GetStockingsAsync();

    /// <summary>
    /// 获取鞋子列表
    /// </summary>
    Task<List<KanbanShoesModel>> GetShoesAsync();
}
