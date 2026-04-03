using System.Net.Http.Json;
using CnGalWebSite.Core.Models;
using CnGalWebSite.DrawingBed.Models.ViewModels;
using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Infrastructure;
using CnGalWebSite.SDK.MainSite.Models;
using Microsoft.Extensions.Logging;

namespace CnGalWebSite.SDK.MainSite.Queries;

public sealed class FileQueryService(HttpClient httpClient, ILogger<FileQueryService> logger)
    : QueryServiceBase(httpClient), IFileQueryService
{
    protected override ILogger Logger => logger;

    public async Task<SdkResult<QueryResultModel<UploadRecordOverviewModel>>> GetUploadRecordsAsync(
        QueryParameterModel parameter, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await HttpClient.PostAsJsonAsync(
                "api/files/list", parameter, SdkJsonSerializerOptions.Default, cancellationToken);

            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                Logger.LogError(
                    "上传记录列表接口请求失败。StatusCode={StatusCode}; BaseAddress={BaseAddress}; ResponseBody={ResponseBody}",
                    (int)response.StatusCode, HttpClient.BaseAddress, TrimForLog(body));
                return SdkResult<QueryResultModel<UploadRecordOverviewModel>>.Fail(
                    "FILES_HTTP_FAILED",
                    $"获取上传记录列表失败（HTTP {(int)response.StatusCode}）");
            }

            try
            {
                var result = Deserialize<QueryResultModel<UploadRecordOverviewModel>>(body);
                return SdkResult<QueryResultModel<UploadRecordOverviewModel>>.Ok(result!);
            }
            catch (System.Text.Json.JsonException ex)
            {
                Logger.LogError(ex,
                    "上传记录列表反序列化失败。BaseAddress={BaseAddress}; ResponseBody={ResponseBody}",
                    HttpClient.BaseAddress, TrimForLog(body));
                return SdkResult<QueryResultModel<UploadRecordOverviewModel>>.Fail(
                    "FILES_DESERIALIZE_FAILED",
                    "上传记录列表数据格式不兼容");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex,
                "获取上传记录列表异常。BaseAddress={BaseAddress}",
                HttpClient.BaseAddress);
            return SdkResult<QueryResultModel<UploadRecordOverviewModel>>.Fail(
                "FILES_EXCEPTION",
                "请求上传记录列表时发生异常");
        }
    }
}
