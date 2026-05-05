using CnGalWebSite.DataModel.ViewModel.Accounts;
using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Infrastructure;
using CnGalWebSite.SDK.MainSite.Models;
using Microsoft.Extensions.Logging;

namespace CnGalWebSite.SDK.MainSite.Queries;

public sealed class AccountQueryService(
    HttpClient httpClient,
    ILogger<AccountQueryService> logger) : QueryServiceBase(httpClient), IAccountQueryService
{
    private const string GeetestCodePath = "api/account/GetGeetestCode";

    protected override ILogger Logger => logger;

    public async Task<SdkResult<GeetestCodeModel>> GetGeetestCodeAsync(CancellationToken cancellationToken = default)
    {
        var result = await GetAsync<GeetestCodeModel>(
            GeetestCodePath,
            "ACCOUNT_GEETEST",
            "Geetest 人机验证初始化参数",
            cancellationToken);

        if (!result.Success || result.Data is null)
        {
            return result;
        }

        if (string.IsNullOrWhiteSpace(result.Data.Gt))
        {
            logger.LogError(
                "Geetest 服务端返回数据无效。Path={Path}; BaseAddress={BaseAddress}",
                GeetestCodePath,
                httpClient.BaseAddress);

            return SdkResult<GeetestCodeModel>.Fail(
                "ACCOUNT_GEETEST_INVALID_RESPONSE",
                "Geetest 服务端返回数据无效");
        }

        return result;
    }
}
