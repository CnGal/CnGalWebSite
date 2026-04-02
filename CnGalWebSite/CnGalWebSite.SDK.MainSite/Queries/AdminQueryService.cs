using System.Net.Http.Json;
using CnGalWebSite.Core.Models;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Infrastructure;
using CnGalWebSite.SDK.MainSite.Models;
using Microsoft.Extensions.Logging;

namespace CnGalWebSite.SDK.MainSite.Queries;

public sealed class AdminQueryService(HttpClient httpClient, ILogger<AdminQueryService> logger)
    : QueryServiceBase(httpClient), IAdminQueryService
{
    protected override ILogger Logger => logger;

    public Task<SdkResult<ServerStaticOverviewModel>> GetServerOverviewAsync(CancellationToken cancellationToken = default)
    {
        return GetAsync<ServerStaticOverviewModel>(
            "api/admin/GetServerStaticDataOverview",
            "ADMIN",
            "服务器概览",
            cancellationToken);
    }

    public async Task<SdkResult<QueryResultModel<CommentOverviewModel>>> GetCommentsAsync(
        QueryParameterModel parameter,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await HttpClient.PostAsJsonAsync(
                "api/comments/List",
                parameter,
                SdkJsonSerializerOptions.Default,
                cancellationToken);

            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                Logger.LogError(
                    "评论列表接口请求失败。Path=api/comments/List; StatusCode={StatusCode}; BaseAddress={BaseAddress}; ResponseBody={ResponseBody}",
                    (int)response.StatusCode,
                    HttpClient.BaseAddress,
                    TrimForLog(body));
                return SdkResult<QueryResultModel<CommentOverviewModel>>.Fail("ADMIN_COMMENTS_HTTP_FAILED", $"获取评论列表失败（HTTP {(int)response.StatusCode}）");
            }

            try
            {
                var result = Deserialize<QueryResultModel<CommentOverviewModel>>(body);
                return SdkResult<QueryResultModel<CommentOverviewModel>>.Ok(result!);
            }
            catch (System.Text.Json.JsonException ex)
            {
                Logger.LogError(
                    ex,
                    "评论列表反序列化失败。Path=api/comments/List; BaseAddress={BaseAddress}; ResponseBody={ResponseBody}",
                    HttpClient.BaseAddress,
                    TrimForLog(body));
                return SdkResult<QueryResultModel<CommentOverviewModel>>.Fail("ADMIN_COMMENTS_DESERIALIZE_FAILED", "评论列表数据格式不兼容");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(
                ex,
                "获取评论列表异常。Path=api/comments/List; BaseAddress={BaseAddress}",
                HttpClient.BaseAddress);
            return SdkResult<QueryResultModel<CommentOverviewModel>>.Fail("ADMIN_COMMENTS_EXCEPTION", "请求评论列表时发生异常");
        }
    }
}
