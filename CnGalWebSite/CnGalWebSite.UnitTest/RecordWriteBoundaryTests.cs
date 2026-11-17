using CnGalWebSite.APIServer.Application.Examines;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.APIServer.Infrastructure;
using CnGalWebSite.DataModel.ExamineModel.Shared;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace CnGalWebSite.UnitTest;

/// <summary>
/// 审核写入边界、仓储更新语义与审批事务的回归测试。
/// 这些是被 UI 测试完全覆盖不到的 APIServer 侧契约，曾经出现过两个回归：
/// 标签父级变更被边界拒绝、分离实体更新导致商店信息定时任务整体失败。
/// </summary>
public class RecordWriteBoundaryTests
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

    private static ExamineMain Main(params (string Key, string? Value)[] items) => new()
    {
        Items = items.Select(item => new ExamineMainAlone { Key = item.Key, Value = item.Value }).ToList()
    };

    [Test]
    public async Task TagParentChangeIsAcceptedByTheWriteBoundary()
    {
        using var db = Context();
        var tag = new Tag { Name = "标签子级" };
        var parent = new Tag { Name = "标签父级" };
        db.Tags.AddRange(tag, parent);
        await db.SaveChangesAsync();

        // 编辑记录里 ParentTagId 是命令键（由 TagService 转成导航赋值），不是实体字段。
        var main = Main(("Name", "标签子级-改名"), ("ParentTagId", parent.Id.ToString()));
        using (new EditWriteBoundary(db, tag, Operation.EditTagMain, main))
        {
            tag.Name = "标签子级-改名";
            tag.ParentCodeNavigation = parent;
            Assert.That(async () => { await db.SaveChangesAsync(); }, Throws.Nothing);
        }

        using var verify = Context();
        var saved = await verify.Tags.Include(s => s.ParentCodeNavigation).SingleAsync(s => s.Name == "标签子级-改名");
        Assert.That(saved.ParentCodeNavigation?.Name, Is.EqualTo("标签父级"));
    }

    [Test]
    public void TagMainAllowedKeysContainTheParentCommandKeyButNeverLastEditTime()
    {
        var keys = EditWriteBoundary.AllowedKeys(Operation.EditTagMain);

        Assert.That(keys, Does.Contain("ParentTagId").And.Contain("Name"));
        Assert.That(keys, Does.Not.Contain("LastEditTime"));
        Assert.That(EditWriteBoundary.IsGuarded(Operation.EditTagMain), Is.True);
        Assert.That(EditWriteBoundary.IsGuarded(Operation.PubulishComment), Is.False);
    }

    [Test]
    public async Task UndeclaredRecordKeyIsRejectedByTheBoundary()
    {
        using var db = Context();
        var entry = new Entry { Name = "越界词条" };
        db.Entries.Add(entry);
        await db.SaveChangesAsync();

        var main = Main(("MainPage", "# 不应该出现在主要信息里"));
        Assert.That(() => new EditWriteBoundary(db, entry, Operation.EstablishMain, main), Throws.InvalidOperationException);
    }

    [Test]
    public async Task UndeclaredColumnChangeIsRejectedOnSave()
    {
        using var db = Context();
        var tag = new Tag { Name = "越界列标签" };
        db.Tags.Add(tag);
        await db.SaveChangesAsync();

        using var boundary = new EditWriteBoundary(db, tag, Operation.EditTagMain, Main(("Name", "越界列标签-改名")));
        tag.Priority = 9;
        Assert.That(async () => { await db.SaveChangesAsync(); }, Throws.InvalidOperationException);
    }

    [Test]
    public async Task RepositoryUpdateStillAcceptsDetachedEntitiesOutsideABoundary()
    {
        using var db = Context();
        db.Tags.Add(new Tag { Name = "商店信息等价场景" });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var detached = await db.Tags.AsNoTracking().SingleAsync(s => s.Name == "商店信息等价场景");
        detached.Name = "商店信息等价场景-已更新";
        var repository = new RepositoryBase<Tag, int>(db);
        await repository.UpdateAsync(detached);

        using var verify = Context();
        Assert.That(verify.Tags.Single(s => s.Name == "商店信息等价场景-已更新").Name, Is.EqualTo("商店信息等价场景-已更新"));
    }

    [Test]
    public async Task RepositoryUpdateRejectsDetachedEntitiesInsideABoundary()
    {
        using var db = Context();
        var tag = new Tag { Name = "边界内标签" };
        var other = new Tag { Name = "边界外标签" };
        db.Tags.AddRange(tag, other);
        await db.SaveChangesAsync();

        using var boundary = new EditWriteBoundary(db, tag, Operation.EditTagMain, Main(("Name", "边界内标签-改名")));
        db.ChangeTracker.Clear();

        // 边界内无法证明“改了哪些列”，分离实体必须被拒绝（调用方应改用 UpdateFields）。
        var detached = await db.Tags.AsNoTracking().SingleAsync(s => s.Name == "边界外标签");
        detached.Name = "边界外标签-改名";
        var repository = new RepositoryBase<Tag, int>(db);
        Assert.That(async () => { await repository.UpdateAsync(detached); }, Throws.InvalidOperationException);
    }

    [Test]
    public async Task RepositoryUpdateFieldsWritesOnlyTheDeclaredColumnsForDetachedEntities()
    {
        using var db = Context();
        db.Tags.Add(new Tag { Name = "字段更新", BriefIntroduction = "原文" });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        // 边界内推荐的做法：只声明要写的列，其余列即使被改过也不会落库。
        var detached = await db.Tags.AsNoTracking().SingleAsync(s => s.Name == "字段更新");
        detached.Name = "字段更新-改名";
        detached.BriefIntroduction = "不应写入";
        var repository = new RepositoryBase<Tag, int>(db);
        await repository.UpdateFieldsAsync(detached, s => s.Name);

        using var verify = Context();
        var saved = verify.Tags.Single(s => s.Name == "字段更新-改名");
        Assert.That(saved.BriefIntroduction, Is.EqualTo("原文"));
    }

    [Test]
    public async Task UnitOfWorkCommitsSuccessfulActionsAndRollsBackFailedOnes()
    {
        using (var db = Context())
        {
            var unitOfWork = new EditRecordUnitOfWork(db, NullLogger<EditRecordUnitOfWork>.Instance);
            await unitOfWork.RunAsync(async () =>
            {
                db.Tags.Add(new Tag { Name = "应被回滚" });
                await db.SaveChangesAsync();
                return true;
            }, _ => false);
        }

        using (var db = Context())
        {
            var unitOfWork = new EditRecordUnitOfWork(db, NullLogger<EditRecordUnitOfWork>.Instance);
            await unitOfWork.RunAsync(async () =>
            {
                db.Tags.Add(new Tag { Name = "应被提交" });
                await db.SaveChangesAsync();
                return true;
            });
        }

        using var verify = Context();
        Assert.That(await verify.Tags.AnyAsync(s => s.Name == "应被回滚"), Is.False);
        Assert.That(await verify.Tags.AnyAsync(s => s.Name == "应被提交"), Is.True);
    }

    [Test]
    public async Task RepeatedApprovalIsRejectedByTheClaim()
    {
        using (var db = Context())
        {
            var entry = new Entry { Name = "抢占词条" };
            db.Entries.Add(entry);
            await db.SaveChangesAsync();
            db.Examines.Add(new Examine
            {
                EntryId = entry.Id,
                Operation = Operation.EstablishMain,
                ApplyTime = DateTime.UtcNow,
                Context = "{}",
            });
            await db.SaveChangesAsync();

            var unitOfWork = new EditRecordUnitOfWork(db, NullLogger<EditRecordUnitOfWork>.Instance);
            var record = await db.Examines.SingleAsync();
            var first = await unitOfWork.RunAsync(() => unitOfWork.TryClaimAsync(record, true));
            var second = await unitOfWork.RunAsync(() => unitOfWork.TryClaimAsync(record, true));

            Assert.That(new[] { first, second }, Is.EqualTo(new[] { true, false }));
        }

        using var verify = Context();
        Assert.That((await verify.Examines.SingleAsync()).IsPassed, Is.True);
    }

    [Test]
    public async Task ClaimRequiresATransaction()
    {
        using var db = Context();
        db.Examines.Add(new Examine { Operation = Operation.EstablishMain, ApplyTime = DateTime.UtcNow, Context = "{}" });
        await db.SaveChangesAsync();

        var unitOfWork = new EditRecordUnitOfWork(db, NullLogger<EditRecordUnitOfWork>.Instance);
        var record = await db.Examines.SingleAsync();
        Assert.That(async () => { await unitOfWork.TryClaimAsync(record, true); }, Throws.InvalidOperationException);
    }

    /// <summary>
    /// 契约漂移守卫：只要 <c>ToolHelper.GetEditingRecordFromContrastData</c> 能产出的 key
    /// 没有被对应 Operation 的契约覆盖，审批时就会直接失败。新增实体字段时会在这里报错。
    /// </summary>
    [Test]
    public void MainEditContractsCoverEveryGeneratedKey()
    {
        AssertGeneratedKeysAreDeclared<Entry>(Operation.EstablishMain, "PubulishTime");
        AssertGeneratedKeysAreDeclared<Article>(Operation.EditArticleMain);
        AssertGeneratedKeysAreDeclared<Video>(Operation.EditVideoMain);
        AssertGeneratedKeysAreDeclared<Periphery>(Operation.EditPeripheryMain);
        AssertGeneratedKeysAreDeclared<Tag>(Operation.EditTagMain);
    }

    private static void AssertGeneratedKeysAreDeclared<T>(Operation operation, params string[] removedByApplyStep)
        where T : class, new()
    {
        var before = new T();
        var after = new T();
        foreach (var property in typeof(T).GetProperties().Where(p => p.CanWrite && IsComparable(p.PropertyType)))
        {
            property.SetValue(after, ChangedValue(property.PropertyType));
        }

        var generated = ToolHelper.GetEditingRecordFromContrastData(before, after)
            .Select(s => s.Key)
            .Except(removedByApplyStep)
            .Distinct()
            .ToList();
        var allowed = EditWriteBoundary.AllowedKeys(operation);
        var missing = generated.Except(allowed).ToList();

        Assert.That(missing, Is.Empty,
            $"{operation} 的写入边界缺少 key：{string.Join("、", missing)}（契约漂移会导致审批失败）");
    }

    private static bool IsComparable(Type type) => type == typeof(string) || type == typeof(DateTime)
        || type == typeof(DateTime?) || type == typeof(TimeSpan) || type == typeof(EntryType)
        || type == typeof(ArticleType) || type == typeof(PeripheryType) || type == typeof(CopyrightType)
        || type == typeof(bool) || type == typeof(int);

    private static object? ChangedValue(Type type)
    {
        if (type == typeof(string)) return "changed";
        if (type == typeof(DateTime)) return new DateTime(2001, 2, 3);
        if (type == typeof(DateTime?)) return new DateTime(2001, 2, 3);
        if (type == typeof(TimeSpan)) return TimeSpan.FromMinutes(90);
        if (type == typeof(bool)) return true;
        if (type == typeof(int)) return 1;
        var fallback = Activator.CreateInstance(type);
        return Enum.GetValues(type).Cast<object>().FirstOrDefault(value => !value.Equals(fallback));
    }
}
