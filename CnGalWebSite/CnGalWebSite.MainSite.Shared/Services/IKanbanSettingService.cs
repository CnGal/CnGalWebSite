using System;
using System.Threading.Tasks;
using CnGalWebSite.MainSite.Shared.Services.KanbanModels;

namespace CnGalWebSite.MainSite.Shared.Services;

/// <summary>
/// 看板娘位置/大小持久化服务接口
/// </summary>
public interface IKanbanSettingService
{
    /// <summary>
    /// 当前设置
    /// </summary>
    KanbanSettingModel Kanban { get; }
    ButtonSettingModel Button { get; }
    DialogBoxSettingModel DialogBox { get; }
    ChatCardSettingModel Chat { get; }

    /// <summary>
    /// 从 LocalStorage 加载设置，若不存在则重置为默认值
    /// </summary>
    Task LoadAsync();

    /// <summary>
    /// 保存当前设置到 LocalStorage（含边界校验）
    /// </summary>
    Task SaveAsync();

    /// <summary>
    /// 重置为默认值并保存
    /// </summary>
    Task ResetAsync();

    /// <summary>
    /// 生成看板娘 CSS 样式字符串（position: fixed + left/top）
    /// </summary>
    string GetStyles();

    /// <summary>
    /// 设置变更时触发
    /// </summary>
    event Action OnSettingChanged;
}
