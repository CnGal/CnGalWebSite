using CnGalWebSite.DataModel.ViewModel.Space;
using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Infrastructure;
using CnGalWebSite.SDK.MainSite.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CnGalWebSite.SDK.MainSite.Queries;

public sealed class SpaceQueryService(
    HttpClient httpClient,
    IMemoryCache memoryCache,
    ILogger<SpaceQueryService> logger) : QueryServiceBase(httpClient), ISpaceQueryService
{
    private static readonly TimeSpan SpaceCacheDuration = TimeSpan.FromMinutes(5);
    private const string SpaceDetailPathTemplate = "api/space/GetUserView/{0}";

    public async Task<SdkResult<SpaceDetailViewModel>> GetSpaceDetailAsync(string id, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"main-site:space-detail:{id}";
        if (memoryCache.TryGetValue(cacheKey, out SpaceDetailViewModel? cached) && cached is not null)
        {
            return SdkResult<SpaceDetailViewModel>.Ok(cached);
        }

        try
        {
            var path = string.Format(SpaceDetailPathTemplate, id);
            var (response, responseBody) = await GetAsyncWithBody(HttpClient, path, cancellationToken);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                logger.LogWarning(
                    "用户空间不存在。UserId={UserId}; Path={Path}; StatusCode={StatusCode}; BaseAddress={BaseAddress}; ResponseBody={ResponseBody}",
                    id,
                    path,
                    (int)response.StatusCode,
                    HttpClient.BaseAddress,
                    TrimForLog(responseBody));
                return SdkResult<SpaceDetailViewModel>.Fail("SPACE_NOT_FOUND", "未找到该用户", (int)response.StatusCode);
            }

            if (!response.IsSuccessStatusCode)
            {
                logger.LogError(
                    "用户空间接口请求失败。UserId={UserId}; Path={Path}; StatusCode={StatusCode}; BaseAddress={BaseAddress}; ResponseBody={ResponseBody}",
                    id,
                    path,
                    (int)response.StatusCode,
                    HttpClient.BaseAddress,
                    TrimForLog(responseBody));
                return SdkResult<SpaceDetailViewModel>.Fail("SPACE_QUERY_FAILED", "获取用户空间信息失败", (int)response.StatusCode);
            }

            PersonalSpaceViewModel? space;
            try
            {
                space = Deserialize<PersonalSpaceViewModel>(responseBody);
            }
            catch (JsonException ex)
            {
                logger.LogError(
                    ex,
                    "用户空间接口反序列化失败。UserId={UserId}; Path={Path}; BaseAddress={BaseAddress}; ResponseBody={ResponseBody}",
                    id,
                    path,
                    HttpClient.BaseAddress,
                    TrimForLog(responseBody));
                return SdkResult<SpaceDetailViewModel>.Fail("SPACE_INVALID_RESPONSE", "用户空间数据格式不兼容", 200);
            }

            if (space is null)
            {
                return SdkResult<SpaceDetailViewModel>.Fail("SPACE_EMPTY_RESPONSE", "用户空间数据为空");
            }

            var basicInfo = space.BasicInfor ?? new UserInforViewModel();

            var model = new SpaceDetailViewModel
            {
                Id = space.Id ?? string.Empty,
                Name = basicInfo.Name ?? string.Empty,
                PersonalSignature = basicInfo.PersonalSignature ?? string.Empty,
                BackgroundImage = basicInfo.BackgroundImage ?? string.Empty,
                MBgImage = basicInfo.MBgImage ?? string.Empty,
                SBgImage = basicInfo.SBgImage ?? string.Empty,
                PhotoPath = basicInfo.PhotoPath ?? string.Empty,
                MainPageHtml = space.MainPageContext ?? string.Empty,
                Integral = basicInfo.Integral,
                GCoins = basicInfo.GCoins,
                ContributionValue = space.ContributionValue,
                EditCount = basicInfo.EditCount,
                ArticleCount = basicInfo.ArticleCount,
                VideoCount = basicInfo.VideoCount,
                SignInDays = basicInfo.SignInDays,
                Birthday = space.Birthday,
                RegisteTime = space.RegisteTime,
                LastOnlineTime = space.LastOnlineTime,
                OnlineTime = space.OnlineTime,
                Ranks = basicInfo.Ranks?.ToList() ?? [],
                UserCertification = space.UserCertification
            };

            memoryCache.Set(cacheKey, model, SpaceCacheDuration);
            return SdkResult<SpaceDetailViewModel>.Ok(model);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "获取用户空间详情异常。UserId={UserId}; Path={Path}; BaseAddress={BaseAddress}",
                id,
                string.Format(SpaceDetailPathTemplate, id),
                HttpClient.BaseAddress);
            return SdkResult<SpaceDetailViewModel>.Fail("SPACE_QUERY_EXCEPTION", "请求用户空间详情时发生异常");
        }
    }
}
