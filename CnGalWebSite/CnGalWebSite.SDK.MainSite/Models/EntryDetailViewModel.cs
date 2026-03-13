namespace CnGalWebSite.SDK.MainSite.Models;

public sealed class EntryDetailViewModel
{
    public required int Id { get; init; }

    public required string Name { get; init; }

    public required string Description { get; init; }

    public required IReadOnlyList<string> Tags { get; init; }

    public required string? MainPicture { get; init; }
}
