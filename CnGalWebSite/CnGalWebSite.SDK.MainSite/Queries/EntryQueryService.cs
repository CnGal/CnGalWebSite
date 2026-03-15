using CnGalWebSite.DataModel.ViewModel;
using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Infrastructure;
using CnGalWebSite.SDK.MainSite.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CnGalWebSite.SDK.MainSite.Queries;

public sealed class EntryQueryService(
    HttpClient httpClient,
    IMemoryCache memoryCache,
    ILogger<EntryQueryService> logger) : QueryServiceBase(httpClient), IEntryQueryService
{
    private static readonly TimeSpan EntryCacheDuration = TimeSpan.FromMinutes(5);
    private const string EntryDetailPathTemplate = "api/entries/GetEntryView/{0}";

    public async Task<SdkResult<EntryDetailViewModel>> GetEntryDetailAsync(int id, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"main-site:entry-detail:{id}";
        if (memoryCache.TryGetValue(cacheKey, out EntryDetailViewModel? cached) && cached is not null)
        {
            return SdkResult<EntryDetailViewModel>.Ok(cached);
        }

        try
        {
            var path = string.Format(EntryDetailPathTemplate, id);
            var (response, responseBody) = await GetAsyncWithBody(HttpClient, path, cancellationToken);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                logger.LogWarning(
                    "条目不存在。EntryId={EntryId}; Path={Path}; StatusCode={StatusCode}; BaseAddress={BaseAddress}; ResponseBody={ResponseBody}",
                    id,
                    path,
                    (int)response.StatusCode,
                    HttpClient.BaseAddress,
                    TrimForLog(responseBody));
                return SdkResult<EntryDetailViewModel>.Fail("ENTRY_NOT_FOUND", "未找到对应条目", (int)response.StatusCode);
            }

            if (!response.IsSuccessStatusCode)
            {
                logger.LogError(
                    "条目接口请求失败。EntryId={EntryId}; Path={Path}; StatusCode={StatusCode}; BaseAddress={BaseAddress}; ResponseBody={ResponseBody}",
                    id,
                    path,
                    (int)response.StatusCode,
                    HttpClient.BaseAddress,
                    TrimForLog(responseBody));
                return SdkResult<EntryDetailViewModel>.Fail("ENTRY_QUERY_FAILED", "获取条目详情失败", (int)response.StatusCode);
            }

            EntryIndexViewModel? entry;
            try
            {
                entry = Deserialize<EntryIndexViewModel>(responseBody);
            }
            catch (JsonException ex)
            {
                logger.LogError(
                    ex,
                    "条目接口反序列化失败。EntryId={EntryId}; Path={Path}; BaseAddress={BaseAddress}; ResponseBody={ResponseBody}",
                    id,
                    path,
                    HttpClient.BaseAddress,
                    TrimForLog(responseBody));
                return SdkResult<EntryDetailViewModel>.Fail("ENTRY_INVALID_RESPONSE", "条目数据格式不兼容", 200);
            }

            if (entry is null)
            {
                return SdkResult<EntryDetailViewModel>.Fail("ENTRY_EMPTY_RESPONSE", "条目数据为空");
            }

            var model = new EntryDetailViewModel
            {
                Id = entry.Id,
                Name = entry.Name ?? string.Empty,
                AnotherName = entry.AnotherName ?? string.Empty,
                BriefIntroduction = entry.BriefIntroduction ?? string.Empty,
                Type = entry.Type,
                MainPicture = entry.MainPicture ?? string.Empty,
                Thumbnail = entry.Thumbnail ?? string.Empty,
                BackgroundPicture = entry.BackgroundPicture ?? string.Empty,
                SmallBackgroundPicture = entry.SmallBackgroundPicture ?? string.Empty,
                IsEdit = entry.IsEdit,
                IsHidden = entry.IsHidden,
                IsHideOutlink = entry.IsHideOutlink,
                IsScored = entry.IsScored,
                CanComment = entry.CanComment,
                TabIndex = entry.TabIndex <= 0 ? 1 : entry.TabIndex,
                MainPage = entry.MainPage ?? string.Empty,
                Booking = entry.Booking,
                Information = entry.Information?.ToList() ?? [],
                Tags = entry.Tags?
                    .Select(s => s.Name)
                    .Where(s => string.IsNullOrWhiteSpace(s) is false)
                    .Distinct()
                    .Take(24)
                    .ToList() ?? [],
                Audio = entry.Audio?
                    .Where(s => string.IsNullOrWhiteSpace(s.Url) is false)
                    .OrderBy(s => s.Priority)
                    .ToList() ?? [],
                Pictures = entry.Pictures?
                    .SelectMany(s => s.Pictures ?? [])
                    .Where(s => string.IsNullOrWhiteSpace(s.Url) is false)
                    .OrderBy(s => s.Priority)
                    .ToList() ?? [],
                NewsOfEntry = entry.NewsOfEntry?
                    .OrderByDescending(s => s.HappenedTime)
                    .ToList() ?? [],
                EntryRelevances = entry.EntryRelevances?.ToList() ?? [],
                ArticleRelevances = entry.ArticleRelevances?.ToList() ?? [],
                VideoRelevances = entry.VideoRelevances?.ToList() ?? [],
                OtherRelevances = entry.OtherRelevances?.ToList() ?? [],
                Roles = entry.Roles?.ToList() ?? [],
                StaffGames = entry.StaffGames?.ToList() ?? [],
                Staffs = entry.Staffs?.ToList() ?? [],
                ProductionGroups = entry.ProductionGroups?.ToList() ?? [],
                Publishers = entry.Publishers?.ToList() ?? [],
                Releases = entry.Releases?
                    .OrderBy(s => s.Time ?? DateTime.MaxValue)
                    .ToList() ?? []
            };

            memoryCache.Set(cacheKey, model, EntryCacheDuration);
            return SdkResult<EntryDetailViewModel>.Ok(model);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "获取条目详情异常。EntryId={EntryId}; Path={Path}; BaseAddress={BaseAddress}",
                id,
                string.Format(EntryDetailPathTemplate, id),
                HttpClient.BaseAddress);
            return SdkResult<EntryDetailViewModel>.Fail("ENTRY_QUERY_EXCEPTION", "请求条目详情时发生异常");
        }
    }
}
