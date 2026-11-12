using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using CnGalWebSite.Core.Models;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Home;
using CnGalWebSite.DataModel.ViewModel.Search;
using CnGalWebSite.MainSite.Shared.Components.DesignSystem;
using CnGalWebSite.MainSite.Shared.Components.Features.Examine;
using CnGalWebSite.MainSite.Shared.Pages.Examine;
using CnGalWebSite.MainSite.Shared.Services;
using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using NUnit.Framework;

namespace CnGalWebSite.UnitTest;

#pragma warning disable BL0006
[TestFixture]
public class ExaminePresentationTests
{
    public static IEnumerable<int> Masks => Enumerable.Range(0, 64);

    [TestCaseSource(nameof(Masks))]
    public async Task ContentCombinationsShowOnlyChangesAndKeepDuplicateItems(int mask)
    {
        var data = Content(mask);
        using var services = Services();
        await using var renderer = new HtmlRenderer(services, services.GetRequiredService<ILoggerFactory>());
        foreach (var reversed in new[] { false, true })
        {
            var html = await renderer.Dispatcher.InvokeAsync(async () =>
            {
                var root = await renderer.RenderComponentAsync<RecordContentComparison>(ParameterView.FromDictionary(new Dictionary<string, object?>
                {
                    ["Before"] = reversed ? data : null, ["After"] = reversed ? null : data,
                }));
                return WebUtility.HtmlDecode(root.ToHtmlString());
            });
            Assert.That(html, Does.Not.Contain("已删除"));
            if (mask == 0) Assert.That(html, Does.Contain("无变化"));
            else
            {
                Assert.That(html.Split("record-comparison__row--").Length - 1, Is.EqualTo(ItemCount(mask)));
                if ((mask & 2) != 0) Assert.That(html, Does.Contain("重复字段").And.Contain("第一行").And.Contain("第二行"));
            }
        }
    }
    [Test]
    public async Task MediaLinksAndStringsAreRenderedWithoutExecutableHtml()
    {
        var data = Content(63);
        data.Items.First(i => i.Kind == RecordPreviewKind.Text).DisplayValue = "<script>alert(1)</script>";
        var link = data.Items.First(i => i.Kind == RecordPreviewKind.Link);
        link.Url = "javascript:alert(1)";
        link.Value = "javascript:alert(1)";
        link.DisplayValue = "unsafe-link";
        using var services = Services();
        await using var renderer = new HtmlRenderer(services, services.GetRequiredService<ILoggerFactory>());
        var html = await renderer.Dispatcher.InvokeAsync(async () =>
            (await renderer.RenderComponentAsync<RecordContentComparison>(ParameterView.FromDictionary(new Dictionary<string, object?>
            {
                ["Before"] = null,
                ["After"] = data,
            }))).ToHtmlString());
        Assert.That(html, Does.Contain("&lt;script&gt;").And.Not.Contain("href=\"javascript:"));
        Assert.That(html, Does.Contain("preload=\"none\"").And.Not.Contain("autoplay"));
        Assert.That(html, Does.Contain("https://media.example/picture.png").And.Contain("target=\"_blank\""));
    }
    [Test]
    public async Task TextComparisonHighlightsRemovedAndAddedParts()
    {
        using var services = Services();
        await using var renderer = new HtmlRenderer(services, services.GetRequiredService<ILoggerFactory>());
        var before = new ExaminePreDataModel { Items = [
            new() { Key = "text/1", Group = "主要信息", Kind = RecordPreviewKind.Text, Label = "简介", Value = "旧内容", DisplayValue = "旧内容" }
        ] };
        var after = new ExaminePreDataModel { Items = [
            new() { Key = "text/1", Group = "主要信息", Kind = RecordPreviewKind.Text, Label = "简介", Value = "新内容", DisplayValue = "新内容" }
        ] };

        var html = await renderer.Dispatcher.InvokeAsync(async () =>
            WebUtility.HtmlDecode((await renderer.RenderComponentAsync<RecordContentComparison>(
                ParameterView.FromDictionary(new Dictionary<string, object?> { ["Before"] = before, ["After"] = after }))).ToHtmlString()));

        Assert.That(html, Does.Contain("record-diff-text__change--removed").And.Contain("record-diff-text__change--added"));
    }

