using CnGalWebSite.DataModel.ViewModel.Expo;
using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Infrastructure;
using CnGalWebSite.SDK.MainSite.Models;
using Microsoft.Extensions.Logging;

namespace CnGalWebSite.SDK.MainSite.Queries;

public sealed class ExpoQueryService(
    HttpClient httpClient,
    ILogger<ExpoQueryService> logger) : QueryServiceBase(httpClient), IExpoQueryService
{
    protected override ILogger Logger => logger;

    private const string GetUserTaskPath = "api/expo/GetUserTask";
    private const string GetAllAwardsPath = "api/expo/GetAllAwards";
    private const string GetUserPrizesPath = "api/expo/GetUserPrizes";
    private const string CheckUserGameRatingPath = "api/expo/CheckUserGameRating";
    private const string CheckUserBindQQPath = "api/expo/CheckUserBindQQ";
    private const string CheckUserChangeAvatarPath = "api/expo/CheckUserChangeAvatar";
    private const string CheckUserChangeSignaturePath = "api/expo/CheckUserChangeSignature";

    public Task<SdkResult<ExpoUserTaskModel>> GetUserTaskAsync(CancellationToken cancellationToken = default)
    {
        return GetAsync<ExpoUserTaskModel>(GetUserTaskPath, "EXPO_USER_TASK", "用户任务状态", cancellationToken);
    }

    public Task<SdkResult<List<ExpoAwardOverviewModel>>> GetAllAwardsAsync(CancellationToken cancellationToken = default)
    {
        return GetAsync<List<ExpoAwardOverviewModel>>(GetAllAwardsPath, "EXPO_AWARDS", "奖项列表", cancellationToken);
    }

    public Task<SdkResult<List<ExpoPrizeOverviewModel>>> GetUserPrizesAsync(CancellationToken cancellationToken = default)
    {
        return GetAsync<List<ExpoPrizeOverviewModel>>(GetUserPrizesPath, "EXPO_USER_PRIZES", "用户奖品", cancellationToken);
    }

    public Task<SdkResult<bool>> CheckUserGameRatingAsync(CancellationToken cancellationToken = default)
    {
        return GetAsync<bool>(CheckUserGameRatingPath, "EXPO_CHECK_RATING", "游戏评分检查", cancellationToken);
    }

    public Task<SdkResult<bool>> CheckUserBindQQAsync(CancellationToken cancellationToken = default)
    {
        return GetAsync<bool>(CheckUserBindQQPath, "EXPO_CHECK_QQ", "QQ绑定检查", cancellationToken);
    }

    public Task<SdkResult<bool>> CheckUserChangeAvatarAsync(CancellationToken cancellationToken = default)
    {
        return GetAsync<bool>(CheckUserChangeAvatarPath, "EXPO_CHECK_AVATAR", "头像更换检查", cancellationToken);
    }

    public Task<SdkResult<bool>> CheckUserChangeSignatureAsync(CancellationToken cancellationToken = default)
    {
        return GetAsync<bool>(CheckUserChangeSignaturePath, "EXPO_CHECK_SIGNATURE", "签名更换检查", cancellationToken);
    }
}
