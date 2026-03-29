using CnGalWebSite.DataModel.Model;
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
            IsShowFavorites = space.IsShowFavorites
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
}

