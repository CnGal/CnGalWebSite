using CnGalWebSite.MainSite.Shared.Services.KanbanModels;

namespace CnGalWebSite.MainSite.Shared.Services;

/// <summary>
/// 看板娘换装偏好持久化服务接口
/// </summary>
public interface IKanbanUserDataService
{
    /// <summary>
    /// 当前用户数据（衣服/丝袜/鞋子选择）
    /// </summary>
    KanbanUserDataModel UserData { get; }

    /// <summary>
    /// 从 LocalStorage 加载换装偏好
    /// </summary>
    Task LoadAsync();

    /// <summary>
    /// 保存当前换装偏好到 LocalStorage
    /// </summary>
    Task SaveAsync();

    /// <summary>
    /// 重置为默认值并保存
    /// </summary>
    Task ResetAsync();
}
