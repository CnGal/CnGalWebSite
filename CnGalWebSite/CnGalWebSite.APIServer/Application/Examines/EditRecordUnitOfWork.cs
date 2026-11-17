using CnGalWebSite.APIServer.Infrastructure;
using CnGalWebSite.DataModel.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using System;
using System.Data.Common;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.Examines;

/// <summary>Repository saves participate in this transaction instead of committing an approval separately from its content.</summary>
public sealed class EditRecordUnitOfWork(AppDbContext db, ILogger<EditRecordUnitOfWork> logger)
{
    /// <summary>死锁重试次数（含首次执行）。</summary>
    private const int MaxDeadlockAttempts = 3;

    /// <summary>
    /// 在单个事务中执行审核写入。
    /// </summary>
    /// <param name="action">要执行的业务动作。</param>
    /// <param name="successful">返回 false 时回滚并返回结果，不抛异常。</param>
    /// <param name="retryOnDeadlock">
    /// 遇到 MySQL 死锁/锁等待超时时是否重试。仅当 <paramref name="action"/> 内部会重新查询
    /// 所有实体时才能开启——重试前会回滚并清空变更跟踪器，动作不能依赖外部传入的已跟踪实体。
    /// </param>
    public async Task<T> RunAsync<T>(Func<Task<T>> action, Func<T, bool> successful = null, bool retryOnDeadlock = false)
    {
        // 嵌套调用沿用外层事务，由外层负责提交与重试。
        if (db.Database.CurrentTransaction is not null) return await action();

        var attempts = retryOnDeadlock ? MaxDeadlockAttempts : 1;
        for (var attempt = 1; ; attempt++)
        {
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
            catch (Exception error) when (attempt < attempts && IsDeadlock(error))
            {
                // 事务化后写锁覆盖审核记录、目标行与反向关联，并发编辑可能触发
                // “Deadlock found when trying to get lock”。MySQL 要求重开事务，
                // 因此回滚、清空跟踪器后重跑动作，让实体重新从数据库读取。
                await transaction.RollbackAsync();
                db.ChangeTracker.Clear();
                logger.LogWarning(error, "审核提交遇到数据库死锁，第 {Attempt} 次重试。", attempt);
                await Task.Delay(TimeSpan.FromMilliseconds(50 * attempt));
            }
            catch
            {
                await transaction.RollbackAsync();
                db.ChangeTracker.Clear();
                throw;
            }
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

    /// <summary>
    /// 判断异常链中是否包含 MySQL 死锁（1213）或锁等待超时（1205）。
    /// 兼容其他提供程序：EF 会把非 MySQL 的并发冲突标记为 SQLSTATE 40001。
    /// </summary>
    internal static bool IsDeadlock(Exception error)
    {
        for (var current = error; current is not null; current = current.InnerException)
        {
            if (current is MySqlException { Number: 1213 or 1205 })
            {
                return true;
            }
            if (current is DbException { SqlState: "40001" })
            {
                return true;
            }
        }
        return false;
    }
}
