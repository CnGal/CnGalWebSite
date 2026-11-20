using System.Data.Common;
using System.Security.Claims;
using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.Application.PlayedGames;
using CnGalWebSite.APIServer.Application.Ranks;
using CnGalWebSite.APIServer.Application.Users;
using CnGalWebSite.APIServer.Controllers;
using CnGalWebSite.APIServer.Infrastructure;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Tables;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CnGalWebSite.UnitTest;

public class PlayedGameOverviewTests
{
    [TestCase(1, QuerySplittingBehavior.SingleQuery, 3)]
    [TestCase(32, QuerySplittingBehavior.SingleQuery, 3)]
    [TestCase(1, QuerySplittingBehavior.SplitQuery, 5)]
    [TestCase(32, QuerySplittingBehavior.SplitQuery, 5)]
    public async Task AnonymousOverviewReturnsPublicScoresAndVisibleRanks(
        int publicCount, QuerySplittingBehavior querySplitting, int expectedCommands)
    {
        await using var fixture = await OverviewFixture.CreateAsync(publicCount, querySplitting: querySplitting);
        var result = await fixture.Controller.GetPlayedGameOverview(fixture.EntryId);

        Assert.That(result.Value, Is.Not.Null);
        var model = result.Value!;
        Assert.That(model.IsDubbing, Is.False);
        Assert.That(model.GameTotalScores.TotalSocre, Is.EqualTo(8.5));
        Assert.That(model.GameReviewsScores.TotalSocre, Is.EqualTo(9));
        Assert.That(model.UserScores.Select(score => score.User.Id),
            Is.EquivalentTo(Enumerable.Range(0, publicCount).Select(index => $"public-{index}")));
        Assert.That(model.UserScores.Single(score => score.User.Id == "public-0").User.Ranks.Select(rank => rank.Name),
            Is.EqualTo(new[] { "Badge", "Lv 0" }));
        Assert.That(model.UserScores.All(score => score.Socres.TotalSocre == 8), Is.True);
        Assert.That(model.IsCurrentUserScoreExist, Is.False);
        TestContext.Out.WriteLine($"{publicCount} public scores ({querySplitting}): {fixture.Commands.Sql.Count} SQL commands");
        Assert.That(fixture.Commands.Sql, Has.Count.EqualTo(expectedCommands));
        Assert.That(fixture.Commands.Sql.Count(sql => sql.Contains("\"RankUsers\"")), Is.EqualTo(1));
        Assert.That(fixture.Commands.Sql.All(sql => sql.StartsWith("SELECT", StringComparison.Ordinal)), Is.True);
    }

    [Test]
    public async Task PublicReviewsIncludeCompleteScoresAndTextOnlyImpressions()
    {
        await using var fixture = await OverviewFixture.CreateAsync(1, includeTextReview: true);
        var result = await fixture.Controller.GetPlayedGameOverview(fixture.EntryId);
        var reviews = result.Value!.UserScores;
        Assert.That(reviews.Select(s => s.User.Id), Is.EquivalentTo(new[] { "public-0", "text-only" }));
        Assert.That(reviews.Single(s => s.Socres.IsScored).User.Id, Is.EqualTo("public-0"));
        Assert.That(reviews.Single(s => !s.Socres.IsScored).PlayImpressions, Is.EqualTo("短评"));
    }

    [Test]
    public async Task OverviewWithoutPublicScoresDoesNotQueryRanks()
    {
        await using var fixture = await OverviewFixture.CreateAsync(0);
        var result = await fixture.Controller.GetPlayedGameOverview(fixture.EntryId);

        Assert.That(result.Value!.UserScores, Is.Empty);
        Assert.That(fixture.Commands.Sql, Has.Count.EqualTo(2));
        Assert.That(fixture.Commands.Sql.Any(sql => sql.Contains("\"RankUsers\"")), Is.False);
    }

    [TestCase("private", 2, true)]
    [TestCase("public-0", 1, false)]
    public async Task CurrentUserSeesTheirPendingReviewWithoutLeakingIt(string userId, int expectedCount, bool pendingIsPublic)
    {
        await using var fixture = await OverviewFixture.CreateAsync(1, userId);
        var result = await fixture.Controller.GetPlayedGameOverview(fixture.EntryId);

        var model = result.Value!;
        Assert.That(model.CurrentUserId, Is.EqualTo(userId));
        Assert.That(model.IsCurrentUserScoreExist, Is.True);
        Assert.That(model.IsCurrentUserScorePublic, Is.EqualTo(pendingIsPublic));
        Assert.That(model.UserScores, Has.Count.EqualTo(expectedCount));
        var ownScore = model.UserScores.Single(score => score.User.Id == userId);
        Assert.That(ownScore.IsPersonalOnly, Is.EqualTo(userId == "private"));
        Assert.That(ownScore.PlayImpressions, Is.EqualTo("Pending"));
        Assert.That(ownScore.User.Ranks.Select(rank => rank.Name), Is.EqualTo(new[] { "Badge", "Lv 0" }));
        Assert.That(fixture.Commands.Sql.Count(sql => sql.Contains("\"RankUsers\"")), Is.EqualTo(1));
        Assert.That(fixture.Commands.Sql.All(sql => sql.StartsWith("SELECT", StringComparison.Ordinal)), Is.True);

        fixture.Controller.HttpContext.User = new ClaimsPrincipal();
        var anonymous = await fixture.Controller.GetPlayedGameOverview(fixture.EntryId);
        Assert.That(anonymous.Value!.UserScores.Single().PlayImpressions, Is.EqualTo("Published"));
    }

