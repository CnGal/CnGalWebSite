namespace CnGalWebSite.SDK.MainSite.Models.EntryEdit;

/// <summary>
/// Steam 商店应用详情（用于导入 Steam 信息到词条编辑）
/// </summary>
public sealed class SteamAppDetailsViewModel
{
    /// <summary>
    /// 游戏名称
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 主图 URL（header_image）
    /// </summary>
    public string? MainImage { get; set; }

    /// <summary>
    /// 简介（清洗后的 short_description）
    /// </summary>
    public string? BriefIntroduction { get; set; }

    /// <summary>
    /// 开发商（逗号分隔）
    /// </summary>
    public string? Developers { get; set; }

    /// <summary>
    /// 发行商（逗号分隔）
    /// </summary>
    public string? Publishers { get; set; }

    /// <summary>
    /// 截图完整尺寸 URL 列表
    /// </summary>
    public List<string> Screenshots { get; set; } = [];

    /// <summary>
    /// 详情描述，已从 HTML 转换为 Markdown
    /// </summary>
    public string? MainPageMarkdown { get; set; }

    /// <summary>
    /// 原始发行日期文本（如 "2016年6月28日"）
    /// </summary>
    public string? ReleaseDateString { get; set; }

    /// <summary>
    /// 解析后的发行日期（UTC，已补偿时区偏移）
    /// </summary>
    public DateTime? ReleaseDate { get; set; }
}
