using System.Text.Json.Nodes;
using System.Net.Http.Json;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel;
using CnGalWebSite.DataModel.ViewModel.Articles;
using CnGalWebSite.DataModel.ViewModel.Videos;
using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Infrastructure;
using CnGalWebSite.SDK.MainSite.Models;
using CnGalWebSite.SDK.MainSite.Models.Toolbox;
using Microsoft.Extensions.Logging;

namespace CnGalWebSite.SDK.MainSite.Commands;

public sealed class ToolboxCommandService(
    HttpClient httpClient,
    ILogger<ToolboxCommandService> logger) : CommandServiceBase(httpClient), IToolboxCommandService
{
    protected override ILogger Logger => logger;

    public async Task<SdkResult<ToolboxBilibiliVideoInfo>> GetBilibiliVideoInfoAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            var json = await HttpClient.GetStringAsync($"api/thirdparties/GetBilibiliVideoInfor?id={Uri.EscapeDataString(id)}", cancellationToken);
            var root = JsonNode.Parse(json);
            if (root?["code"]?.GetValue<int>() != 0)
            {
                return SdkResult<ToolboxBilibiliVideoInfo>.Fail("TOOLBOX_BILIBILI_QUERY_FAILED", root?["message"]?.GetValue<string>() ?? "获取 B 站视频信息失败");
            }

            var view = root["data"]?["View"];
            if (view is null)
            {
                return SdkResult<ToolboxBilibiliVideoInfo>.Fail("TOOLBOX_BILIBILI_INVALID_RESPONSE", "B 站视频信息格式不兼容");
            }

            var title = view["title"]?.GetValue<string>()?.Replace("\n", string.Empty) ?? string.Empty;
            var durationSeconds = view["duration"]?.GetValue<int>() ?? 0;
            var publishTime = FromUnixTimeSeconds(view["pubdate"]?.GetValue<long>() ?? 0);
            var copyright = view["copyright"]?.GetValue<int>() == 1 ? CopyrightType.Original : CopyrightType.Transshipment;

            return SdkResult<ToolboxBilibiliVideoInfo>.Ok(new ToolboxBilibiliVideoInfo
            {
                BilibiliId = id,
                Type = view["tname"]?.GetValue<string>() ?? string.Empty,
                Copyright = copyright,
                MainPicture = view["pic"]?.GetValue<string>() ?? string.Empty,
                Name = title,
                PublishTime = publishTime,
                Description = (view["desc"]?.GetValue<string>() ?? string.Empty).Replace(title, string.Empty),
                Duration = TimeSpan.FromSeconds(durationSeconds),
                IsInteractive = ReadBoolCompatible(view["rights"]?["is_stein_gate"]),
                OriginalAuthor = view["owner"]?["name"]?.GetValue<string>() ?? string.Empty
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "获取 B 站视频信息异常。BilibiliId={BilibiliId}; BaseAddress={BaseAddress}", id, HttpClient.BaseAddress);
            return SdkResult<ToolboxBilibiliVideoInfo>.Fail("TOOLBOX_BILIBILI_QUERY_EXCEPTION", "请求 B 站视频信息时发生异常");
        }
    }

    public async Task<SdkResult<IReadOnlyList<string>>> GetBilibiliVideoImageImpositionAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            var images = await GetFromJsonAsync<List<string>>($"api/thirdparties/GetBilibiliVideoImageImposition?id={Uri.EscapeDataString(id)}", cancellationToken);
            return SdkResult<IReadOnlyList<string>>.Ok(images ?? []);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "获取 B 站视频快照异常。BilibiliId={BilibiliId}; BaseAddress={BaseAddress}", id, HttpClient.BaseAddress);
            return SdkResult<IReadOnlyList<string>>.Fail("TOOLBOX_BILIBILI_IMAGES_EXCEPTION", "请求 B 站视频快照时发生异常");
        }
    }

    public async Task<SdkResult<int>> GetEntryIdAsync(string name, CancellationToken cancellationToken = default)
    {
        try
        {
            var id = await GetFromJsonAsync<int>($"api/entries/GetId/{SdkToolHelper.Base64EncodeUrl(name)}", cancellationToken);
            return id == 0
                ? SdkResult<int>.Fail("TOOLBOX_ENTRY_NOT_FOUND", $"未找到词条：{name}")
                : SdkResult<int>.Ok(id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "获取词条 Id 异常。EntryName={EntryName}; BaseAddress={BaseAddress}", name, HttpClient.BaseAddress);
            return SdkResult<int>.Fail("TOOLBOX_ENTRY_ID_EXCEPTION", "请求词条 Id 时发生异常");
        }
    }

    public Task<SdkResult<EntryIndexViewModel>> GetEntryViewAsync(int id, CancellationToken cancellationToken = default)
        => GetAsync<EntryIndexViewModel>($"api/entries/GetEntryView/{id}", "TOOLBOX_ENTRY_VIEW", "词条详情", cancellationToken);

    public Task<SdkResult<EditRelevancesViewModel>> GetEntryRelevancesAsync(int id, CancellationToken cancellationToken = default)
        => GetAsync<EditRelevancesViewModel>($"api/entries/EditRelevances/{id}", "TOOLBOX_ENTRY_RELEVANCES", "词条关联编辑数据", cancellationToken);

    public Task<SdkResult<EditAddInforViewModel>> GetEntryAddInforAsync(int id, CancellationToken cancellationToken = default)
        => GetAsync<EditAddInforViewModel>($"api/entries/EditAddInfor/{id}", "TOOLBOX_ENTRY_ADDINFOR", "词条附加信息编辑数据", cancellationToken);

    public Task<SdkResult<EditMainViewModel>> GetEntryMainAsync(int id, CancellationToken cancellationToken = default)
        => GetAsync<EditMainViewModel>($"api/entries/EditMain/{id}", "TOOLBOX_ENTRY_MAIN", "词条主要信息编辑数据", cancellationToken);

    public Task<SdkResult<EditArticleRelevancesViewModel>> GetArticleRelevancesAsync(long id, CancellationToken cancellationToken = default)
        => GetAsync<EditArticleRelevancesViewModel>($"api/articles/EditRelevances/{id}", "TOOLBOX_ARTICLE_RELEVANCES", "文章关联编辑数据", cancellationToken);

    public Task<SdkResult<EditVideoRelevancesViewModel>> GetVideoRelevancesAsync(long id, CancellationToken cancellationToken = default)
        => GetAsync<EditVideoRelevancesViewModel>($"api/videos/EditRelevances/{id}", "TOOLBOX_VIDEO_RELEVANCES", "视频关联编辑数据", cancellationToken);

    public Task<SdkResult<bool>> SubmitEntryRelevancesAsync(EditRelevancesViewModel model, CancellationToken cancellationToken = default)
        => SubmitResultAsync("api/entries/EditRelevances", model, "TOOLBOX_ENTRY_RELEVANCES_SUBMIT", cancellationToken);

    public Task<SdkResult<bool>> SubmitEntryAddInforAsync(EditAddInforViewModel model, CancellationToken cancellationToken = default)
        => SubmitResultAsync("api/entries/EditAddInfor", model, "TOOLBOX_ENTRY_ADDINFOR_SUBMIT", cancellationToken);

    public Task<SdkResult<bool>> SubmitEntryMainAsync(EditMainViewModel model, CancellationToken cancellationToken = default)
        => SubmitResultAsync("api/entries/EditMain", model, "TOOLBOX_ENTRY_MAIN_SUBMIT", cancellationToken);

    public Task<SdkResult<bool>> SubmitArticleRelevancesAsync(EditArticleRelevancesViewModel model, CancellationToken cancellationToken = default)
        => SubmitResultAsync("api/articles/EditRelevances", model, "TOOLBOX_ARTICLE_RELEVANCES_SUBMIT", cancellationToken);

    public Task<SdkResult<bool>> SubmitVideoRelevancesAsync(EditVideoRelevancesViewModel model, CancellationToken cancellationToken = default)
        => SubmitResultAsync("api/videos/EditRelevances", model, "TOOLBOX_VIDEO_RELEVANCES_SUBMIT", cancellationToken);

    public Task<SdkResult<bool>> HideEntryAsync(int id, CancellationToken cancellationToken = default)
        => SubmitResultAsync("api/entries/HiddenEntry", new HiddenEntryModel { Ids = [id], IsHidden = true }, "TOOLBOX_ENTRY_HIDE", cancellationToken);

    private async Task<SdkResult<bool>> SubmitResultAsync<TRequest>(string path, TRequest request, string errorCode, CancellationToken cancellationToken)
    {
        try
        {
            var result = await PostAsJsonAsync<TRequest, Result>(path, request, cancellationToken);
            if (result?.Successful == true)
            {
                return SdkResult<bool>.Ok(true);
            }

            return SdkResult<bool>.Fail(errorCode, result?.Error ?? "提交失败");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "工具箱提交请求异常。Path={Path}; BaseAddress={BaseAddress}; ErrorCode={ErrorCode}", path, HttpClient.BaseAddress, errorCode);
            return SdkResult<bool>.Fail($"{errorCode}_EXCEPTION", "提交时发生异常");
        }
    }

    private static DateTime FromUnixTimeSeconds(long seconds)
    {
        var utc = DateTime.UnixEpoch.AddSeconds(seconds);
        return TimeZoneInfo.ConvertTimeFromUtc(utc, TimeZoneInfo.Local);
    }

    private static bool ReadBoolCompatible(JsonNode? node)
    {
        if (node is null)
        {
            return false;
        }

        if (node.GetValueKind() == System.Text.Json.JsonValueKind.True || node.GetValueKind() == System.Text.Json.JsonValueKind.False)
        {
            return node.GetValue<bool>();
        }

        if (node.GetValueKind() == System.Text.Json.JsonValueKind.Number)
        {
            return node.GetValue<int>() != 0;
        }

        return bool.TryParse(node.ToString(), out var value) && value;
    }

    private async Task<SdkResult<T>> GetAsync<T>(string path, string errorCodePrefix, string description, CancellationToken cancellationToken)
    {
        try
        {
            var response = await HttpClient.GetAsync(path, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "工具箱 GET 请求失败。Description={Description}; Path={Path}; StatusCode={StatusCode}; BaseAddress={BaseAddress}",
                    description,
                    path,
                    (int)response.StatusCode,
                    HttpClient.BaseAddress);
                return SdkResult<T>.Fail($"{errorCodePrefix}_HTTP_FAILED", $"获取{description}失败（HTTP {(int)response.StatusCode}）", (int)response.StatusCode);
            }

            var data = await response.Content.ReadFromJsonAsync<T>(SdkJsonSerializerOptions.Default, cancellationToken);
            if (data is null)
            {
                return SdkResult<T>.Fail($"{errorCodePrefix}_EMPTY_RESPONSE", $"{description}数据为空");
            }

            return SdkResult<T>.Ok(data);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "工具箱 GET 请求异常。Description={Description}; Path={Path}; BaseAddress={BaseAddress}",
                description,
                path,
                HttpClient.BaseAddress);
            return SdkResult<T>.Fail($"{errorCodePrefix}_EXCEPTION", $"请求{description}时发生异常");
        }
    }
}
