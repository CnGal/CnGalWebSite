#nullable enable
using System.Collections.Generic;

namespace CnGalWebSite.DataModel.ViewModel.Mcp;

public sealed class McpItem
{
    public long Id { get; set; }
    public string Kind { get; set; } = "entry";
    public string Title { get; set; } = "";
    public string Summary { get; set; } = "";
    public string Url { get; set; } = "";
}

public sealed class McpQueryResult
{
    public List<McpItem> Items { get; set; } = new();
    public int? Total { get; set; }
    public int Page { get; set; } = 1;
    public string SourceUrl { get; set; } = "";
}

public sealed class McpDiscountSummary
{
    public int Count { get; set; }
    public string Url { get; set; } = "";
}
