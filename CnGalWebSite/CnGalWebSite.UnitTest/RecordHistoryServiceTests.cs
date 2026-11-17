using CnGalWebSite.APIServer.Application.Examines;
using CnGalWebSite.APIServer.Infrastructure;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CnGalWebSite.UnitTest;

/// <summary>
/// 历史回放的失败路径必须返回错误而不是抛异常——对比接口曾经在缺少同一对象/顺序错误时
/// 抛出未捕获异常，把 400 变成了 500。
/// </summary>
public class RecordHistoryServiceTests
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

    /// <summary>失败路径不会触达任何内容服务，因此可以传 null。</summary>
    private static RecordHistoryService History(AppDbContext db) => new(db, null!, null!, null!, null!, null!);

    [Test]
    public async Task RecordWithoutObjectIsRejectedWithoutThrowing()
    {
        using var db = Context();
        var result = await History(db).CompareAsync<EntryIndexViewModel>(new Examine { ApplyTime = DateTime.UtcNow }, 0);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error, Does.Contain("不支持"));
    }

    [Test]
    public async Task RecordOfAnotherObjectIsReportedAsUnapproved()
    {
        using var db = Context();
        var (first, other) = await SeedAsync(db, "对象一", "对象二");

        var result = await History(db).CompareAsync<EntryIndexViewModel>(first, other.Id);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error, Is.EqualTo("编辑记录未通过审核"));
    }

    [Test]
    public async Task ContrastLaterThanCurrentIsRejected()
    {
        using var db = Context();
        var (first, second) = await SeedAsync(db, "对象一", "对象二");
        // 同一对象的第二个版本不能当作“基准版本”
        var later = new Examine
        {
            EntryId = first.EntryId,
            Operation = Operation.EstablishMain,
            ApplyTime = first.ApplyTime.AddMinutes(1),
            PassedTime = first.ApplyTime.AddMinutes(1),
            IsPassed = true,
            Context = "{}",
        };
        db.Examines.Add(later);
        await db.SaveChangesAsync();

        using (var verify = Context())
        {
            var result = await History(verify).CompareAsync<EntryIndexViewModel>(first, later.Id);
            Assert.That(result.Success, Is.False);
            Assert.That(result.Error, Is.EqualTo("对比的编辑必须先于当前的编辑"));
        }
        Assert.That(second.EntryId, Is.Not.EqualTo(first.EntryId));
    }

    [Test]
    public async Task FrameIsUnavailableForAnUnapprovedRecord()
    {
        using var db = Context();
        var entry = new Entry { Name = "未通过对象" };
        db.Entries.Add(entry);
        await db.SaveChangesAsync();
        var pending = new Examine
        {
            EntryId = entry.Id,
            Operation = Operation.EstablishMain,
            ApplyTime = DateTime.UtcNow,
            Context = "{}",
        };
        db.Examines.Add(pending);
        await db.SaveChangesAsync();

        Assert.That(await History(db).GetFrameAsync(pending), Is.Null);
    }

    private static async Task<(Examine First, Examine Second)> SeedAsync(AppDbContext db, string firstName, string secondName)
    {
        var firstEntry = new Entry { Name = firstName };
        var secondEntry = new Entry { Name = secondName };
        db.Entries.AddRange(firstEntry, secondEntry);
        await db.SaveChangesAsync();

        var first = new Examine
        {
            EntryId = firstEntry.Id,
            Operation = Operation.EstablishMain,
            ApplyTime = DateTime.UtcNow,
            PassedTime = DateTime.UtcNow,
            IsPassed = true,
            Context = "{}",
        };
        var second = new Examine
        {
            EntryId = secondEntry.Id,
            Operation = Operation.EstablishMain,
            ApplyTime = DateTime.UtcNow,
            PassedTime = DateTime.UtcNow,
            IsPassed = true,
            Context = "{}",
        };
        db.Examines.AddRange(first, second);
        await db.SaveChangesAsync();
        return (first, second);
    }
}