    [Test]
    public async Task OverviewReadsExistingRanksWithoutAssigningMissingRanks()
    {
        await using var fixture = await OverviewFixture.CreateAsync(1, seedRanks: false);
        var result = await fixture.Controller.GetPlayedGameOverview(fixture.EntryId);

        Assert.That(result.Value!.UserScores.Single().User.Ranks, Is.Empty);
        Assert.That(fixture.Commands.Sql, Has.Count.EqualTo(3));
        Assert.That(fixture.Commands.Sql.All(sql => sql.StartsWith("SELECT", StringComparison.Ordinal)), Is.True);
    }

    private sealed class OverviewFixture(SqliteConnection connection, AppDbContext db, SqlCounter commands) : IAsyncDisposable
    {
        public SqlCounter Commands { get; } = commands;
        public int EntryId { get; private init; }
        public PlayedGamesAPIController Controller { get; private init; } = null!;

        public static async Task<OverviewFixture> CreateAsync(
            int publicCount, string? currentUserId = null, bool seedRanks = true,
            QuerySplittingBehavior querySplitting = QuerySplittingBehavior.SingleQuery, bool includeTextReview = false)
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            var commands = new SqlCounter();
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(connection, options => options.UseQuerySplittingBehavior(querySplitting))
                .AddInterceptors(commands).Options;
            var db = new AppDbContext(options);
            try
            {
                await db.Database.EnsureCreatedAsync();
                var entry = new Entry { Name = "Game", Type = EntryType.Game, Tags = [new Tag { Name = "无配音" }] };
                var level = new Rank { Name = "Lv 0" };
                var badge = new Rank { Name = "Badge", Priority = 5, Type = RankType.Badge };
                var hidden = new Rank { Name = "Hidden", IsHidden = true };
                var personal = new Rank { Name = "Hidden by user" };
                for (var index = 0; index <= publicCount; index++)
                {
                    var isPublic = index < publicCount;
                    var id = isPublic ? $"public-{index}" : "private";
                    var user = new ApplicationUser { Id = id, UserName = id };
                    var score = new PlayedGame
                    {
                        ApplicationUser = user, ShowPublicly = isPublic, TotalSocre = 8,
                        MusicSocre = 8, ShowSocre = 8, ScriptSocre = 8, SystemSocre = 8, PaintSocre = 8, CVSocre = -1,
                        PlayImpressions = includeTextReview ? null : "Published",
                    };
                    entry.PlayedGames.Add(score);
                    db.Examines.Add(new Examine
                    {
                        ApplicationUser = user, PlayedGame = score, Operation = Operation.EditPlayedGameMain,
                        Context = isPublic
                            ? """{"PlayImpressions":"Pending","ShowPublicly":false}"""
                            : """{"PlayImpressions":"Pending","ShowPublicly":true}""",
                    });
                    if (seedRanks)
                    {
                        db.RankUsers.AddRange(
                            new RankUser { ApplicationUser = user, Rank = level },
                            new RankUser { ApplicationUser = user, Rank = badge },
                            new RankUser { ApplicationUser = user, Rank = hidden },
                            new RankUser { ApplicationUser = user, Rank = personal, IsHidden = true });
                    }
                }

                entry.PlayedGames.Add(new PlayedGame
                {
                    ApplicationUser = new ApplicationUser { Id = "library-only" }, ShowPublicly = true,
                });
                entry.PlayedGames.Add(new PlayedGame
                {
                    ApplicationUser = new ApplicationUser { Id = "incomplete" }, ShowPublicly = true,
                    TotalSocre = 8, PlayImpressions = "   ",
                });
                if (includeTextReview)
                {
                    entry.PlayedGames.Add(new PlayedGame
                    {
                        ApplicationUser = new ApplicationUser { Id = "text-only" }, ShowPublicly = true, PlayImpressions = "短评",
                    });
                }
                db.Add(entry);
                await db.SaveChangesAsync();
                db.GameScores.Add(new GameScoreTableModel { GameId = entry.Id, AllTotalSocre = 8.5, FilterTotalSocre = 9 });
                await db.SaveChangesAsync();
                db.ChangeTracker.Clear();
                commands.Sql.Clear();

                var registrations = new Dictionary<Type, object>();
                var services = new RealRecordServices(db, registrations);
                registrations[typeof(IAppHelper)] = services.Get<AppHelper>();
                registrations[typeof(IRankService)] = services.Get<RankService>();
                registrations[typeof(IUserService)] = services.Get<UserService>();
                registrations[typeof(IPlayedGameService)] = services.Get<PlayedGameService>();
                var controller = services.Get<PlayedGamesAPIController>();
                controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
                if (currentUserId is not null)
                {
                    controller.HttpContext.User = new ClaimsPrincipal(
                        new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, currentUserId)], "test"));
                }

                return new OverviewFixture(connection, db, commands) { EntryId = entry.Id, Controller = controller };
            }
            catch
            {
                await db.DisposeAsync();
                await connection.DisposeAsync();
                throw;
            }
        }

        public async ValueTask DisposeAsync()
        {
            await db.DisposeAsync();
            await connection.DisposeAsync();
        }
    }

    private sealed class SqlCounter : DbCommandInterceptor
    {
        public List<string> Sql { get; } = [];

        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
        {
            Sql.Add(command.CommandText);
            return ValueTask.FromResult(result);
        }

        public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
            DbCommand command, CommandEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            Sql.Add(command.CommandText);
            return ValueTask.FromResult(result);
        }
    }
}
