using System.Net.Http.Json;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Expo;
using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Infrastructure;
using CnGalWebSite.SDK.MainSite.Models;
using Microsoft.Extensions.Logging;

namespace CnGalWebSite.SDK.MainSite.Commands;

public sealed class ExpoCommandService(
    HttpClient httpClient,
    ILogger<ExpoCommandService> logger) : CommandServiceBase(httpClient), IExpoCommandService
{
    protected override ILogger Logger => logger;

    private const string FinishTaskPath = "api/expo/UserFinshTask";
    private const string LotteryPath = "api/expo/Lottery";

    public async Task<SdkResult<bool>> FinishTaskAsync(ExpoTaskType type, CancellationToken cancellationToken = default)
    {
        try
        {
            var model = new ExpoFinshTaskModel { Type = type };
            var response = await HttpClient.PostAsJsonAsync(FinishTaskPath, model, SdkJsonSerializerOptions.Default, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                Logger.LogError(
                    "完成任务失败。Type={Type}; StatusCode={StatusCode}; ResponseBody={ResponseBody}",
                    type,
                    (int)response.StatusCode,
                    TrimForLog(body));

                return SdkResult<bool>.Fail("EXPO_FINISH_TASK_FAILED", $"完成任务失败（HTTP {(int)response.StatusCode}）");
            }

            try
            {
                var result = System.Text.Json.JsonSerializer.Deserialize<Result>(body, SdkJsonSerializerOptions.Default);
                if (result is null)
                {
                    return SdkResult<bool>.Fail("EXPO_FINISH_TASK_EMPTY", "完成任务响应为空");
                }

                if (!result.Successful)
                {
                    return SdkResult<bool>.Fail("EXPO_FINISH_TASK_REJECTED", result.Error ?? "任务完成失败");
                }

                return SdkResult<bool>.Ok(true);
            }
            catch (System.Text.Json.JsonException ex)
            {
                Logger.LogError(ex, "完成任务反序列化失败。Type={Type}; ResponseBody={ResponseBody}", type, TrimForLog(body));
                return SdkResult<bool>.Fail("EXPO_FINISH_TASK_DESERIALIZE", "完成任务响应格式不兼容");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "完成任务异常。Type={Type}", type);
            return SdkResult<bool>.Fail("EXPO_FINISH_TASK_EXCEPTION", "请求完成任务时发生异常");
        }
    }

    public async Task<SdkResult<long?>> LotteryAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // LotteryAsync uses POST with no body parameter — send empty JSON object
            var response = await HttpClient.PostAsJsonAsync(LotteryPath, new { }, SdkJsonSerializerOptions.Default, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                Logger.LogError(
                    "抽奖请求失败。StatusCode={StatusCode}; ResponseBody={ResponseBody}",
                    (int)response.StatusCode,
                    TrimForLog(body));

                return SdkResult<long?>.Fail("EXPO_LOTTERY_FAILED", $"抽奖请求失败（HTTP {(int)response.StatusCode}）");
            }

            try
            {
                var result = System.Text.Json.JsonSerializer.Deserialize<Result>(body, SdkJsonSerializerOptions.Default);
                if (result is null)
                {
                    return SdkResult<long?>.Fail("EXPO_LOTTERY_EMPTY", "抽奖响应为空");
                }

                if (!result.Successful)
                {
                    return SdkResult<long?>.Fail("EXPO_LOTTERY_REJECTED", result.Error ?? "抽奖失败");
                }

                // The awardId is returned in the Error field on success (legacy convention)
                if (long.TryParse(result.Error, out var awardId))
                {
                    return SdkResult<long?>.Ok(awardId);
                }

                Logger.LogWarning("抽奖返回的 awardId 无法解析。ErrorValue={Error}", result.Error);
                return SdkResult<long?>.Fail("EXPO_LOTTERY_INVALID_AWARD", "获取奖项Id失败");
            }
            catch (System.Text.Json.JsonException ex)
            {
                Logger.LogError(ex, "抽奖反序列化失败。ResponseBody={ResponseBody}", TrimForLog(body));
                return SdkResult<long?>.Fail("EXPO_LOTTERY_DESERIALIZE", "抽奖响应格式不兼容");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "抽奖请求异常");
            return SdkResult<long?>.Fail("EXPO_LOTTERY_EXCEPTION", "请求抽奖时发生异常");
        }
    }

    private static string TrimForLog(string text, int maxLength = 800)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;
        return text.Length <= maxLength ? text : $"{text[..maxLength]}...(truncated)";
    }
}
