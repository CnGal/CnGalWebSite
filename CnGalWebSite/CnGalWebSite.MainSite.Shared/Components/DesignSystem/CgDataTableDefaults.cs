namespace CnGalWebSite.MainSite.Shared.Components.DesignSystem;

/// <summary>数据表分页的共享默认值，URL 恢复与 UI 选项都从这里派生。</summary>
public static class CgDataTableDefaults
{
    public const int PageSize = 10;

    public static readonly IReadOnlyList<int> PageSizeOptions = [5, 10, 20, 50];

    public static int[] CreatePageSizeOptions() => PageSizeOptions.ToArray();
}
