using System.Net.Http.Json;
using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Infrastructure;
using CnGalWebSite.SDK.MainSite.Models;
using CnGalWebSite.SDK.MainSite.Models.Kanban;
using Microsoft.Extensions.Logging;

namespace CnGalWebSite.SDK.MainSite.Queries;

public sealed class KanbanQueryService(
    HttpClient httpClient,
    ILogger<KanbanQueryService> logger) : QueryServiceBase(httpClient), IKanbanQueryService
{
    private const string ChatPath = "api/robot/GetKanbanReply/";
    private const string PermissionsPath = "api/space/GetKanbanPermissions/";

    protected override ILogger Logger => logger;

    public async Task<SdkResult<KanbanChatReply>> GetChatReplyAsync(KanbanChatRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await HttpClient.PostAsJsonAsync(ChatPath, request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                Logger.LogError(
                    "看板娘聊天接口请求失败。Path={Path}; StatusCode={StatusCode}; BaseAddress={BaseAddress}; ResponseBody={ResponseBody}",
                    ChatPath,
                    (int)response.StatusCode,
                    HttpClient.BaseAddress,
                    TrimForLog(body));

                return SdkResult<KanbanChatReply>.Fail("KANBAN_CHAT_HTTP_FAILED", $"获取聊天回复失败（HTTP {(int)response.StatusCode}）");
            }

            var reply = await response.Content.ReadFromJsonAsync<KanbanChatReply>(cancellationToken: cancellationToken);

            if (reply is null)
            {
                return SdkResult<KanbanChatReply>.Fail("KANBAN_CHAT_EMPTY_RESPONSE", "聊天回复数据为空");
            }

            return SdkResult<KanbanChatReply>.Ok(reply);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "获取看板娘聊天回复异常。Path={Path}; BaseAddress={BaseAddress}", ChatPath, HttpClient.BaseAddress);
            return SdkResult<KanbanChatReply>.Fail("KANBAN_CHAT_EXCEPTION", "请求聊天回复时发生异常");
        }
    }

    public async Task<SdkResult<KanbanPermissionsReply>> GetPermissionsAsync(CancellationToken cancellationToken = default)
    {
        return await GetAsync<KanbanPermissionsReply>(
            PermissionsPath,
            "KANBAN_PERMISSIONS",
            "看板娘权限",
            cancellationToken);
    }
}
