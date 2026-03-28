namespace CnGalWebSite.MainSite.Shared.Components.DesignSystem;

/// <summary>
/// CgTabs 的 Tab 项数据模型。
/// </summary>
/// <param name="Key">唯一标识符，用于关联面板。</param>
/// <param name="Label">显示文本。</param>
/// <param name="Icon">可选 MDI 图标名称（如 "mdiHomeOutline"）。</param>
/// <param name="Badge">可选角标文本（如数量）。</param>
public record CgTabItem(string Key, string Label, string? Icon = null, string? Badge = null);
