using CnGalWebSite.DataModel.Model;
using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Models;
using CnGalWebSite.SDK.MainSite.Infrastructure;
using Microsoft.Extensions.Logging;

namespace CnGalWebSite.SDK.MainSite.Queries;

public sealed class EntryPageQueryService(
    IEntryQueryService entries,
    IPlayedGameQueryService scores,
    IPeripheryQueryService peripheries,
    IFavoriteFolderQueryService folders,
    IExamineQueryService examines,
    ICurrentUserAccessor currentUser,
    ILogger<EntryPageQueryService> logger) : IEntryPageQueryService
{
    public async Task<SdkResult<EntryPageModel>> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        using var lifetime = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var token = lifetime.Token;
        var tasks = new List<Task>();
        await currentUser.GetIdentityStateAsync(cancellationToken);
        var identityVersion = currentUser.IdentityVersion;
        try
        {
            var entryTask = entries.GetEntryDetailAsync(id, token); tasks.Add(entryTask);
            var peripheryTask = peripheries.GetEntryOverviewPeripheriesAsync(id, token); tasks.Add(peripheryTask);
            var catalogTask = folders.GetRelateFavoriteFoldersAsync(FavoriteObjectType.Entry, id, token); tasks.Add(catalogTask);
            var recordsTask = examines.GetEntryEditRecordsAsync(id, token); tasks.Add(recordsTask);
            var entry = await entryTask;
            if (!entry.Success || entry.Data is null)
            {
                return SdkResult<EntryPageModel>.Fail(entry.Error?.Code ?? "ENTRY_PAGE_FAILED",
                    entry.Error?.Message ?? "词条加载失败", entry.Error?.StatusCode);
            }
            Task<SdkResult<PlayedGameOverviewViewModel>>? scoreTask = entry.Data.Type == EntryType.Game
                ? scores.GetOverviewAsync(id, token) : null;
            if (scoreTask is not null) tasks.Add(scoreTask);
            await Task.WhenAll(tasks);
            cancellationToken.ThrowIfCancellationRequested();
            if (identityVersion != currentUser.IdentityVersion) throw new SdkIdentityChangedException();
            return SdkResult<EntryPageModel>.Ok(new(entry.Data, scoreTask is null ? null : await scoreTask,
                await peripheryTask, await catalogTask, await recordsTask));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch (Exception ex)
        {
            logger.LogError(ex, "Entry page aggregation failed. EntryId={EntryId}", id);
            return SdkFailure.FromException<EntryPageModel>(ex, "ENTRY_PAGE_EXCEPTION", "词条页面加载失败");
        }
        finally
        {
            await lifetime.CancelAsync();
            // Observe every started task even when the main query failed or was cancelled.
            try { await Task.WhenAll(tasks); }
            catch (OperationCanceledException) when (token.IsCancellationRequested) { }
            catch (Exception ex) { logger.LogWarning(ex, "Entry page request cleanup. EntryId={EntryId}", id); }
            cancellationToken.ThrowIfCancellationRequested();
        }
    }
}
