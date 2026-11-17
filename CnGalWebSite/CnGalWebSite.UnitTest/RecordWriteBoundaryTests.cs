using CnGalWebSite.APIServer.Application.Examines;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.APIServer.Infrastructure;
using CnGalWebSite.DataModel.ExamineModel.Entries;
using CnGalWebSite.DataModel.ExamineModel.Shared;
using CnGalWebSite.DataModel.ExamineModel.Tags;
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
        var error = Assert.CatchAsync<InvalidOperationException>(async () => { await db.SaveChangesAsync(); });
        Assert.That(error, Is.Not.Null);
        Assert.That(error!.Message, Does.Contain("Priority"), "错误信息必须指出越界的列，便于线上定位");
    }

    [Test]
    public async Task AddInfoEditCanUpdateTheDerivedPublishTime()
    {
        using var db = Context();
        var entry = new Entry { Name = "附加信息词条" };
        db.Entries.Add(entry);
        await db.SaveChangesAsync();

        // 附加信息应用步骤会用最早的正式发行时间回填 PubulishTime。
        using (new EditWriteBoundary(db, entry, Operation.EstablishAddInfor, new EntryAddInfor()))
        {
            entry.PubulishTime = new DateTime(2020, 1, 2);
            entry.LastEditTime = DateTime.Now;
            Assert.That(async () => { await db.SaveChangesAsync(); }, Throws.Nothing);
        }

        using var verify = Context();
        var saved = verify.Entries.Single(s => s.Name == "附加信息词条");
        Assert.That(saved.PubulishTime, Is.EqualTo(new DateTime(2020, 1, 2)));
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
    public async Task RepositoryUpdateFieldsRejectsUndeclaredTrackedChanges()
    {
        using var db = Context();
        var tag = new Tag { Name = "跟踪字段更新", BriefIntroduction = "原文" };
        db.Tags.Add(tag);
        await db.SaveChangesAsync();

        // 已跟踪实体上还有选择之外的挂起修改时，SaveChanges 会把它们一并写入，
        // 与“只写声明的列”矛盾，必须显式拒绝而不是静默落库。
        tag.Name = "跟踪字段更新-改名";
        tag.BriefIntroduction = "不应写入";
        var repository = new RepositoryBase<Tag, int>(db);
        Assert.That(async () => { await repository.UpdateFieldsAsync(tag, s => s.Name); }, Throws.InvalidOperationException);

        using var verify = Context();
        var saved = verify.Tags.Single(s => s.Id == tag.Id);
        Assert.That(saved.Name, Is.EqualTo("跟踪字段更新"));
        Assert.That(saved.BriefIntroduction, Is.EqualTo("原文"));
    }

    [Test]
    public async Task DependentOwnedByAnotherTargetIsRejected()
    {
        using var db = Context();
        var entry = new Entry { Name = "边界词条" };
        var other = new Entry { Name = "其他词条" };
        db.Entries.AddRange(entry, other);
        await db.SaveChangesAsync();
        entry.Pictures.Add(new EntryPicture { Url = "a.png" });
        other.Pictures.Add(new EntryPicture { Url = "b.png" });
        await db.SaveChangesAsync();

        using var boundary = new EditWriteBoundary(db, entry, Operation.EstablishImages, new EntryImages());
        db.Entry(other.Pictures.Single()).State = EntityState.Deleted;

        var error = Assert.CatchAsync<InvalidOperationException>(async () => { await db.SaveChangesAsync(); });
        Assert.That(error, Is.Not.Null);
        Assert.That(error!.Message, Does.Contain("EntryPicture"), "错误信息必须指出越界的对象，便于线上定位");

        using var verify = Context();
        Assert.That(await verify.Set<EntryPicture>().CountAsync(), Is.EqualTo(2));
    }

    [Test]
    public async Task DependentOwnedByTheTargetCanStillBeRemoved()
    {
        using var db = Context();
        var entry = new Entry { Name = "图片词条" };
        db.Entries.Add(entry);
        await db.SaveChangesAsync();
        entry.Pictures.Add(new EntryPicture { Url = "a.png" });
        await db.SaveChangesAsync();

        using (new EditWriteBoundary(db, entry, Operation.EstablishImages, new EntryImages()))
        {
            // 编辑时是从目标集合里移除，可选外键会被置空，归属只能靠原始关系判断。
            entry.Pictures.Remove(entry.Pictures.Single());
            Assert.That(async () => { await db.SaveChangesAsync(); }, Throws.Nothing);
        }

        using var verify = Context();
        Assert.That((await verify.Entries.Include(s => s.Pictures).SingleAsync(s => s.Id == entry.Id)).Pictures, Is.Empty);
    }

    [Test]
    public async Task NestedDependentOfTheTargetIsAccepted()
    {
        using var db = Context();
        var entry = new Entry { Name = "官网词条" };
        db.Entries.Add(entry);
        await db.SaveChangesAsync();

        using (new EditWriteBoundary(db, entry, Operation.EstablishWebsite, new EntryWebsiteExamineModel()))
        {
            // EntryWebsiteImage → EntryWebsite → Entry：归属要能穿过中间子项。
            entry.WebsiteAddInfor = new EntryWebsite();
            entry.WebsiteAddInfor.Images.Add(new EntryWebsiteImage { Url = "w.png" });
            Assert.That(async () => { await db.SaveChangesAsync(); }, Throws.Nothing);
        }

        using var verify = Context();
        var saved = await verify.EntryWebsites.Include(s => s.Images).SingleAsync();
        Assert.That(saved.Images.Single().Url, Is.EqualTo("w.png"));
    }

    [Test]
    public async Task JoinRowOfTheTargetIsAccepted()
    {
        using var db = Context();
        var entry = new Entry { Name = "标签词条" };
        var tag = new Tag { Name = "标签" };
        db.Entries.Add(entry);
        db.Tags.Add(tag);
        await db.SaveChangesAsync();

        using (new EditWriteBoundary(db, entry, Operation.EstablishTags, new EntryTags()))
        {
            entry.Tags.Add(tag);
            Assert.That(async () => { await db.SaveChangesAsync(); }, Throws.Nothing);
        }

        using var verify = Context();
        var saved = await verify.Entries.Include(s => s.Tags).SingleAsync(s => s.Id == entry.Id);
        Assert.That(saved.Tags.Select(s => s.Name), Is.EqualTo(new[] { "标签" }));
    }

    [Test]
    public async Task JoinRowOfAnotherTargetIsRejected()
    {
        using var db = Context();
        var entry = new Entry { Name = "边界词条" };
        var other = new Entry { Name = "其他词条" };
        var tag = new Tag { Name = "标签" };
        db.Entries.AddRange(entry, other);
        db.Tags.Add(tag);
        await db.SaveChangesAsync();

        using var boundary = new EditWriteBoundary(db, entry, Operation.EstablishTags, new EntryTags());
        other.Tags.Add(tag);

        var error = Assert.CatchAsync<InvalidOperationException>(async () => { await db.SaveChangesAsync(); });
        Assert.That(error, Is.Not.Null);
    }

    [Test]
    public async Task RelevanceEditOfTheTargetIsAccepted()
    {
        using var db = Context();
        var entry = new Entry { Name = "关联词条" };
        var to = new Entry { Name = "关联目标" };
        var article = new Article { Name = "关联文章" };
        var video = new Video { Name = "关联视频" };
        db.Entries.AddRange(entry, to);
        db.Articles.Add(article);
        db.Videos.Add(video);
        await db.SaveChangesAsync();

        using (new EditWriteBoundary(db, entry, Operation.EstablishRelevances, new EntryRelevances()))
        {
            // 与 EntryService.UpdateEntryDataRelevances 的写法保持一致。
            entry.EntryRelationFromEntryNavigation.Add(new EntryRelation
            {
                FromEntry = entry.Id,
                FromEntryNavigation = entry,
                ToEntry = to.Id,
                ToEntryNavigation = to
            });
            entry.Outlinks.Add(new Outlink { Name = "官网", Link = "https://example.com" });
            entry.Articles.Add(article);
            entry.Videos.Add(video);
            Assert.That(async () => { await db.SaveChangesAsync(); }, Throws.Nothing);
        }

        using var verify = Context();
        Assert.That((await verify.Entries.Include(s => s.EntryRelationFromEntryNavigation).SingleAsync(s => s.Id == entry.Id))
            .EntryRelationFromEntryNavigation.Single().ToEntry, Is.EqualTo(to.Id));
    }

    [Test]
    public async Task BookingGoalOfTheTargetIsAccepted()
    {
        using var db = Context();
        var entry = new Entry { Name = "预约词条" };
        db.Entries.Add(entry);
        await db.SaveChangesAsync();

        using (new EditWriteBoundary(db, entry, Operation.EstablishAddInfor, new EntryAddInfor()))
        {
            // BookingGoal 没有显式外键，只能靠 Booking 这一层中间子项证明归属。
            entry.Booking = new Booking { Open = true };
            entry.Booking.Goals.Add(new BookingGoal { Name = "目标", Target = 100 });
            Assert.That(async () => { await db.SaveChangesAsync(); }, Throws.Nothing);
        }

        using var verify = Context();
        var booking = await verify.Bookings.Include(s => s.Goals).SingleAsync();
        Assert.That(booking.Goals.Single().Name, Is.EqualTo("目标"));
    }

    [Test]
    public async Task ChildTagEditIsStillAcceptedByTheBoundary()
    {
        using var db = Context();
        var parent = new Tag { Name = "父标签" };
        var child = new Tag { Name = "子标签" };
        db.Tags.AddRange(parent, child);
        await db.SaveChangesAsync();

        using (new EditWriteBoundary(db, parent, Operation.EditTagChildTags, new TagChildTags()))
        {
            // 子标签只改父子关联，归属由 ParentCode 外键证明。
            parent.InverseParentCodeNavigation.Add(child);
            Assert.That(async () => { await db.SaveChangesAsync(); }, Throws.Nothing);
        }

        using var verify = Context();
        var saved = await verify.Tags.Include(s => s.ParentCodeNavigation).SingleAsync(s => s.Id == child.Id);
        Assert.That(saved.ParentCodeNavigation!.Id, Is.EqualTo(parent.Id));
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
