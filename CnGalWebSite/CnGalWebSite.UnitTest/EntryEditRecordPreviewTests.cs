using System.Data.Common;
using System.Security.Claims;
using CnGalWebSite.APIServer.Application.Ranks;
using CnGalWebSite.APIServer.Application.Users;
using CnGalWebSite.APIServer.Controllers;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.APIServer.Infrastructure;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Entries;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CnGalWebSite.UnitTest;

public class EntryEditRecordPreviewTests
{
    [TestCase(1)]
    [TestCase(40)]
    public async Task PreviewFiltersBeforeLimitingAndReadsRanksInOneQuery(int recordCount)
    {
        await using var fixture = await Fixture.CreateAsync();
        var author = new ApplicationUser { Id = "author", UserName = "Author", PhotoPath = "avatar.png" };
        var bot = new ApplicationUser { Id = "bot", UserName = "看板娘" };
        var time = new DateTime(2026, 1, 1);
        var visible = Enumerable.Range(0, recordCount)
            .Select(_ => new Examine { EntryId = fixture.EntryId, ApplicationUser = author, ApplyTime = time, IsPassed = true })
            .ToList();
        fixture.Db.Examines.AddRange(visible);
        fixture.Db.Examines.AddRange(
            new Examine { EntryId = fixture.EntryId, ApplicationUser = bot, ApplyTime = time.AddDays(2), IsPassed = true },
            new Examine { EntryId = fixture.EntryId, ApplicationUser = author, ApplyTime = time.AddDays(1) },
            new Examine { EntryId = fixture.EntryId, ApplicationUser = author, ApplyTime = time.AddDays(1), IsPassed = false });
        fixture.Db.RankUsers.AddRange(
            new RankUser { ApplicationUser = author, Rank = new Rank { Name = "Badge", Priority = 10 } },
            new RankUser { ApplicationUser = author, Rank = new Rank { Name = "Title" } },
            new RankUser { ApplicationUser = author, Rank = new Rank { Name = "编辑者" } },
            new RankUser { ApplicationUser = author, Rank = new Rank { Name = "Hidden", IsHidden = true } },
            new RankUser { ApplicationUser = author, Rank = new Rank { Name = "Private" }, IsHidden = true });
        await fixture.Db.SaveChangesAsync();
        fixture.Db.ChangeTracker.Clear();
        fixture.Commands.Sql.Clear();

        var result = (await fixture.GetAsync(fixture.EntryId)).Value!;
        Assert.That(result.TotalCount, Is.EqualTo(recordCount));
        Assert.That(result.Items.Select(record => record.Id),
            Is.EqualTo(visible.Select(record => record.Id).OrderDescending().Take(8)));
        foreach (var record in result.Items)
        {
            Assert.That(record.AuthorPhoto, Is.EqualTo("avatar.png"));
            Assert.That(record.AuthorName, Is.EqualTo("Author"));
            Assert.That(record.AuthorRanks.Select(rank => rank.Name), Is.EqualTo(new[] { "Badge", "Title" }));
        }
        Assert.That(fixture.Commands.Sql, Has.Count.EqualTo(4));
        Assert.That(fixture.Commands.Sql.Count(sql => sql.Contains("\"RankUsers\"")), Is.EqualTo(1));
    }

    [Test]
    public async Task HiddenPreviewRequiresAnExistingEditorAndAnEmptyListSkipsRankQueries()
    {
        await using var fixture = await Fixture.CreateAsync();
        Assert.That((await fixture.GetAsync(fixture.HiddenEntryId)).Result, Is.TypeOf<NotFoundObjectResult>());

        fixture.SignIn("member", "Member");
        Assert.That((await fixture.GetAsync(fixture.HiddenEntryId)).Result, Is.TypeOf<NotFoundObjectResult>());
        fixture.SignIn("missing", "Editor");
        Assert.That((await fixture.GetAsync(fixture.HiddenEntryId)).Result, Is.TypeOf<NotFoundObjectResult>());

        fixture.SignIn("member", "Editor");
        fixture.Commands.Sql.Clear();
        var result = (await fixture.GetAsync(fixture.HiddenEntryId)).Value!;
        Assert.That(result.TotalCount, Is.Zero);
        Assert.That(result.Items, Is.Empty);
        Assert.That(fixture.Commands.Sql, Has.Count.EqualTo(2));
    }

    private sealed class Fixture(SqliteConnection connection, AppDbContext db, SqlCounter commands) : IAsyncDisposable
    {
        public AppDbContext Db => db;
        public SqlCounter Commands => commands;
        public int EntryId { get; private init; }
        public int HiddenEntryId { get; private init; }
        private EntriesAPIController Controller { get; init; } = null!;
        private IRankService Ranks { get; init; } = null!;

        public static async Task<Fixture> CreateAsync()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            var commands = new SqlCounter();
            var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(connection).AddInterceptors(commands).Options);
            try
            {
                await db.Database.EnsureCreatedAsync();
                var entry = new Entry { Name = "Visible" };
                var hidden = new Entry { Name = "Hidden", IsHidden = true };
                db.AddRange(entry, hidden, new ApplicationUser { Id = "member", UserName = "Member" });
                await db.SaveChangesAsync();
                db.ChangeTracker.Clear();
                commands.Sql.Clear();

                var context = new DefaultHttpContext();
                var services = new Dictionary<Type, object>
                {
                    [typeof(IHttpContextAccessor)] = new FixtureContextAccessor { HttpContext = context },
                };
                var resolvers = new RealRecordServices(db, services);
                var ranks = resolvers.Get<RankService>();
                services[typeof(IRankService)] = ranks;
                services[typeof(IUserService)] = resolvers.Get<UserService>();
                var controller = resolvers.Get<EntriesAPIController>();
                controller.ControllerContext = new ControllerContext { HttpContext = context };
                return new Fixture(connection, db, commands)
                {
                    EntryId = entry.Id, HiddenEntryId = hidden.Id, Controller = controller, Ranks = ranks,
                };
            }
            catch
            {
                await db.DisposeAsync();
                await connection.DisposeAsync();
                throw;
            }
        }

        public Task<ActionResult<EntryEditRecordPreview>> GetAsync(int id) =>
            Controller.GetEntryEditRecordPreview(id, Ranks, new RepositoryBase<ApplicationUser, string>(db), default);

        public void SignIn(string id, string role) =>
            Controller.HttpContext.User = new ClaimsPrincipal(
                new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, id), new Claim(ClaimTypes.Role, role)], "test"));

        public async ValueTask DisposeAsync()
        {
            await db.DisposeAsync();
            await connection.DisposeAsync();
        }
    }

    private sealed class FixtureContextAccessor : IHttpContextAccessor
    {
        public HttpContext? HttpContext { get; set; }
    }

    private sealed class SqlCounter : DbCommandInterceptor
    {
        public List<string> Sql { get; } = [];

        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = default)
        {
            Sql.Add(command.CommandText);
            return ValueTask.FromResult(result);
        }
    }
}
