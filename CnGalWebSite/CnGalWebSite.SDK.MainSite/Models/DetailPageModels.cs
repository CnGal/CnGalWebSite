using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Tags;

namespace CnGalWebSite.SDK.MainSite.Models;

public sealed record ArticlePageModel(ArticleDetailViewModel Article, SdkResult<IReadOnlyList<ExaminedNormalListModel>> EditRecords);
public sealed record VideoPageModel(VideoDetailViewModel Video, SdkResult<IReadOnlyList<ExaminedNormalListModel>> EditRecords);
public sealed record TagPageModel(TagIndexViewModel Tag, SdkResult<IReadOnlyList<ExaminedNormalListModel>> EditRecords);
public sealed record PeripheryPageModel(PeripheryDetailViewModel Periphery, SdkResult<IReadOnlyList<ExaminedNormalListModel>> EditRecords);
