namespace CnGalWebSite.SDK.MainSite.Models;

public enum RecordPreviewKind { Body, Text, Image, Relation, Link, Audio }

public sealed class RecordPreviewProperty
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string DisplayValue { get; set; } = string.Empty;
}

public sealed class RecordPreviewItem
{
    public string Key { get; set; } = string.Empty;
    public string Group { get; set; } = string.Empty;
    public RecordPreviewKind Kind { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string DisplayValue { get; set; } = string.Empty;
    public string? Url { get; set; }
    public string? Html { get; set; }
    public List<RecordPreviewProperty> Properties { get; set; } = [];
    public bool IsKnown { get; set; } = true;
}

public sealed class RecordPreviewModel
{
    public List<RecordPreviewItem> Items { get; set; } = [];
    public bool IsComplete { get; set; } = true;
}

public sealed class EditRecordComparison
{
    public long ContrastId { get; set; }
    public long CurrentId { get; set; }
    public RecordPreviewModel Before { get; set; } = new();
    public RecordPreviewModel After { get; set; } = new();
}
