using System.Net;
using System.Text;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Lotteries;
using CnGalWebSite.DataModel.ViewModel.Questionnaires;
using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Commands;
using CnGalWebSite.SDK.MainSite.Infrastructure;
using CnGalWebSite.SDK.MainSite.Models;
using CnGalWebSite.SDK.MainSite.Queries;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace CnGalWebSite.UnitTest;

[TestFixture]
public class MainSiteSdkRegressionTests
{
    [TestCase(true)]
    [TestCase(false)]
    public async Task ConcurrentCommentDoesNotFailEntryReadOrRefillInvalidatedCache(bool writeSucceeded)
    {
        using var storage = new MemoryCache(new MemoryCacheOptions());
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var reads = 0;
        using var client = CreateClient(async (request, token) =>
        {
            if (request.Method == HttpMethod.Post)
                return Json(HttpStatusCode.OK, writeSucceeded
                    ? """{"successful":true}""" : """{"successful":false,"error":"Rejected"}""");

            if (Interlocked.Increment(ref reads) == 1)
            {
                started.SetResult();
                await release.Task.WaitAsync(token);
                return Json(HttpStatusCode.OK, """{"id":42,"name":"Before write"}""");
            }

            return Json(HttpStatusCode.OK, """{"id":42,"name":"After write"}""");
        });
        var entries = new EntryQueryService(client, new(storage, new User()), NullLogger<EntryQueryService>.Instance);
        var comments = new CommentCommandService(client,
            new(storage, new User { State = CurrentIdentityState.Authenticated }), NullLogger<CommentCommandService>.Instance);

        var pending = entries.GetEntryDetailAsync(42);
        await started.Task.WaitAsync(TimeSpan.FromSeconds(5));
        try
        {
            var write = await comments.PublishCommentAsync(CommentType.CommentEntries, "99", "Unrelated comment");
            Assert.That(write.Success, Is.EqualTo(writeSucceeded));
        }
        finally { release.TrySetResult(); }

        var original = await pending;
        var fresh = await entries.GetEntryDetailAsync(42);
        var cached = await entries.GetEntryDetailAsync(42);
        Assert.Multiple(() =>
        {
            Assert.That(original.Success, Is.True, original.Error?.Message);
            Assert.That(original.Data?.Name, Is.EqualTo("Before write"));
            Assert.That(fresh.Data?.Name, Is.EqualTo("After write"));
            Assert.That(cached.Data?.Name, Is.EqualTo("After write"));
            Assert.That(reads, Is.EqualTo(2), "Old in-flight data must not repopulate the cache; subsequent reads should cache fresh data.");
        });
    }

    [Test]
    public async Task InvalidationPreservesAnExistingQueryFailure()
    {
        using var storage = new MemoryCache(new MemoryCacheOptions());
        var cache = new PublicDataCache(storage, new User()).ForSource(NewSource());
        var scope = await cache.BeginQueryAsync(default);
        cache.Invalidate();
        var result = cache.CompleteResult(SdkResult<string>.Fail("NOT_FOUND", "Missing", 404), scope);
        Assert.That(result.Error?.StatusCode, Is.EqualTo(404));
    }

