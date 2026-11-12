using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Admin;

namespace CnGalWebSite.MainSite.Shared.Components.Features.Examine;

public static class RecordPresentation
{
    public static bool HasContent(ExaminePreDataModel? data) => data?.Items?.Count > 0;

    public static string Display(RecordPreviewItem item) =>
        string.IsNullOrWhiteSpace(item.DisplayValue) ? item.Value : item.DisplayValue;

    public static bool SupportsHistory(ExaminedNormalListModelType type) => type is
        ExaminedNormalListModelType.Entry or ExaminedNormalListModelType.Article or ExaminedNormalListModelType.Tag
        or ExaminedNormalListModelType.Periphery or ExaminedNormalListModelType.Video;

    public static string? ObjectUrl(ExaminedNormalListModelType type, long id, string? userId = null) => type switch
    {
        ExaminedNormalListModelType.User or ExaminedNormalListModelType.UserCertification =>
            string.IsNullOrWhiteSpace(userId) ? null : $"/space/index/{Uri.EscapeDataString(userId)}",
        ExaminedNormalListModelType.Entry when id > 0 => $"/entries/index/{id}",
        ExaminedNormalListModelType.Article when id > 0 => $"/articles/index/{id}",
        ExaminedNormalListModelType.Tag when id > 0 => $"/tags/index/{id}",
        ExaminedNormalListModelType.Periphery when id > 0 => $"/peripheries/index/{id}",
        ExaminedNormalListModelType.Video when id > 0 => $"/videos/index/{id}",
        ExaminedNormalListModelType.FavoriteFolder when id > 0 => $"/favoritefolders/index/{id}",
        _ => null
    };

    public static bool IsSingleSubmission(Operation operation) => operation is
        Operation.PubulishComment or Operation.RequestUserCertification;

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
