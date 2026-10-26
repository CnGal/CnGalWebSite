#nullable enable
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CnGalWebSite.DataModel.ViewModel.Robots;

public sealed class RobotItem
{
    public long Id { get; set; }
    public string Kind { get; set; } = "entry";
    public string Title { get; set; } = "";
    public string Summary { get; set; } = "";
    public string Url { get; set; } = "";
}

public sealed class RobotQueryResult
{
    public List<RobotItem> Items { get; set; } = new();
    public int? Total { get; set; }
    public int Page { get; set; } = 1;
    public string SourceUrl { get; set; } = "https://www.cngal.org/";
}

public sealed class RobotDiscountSummary
{
    public int Count { get; set; }
    public string Url { get; set; } = "https://www.cngal.org/discount";
}

public sealed class RobotBindRequest
{
    [Required, StringLength(2048, MinimumLength = 1)]
    public string Code { get; set; } = "";

    [Range(1, long.MaxValue)]
    public long SenderQQ { get; set; }
}
