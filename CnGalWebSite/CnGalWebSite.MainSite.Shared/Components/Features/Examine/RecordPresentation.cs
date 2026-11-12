using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Admin;

namespace CnGalWebSite.MainSite.Shared.Components.Features.Examine;

public enum RecordContentKind { MainPage, Texts, Pictures, Relevances, Outlinks, Audio }

public static class RecordPresentation
{
    public static readonly IReadOnlyList<RecordContentKind> ContentKinds = Enum.GetValues<RecordContentKind>();

    public static bool HasContent(ExaminePreDataModel? data, RecordContentKind kind) => kind switch
    {
        RecordContentKind.MainPage => !string.IsNullOrWhiteSpace(data?.MainPage),
        RecordContentKind.Texts => data?.Texts?.Any(g => g.Informations?.Count > 0) == true,
        RecordContentKind.Pictures => data?.Pictures?.Count > 0,
        RecordContentKind.Relevances => data?.Relevances?.Count > 0,
        RecordContentKind.Outlinks => data?.Outlinks?.Count > 0,
        RecordContentKind.Audio => data?.Audio?.Count > 0,
        _ => false
    };

    public static bool HasContent(ExaminePreDataModel? data) => ContentKinds.Any(k => HasContent(data, k));
    public static string Title(RecordContentKind kind) => kind switch
    {
        RecordContentKind.MainPage => "正文", RecordContentKind.Texts => "文本信息",
        RecordContentKind.Pictures => "图片", RecordContentKind.Relevances => "关联对象",
        RecordContentKind.Outlinks => "外链", RecordContentKind.Audio => "音频", _ => "内容"
    };

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
        Operation.PubulishComment or Operation.RequestUserCertification or Operation.EditPlayedGameMain or Operation.EditFavoriteFolderMain;

    public static bool CanProcess(ExamineViewModel model) => model.IsAdmin && model.IsPassed is null && model.PrepositionExamineId <= 0;

    public static bool IsCompareMode(string? mode, long? currentId) => mode?.ToLowerInvariant() switch
    {
        "record" => false, "compare" => true, _ => currentId.HasValue
    };

    public static string? HttpUrl(string? url) => Uri.TryCreate(url, UriKind.Absolute, out var parsed)
        && (parsed.Scheme == Uri.UriSchemeHttp || parsed.Scheme == Uri.UriSchemeHttps) ? url : null;
}
