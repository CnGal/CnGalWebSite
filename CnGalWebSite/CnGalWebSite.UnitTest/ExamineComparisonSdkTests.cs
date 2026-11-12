using System.Net;
using System.Text;
using CnGalWebSite.DataModel.ViewModel;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Infrastructure;
using CnGalWebSite.SDK.MainSite.Models;
using CnGalWebSite.SDK.MainSite.Queries;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace CnGalWebSite.UnitTest;

[TestFixture]
public class ExamineComparisonSdkTests
{
    [TestCase(ExaminedNormalListModelType.Entry, "entries")]
    [TestCase(ExaminedNormalListModelType.Article, "articles")]
    [TestCase(ExaminedNormalListModelType.Tag, "tags")]
    [TestCase(ExaminedNormalListModelType.Periphery, "peripheries")]
    [TestCase(ExaminedNormalListModelType.Video, "videos")]
    public async Task EachTypeUsesItsRealHistoryEndpoint(ExaminedNormalListModelType type, string path)
    {
        var calls = new List<string>();
        using var storage = new MemoryCache(new MemoryCacheOptions());
        using var userCache = new ExamineUserCache(NullLogger<ExamineUserCache>.Instance);
        using var http = Client((request, _) =>
        {
            calls.Add(request.RequestUri!.AbsolutePath);
            return Task.FromResult(Json(HttpStatusCode.OK, """
                {"contrastId":10,"currentId":20,"before":{"items":[{"key":"name","group":"主要信息","kind":1,"label":"名称","value":"Historical baseline","displayValue":"Historical baseline"}]},"after":{"items":[{"key":"name","group":"主要信息","kind":1,"label":"名称","value":"Historical target","displayValue":"Historical target"}]}}
                """));
        });
        var result = await Query(http, storage, userCache).GetEditRecordComparisonAsync(type, 10, 20);
        Assert.Multiple(() =>
        {
            Assert.That(calls, Is.EqualTo(new[] { $"/api/{path}/GetContrastEditRecordViews/10/20" }));
            Assert.That(result.Success, Is.True, result.Error?.Message);
            Assert.That(result.Data?.ContrastId, Is.EqualTo(10));
            Assert.That(result.Data?.CurrentId, Is.EqualTo(20));
            Assert.That(result.Data?.Before.Items.Any(f => f.DisplayValue == "Historical baseline"), Is.True);
            Assert.That(result.Data?.After.Items.Any(f => f.DisplayValue == "Historical target"), Is.True);
        });
    }

    [TestCase(-1, 20)]
    [TestCase(10, 0)]
    [TestCase(20, 10)]
    public async Task InvalidRangesDoNotSendRequests(long before, long after)
    {
        var requestCount = 0;
        using var storage = new MemoryCache(new MemoryCacheOptions());
        using var userCache = new ExamineUserCache(NullLogger<ExamineUserCache>.Instance);
        using var http = Client((_, _) =>
        {
            requestCount++;
            return Task.FromResult(Json(HttpStatusCode.OK, "{}"));
        });
        var result = await Query(http, storage, userCache).GetEditRecordComparisonAsync(ExaminedNormalListModelType.Entry, before, after);
        Assert.Multiple(() =>
        {
            Assert.That(requestCount, Is.Zero);
            Assert.That(result.Success, Is.False);
            Assert.That(result.Error?.Code, Is.EqualTo("EXAMINE_COMPARISON_RANGE"));
        });
    }

    [TestCase(ExaminedNormalListModelType.User)]
    [TestCase(ExaminedNormalListModelType.Comment)]
    public async Task UnsupportedTypesDoNotSendRequests(ExaminedNormalListModelType type)
    {
        using var storage = new MemoryCache(new MemoryCacheOptions());
        using var userCache = new ExamineUserCache(NullLogger<ExamineUserCache>.Instance);
        using var http = Client((_, _) => throw new AssertionException("Unsupported type should not reach HTTP"));
        var result = await Query(http, storage, userCache).GetEditRecordComparisonAsync(type, 10, 20);
        Assert.That(result.Error?.Code, Is.EqualTo("EXAMINE_COMPARISON_TYPE"));
    }

    [TestCase(404)]
    [TestCase(500)]
    public async Task MissingOrCrossObjectVersionsAndServerFailuresAreReported(int status)
    {
        using var storage = new MemoryCache(new MemoryCacheOptions());
        using var userCache = new ExamineUserCache(NullLogger<ExamineUserCache>.Instance);
        using var http = Client((_, _) => Task.FromResult(Json((HttpStatusCode)status, "{}")));
        var result = await Query(http, storage, userCache).GetEditRecordComparisonAsync(ExaminedNormalListModelType.Entry, 10, 20);
        Assert.That(result.Error?.StatusCode, Is.EqualTo(status));
    }

