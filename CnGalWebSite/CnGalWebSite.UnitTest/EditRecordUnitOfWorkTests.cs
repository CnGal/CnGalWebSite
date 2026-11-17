using CnGalWebSite.APIServer.Application.Examines;
using CnGalWebSite.APIServer.Infrastructure;
using CnGalWebSite.DataModel.Model;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using System.Data.Common;

namespace CnGalWebSite.UnitTest;

/// <summary>
/// 审核事务的死锁处理：MySQL 在并发编辑同一对象时会报
/// “Deadlock found when trying to get lock”，EF 默认不会重试。
/// </summary>
public class EditRecordUnitOfWorkTests
{
    private SqliteConnection _connection = default!;
    private DbContextOptions<AppDbContext> _options = default!;

    [SetUp]
    public void SetUp()
    {
        var database = $"cngal_tests_{Guid.NewGuid():N}";
        _connection = new SqliteConnection($"Data Source=file:{database}?mode=memory&cache=shared;Pooling=False");
        _connection.Open();
        _options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options;
        using var db = new AppDbContext(_options);
        db.Database.EnsureCreated();
    }

    [TearDown]
    public void TearDown() => _connection.Dispose();

    private AppDbContext Context() => new(_options);

    private static EditRecordUnitOfWork UnitOfWork(AppDbContext db) => new(db, NullLogger<EditRecordUnitOfWork>.Instance);

    [Test]
    public void DeadlockIsDetectedThroughTheExceptionChain()
    {
        Assert.That(EditRecordUnitOfWork.IsDeadlock(new DbUpdateException("deadlock", new FakeDeadlockException())), Is.True);
        Assert.That(EditRecordUnitOfWork.IsDeadlock(new DbUpdateException("other", new InvalidOperationException())), Is.False);
        Assert.That(EditRecordUnitOfWork.IsDeadlock(new InvalidOperationException()), Is.False);
    }

    [Test]
    public async Task DeadlockedActionIsRetriedAndCommittedOnce()
    {
        using var db = Context();
        var attempts = 0;

        var result = await UnitOfWork(db).RunAsync(async () =>
        {
            attempts++;
            db.Tags.Add(new Tag { Name = "重试后提交" });
            await db.SaveChangesAsync();
            if (attempts == 1)
            {
                throw new DbUpdateException("deadlock", new FakeDeadlockException());
            }
            return true;
        }, retryOnDeadlock: true);

        Assert.That(result, Is.True);
        Assert.That(attempts, Is.EqualTo(2));

        using var verify = Context();
        Assert.That(verify.Tags.Count(s => s.Name == "重试后提交"), Is.EqualTo(1));
    }

    [Test]
    public async Task DeadlockIsNotRetriedUnlessTheActionCanRerun()
    {
        using var db = Context();
        var attempts = 0;

        Assert.That(async () => await UnitOfWork(db).RunAsync<bool>(async () =>
        {
            attempts++;
            db.Tags.Add(new Tag { Name = "不应重试" });
            await db.SaveChangesAsync();
            throw new DbUpdateException("deadlock", new FakeDeadlockException());
        }), Throws.TypeOf<DbUpdateException>());

        Assert.That(attempts, Is.EqualTo(1));
        using var verify = Context();
        Assert.That(verify.Tags.Any(s => s.Name == "不应重试"), Is.False);
    }

    /// <summary>模拟 MySQL 1213：SQLSTATE 40001，避免测试依赖真实数据库连接。</summary>
    private sealed class FakeDeadlockException() : DbException("Deadlock found when trying to get lock")
    {
        public override string? SqlState => "40001";
    }
}
