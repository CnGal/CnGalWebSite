using CnGalWebSite.Core.Models;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.SDK.MainSite.Models;

namespace CnGalWebSite.SDK.MainSite.Abstractions;

public interface IAdminQueryService
{
    /// <summary>
    /// 获取 API Server 静态概览数据。
    /// </summary>
    Task<SdkResult<ServerStaticOverviewModel>> GetServerOverviewAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 分页查询评论列表（服务端分页）。
    /// </summary>
    Task<SdkResult<QueryResultModel<CommentOverviewModel>>> GetCommentsAsync(
        QueryParameterModel parameter,
        CancellationToken cancellationToken = default);
}