    [TestCase(CurrentIdentityState.Authenticated)]
    [TestCase(CurrentIdentityState.Unknown)]
    public async Task NonAnonymousReadsNeverPopulateThePublicCache(CurrentIdentityState state)
    {
        using var storage = new MemoryCache(new MemoryCacheOptions());
        var user = new User { State = state };
        var cache = new PublicDataCache(storage, user).ForSource(NewSource());
        var scope = await cache.BeginQueryAsync(default);
        cache.Set("entry", "private data", TimeSpan.FromMinutes(1), scope);
        user.State = CurrentIdentityState.Anonymous;
        var anonymousScope = await cache.BeginQueryAsync(default);
        Assert.That(cache.TryGetValue<string>("entry", out _, anonymousScope), Is.False);
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task IdentityChangesStillRejectResultsAndCacheWrites(bool changeVersion)
    {
        using var storage = new MemoryCache(new MemoryCacheOptions());
        var user = new User();
        var cache = new PublicDataCache(storage, user).ForSource(NewSource());
        var scope = await cache.BeginQueryAsync(default);
        if (changeVersion) user.IdentityVersion++;
        else user.State = CurrentIdentityState.Authenticated;
        cache.Set("entry", "old identity", TimeSpan.FromMinutes(1), scope);

        Assert.Multiple(() =>
        {
            Assert.That(cache.Complete("old identity", scope).Error?.Code, Is.EqualTo("SDK_IDENTITY_CHANGED"));
            Assert.That(cache.TryGetValue<string>("entry", out _, scope), Is.False);
        });
    }

    [TestCase(400)]
    [TestCase(409)]
    [TestCase(422)]
    public async Task FormValidationRetainsBusinessMessageAndHttpStatus(int status)
    {
        using var storage = new MemoryCache(new MemoryCacheOptions());
        var sends = 0;
        using var client = CreateClient((_, _) =>
        {
            sends++;
            return Task.FromResult(Json((HttpStatusCode)status, """{"successful":false,"error":"问卷名称已存在"}"""));
        });
        var command = new AdminCommandService(client, new(storage, new User()), NullLogger<AdminCommandService>.Instance);
        var result = await command.CreateQuestionnaireAsync(new EditQuestionnaireModel());
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Error?.Message, Is.EqualTo("问卷名称已存在"));
            Assert.That(result.Error?.StatusCode, Is.EqualTo(status));
            Assert.That(sends, Is.EqualTo(1), "Validation failures must not replay writes.");
        });
    }

    [TestCase(400, "text/html", "<html>Internal proxy details</html>")]
    [TestCase(400, "application/json", "invalid json")]
    [TestCase(400, "application/json", "{\"error\":\"Unrecognized error contract\"}")]
    [TestCase(400, "application/json", "{\"successful\":true,\"error\":\"Wrong status\"}")]
    [TestCase(400, "application/json", "{\"successful\":false,\"error\":\" \"}")]
    [TestCase(500, "application/json", "{\"successful\":false,\"error\":\"Internal server details\"}")]
    [TestCase(401, "application/json", "{}")]
    public async Task UnrecognizedHttpFailuresRemainGeneric(int status, string mediaType, string body)
    {
        using var storage = new MemoryCache(new MemoryCacheOptions());
        using var client = CreateClient((_, _) => Task.FromResult(new HttpResponseMessage((HttpStatusCode)status)
        {
            Content = new StringContent(body, Encoding.UTF8, mediaType)
        }));
        var command = new AdminCommandService(client, new(storage, new User()), NullLogger<AdminCommandService>.Instance);
        var result = await command.CreateQuestionnaireAsync(new EditQuestionnaireModel());
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Error?.Message, Is.EqualTo("接口请求失败"));
            Assert.That(result.Error?.StatusCode, Is.EqualTo(status));
        });
    }

    [Test]
    public async Task SuccessfulHttpBusinessRejectionsStillUseTheOriginalContract()
    {
        using var storage = new MemoryCache(new MemoryCacheOptions());
        using var client = CreateClient((_, _) => Task.FromResult(Json(HttpStatusCode.OK,
            """{"successful":false,"error":"问卷名称已存在"}""")));
        var command = new AdminCommandService(client, new(storage, new User()), NullLogger<AdminCommandService>.Instance);
        var result = await command.CreateQuestionnaireAsync(new EditQuestionnaireModel());
        Assert.Multiple(() =>
        {
            Assert.That(result.Error?.Code, Is.EqualTo("ADMIN_CREATE_QUESTIONNAIRE_FAILED"));
            Assert.That(result.Error?.Message, Is.EqualTo("问卷名称已存在"));
        });
    }

    [Test]
    public async Task RawCommandResponsesRetainValidationStatus()
    {
        using var storage = new MemoryCache(new MemoryCacheOptions());
        using var client = CreateClient((_, _) => Task.FromResult(Json(HttpStatusCode.BadRequest,
            """{"Successful":false,"Error":"抽奖名称不能为空"}""")));
        var command = new LotteryCommandService(client, new(storage, new User()), NullLogger<LotteryCommandService>.Instance);
        var result = await command.CreateLotteryAsync(new EditLotteryModel());
        Assert.Multiple(() =>
        {
            Assert.That(result.Error?.Message, Is.EqualTo("抽奖名称不能为空"));
            Assert.That(result.Error?.StatusCode, Is.EqualTo(400));
        });
    }

    [TestCase(200, true)]
    [TestCase(400, false)]
    public async Task RawCommandsRequireHttpSuccessEvenWhenTheBodyClaimsSuccess(int status, bool succeeds)
    {
        using var storage = new MemoryCache(new MemoryCacheOptions());
        using var client = CreateClient((_, _) => Task.FromResult(Json((HttpStatusCode)status,
            """{"successful":true,"error":"123"}""")));
        var command = new LotteryCommandService(client, new(storage, new User()), NullLogger<LotteryCommandService>.Instance);
        var result = await command.CreateLotteryAsync(new EditLotteryModel());
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.EqualTo(succeeds));
            if (succeeds) Assert.That(result.Data, Is.EqualTo(123));
            else Assert.That(result.Error?.StatusCode, Is.EqualTo(status));
        });
    }

    [Test]
    public void CallerCancellationStillPropagates()
    {
        using var storage = new MemoryCache(new MemoryCacheOptions());
        using var cancellation = new CancellationTokenSource();
        using var client = CreateClient((_, _) =>
        {
            cancellation.Cancel();
            throw new OperationCanceledException(cancellation.Token);
        });
        var command = new AdminCommandService(client, new(storage, new User()), NullLogger<AdminCommandService>.Instance);
        Assert.ThrowsAsync<OperationCanceledException>(() => command.CreateQuestionnaireAsync(new EditQuestionnaireModel(), cancellation.Token));
    }

    private static Uri NewSource() => new($"https://{Guid.NewGuid():N}.invalid/");
    private static HttpClient CreateClient(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> send)
        => new(new Handler(send)) { BaseAddress = NewSource() };
    private static HttpResponseMessage Json(HttpStatusCode status, string body)
        => new(status) { Content = new StringContent(body, Encoding.UTF8, "application/json") };

    private sealed class Handler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> send) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => send(request, cancellationToken);
    }

    private sealed class User : ICurrentUserAccessor
    {
        public CurrentIdentityState State { get; set; } = CurrentIdentityState.Anonymous;
        public long IdentityVersion { get; set; }
        public CurrentIdentityState GetIdentityState() => State;
        public string? GetCurrentUserId() => State == CurrentIdentityState.Authenticated ? "test-user" : null;
    }
}
