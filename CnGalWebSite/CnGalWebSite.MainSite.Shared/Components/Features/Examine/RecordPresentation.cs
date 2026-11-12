using CnGalWebSite.DataModel.ViewModel.Admin;

namespace CnGalWebSite.MainSite.Shared.Components.Features.Examine;

public static class RecordPresentation
{
    public static string Display(RecordPreviewItem item) =>
        string.IsNullOrWhiteSpace(item.DisplayValue) ? item.Value : item.DisplayValue;

    public static bool SupportsHistory(ExaminedNormalListModelType type) => ExamineRecordTypeMetadata.SupportsHistory(type);

    public static string? ObjectUrl(ExaminedNormalListModelType type, long id, string? userId = null) =>
        ExamineRecordTypeMetadata.ObjectUrl(type, id, userId);

    public static bool CanProcess(ExamineViewModel model) => model.IsAdmin && model.IsPassed is null && model.PrepositionExamineId <= 0;

    public static bool IsCompareMode(string? mode, long? currentId) => mode?.ToLowerInvariant() switch
    {
        "record" => false, "compare" => true, _ => currentId.HasValue
    };

    public static string? ContentUrl(string? url) => HttpUrl(url)
        ?? (url?.StartsWith("/", StringComparison.Ordinal) == true ? url : null);

    public static string? HttpUrl(string? url) => Uri.TryCreate(url, UriKind.Absolute, out var parsed)
        && (parsed.Scheme == Uri.UriSchemeHttp || parsed.Scheme == Uri.UriSchemeHttps) ? url : null;
}
