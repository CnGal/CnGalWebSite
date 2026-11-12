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
                {"contrastId":10,"currentId":20,"contrastModel":{"name":"Historical baseline"},"currentModel":{"name":"Historical target"}}
                """));
        });
        var result = await Query(http, storage, userCache).GetEditRecordComparisonAsync(type, 10, 20);
        Assert.Multiple(() =>
        {
            Assert.That(calls, Is.EqualTo(new[] { $"/api/{path}/GetContrastEditRecordViews/10/20" }));
            Assert.That(result.Success, Is.True, result.Error?.Message);
            Assert.That(result.Data?.ContrastId, Is.EqualTo(10));
            Assert.That(result.Data?.CurrentId, Is.EqualTo(20));
            Assert.That(result.Data?.Before.Texts.SelectMany(t => t.Informations).Any(f => f.DisplayValue == "Historical baseline"), Is.True);
            Assert.That(result.Data?.After.Texts.SelectMany(t => t.Informations).Any(f => f.DisplayValue == "Historical target"), Is.True);
        });
    }

    [TestCase(-1, 20)]
    [TestCase(10, 0)]
    [TestCase(20, 10)]
    public async Task InvalidRangesDoNotSendRequests(long before, long after)
    {
        using var storage = new MemoryCache(new MemoryCacheOptions());
        using var userCache = new ExamineUserCache(NullLogger<ExamineUserCache>.Instance);
        using var http = Client((_, _) => throw new AssertionException("Invalid selection should not reach HTTP"));
        var result = await Query(http, storage, userCache).GetEditRecordComparisonAsync(ExaminedNormalListModelType.Entry, before, after);
        Assert.That(result.Success, Is.False);
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
        Assert.ThrowsAsync<OperationCanceledException>(() => Query(http, storage, userCache)
            .GetEditRecordComparisonAsync(ExaminedNormalListModelType.Entry, 10, 20, cancellation.Token));
    }

    [Test]
    public void EntryProjectionPreservesSpecialGroupsAndDuplicates()
    {
        var result = EditRecordContentMapper.Map(new EntryIndexViewModel
        {
            Name = "历史词条", MainPage = "<p>历史正文</p>", Audio = [new() { Name = "音频", Url = "https://example.com/audio" }],
            ProductionGroups = [new() { Id = 123, DisplayName = "制作组甲" }],
            Publishers = [new() { Id = 456, DisplayName = "发行商乙" }],
            Information = [new() { Name = "同名字段", Value = "值一" }, new() { Name = "同名字段", Value = "值二" }],
            Staffs = [new() { Modifier = "制作", StaffList = [new() { Modifier = "编剧", Names = [new() { DisplayName = "作者甲" }, new() { DisplayName = "作者乙" }] }] }],
            Releases = [new() { Name = "首发", Engine = "测试引擎", TimeNote = "待定", Link = "https://example.com/release" }],
            Booking = new() { Open = true, Goals = [new() { Name = "预约目标", Target = 100 }] },
            WebsiteAddInfor = new() { Html = "<script>untrusted()</script>", Images = [new() { Note = "官网图片", Url = "https://example.com/website.png" }] },
            Pictures = [new() { Modifier = "CG", Pictures = [new() { Note = "图片备注", Url = "https://example.com/cg.png" }] }],
            Tags = [new() { Id = 3, Name = "历史标签" }]
        });
        Assert.Multiple(() =>
        {
            Assert.That(result.Texts.Single(t => t.Modifier == "附加信息").Informations.Select(i => i.DisplayValue), Is.EqualTo(new[] { "值一", "值二" }));
            Assert.That(result.Texts.Single(t => t.Modifier == "Staff · 制作").Informations.Select(i => i.DisplayValue), Is.EqualTo(new[] { "作者甲", "作者乙" }));
            Assert.That(result.Texts.Single(t => t.Modifier == "发行信息 · 首发").Informations.Any(i => i.DisplayValue == "测试引擎"), Is.True);
            Assert.That(result.Texts.Any(t => t.Modifier == "预约"), Is.True);
            Assert.That(result.Pictures.Select(p => p.Note), Does.Contain("CG · 图片备注").And.Contain("官网 · 官网图片"));
            Assert.That(result.Texts.Single(t => t.Modifier == "制作组").Informations.Single().DisplayValue, Is.EqualTo("制作组甲"));
            Assert.That(result.Texts.Single(t => t.Modifier == "发行商").Informations.Single().DisplayValue, Is.EqualTo("发行商乙"));
            Assert.That(result.Audio.Single().Name, Is.EqualTo("音频"));
            Assert.That(result.Relevances.Single().tag.Name, Is.EqualTo("历史标签"));
            Assert.That(result.MainPage, Is.EqualTo("<p>历史正文</p>"));
            Assert.That(result.MainPage, Does.Not.Contain("untrusted"));
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
