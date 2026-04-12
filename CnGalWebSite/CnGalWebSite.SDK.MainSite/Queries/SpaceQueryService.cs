using System.Text.Json;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Dtos;
using CnGalWebSite.DataModel.ViewModel.Others;
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

    public async Task<SdkResult<MessageListViewModel>> GetUserMessagesAsync(CancellationToken cancellationToken = default)
    {
        var result = await GetAsync<List<Message>>(
            "api/space/GetUserMessages",
            "MESSAGE",
            "用户消息列表",
            cancellationToken);

        if (!result.Success || result.Data is null)
        {
            return SdkResult<MessageListViewModel>.Fail(
                result.Error?.Code ?? "MESSAGE_QUERY_FAILED",
                result.Error?.Message ?? "获取用户消息列表失败",
                result.Error?.StatusCode);
        }

        var allItems = result.Data
            .OrderByDescending(m => m.PostTime)
            .Select(MapToMessageViewModel)
            .ToList();

        var replies = allItems
            .Where(m => m.Type is MessageType.ArticleReply or MessageType.SpaceReply or MessageType.CommentReply)
            .ToList();

        var passed = allItems
            .Where(m => m.Type is MessageType.ExaminePassed)
            .ToList();

        var rejected = allItems
            .Where(m => m.Type is MessageType.ExamineUnPassed)
            .ToList();

        var viewModel = new MessageListViewModel
        {
            All = allItems,
            Replies = replies,
            Passed = passed,
            Rejected = rejected,
            HasUnreadReplies = replies.Any(m => !m.IsReaded),
            HasUnreadPassed = passed.Any(m => !m.IsReaded),
            HasUnreadRejected = rejected.Any(m => !m.IsReaded),
        };

        return SdkResult<MessageListViewModel>.Ok(viewModel);
    }

    public async Task<SdkResult<long>> GetUnreadMessageCountAsync(CancellationToken cancellationToken = default)
    {
        return await GetAsync<long>(
            "api/space/GetUserUnReadedMessageCount",
            "MESSAGE",
            "未读消息数量",
            cancellationToken);
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
            IsCurrentUser = space.IsCurrentUser,
            SteamId = space.SteamId ?? string.Empty,
            CanComment = space.CanComment,
            IsShowFavorites = space.IsShowFavorites,
            IsShowGameRecord = space.IsShowGameRecord
        };
    }

    private static MessageItemViewModel MapToMessageViewModel(Message m) => new()
    {
        Id = m.Id,
        Title = m.Title ?? string.Empty,
        Text = m.Text ?? string.Empty,
        Image = m.Image ?? string.Empty,
        Link = m.Link ?? string.Empty,
        LinkTitle = m.LinkTitle ?? string.Empty,
        PostTime = m.PostTime,
        IsReaded = m.IsReaded,
        Type = m.Type,
        CommentId = ParseCommentId(m.AdditionalInfor),
    };

    private static long ParseCommentId(string? additionalInfo)
    {
        if (string.IsNullOrWhiteSpace(additionalInfo))
        {
            return 0;
        }

        return long.TryParse(additionalInfo, out var id) ? id : 0;
    }

    public async Task<SdkResult<UserArticlesViewModel>> GetUserArticlesAsync(string userId, int currentPage = 1, int maxResultCount = 10, CancellationToken cancellationToken = default)
    {
        var path = $"api/space/GetUserArticles/{userId}?currentPage={currentPage}&maxResultCount={maxResultCount}";
        var result = await GetAsync<UserArticleListModel>(path, "SPACE", "用户文章列表", cancellationToken);

        if (!result.Success || result.Data is null)
        {
            return SdkResult<UserArticlesViewModel>.Fail(
                result.Error?.Code ?? "SPACE_ARTICLES_FAILED",
                result.Error?.Message ?? "获取用户文章列表失败",
                result.Error?.StatusCode);
        }

        var vm = new UserArticlesViewModel
        {
            TotalCount = result.Data.TotalCount,
            CurrentPage = result.Data.CurrentPage,
            MaxCount = result.Data.MaxCount,
            Items = result.Data.Items ?? []
        };
        return SdkResult<UserArticlesViewModel>.Ok(vm);
    }

    public async Task<SdkResult<UserVideosViewModel>> GetUserVideosAsync(string userId, CancellationToken cancellationToken = default)
    {
        var path = $"api/space/GetUserVideos/{userId}";
        var result = await GetAsync<UserVideoListModel>(path, "SPACE", "用户视频列表", cancellationToken);

        if (!result.Success || result.Data is null)
        {
            return SdkResult<UserVideosViewModel>.Fail(
                result.Error?.Code ?? "SPACE_VIDEOS_FAILED",
                result.Error?.Message ?? "获取用户视频列表失败",
                result.Error?.StatusCode);
        }

        var vm = new UserVideosViewModel
        {
            Items = result.Data.Items ?? []
        };
        return SdkResult<UserVideosViewModel>.Ok(vm);
    }

    public async Task<SdkResult<UserEditRecordsViewModel>> GetUserEditRecordsAsync(string userId, int currentPage = 1, int maxResultCount = 12, CancellationToken cancellationToken = default)
    {
        var path = $"api/space/GetUserEditRecord?userId={userId}&sorting=ApplyTime desc&maxResultCount={maxResultCount}&currentPage={currentPage}&screeningConditions=全部";
        var result = await GetAsync<PagedResultDto<ExaminedNormalListModel>>(path, "SPACE", "用户编辑记录", cancellationToken);

        if (!result.Success || result.Data is null)
        {
            return SdkResult<UserEditRecordsViewModel>.Fail(
                result.Error?.Code ?? "SPACE_EDIT_RECORDS_FAILED",
                result.Error?.Message ?? "获取用户编辑记录失败",
                result.Error?.StatusCode);
        }

        var vm = new UserEditRecordsViewModel
        {
            TotalCount = result.Data.TotalCount,
            CurrentPage = currentPage,
            MaxCount = maxResultCount,
            Items = result.Data.Data ?? []
        };
        return SdkResult<UserEditRecordsViewModel>.Ok(vm);
    }

    public async Task<SdkResult<UserHeatMapViewModel>> GetUserHeatMapAsync(string userId, UserHeatMapType type, DateTime? after = null, DateTime? before = null, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        after ??= now.AddYears(-1);
        before ??= now;

        var afterTs = new DateTimeOffset(after.Value).ToUnixTimeMilliseconds();
        var beforeTs = new DateTimeOffset(before.Value).ToUnixTimeMilliseconds();

        var path = $"api/space/GetUserHeatMap?id={userId}&type={type}&afterTime={afterTs}&beforeTime={beforeTs}";
        var result = await GetAsync<EChartsHeatMapOptionModel>(path, "SPACE", "用户热力图", cancellationToken);

        if (!result.Success || result.Data is null)
        {
            return SdkResult<UserHeatMapViewModel>.Fail(
                result.Error?.Code ?? "SPACE_HEATMAP_FAILED",
                result.Error?.Message ?? "获取用户热力图失败",
                result.Error?.StatusCode);
        }

        var maxCount = result.Data.VisualMap?.Max ?? 10;
        var dataPoints = new List<HeatMapDataPoint>();

        foreach (var series in result.Data.Series ?? [])
        {
            foreach (var item in series.Data ?? [])
            {
                if (item.Value is not { Count: >= 2 })
                    continue;

                // System.Text.Json 反序列化 List<object> 时，元素实际类型为 JsonElement
                var dateRaw = item.Value[0];
                var countRaw = item.Value[1];

                var date = dateRaw is JsonElement dateEl ? dateEl.GetString() : dateRaw?.ToString();
                var count = 0;

                if (countRaw is JsonElement countEl)
                {
                    count = countEl.ValueKind switch
                    {
                        JsonValueKind.Number => countEl.TryGetInt32(out var iv) ? iv : (int)countEl.GetDouble(),
                        JsonValueKind.String when int.TryParse(countEl.GetString(), out var sp) => sp,
                        _ => 0
                    };
                }
                else if (countRaw is int intVal) count = intVal;
                else if (countRaw is long longVal) count = (int)longVal;
                else if (countRaw is double dblVal) count = (int)dblVal;
                else if (int.TryParse(countRaw?.ToString(), out var parsed)) count = parsed;

                if (!string.IsNullOrWhiteSpace(date))
                {
                    dataPoints.Add(new HeatMapDataPoint { Date = date, Count = count });
                }
            }
        }

        var vm = new UserHeatMapViewModel
        {
            Data = dataPoints,
            MaxCount = maxCount
        };
        return SdkResult<UserHeatMapViewModel>.Ok(vm);
    }
}
