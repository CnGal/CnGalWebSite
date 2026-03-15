using CnGalWebSite.DataModel.ViewModel.Tags;
using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Infrastructure;
using CnGalWebSite.SDK.MainSite.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CnGalWebSite.SDK.MainSite.Queries;

public sealed class TagQueryService(
    HttpClient httpClient,
    ILogger<TagQueryService> logger) : QueryServiceBase(httpClient), ITagQueryService
{
    public async Task<SdkResult<List<TagTreeModel>>> GetTagTreeAsync(CancellationToken cancellationToken = default)
    {
        const string path = "api/tags/GetTagsTreeView";
        try
        {
            var (response, responseBody) = await GetAsyncWithBody(HttpClient, path, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogError(
                    "获取标签树失败。Path={Path}; StatusCode={StatusCode}; BaseAddress={BaseAddress}; ResponseBody={ResponseBody}",
                    path,
                    (int)response.StatusCode,
                    HttpClient.BaseAddress,
                    TrimForLog(responseBody));

                return SdkResult<List<TagTreeModel>>.Fail("TAG_TREE_HTTP_FAILED", $"获取标签树失败（HTTP {(int)response.StatusCode}）");
            }

            try
            {
                var data = Deserialize<List<TagTreeModel>>(responseBody);
                return SdkResult<List<TagTreeModel>>.Ok(data ?? []);
            }
            catch (JsonException ex)
            {
                logger.LogError(
                    ex,
                    "标签树反序列化失败。Path={Path}; BaseAddress={BaseAddress}; ResponseBody={ResponseBody}",
                    path,
                    HttpClient.BaseAddress,
                    TrimForLog(responseBody));

                return SdkResult<List<TagTreeModel>>.Fail("TAG_TREE_DESERIALIZE_FAILED", "标签树数据格式不兼容");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "获取标签树异常。Path={Path}; BaseAddress={BaseAddress}",
                path,
                HttpClient.BaseAddress);

            return SdkResult<List<TagTreeModel>>.Fail("TAG_TREE_EXCEPTION", "请求标签树数据时发生异常");
        }
    }
}