    [Test]
    public async Task BodyComparisonUsesInlineHtmlHighlighting()
    {
        using var services = Services();
        await using var renderer = new HtmlRenderer(services, services.GetRequiredService<ILoggerFactory>());
        var before = new ExaminePreDataModel { Items = [
            new() { Key = "body", Group = "正文", Kind = RecordPreviewKind.Body, Label = "正文", Value = "旧内容", Html = "<p>旧内容</p>" }
        ] };
        var after = new ExaminePreDataModel { Items = [
            new() { Key = "body", Group = "正文", Kind = RecordPreviewKind.Body, Label = "正文", Value = "新内容", Html = "<p>新内容</p>" }
        ] };

        var html = await renderer.Dispatcher.InvokeAsync(async () =>
            WebUtility.HtmlDecode((await renderer.RenderComponentAsync<RecordContentComparison>(
                ParameterView.FromDictionary(new Dictionary<string, object?> { ["Before"] = before, ["After"] = after }))).ToHtmlString()));

        Assert.That(html, Does.Contain("record-value__body-diff--before").And.Contain("record-value__body-diff--after"));
        Assert.That(html, Does.Contain("<del").And.Contain("<ins"));
    }

    [Test]
    public async Task AddedAndRemovedItemsDoNotUseInlineDiffHighlighting()
    {
        using var services = Services();
        await using var renderer = new HtmlRenderer(services, services.GetRequiredService<ILoggerFactory>());
        var textItem = new RecordPreviewItem
        {
            Key = "text/1", Group = "主要信息", Kind = RecordPreviewKind.Text,
            Label = "简介", Value = "新增内容", DisplayValue = "新增内容"
        };

        var addedHtml = await renderer.Dispatcher.InvokeAsync(async () =>
            WebUtility.HtmlDecode((await renderer.RenderComponentAsync<RecordContentComparison>(
                ParameterView.FromDictionary(new Dictionary<string, object?>
                {
                    ["Before"] = new ExaminePreDataModel { Items = [] },
                    ["After"] = new ExaminePreDataModel { Items = [textItem] }
                }))).ToHtmlString()));
        var removedHtml = await renderer.Dispatcher.InvokeAsync(async () =>
            WebUtility.HtmlDecode((await renderer.RenderComponentAsync<RecordContentComparison>(
                ParameterView.FromDictionary(new Dictionary<string, object?>
                {
                    ["Before"] = new ExaminePreDataModel { Items = [textItem] },
                    ["After"] = new ExaminePreDataModel { Items = [] }
                }))).ToHtmlString()));

        Assert.Multiple(() =>
        {
            Assert.That(addedHtml, Does.Contain("record-comparison__row--added"));
            Assert.That(addedHtml, Does.Not.Contain("record-diff-text__change--added"));
            Assert.That(removedHtml, Does.Contain("record-comparison__row--removed"));
            Assert.That(removedHtml, Does.Not.Contain("record-diff-text__change--removed"));
        });
    }

    [Test]
    public async Task ReviewContentDefaultsToChangeSummaryTab()
    {
        using var services = Services();
        await using var renderer = new HtmlRenderer(services, services.GetRequiredService<ILoggerFactory>());
        var model = Review(7);
        model.EditOverview = "<p>description-only</p>";
        var html = await renderer.Dispatcher.InvokeAsync(async () =>
            (await renderer.RenderComponentAsync<ReviewContent>(ParameterView.FromDictionary(new Dictionary<string, object?> { ["Model"] = model }))).ToHtmlString());

        Assert.That(html, Does.Contain("变更摘要").And.Contain("原始数据"));
        Assert.That(html, Does.Contain("id=\"review-panel-summary\"").And.Not.Contain("id=\"review-panel-description\""));
        Assert.That(WebUtility.HtmlDecode(html), Does.Not.Contain("description-only"));
    }

