using CnGalWebSite.APIServer.Infrastructure;
using CnGalWebSite.DataModel.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.Examines;

/// <summary>Repository saves participate in this transaction instead of committing an approval separately from its content.</summary>
public sealed class EditRecordUnitOfWork(AppDbContext db, ILogger<EditRecordUnitOfWork> logger)
{
    public async Task<T> RunAsync<T>(Func<Task<T>> action, Func<T, bool> successful = null)
    {
        if (db.Database.CurrentTransaction is not null) return await action();
        await using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            var result = await action();
            if (successful is not null && !successful(result))
            {
                await transaction.RollbackAsync();
                db.ChangeTracker.Clear();
                return result;
            }
            await db.SaveChangesAsync();
            await transaction.CommitAsync();
            return result;
        }
        catch
        {
            await transaction.RollbackAsync();
            db.ChangeTracker.Clear();
            throw;
        }
    }

    public async Task<bool> TryClaimAsync(Examine record, bool passed)
    {
        if (db.Database.CurrentTransaction is null)
            throw new InvalidOperationException("审核状态必须在事务中处理。");
        var count = await db.Examines.Where(e => e.Id == record.Id && e.IsPassed == null)
            .ExecuteUpdateAsync(update => update.SetProperty(e => e.IsPassed, (bool?)passed));
        if (count == 0) return false;
        await db.Entry(record).ReloadAsync();
        return true;
    }

    public async Task AfterCommitAsync(Func<Task> action)
    {
        try { await action(); }
        catch (Exception error) { logger.LogError(error, "审核内容已提交，但更新积分或等级失败。"); }
    }
}
