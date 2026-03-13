using CnGalWebSite.DataModel.ViewModel;
using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace CnGalWebSite.SDK.MainSite.Queries;

public sealed class EntryQueryService(
    HttpClient httpClient,
    IMemoryCache memoryCache,
    ILogger<EntryQueryService> logger) : IEntryQueryService
{
    private static readonly TimeSpan EntryCacheDuration = TimeSpan.FromMinutes(5);

    public async Task<SdkResult<EntryDetailViewModel>> GetEntryDetailAsync(int id, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"main-site:entry-detail:{id}";
        if (memoryCache.TryGetValue(cacheKey, out EntryDetailViewModel? cached) && cached is not null)
        {
            return SdkResult<EntryDetailViewModel>.Ok(cached);
        }

        try
        {
            var response = await httpClient.GetAsync($"api/entries/GetEntryViewAsync?id={id}", cancellationToken);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return SdkResult<EntryDetailViewModel>.Fail("ENTRY_NOT_FOUND", "未找到对应条目", (int)response.StatusCode);
            }

            if (!response.IsSuccessStatusCode)
            {
                return SdkResult<EntryDetailViewModel>.Fail("ENTRY_QUERY_FAILED", "获取条目详情失败", (int)response.StatusCode);
            }

            var entry = await response.Content.ReadFromJsonAsync<EntryIndexViewModel>(cancellationToken: cancellationToken);
            if (entry is null)
            {
                return SdkResult<EntryDetailViewModel>.Fail("ENTRY_EMPTY_RESPONSE", "条目数据为空");
            }

            var model = new EntryDetailViewModel
            {
                Id = entry.Id,
                Name = entry.Name ?? string.Empty,
                Description = entry.BriefIntroduction ?? string.Empty,
                MainPicture = entry.MainPicture,
                Tags = entry.Tags
                    .Select(s => s.Name)
                    .Where(s => string.IsNullOrWhiteSpace(s) is false)
                    .Distinct()
                    .Take(16)
                    .ToList()
            };

            memoryCache.Set(cacheKey, model, EntryCacheDuration);
            return SdkResult<EntryDetailViewModel>.Ok(model);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "获取条目详情失败，EntryId={EntryId}", id);
            return SdkResult<EntryDetailViewModel>.Fail("ENTRY_QUERY_EXCEPTION", "请求条目详情时发生异常");
        }
    }
}
