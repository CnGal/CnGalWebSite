using System.Net.Http.Json;
using CnGalWebSite.Core.Models;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Commodities;
using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Infrastructure;
using CnGalWebSite.SDK.MainSite.Models;
using Microsoft.Extensions.Logging;

namespace CnGalWebSite.SDK.MainSite.Queries;

public sealed class CommodityQueryService(
    HttpClient httpClient,
    ILogger<CommodityQueryService> logger) : QueryServiceBase(httpClient), ICommodityQueryService
{
    private const string GetAllPath = "api/commodities/GetAllCommodities";
    private const string BuyPath = "api/commodities/BuyCommodity";
    private const string RedeemPath = "api/commodities/RedeemedCommodityCode";
    private const string ListRecordPath = "api/commodities/ListGCoinsRecord";

    protected override ILogger Logger => logger;

    public async Task<SdkResult<List<CommodityUserModel>>> GetAllCommoditiesAsync(CancellationToken cancellationToken = default)
    {
        return await GetAsync<List<CommodityUserModel>>(
            GetAllPath,
            "COMMODITY",
            "商品列表",
            cancellationToken);
    }

    public async Task<SdkResult<bool>> BuyCommodityAsync(long commodityId, CancellationToken cancellationToken = default)
    {
        const string operationName = "购买商品";

        try
        {
            var model = new BuyCommodityModel { Id = commodityId };
            var response = await HttpClient.PostAsJsonAsync(BuyPath, model, SdkJsonSerializerOptions.Default, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                Logger.LogError(
                    "{Operation}请求失败。Path={Path}; StatusCode={StatusCode}; BaseAddress={BaseAddress}; ResponseBody={ResponseBody}",
                    operationName,
                    BuyPath,
                    (int)response.StatusCode,
                    HttpClient.BaseAddress,
                    TrimForLog(body));

                return SdkResult<bool>.Fail("COMMODITY_BUY_HTTP_FAILED", $"{operationName}失败（HTTP {(int)response.StatusCode}）");
            }

            var result = await response.Content.ReadFromJsonAsync<CnGalWebSite.DataModel.Model.Result>(SdkJsonSerializerOptions.Default, cancellationToken);

            if (result is null)
            {
                return SdkResult<bool>.Fail("COMMODITY_BUY_EMPTY_RESPONSE", $"{operationName}返回数据为空");
            }

            if (!result.Successful)
            {
                return SdkResult<bool>.Fail("COMMODITY_BUY_FAILED", result.Error ?? $"{operationName}失败");
            }

            return SdkResult<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "{Operation}异常。Path={Path}; BaseAddress={BaseAddress}", operationName, BuyPath, HttpClient.BaseAddress);
            return SdkResult<bool>.Fail("COMMODITY_BUY_EXCEPTION", $"请求{operationName}时发生异常");
        }
    }

    public async Task<SdkResult<bool>> RedeemedCommodityCodeAsync(RedeemedCommodityCodeModel model, CancellationToken cancellationToken = default)
    {
        const string operationName = "兑换G币";

        try
        {
            var response = await HttpClient.PostAsJsonAsync(RedeemPath, model, SdkJsonSerializerOptions.Default, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                Logger.LogError(
                    "{Operation}请求失败。Path={Path}; StatusCode={StatusCode}; BaseAddress={BaseAddress}; ResponseBody={ResponseBody}",
                    operationName,
                    RedeemPath,
                    (int)response.StatusCode,
                    HttpClient.BaseAddress,
                    TrimForLog(body));

                return SdkResult<bool>.Fail("COMMODITY_REDEEM_HTTP_FAILED", $"{operationName}失败（HTTP {(int)response.StatusCode}）");
            }

            var result = await response.Content.ReadFromJsonAsync<CnGalWebSite.DataModel.Model.Result>(SdkJsonSerializerOptions.Default, cancellationToken);

            if (result is null)
            {
                return SdkResult<bool>.Fail("COMMODITY_REDEEM_EMPTY_RESPONSE", $"{operationName}返回数据为空");
            }

            if (!result.Successful)
            {
                return SdkResult<bool>.Fail("COMMODITY_REDEEM_FAILED", result.Error ?? $"{operationName}失败");
            }

            return SdkResult<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "{Operation}异常。Path={Path}; BaseAddress={BaseAddress}", operationName, RedeemPath, HttpClient.BaseAddress);
            return SdkResult<bool>.Fail("COMMODITY_REDEEM_EXCEPTION", $"请求{operationName}时发生异常");
        }
    }

    public async Task<SdkResult<QueryResultModel<GCoinsRecordOverviewModel>>> ListGCoinsRecordAsync(QueryParameterModel model, CancellationToken cancellationToken = default)
    {
        const string entityDescription = "G币变动记录";

        try
        {
            var response = await HttpClient.PostAsJsonAsync(ListRecordPath, model, SdkJsonSerializerOptions.Default, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                Logger.LogError(
                    "获取{EntityDescription}失败。Path={Path}; StatusCode={StatusCode}; BaseAddress={BaseAddress}; ResponseBody={ResponseBody}",
                    entityDescription,
                    ListRecordPath,
                    (int)response.StatusCode,
                    HttpClient.BaseAddress,
                    TrimForLog(body));

                return SdkResult<QueryResultModel<GCoinsRecordOverviewModel>>.Fail(
                    "COMMODITY_RECORD_HTTP_FAILED",
                    $"获取{entityDescription}失败（HTTP {(int)response.StatusCode}）");
            }

            var result = await response.Content.ReadFromJsonAsync<QueryResultModel<GCoinsRecordOverviewModel>>(
                SdkJsonSerializerOptions.Default,
                cancellationToken);

            if (result is null)
            {
                return SdkResult<QueryResultModel<GCoinsRecordOverviewModel>>.Fail(
                    "COMMODITY_RECORD_EMPTY_RESPONSE",
                    $"{entityDescription}数据为空");
            }

            return SdkResult<QueryResultModel<GCoinsRecordOverviewModel>>.Ok(result);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex,
                "获取{EntityDescription}异常。Path={Path}; BaseAddress={BaseAddress}",
                entityDescription,
                ListRecordPath,
                HttpClient.BaseAddress);

            return SdkResult<QueryResultModel<GCoinsRecordOverviewModel>>.Fail(
                "COMMODITY_RECORD_EXCEPTION",
                $"请求{entityDescription}时发生异常");
        }
    }
}
