using CnGalWebSite.DataModel.ViewModel;
using CnGalWebSite.DataModel.ViewModel.Space;
using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Infrastructure;
using CnGalWebSite.SDK.MainSite.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CnGalWebSite.SDK.MainSite.Queries;

public sealed class SpaceQueryService(
    HttpClient httpClient,
    IMemoryCache memoryCache,
    ILogger<SpaceQueryService> logger) : QueryServiceBase(httpClient), ISpaceQueryService
{
    private static readonly TimeSpan SpaceCacheDuration = TimeSpan.FromMinutes(5);
    private const string SpaceDetailPathTemplate = "api/space/GetUserView/{0}";

    protected override ILogger Logger => logger;

    public async Task<SdkResult<SpaceDetailViewModel>> GetSpaceDetailAsync(string id, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"main-site:space-detail:{id}";
        if (memoryCache.TryGetValue(cacheKey, out SpaceDetailViewModel? cached) && cached is not null)
        {
            return SdkResult<SpaceDetailViewModel>.Ok(cached);
        }

        var path = string.Format(SpaceDetailPathTemplate, id);
        var result = await GetSingleAsync<PersonalSpaceViewModel, SpaceDetailViewModel>(
            path,
            MapToViewModel,
            "SPACE",
            "用户空间",
            id,
            cancellationToken);

        if (result.Success && result.Data is not null)
        {
            memoryCache.Set(cacheKey, result.Data, SpaceCacheDuration);
        }

        return result;
    }

    private static SpaceDetailViewModel MapToViewModel(PersonalSpaceViewModel space)
    {
        var basicInfo = space.BasicInfor ?? new UserInforViewModel();

        return new SpaceDetailViewModel
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
            UserCertification = space.UserCertification,
            IsCurrentUser = space.IsCurrentUser
        };
    }
}