    [Test]
    public async Task EmptyRecordNotesDoNotRenderPlaceholder()
    {
        using var services = Services();
        await using var renderer = new HtmlRenderer(services, services.GetRequiredService<ILoggerFactory>());
        var html = await renderer.Dispatcher.InvokeAsync(async () =>
            (await renderer.RenderComponentAsync<RecordNotes>(ParameterView.FromDictionary(new Dictionary<string, object?>()))).ToHtmlString());

        Assert.That(WebUtility.HtmlDecode(html), Does.Not.Contain("此记录未附加原始数据"));
    }
    [TestCase(null, null, false)]
    [TestCase(null, 0L, true)]
    [TestCase(null, 2L, true)]
    [TestCase("record", 2L, false)]
    [TestCase("compare", null, true)]
    [TestCase("COMPARE", null, true)]
    public void LegacyLinksAndExplicitModeAreCompatible(string? mode, long? currentId, bool compare)
        => Assert.That(RecordPresentation.IsCompareMode(mode, currentId), Is.EqualTo(compare));

    public static IEnumerable<TestCaseData> ReviewStates =>
        from bool? passed in new bool?[] { null, true, false }
        from admin in new[] { false, true }
        from dependency in new[] { -1L, 0L, 42L }
        select new TestCaseData(passed, admin, dependency);

    [TestCaseSource(nameof(ReviewStates))]
    public async Task ReviewStateAndPermissionMatrix(bool? passed, bool admin, long dependency)
    {
        using var services = Services();
        await using var renderer = new HtmlRenderer(services, services.GetRequiredService<ILoggerFactory>());
        var model = Review(7, passed, admin, dependency);
        var html = await renderer.Dispatcher.InvokeAsync(async () => WebUtility.HtmlDecode(
            (await renderer.RenderComponentAsync<ExamineDetailPanel>(ParameterView.FromDictionary(new Dictionary<string, object?> { ["Model"] = model }))).ToHtmlString()));
        Assert.That(html.Contains("aria-label=\"审核操作\""), Is.EqualTo(admin && passed is null));
        Assert.That(RecordPresentation.CanProcess(model), Is.EqualTo(admin && passed is null && dependency <= 0));
        if (admin && passed is null && dependency > 0)
        {
            Assert.Multiple(() =>
            {
                Assert.That(ButtonIsDisabled(html, "通过"), Is.True);
                Assert.That(ButtonIsDisabled(html, "驳回"), Is.True);
                Assert.That(html, Does.Contain("查看前置审核 #42"));
            });
        }
        if (passed is null) Assert.That(html, Does.Contain("提交内容").And.Contain("当前内容"));
        else Assert.That(html, Does.Contain(passed == false ? "拟提交内容" : "修改后").And.Not.Contain("编辑后"));
    }

    [Test]
    public async Task RefreshDependencyReloadsUncachedRecord()
    {
        var refreshCalls = 0;
        var refreshed = Review(7, admin: true, dependency: 0);
        var query = Proxy<IExamineQueryService>((method, _) => method.Name switch
        {
            "RefreshExamineDetailAsync" => Refresh(),
            _ => throw new InvalidOperationException(method.Name)
        });
        Task<SdkResult<ExamineViewModel>> Refresh()
        {
            refreshCalls++;
            return Task.FromResult(SdkResult<ExamineViewModel>.Ok(refreshed));
        }

        using var services = Services(query);
        await using var renderer = new TestRenderer(services);
        var root = await renderer.Mount<ExamineDetailPanel>(new() { ["Model"] = Review(7, admin: true, dependency: 42) });
        var refresh = renderer.Children<CgButton>(root).Single(button => renderer.AllText(button).Contains("刷新状态"));
        await renderer.Click(refresh);

        Assert.Multiple(() =>
        {
            Assert.That(refreshCalls, Is.EqualTo(1));
            Assert.That(renderer.AllText(root), Does.Not.Contain("请先处理前置审核").And.Contain("审核操作"));
        });
    }

