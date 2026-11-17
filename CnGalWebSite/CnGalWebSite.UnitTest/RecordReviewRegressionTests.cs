using System.Reflection;
using CnGalWebSite.APIServer.Application.Examines;
using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.Application.Ranks;
using CnGalWebSite.APIServer.Application.Users;
using CnGalWebSite.APIServer.Controllers;
using CnGalWebSite.APIServer.ExamineX;
using CnGalWebSite.APIServer.Infrastructure;
using CnGalWebSite.DataModel.ExamineModel.Entries;
using CnGalWebSite.DataModel.ExamineModel.Peripheries;
using CnGalWebSite.DataModel.ExamineModel.Shared;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.SDK.MainSite.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace CnGalWebSite.UnitTest;

public class RecordReviewRegressionTests
{
    private SqliteConnection keeper = null!;
    private DbContextOptions<AppDbContext> options = null!;
    private const string AuthorId = "review-editor";

    [SetUp]
    public async Task SetUp()
    {
        var connectionString = $"Data Source=review_{Guid.NewGuid():N};Mode=Memory;Cache=Shared;Pooling=False";
        keeper = new SqliteConnection(connectionString);
        await keeper.OpenAsync();
        options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connectionString).Options;
        await using var db = Context();
        await db.Database.EnsureCreatedAsync();
        db.Users.Add(new ApplicationUser { Id = AuthorId, UserName = "review-editor" });
        db.EntryInformationTypes.Add(new EntryInformationType { Name = "QQ群", Icon = "qq" });
        await db.SaveChangesAsync();
    }

    [TearDown]
    public void TearDown() => keeper.Dispose();

    private AppDbContext Context() => new(options);

    [TestCase(false)]
    [TestCase(true)]
    public async Task DirectEditAndApprovalMaintainBothPeripheryDirections(bool approve)
    {
        long firstId, secondId;
        await using (var seed = Context())
        {
            var first = new Periphery { Name = "first" };
            var second = new Periphery { Name = "second" };
            seed.AddRange(first, second);
            await seed.SaveChangesAsync();
            (firstId, secondId) = (first.Id, second.Id);
        }
        // A fresh request for every write ensures approval loads the incoming direction itself.
        foreach (var remove in new[] { false, false, true })
        {
            await using var db = Context();
            var actor = await db.Users.SingleAsync(u => u.Id == AuthorId);
            var data = new PeripheryRelatedPeripheries
            {
                Relevances = [new() { PeripheryId = secondId, IsDelete = remove }]
            };
            var services = ApprovalServices(db, actor);
            if (approve)
            {
                var record = new Examine { PeripheryId = firstId, ApplicationUserId = AuthorId,
                    Operation = Operation.EditPeripheryRelatedPeripheries, ApplyTime = DateTime.UtcNow,
                    Context = JsonConvert.SerializeObject(data) };
                db.Examines.Add(record);
                await db.SaveChangesAsync();
                db.ChangeTracker.Clear();
                var result = await services.Get<ExaminesAPIController>().ProcAsync(new()
                {
                    Id = record.Id, IsPassed = true
                });
                Assert.That(result.Value?.Successful, Is.True, result.Value?.Error);
            }
            else
            {
                var target = await db.Peripheries
                    .Include(p => p.PeripheryRelationFromPeripheryNavigation).ThenInclude(r => r.ToPeripheryNavigation)
                    .Include(p => p.PeripheryRelationToPeripheryNavigation).SingleAsync(p => p.Id == firstId);
                await services.Get<EditRecordService>().SaveAndApplyEditRecord(target, actor, data,
                    Operation.EditPeripheryRelatedPeripheries, "regression");
            }
            await using var verify = Context();
            var edges = await verify.PeripheryRelations.Where(r => r.FromPeriphery != null && r.ToPeriphery != null).ToListAsync();
            Assert.That(edges.Select(r => (r.FromPeriphery, r.ToPeriphery)),
                Is.EquivalentTo(remove ? Array.Empty<(long?, long?)>() : new (long?, long?)[] { (firstId, secondId), (secondId, firstId) }));
            Assert.That(await verify.Examines.AllAsync(e => e.IsPassed == true), Is.True);
        }
    }

    [TestCase(EntityState.Added, false)]
    [TestCase(EntityState.Deleted, false)]
    [TestCase(EntityState.Modified, false)]
    [TestCase(EntityState.Modified, true)]
    public async Task SymmetricPeripheryBoundaryStillRejectsUnrelatedRows(EntityState state, bool transferTarget)
    {
        await using var db = Context();
        var root = new Periphery { Name = "root" };
        var other = new Periphery { Name = "other" };
        var third = new Periphery { Name = "third" };
        db.AddRange(root, other, third);
        await db.SaveChangesAsync();
        var relation = new PeripheryRelation { FromPeripheryNavigation = other, ToPeripheryNavigation = third };
        if (state != EntityState.Added)
        {
            db.PeripheryRelations.Add(relation);
            await db.SaveChangesAsync();
        }
        using var boundary = new EditWriteBoundary(db, root, Operation.EditPeripheryRelatedPeripheries, new PeripheryRelatedPeripheries());
        if (state == EntityState.Added) db.PeripheryRelations.Add(relation);
        else if (state == EntityState.Deleted) db.PeripheryRelations.Remove(relation);
        else if (transferTarget) relation.ToPeripheryNavigation = root;
        else relation.FromPeripheryNavigation = root;
        Assert.That(async () => await db.SaveChangesAsync(), Throws.InvalidOperationException);
    }

    [TestCase(false, 2, true)]
    [TestCase(true, 2, true)]
    [TestCase(false, 0, false)]
    [TestCase(true, 0, false)]
    public async Task BookingHistoryRetainsConfigurationAfterRelease(bool prepareFrames, int lotteryId, bool notify)
    {
        await using var db = Context();
        var entry = new Entry { Name = "booking-history", PubulishTime = new DateTime(2000, 1, 1),
            Booking = new Booking { BookingCount = 999, LotteryId = 1, Open = false } };
        db.Entries.Add(entry);
        await db.SaveChangesAsync();
        var main = Record(entry, Operation.EstablishMain, new ExamineMain { Items = [new() { Key = "Name", Value = entry.Name }] }, 0);
        var initial = new EntryAddInfor
        {
            Releases = [new() { Name = "official", Type = GameReleaseType.Official, Time = entry.PubulishTime }]
        };
        initial.Booking.MainInfor = [new() { Key = "LotteryId", Value = "1" }, new() { Key = "Open", Value = "false" },
            new() { Key = "IsNeedNotification", Value = (!notify).ToString() }];
        initial.Booking.Goals = [new() { Name = "goal", Target = 100 }];
        var change = new EntryAddInfor();
        change.Booking.MainInfor = [new() { Key = "LotteryId", Value = lotteryId.ToString() },
            new() { Key = "IsNeedNotification", Value = notify.ToString() }];
        change.Booking.Goals = [new() { Name = "goal", Target = 200 }];
        var before = Record(entry, Operation.EstablishAddInfor, initial, 1);
        var after = Record(entry, Operation.EstablishAddInfor, change, 2);
        db.Examines.AddRange(main, before, after);
        await db.SaveChangesAsync();
        var services = new RealRecordServices(db);
        var history = services.Get<RecordHistoryService>();
        var comparison = await history.CompareAsync<EntryIndexViewModel>(after, before.Id);
        Assert.That(comparison.Success, Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(comparison.Before.Booking, Is.Null);
            Assert.That(comparison.After.Booking, Is.Null);
            Assert.That(comparison.After.BookingHistory.LotteryId, Is.EqualTo(lotteryId));
            Assert.That(comparison.After.BookingHistory.IsNeedNotification, Is.EqualTo(notify));
            Assert.That(comparison.After.BookingHistory.Goals.Single().Target, Is.EqualTo(200));
            Assert.That(JsonConvert.SerializeObject(comparison.After.BookingHistory), Does.Not.Contain("BookingCount"));
        });
        if (prepareFrames) await history.PrepareFramesAsync([after]);
        var frame = (await history.GetFrameAsync(after))!;
        var diff = RecordDiffBuilder.Build(ExaminePreviewMapper.FromPreData(frame.Before), ExaminePreviewMapper.FromPreData(frame.After));
        foreach (var label in new[] { "关联抽奖", "上线后通知用户", "goal" })
            Assert.That(diff.Items.Single(i => i.Item.Label == label).Change, Is.EqualTo(RecordChangeKind.Modified), label);
        var ordinary = await services.Get<CnGalWebSite.APIServer.Application.Entries.EntryService>().GetEntryIndexViewModelAsync(entry);
        Assert.That(ordinary.BookingHistory, Is.Null, "Ordinary entry responses must not contain historical configuration");
    }

    [TestCase(Operation.EstablishMainPage, null)]
    [TestCase(Operation.EstablishMainPage, false)]
    [TestCase(Operation.EstablishMainPage, true)]
    [TestCase(Operation.EditArticleMainPage, null)]
    [TestCase(Operation.EditArticleMainPage, false)]
    [TestCase(Operation.EditArticleMainPage, true)]
    [TestCase(Operation.EditVideoMainPage, null)]
    [TestCase(Operation.EditVideoMainPage, false)]
    [TestCase(Operation.EditVideoMainPage, true)]
    public async Task RawMainPageUsesOriginalSourcesForPendingRejectedAndApprovedRecords(Operation operation, bool? passed)
    {
        const string beforeText = "<del>原有删除线</del><br>\r\n&amp; 旧<script>before()</script>";
        const string afterText = "<del>原有删除线</del><br>\n&amp; 新<script>after()</script>";
        await using var db = Context();
        var liveText = passed == true ? afterText : beforeText;
        object target = operation switch
        {
            Operation.EditArticleMainPage => new Article { Name = "raw-article", MainPage = liveText },
            Operation.EditVideoMainPage => new Video { Name = "raw-video", MainPage = liveText },
            _ => new Entry { Name = "raw-entry", MainPage = liveText }
        };
        db.Add(target);
        await db.SaveChangesAsync();
        Examine PageRecord(string text, int offset, bool? approved) => new()
        {
            EntryId = (target as Entry)?.Id, ArticleId = (target as Article)?.Id, VideoId = (target as Video)?.Id,
            ApplicationUserId = AuthorId, Operation = operation, Context = text, IsPassed = approved,
            ApplyTime = DateTime.UtcNow.AddMinutes(offset), PassedTime = approved == true ? DateTime.UtcNow.AddMinutes(offset) : null
        };
        var first = PageRecord(beforeText, -2, true);
        var current = PageRecord(afterText, -1, passed);
        db.Examines.AddRange(first, current);
        await db.SaveChangesAsync();
        var model = new ExamineViewModel();
        Assert.That(await new RealRecordServices(db).Get<ExamineService>().GetExamineView(model, current), Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(model.RawBeforeContent, Is.EqualTo(beforeText));
            Assert.That(model.RawContent, Is.EqualTo(afterText));
            Assert.That(db.ChangeTracker.HasChanges(), Is.False);
            Assert.That(target.GetType().GetProperty("MainPage")!.GetValue(target), Is.EqualTo(liveText));
        });
    }

    private static Examine Record(Entry entry, Operation operation, object data, int offset) => new()
    {
        EntryId = entry.Id, ApplicationUserId = AuthorId, Operation = operation,
        Context = JsonConvert.SerializeObject(data), IsPassed = true,
        ApplyTime = new DateTime(2020, 1, 1).AddMinutes(offset), PassedTime = new DateTime(2020, 1, 1).AddMinutes(offset)
    };

    private static RealRecordServices ApprovalServices(AppDbContext db, ApplicationUser actor)
    {
        var helper = DispatchProxy.Create<IAppHelper, ReviewApprovalProxy>();
        ((ReviewApprovalProxy)helper).Actor = actor;
        return new(db, new Dictionary<Type, object>
        {
            [typeof(IAppHelper)] = helper,
            [typeof(IUserService)] = DispatchProxy.Create<IUserService, ReviewApprovalProxy>(),
            [typeof(IRankService)] = DispatchProxy.Create<IRankService, ReviewApprovalProxy>()
        });
    }
}

public class ReviewApprovalProxy : DispatchProxy
{
    public ApplicationUser Actor { get; set; } = null!;
    protected override object? Invoke(MethodInfo? method, object?[]? args) => method!.Name switch
    {
        "GetAPICurrentUserAsync" => Task.FromResult(Actor),
        "CheckCurrentUserRole" => true,
        "UpdateUserIntegral" or "UpdateUserRanks" => Task.CompletedTask,
        _ => throw new InvalidOperationException("Unexpected approval dependency: " + method.Name)
    };
}
