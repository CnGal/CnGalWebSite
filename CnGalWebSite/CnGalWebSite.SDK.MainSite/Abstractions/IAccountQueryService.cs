using CnGalWebSite.DataModel.ViewModel.Accounts;
using CnGalWebSite.SDK.MainSite.Models;

namespace CnGalWebSite.SDK.MainSite.Abstractions;

public interface IAccountQueryService
{
    /// <summary>
    /// 获取 Geetest 人机验证初始化参数。
    /// </summary>
    Task<SdkResult<GeetestCodeModel>> GetGeetestCodeAsync(CancellationToken cancellationToken = default);
}