    [TestCase(Operation.PubulishComment)]
    [TestCase(Operation.RequestUserCertification)]
    [TestCase(Operation.EditPlayedGameMain)]
    [TestCase(Operation.EditFavoriteFolderMain)]
    public async Task SingleSubmissionsAreNotRepeated(Operation operation)
    {
        using var services = Services();
        await using var renderer = new HtmlRenderer(services, services.GetRequiredService<ILoggerFactory>());
        var model = Review(7, true);
        model.Operation = operation;
        model.SubmissionOnly = true;
        model.BeforeModel = model.AfterModel = new() { Items = [new() { Key = "body", Group = "正文", Kind = RecordPreviewKind.Body, Label = "正文", Value = "<p>unique-submission</p>", Html = "<p>unique-submission</p>" }] };
        var html = await renderer.Dispatcher.InvokeAsync(async () =>
            (await renderer.RenderComponentAsync<ReviewContent>(ParameterView.FromDictionary(new Dictionary<string, object?> { ["Model"] = model }))).ToHtmlString());
        Assert.That(html.Split("unique-submission").Length - 1, Is.EqualTo(1));
    }

    [Test]
    public void ObjectLinksUseStringUserIdsAndDoNotInventTargets()
    {
        Assert.That(RecordPresentation.ObjectUrl(ExaminedNormalListModelType.User, 0, "user/a"), Is.EqualTo("/space/index/user%2Fa"));
        Assert.That(RecordPresentation.ObjectUrl(ExaminedNormalListModelType.User, 42), Is.Null);
        Assert.That(RecordPresentation.ObjectUrl(ExaminedNormalListModelType.Comment, 42), Is.Null);
        Assert.That(RecordPresentation.ObjectUrl(ExaminedNormalListModelType.Entry, 0), Is.Null);
        Assert.That(RecordPresentation.SupportsHistory(ExaminedNormalListModelType.FavoriteFolder), Is.False);
        Assert.That(RecordPresentation.SupportsHistory(ExaminedNormalListModelType.Video), Is.True);
    }

    [Test]
    public async Task DoubleClickSubmitsOnceAndSuccessCompletesOnce()
    {
        var writes = 0;
        var completed = 0;
        var pending = new TaskCompletionSource<SdkResult<bool>>(TaskCreationOptions.RunContinuationsAsynchronously);
        var command = Proxy<IExamineCommandService>((_, _) => { writes++; return pending.Task; });
        using var services = Services(command: command);
        await using var renderer = new TestRenderer(services);
        var root = await renderer.Mount<ExamineDetailPanel>(new() { ["Model"] = Review(7),
            ["OnProcCompleted"] = EventCallback.Factory.Create(this, () => completed++) });
        var buttons = renderer.Children<CgButton>(root);
        var first = renderer.Click(buttons[0]);
        await renderer.Click(buttons[1]);
        Assert.That(writes, Is.EqualTo(1));
        pending.SetResult(SdkResult<bool>.Ok(true));
        await first;
        Assert.That(completed, Is.EqualTo(1));
    }

    [Test]
    public async Task FailedSubmissionPreservesCommentsAndDetectsAnotherReviewer()
    {
        var writes = 0;
        var latest = Review(7, true);
        latest.Comments = "其他审核员的批注";
        var command = Proxy<IExamineCommandService>((_, args) =>
        {
            writes++;
            Assert.That(args![2], Is.EqualTo("请补充出处"));
            return Task.FromResult(SdkResult<bool>.Fail("ALREADY_REVIEWED", "记录已处理"));
        });
        var query = Proxy<IExamineQueryService>((_, _) => Task.FromResult(SdkResult<ExamineViewModel>.Ok(latest)));
        using var services = Services(query, command);
        await using var renderer = new TestRenderer(services);
        var root = await renderer.Mount<ExamineDetailPanel>(new() { ["Model"] = Review(7) });
        await renderer.Input(renderer.Children<CgTextarea>(root).Single(), "请补充出处");
        await renderer.Click(renderer.Children<CgButton>(root)[0]);
        Assert.That(renderer.AllText(root), Does.Contain("此记录已被处理").And.Contain("其他审核员的批注"));
        Assert.That(writes, Is.EqualTo(1));
        Assert.That(renderer.AllText(root), Does.Not.Contain("审核操作"));
    }

