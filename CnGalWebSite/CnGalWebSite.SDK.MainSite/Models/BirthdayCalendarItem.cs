namespace CnGalWebSite.SDK.MainSite.Models;

/// <summary>
/// 角色生日日历条目，用于日历页按日展示角色。
/// </summary>
public sealed class BirthdayCalendarItem
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required string Image { get; init; }
    public required string BriefIntroduction { get; init; }
    public required int BirthdayMonth { get; init; }
    public required int BirthdayDay { get; init; }
    public string? GameName { get; init; }
}
