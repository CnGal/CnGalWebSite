using CnGalWebSite.Core.Models;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Commodities;
using CnGalWebSite.SDK.MainSite.Models;

namespace CnGalWebSite.SDK.MainSite.Abstractions;

public interface ICommodityQueryService
{
    /// <summary>
    /// 获取所有商品列表（含当前用户拥有状态）
    /// </summary>
    Task<SdkResult<List<CommodityUserModel>>> GetAllCommoditiesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 购买商品（扣减 G 币）
    /// </summary>
    Task<SdkResult<bool>> BuyCommodityAsync(long commodityId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 兑换 G 币（B 站工房订单号）
    /// </summary>
    Task<SdkResult<bool>> RedeemedCommodityCodeAsync(RedeemedCommodityCodeModel model, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取 G 币变动记录（服务端分页）
    /// </summary>
    Task<SdkResult<QueryResultModel<GCoinsRecordOverviewModel>>> ListGCoinsRecordAsync(QueryParameterModel model, CancellationToken cancellationToken = default);
}