    [Test]
    public async Task RetryKeepsTypedCommentsAndChangingRecordResetsThem()
    {
        var command = Proxy<IExamineCommandService>((_, _) => Task.FromResult(SdkResult<bool>.Fail("OFFLINE", "网络错误")));
        var query = Proxy<IExamineQueryService>((_, _) => Task.FromResult(SdkResult<ExamineViewModel>.Ok(Review(7))));
        using var services = Services(query, command);
        await using var renderer = new TestRenderer(services);
        var root = await renderer.Mount<ExamineDetailPanel>(new() { ["Model"] = Review(7) });
        await renderer.Input(renderer.Children<CgTextarea>(root).Single(), "保留草稿");
        await renderer.Click(renderer.Children<CgButton>(root)[0]);
        Assert.That(renderer.Attribute(renderer.Children<CgTextarea>(root).Single(), "value"), Is.EqualTo("保留草稿"));
        await renderer.Render(root, new() { ["Model"] = Review(8) });
        Assert.That(renderer.Attribute(renderer.Children<CgTextarea>(root).Single(), "value"), Is.EqualTo(""));
    }

    [Test]
    public async Task ChangingRecordDiscardsAnOldSubmissionCallback()
    {
        var pending = new TaskCompletionSource<SdkResult<bool>>(TaskCreationOptions.RunContinuationsAsynchronously);
        var completed = 0;
        var command = Proxy<IExamineCommandService>((_, _) => pending.Task);
        using var services = Services(command: command);
        await using var renderer = new TestRenderer(services);
        var callback = EventCallback.Factory.Create(this, () => completed++);
        var root = await renderer.Mount<ExamineDetailPanel>(new() { ["Model"] = Review(7), ["OnProcCompleted"] = callback });
        var first = renderer.Click(renderer.Children<CgButton>(root)[0]);
        await renderer.Render(root, new() { ["Model"] = Review(8), ["OnProcCompleted"] = callback });
        pending.SetResult(SdkResult<bool>.Ok(true));
        await first;
        Assert.That(completed, Is.Zero);
        Assert.That(renderer.AllText(root), Does.Contain("记录 #8"));
    }

    [Test]
    public async Task SameVersionComparisonShowsNoticeWithoutCallingComparisonApi()
    {
        var query = Proxy<IExamineQueryService>((method, _) => method.Name switch
        {
            "GetExaminesOverviewAsync" => Task.FromResult(SdkResult<ExaminesOverviewViewModel>.Ok(new()
                { ObjectId = 1, ObjectName = "测试对象", Examines = [History(1), History(2)] })),
            "GetEditRecordComparisonAsync" => throw new AssertionException("Same-version comparison should not call the API"),
            _ => throw new InvalidOperationException(method.Name)
        });
        using var services = Services(query);
        await using var renderer = new TestRenderer(services);
        var root = await renderer.Attach<EditRecordPage>();
        await renderer.Render(root, new()
        {
            ["ContrastIdQuery"] = "1",
            ["CurrentIdQuery"] = "1",
            ["Mode"] = "compare"
        });

        Assert.That(renderer.AllText(root), Does.Contain("两个版本相同。"));
    }