    [Test]
    public void CancellationIsPropagated()
    {
        using var storage = new MemoryCache(new MemoryCacheOptions());
        using var userCache = new ExamineUserCache(NullLogger<ExamineUserCache>.Instance);
        using var cancellation = new CancellationTokenSource();
        using var http = Client(async (_, token) => { cancellation.Cancel(); await Task.Delay(100, token); return Json(HttpStatusCode.OK, "{}"); });
        Assert.CatchAsync<OperationCanceledException>(() => Query(http, storage, userCache)
            .GetEditRecordComparisonAsync(ExaminedNormalListModelType.Entry, 10, 20, cancellation.Token));
    }

    [Test]
    public void ImageReplacementIsReportedAsModifiedInsteadOfRemovedAndAdded()
    {
        var before = new ExaminePreDataModel { Items = [
            new() { Key = "picture/old", Group = "图片", Kind = RecordPreviewKind.Image, Label = "主视觉",
                Value = "https://media.example/old.png", DisplayValue = "https://media.example/old.png", Url = "https://media.example/old.png" }
        ] };
        var after = new ExaminePreDataModel { Items = [
            new() { Key = "picture/new", Group = "图片", Kind = RecordPreviewKind.Image, Label = "主视觉",
                Value = "https://media.example/new.png", DisplayValue = "https://media.example/new.png", Url = "https://media.example/new.png" }
        ] };

        var diff = RecordDiffBuilder.Build(before, after);

        Assert.Multiple(() =>
        {
            Assert.That(diff.Items, Has.Count.EqualTo(1));
            Assert.That(diff.Items[0].Change, Is.EqualTo(RecordChangeKind.Modified));
            Assert.That(diff.Items[0].Before?.Value, Is.EqualTo("https://media.example/old.png"));
            Assert.That(diff.Items[0].After?.Value, Is.EqualTo("https://media.example/new.png"));
        });
    }

    [Test]
    public void DiffKeepsDuplicateRowsAndOnlyReportsChangedValues()
    {
        var before = new ExaminePreDataModel { Items = [
            new() { Key = "text/1", Group = "信息", Kind = RecordPreviewKind.Text, Label = "同名字段", Value = "值一", DisplayValue = "值一" },
            new() { Key = "text/2", Group = "信息", Kind = RecordPreviewKind.Text, Label = "同名字段", Value = "值二", DisplayValue = "值二" },
            new() { Key = "stable", Group = "信息", Kind = RecordPreviewKind.Text, Label = "固定字段", Value = "相同", DisplayValue = "相同" },
            new() { Key = "property", Group = "信息", Kind = RecordPreviewKind.Text, Label = "附加字段", Value = "值", DisplayValue = "值",
                Properties = [new() { Key = "note", Label = "备注", Value = "旧", DisplayValue = "旧" }] }
        ] };
        var after = new ExaminePreDataModel { Items = [
            new() { Key = "text/1", Group = "信息", Kind = RecordPreviewKind.Text, Label = "同名字段", Value = "值一", DisplayValue = "值一" },
            new() { Key = "text/2", Group = "信息", Kind = RecordPreviewKind.Text, Label = "同名字段", Value = "值二", DisplayValue = "值二" },
            new() { Key = "stable", Group = "信息", Kind = RecordPreviewKind.Text, Label = "固定字段", Value = "相同", DisplayValue = "相同" },
            new() { Key = "property", Group = "信息", Kind = RecordPreviewKind.Text, Label = "附加字段", Value = "值", DisplayValue = "值",
                Properties = [new() { Key = "note", Label = "备注", Value = "新", DisplayValue = "新" }] }
        ] };

        var diff = RecordDiffBuilder.Build(before, after);

        Assert.Multiple(() =>
        {
            Assert.That(diff.Items.Count, Is.EqualTo(4));
            Assert.That(diff.Items.Count(i => i.Changed), Is.EqualTo(1));
            Assert.That(diff.Items.Single(i => i.Changed).Item.Key, Is.EqualTo("property"));
            Assert.That(diff.Items.Where(i => !i.Changed).Select(i => i.Item.Key), Does.Contain("text/1").And.Contain("text/2"));
        });
    }
    private static ExamineQueryService Query(HttpClient http, MemoryCache storage, ExamineUserCache users)
        => new(http, new PublicDataCache(storage, new User()), NullLogger<ExamineQueryService>.Instance, users, new User());
    private static HttpClient Client(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> send)
        => new(new Handler(send)) { BaseAddress = new Uri("https://test.invalid/") };
    private static HttpResponseMessage Json(HttpStatusCode status, string body)
        => new(status) { Content = new StringContent(body, Encoding.UTF8, "application/json") };
    private sealed class Handler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> send) : HttpMessageHandler
    { protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token) => send(request, token); }
    private sealed class User : ICurrentUserAccessor
    { public string? GetCurrentUserId() => null; public CurrentIdentityState GetIdentityState() => CurrentIdentityState.Anonymous; }
}
