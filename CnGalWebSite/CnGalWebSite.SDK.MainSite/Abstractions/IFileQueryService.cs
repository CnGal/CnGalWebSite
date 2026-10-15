using CnGalWebSite.DrawingBed.Models.Sorting;
using CnGalWebSite.Core.Models;
using CnGalWebSite.DrawingBed.Models.ViewModels;
using CnGalWebSite.SDK.MainSite.Models;

namespace CnGalWebSite.SDK.MainSite.Abstractions;

/// <summary>
/// 上传记录查询服务接口（DrawingBed 外部 API）。
/// </summary>
public interface IFileQueryService
{
    Task<SdkResult<QueryResultModel<UploadRecordOverviewModel, DrawingBedUploadRecordSortField>>> GetUploadRecordsAsync(
        QueryParameterModel<DrawingBedUploadRecordSortField> parameter,
        CancellationToken cancellationToken = default);
}
