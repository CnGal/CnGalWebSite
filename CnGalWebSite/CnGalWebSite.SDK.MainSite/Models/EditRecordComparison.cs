using CnGalWebSite.DataModel.ViewModel.Admin;

namespace CnGalWebSite.SDK.MainSite.Models;

/// <summary>Two complete, reconstructed historical versions, independent of review-detail snapshots.</summary>
public sealed record EditRecordComparison(long ContrastId, long CurrentId,
    ExaminePreDataModel Before, ExaminePreDataModel After);