    [Test]
    public async Task RecordPageReactsToParametersAndRejectsStaleResults()
    {
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var newerStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var old = new TaskCompletionSource<SdkResult<ExamineViewModel>>(TaskCreationOptions.RunContinuationsAsynchronously);
        var query = Proxy<IExamineQueryService>((method, args) => method.Name switch
        {
            "GetExaminesOverviewAsync" => Task.FromResult(SdkResult<ExaminesOverviewViewModel>.Ok(new()
                { ObjectId = 1, ObjectName = "测试对象", Examines = [History(1), History(2)] })),
            "GetExamineDetailAsync" when (long)args![0]! == 1 => StartOld(),
            "GetExamineDetailAsync" => StartNew(),
            _ => throw new InvalidOperationException(method.Name)
        });
        Task<SdkResult<ExamineViewModel>> StartOld() { started.TrySetResult(); return old.Task; }
        Task<SdkResult<ExamineViewModel>> StartNew() { newerStarted.TrySetResult(); return Task.FromResult(SdkResult<ExamineViewModel>.Ok(new()
            { Id = 2, AfterModel = Body("new-content") })); }
        using var services = Services(query);
        await using var renderer = new TestRenderer(services);
        var root = await renderer.Attach<EditRecordPage>();
        var first = renderer.Render(root, new() { ["ContrastIdQuery"] = "1" });
        await started.Task.WaitAsync(TimeSpan.FromSeconds(5));
        var second = renderer.Render(root, new() { ["ContrastIdQuery"] = "2", ["SelectedIdQuery"] = "2" });
        await newerStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));
        old.SetResult(SdkResult<ExamineViewModel>.Ok(new() { Id = 1, AfterModel = Body("stale-content") }));
        await Task.WhenAll(first, second).WaitAsync(TimeSpan.FromSeconds(5));
        Assert.That(renderer.AllText(root), Does.Contain("new-content").And.Not.Contain("stale-content"));
    }

    public static ExaminePreDataModel Content(int mask)
    {
        var items = new List<RecordPreviewItem>();
        if ((mask & 1) != 0) items.Add(new() { Key = "body", Group = "正文", Kind = RecordPreviewKind.Body, Label = "正文", Value = "<p>正文内容</p>", Html = "<p>正文内容</p>" });
        if ((mask & 2) != 0)
        {
            items.Add(new() { Key = "text/1", Group = "信息分组", Kind = RecordPreviewKind.Text, Label = "重复字段", Value = "第一行", DisplayValue = "第一行" });
            items.Add(new() { Key = "text/2", Group = "信息分组", Kind = RecordPreviewKind.Text, Label = "重复字段", Value = "第二行", DisplayValue = "第二行" });
        }
        if ((mask & 4) != 0) items.Add(new() { Key = "picture", Group = "图片", Kind = RecordPreviewKind.Image, Label = "图片说明", Value = "https://media.example/picture.png", Url = "https://media.example/picture.png" });
        if ((mask & 8) != 0) items.Add(new() { Key = "relation", Group = "关联词条", Kind = RecordPreviewKind.Relation, Label = "关联词条", Value = "entry:1", DisplayValue = "关联词条", Url = "/entries/index/1" });
        if ((mask & 16) != 0) items.Add(new() { Key = "link", Group = "外链", Kind = RecordPreviewKind.Link, Label = "官网", Value = "https://example.com", Url = "https://example.com", DisplayValue = "官方网站" });
        if ((mask & 32) != 0) items.Add(new() { Key = "audio", Group = "音频", Kind = RecordPreviewKind.Audio, Label = "主题曲", Value = "https://media.example/audio.mp3", Url = "https://media.example/audio.mp3" });
        return new() { Items = items };
    }

    private static int ItemCount(int mask)
    {
        var count = 0;
        if ((mask & 1) != 0) count++;
        if ((mask & 2) != 0) count += 2;
        if ((mask & 4) != 0) count++;
        if ((mask & 8) != 0) count++;
        if ((mask & 16) != 0) count++;
        if ((mask & 32) != 0) count++;
        return count;
    }

    private static bool ButtonIsDisabled(string html, string label) =>
        Regex.IsMatch(html,
            $@"<button\b(?=[^>]*\bdisabled=)[^>]*>.*?{Regex.Escape(label)}.*?</button>",
            RegexOptions.Singleline);

    private static ExaminePreDataModel Body(string text) => new()
    {
        Items = [new() { Key = "body", Group = "正文", Kind = RecordPreviewKind.Body, Label = "正文", Value = text, Html = text }]
    };

    private static ExamineViewModel Review(long id, bool? passed = null, bool admin = true, long dependency = 0)
        => new() { Id = id, ObjectId = 1, ObjectName = "测试对象", UserId = "test-user", UserName = "编辑者", ApplyTime = new(2026, 11, 12),
            IsAdmin = admin, IsPassed = passed, PrepositionExamineId = dependency, Operation = Operation.EstablishMain,
            BeforeModel = Body("<p>旧内容</p>"), AfterModel = Body("<p>新内容</p>") };

    private static EditRecordAloneViewModel History(long id) => new() { Id = id, UserName = "编辑者", Operation = Operation.EstablishMain };
    private static ServiceProvider Services(IExamineQueryService? query = null, IExamineCommandService? command = null)
        => new ServiceCollection().AddLogging().AddSingleton(query ?? Proxy<IExamineQueryService>((_, _) => throw new NotSupportedException()))
            .AddSingleton(command ?? Proxy<IExamineCommandService>((_, _) => throw new NotSupportedException()))
            .AddSingleton<ICgToastService, CgToastService>().AddSingleton<NavigationManager, TestNavigation>()
            .AddSingleton<IJSRuntime, NullJs>().BuildServiceProvider();
    private static T Proxy<T>(Func<MethodInfo, object?[]?, object?> handler) where T : class
    {
        var value = DispatchProxy.Create<T, ServiceProxy>();
        ((ServiceProxy)(object)value).Handler = handler;
        return value;
    }
    public class ServiceProxy : DispatchProxy
    {
        public Func<MethodInfo, object?[]?, object?> Handler = default!;
        protected override object? Invoke(MethodInfo? method, object?[]? args) => Handler(method!, args);
    }
    private sealed class TestNavigation : NavigationManager
    {
        public TestNavigation() => Initialize("https://test.example/", "https://test.example/home/editrecord");
        protected override void NavigateToCore(string uri, bool forceLoad) { Uri = ToAbsoluteUri(uri).ToString(); NotifyLocationChanged(false); }
    }
    private sealed class NullJs : IJSRuntime, IJSObjectReference, IDisposable
    {
        public ValueTask<T> InvokeAsync<T>(string identifier, object?[]? args) => ValueTask.FromResult(typeof(T) == typeof(IJSObjectReference) ? (T)(object)this : default!);
        public ValueTask<T> InvokeAsync<T>(string identifier, CancellationToken token, object?[]? args) => InvokeAsync<T>(identifier, args);
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
        public void Dispose() { }
    }
    private sealed class TestRenderer(IServiceProvider services) : Renderer(services, services.GetRequiredService<ILoggerFactory>())
    {
        public override Dispatcher Dispatcher { get; } = Dispatcher.CreateDefault();
        protected override IComponent ResolveComponentForRenderMode(Type type, int? parent, IComponentActivator activator, IComponentRenderMode mode) => activator.CreateInstance(type);
        protected override Task UpdateDisplayAsync(in RenderBatch batch) => Task.CompletedTask;
        protected override void HandleException(Exception ex) => throw new InvalidOperationException("Render failed", ex);
        private RenderTreeFrame[] Frames(int id) { var frames = GetCurrentRenderTreeFrames(id); return frames.Array.Take(frames.Count).ToArray(); }
        public Task<int> Attach<T>() where T : IComponent => Dispatcher.InvokeAsync(() => AssignRootComponentId(InstantiateComponent(typeof(T))));
        public Task Render(int id, Dictionary<string, object?> values) => Dispatcher.InvokeAsync(() => RenderRootComponentAsync(id, ParameterView.FromDictionary(values)));
        public async Task<int> Mount<T>(Dictionary<string, object?> values) where T : IComponent { var id = await Attach<T>(); await Render(id, values); return id; }
        public int[] Children<T>(int id) => Frames(id).Where(f => f.FrameType == RenderTreeFrameType.Component && f.ComponentType == typeof(T)).Select(f => f.ComponentId).ToArray();
        public object? Attribute(int id, string name) => Frames(id).FirstOrDefault(f => f.FrameType == RenderTreeFrameType.Attribute && f.AttributeName == name).AttributeValue;
        public string AllText(int id) => string.Join("", Frames(id).Select(f => f.FrameType switch
            { RenderTreeFrameType.Text => f.TextContent, RenderTreeFrameType.Markup => f.MarkupContent,
              RenderTreeFrameType.Component => AllText(f.ComponentId), _ => "" }));
        public Task Click(int id) => Event(id, "onclick", new MouseEventArgs());
        public Task Input(int id, string text) => Event(id, "oncginputcommit", new ChangeEventArgs { Value = text });
        private Task Event(int id, string name, EventArgs args) => Dispatcher.InvokeAsync(() => DispatchEventAsync(
            Frames(id).Single(f => f.FrameType == RenderTreeFrameType.Attribute && f.AttributeName == name).AttributeEventHandlerId, null, args));
    }
}
#pragma warning restore BL0006
