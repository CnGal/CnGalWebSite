using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Lotteries;
using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Infrastructure;
using CnGalWebSite.SDK.MainSite.Models;
using Microsoft.Extensions.Logging;

namespace CnGalWebSite.SDK.MainSite.Commands;

public sealed class LotteryCommandService(
    HttpClient httpClient,
    ILogger<LotteryCommandService> logger) : CommandServiceBase(httpClient), ILotteryCommandService
{
    protected override ILogger Logger => logger;

    public async Task<SdkResult<List<string>>> GetEntryGameItemsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var items = await GetFromJsonAsync<List<string>>("api/entries/GetAllEntries/0", cancellationToken);
            return SdkResult<List<string>>.Ok(items ?? []);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "获取游戏词条列表异常。BaseAddress={BaseAddress}", HttpClient.BaseAddress);
            return SdkResult<List<string>>.Fail("LOTTERY_ENTRY_ITEMS_EXCEPTION", "获取游戏词条列表时发生异常");
        }
    }

    public async Task<SdkResult<EditLotteryModel>> GetLotteryEditAsync(long id, CancellationToken cancellationToken = default)
    {
        try
        {
            var model = await GetFromJsonAsync<EditLotteryModel>($"api/lotteries/EditLottery/{id}", cancellationToken);
            if (model is null)
            {
                return SdkResult<EditLotteryModel>.Fail("LOTTERY_EDIT_NOT_FOUND", "未找到抽奖编辑数据");
            }
            return SdkResult<EditLotteryModel>.Ok(model);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "获取抽奖编辑数据异常。LotteryId={LotteryId}; BaseAddress={BaseAddress}", id, HttpClient.BaseAddress);
            return SdkResult<EditLotteryModel>.Fail("LOTTERY_EDIT_QUERY_EXCEPTION", "请求抽奖编辑数据时发生异常");
        }
    }

    public async Task<SdkResult<long>> CreateLotteryAsync(EditLotteryModel model, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await PostAsJsonRawAsync("api/lotteries/CreateLottery", model, cancellationToken);
            var result = await ReadResponseAsync<Result>(response, cancellationToken);

            if (result?.Successful == true && long.TryParse(result.Error, out var newId))
            {
                return SdkResult<long>.Ok(newId);
            }

            return SdkResult<long>.Fail("LOTTERY_CREATE_FAILED", result?.Error ?? "创建抽奖失败");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "创建抽奖异常。BaseAddress={BaseAddress}", HttpClient.BaseAddress);
            return SdkResult<long>.Fail("LOTTERY_CREATE_EXCEPTION", "创建抽奖时发生异常");
        }
    }

    public async Task<SdkResult<long>> EditLotteryAsync(EditLotteryModel model, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await PostAsJsonRawAsync("api/lotteries/EditLottery", model, cancellationToken);
            var result = await ReadResponseAsync<Result>(response, cancellationToken);

            if (result?.Successful == true)
            {
                return SdkResult<long>.Ok(model.Id);
            }

            return SdkResult<long>.Fail("LOTTERY_EDIT_FAILED", result?.Error ?? "编辑抽奖失败");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "编辑抽奖异常。LotteryId={LotteryId}; BaseAddress={BaseAddress}", model.Id, HttpClient.BaseAddress);
            return SdkResult<long>.Fail("LOTTERY_EDIT_EXCEPTION", "编辑抽奖时发生异常");
        }
    }
}
