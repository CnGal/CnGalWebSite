using System.Net;
using System.Reflection;
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
    public async Task ContentCombinationsKeepBothSidesAndNeverInferDeletion(int mask)
    {
        var data = Content(mask);
        Assert.That(RecordPresentation.ContentKinds.Where(k => RecordPresentation.HasContent(data, k)).Count(),
            Is.EqualTo(System.Numerics.BitOperations.PopCount((uint)mask)));
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
            if (mask == 0) Assert.That(html, Does.Contain("没有可展示的内容"));
            else
            {
                Assert.That(html.Split("此侧无内容").Length - 1, Is.EqualTo(System.Numerics.BitOperations.PopCount((uint)mask)));
                if ((mask & 2) != 0) Assert.That(html, Does.Contain("重复字段").And.Contain("第一行").And.Contain("第二行"));
            }
        }
    }

    [Test]
    public async Task MediaLinksAndStringsAreRenderedWithoutExecutableHtml()
    {
        var data = Content(63);
        data.Texts[0].Informations[0].DisplayValue = "<script>alert(1)</script>";
        data.Outlinks[0].Link = "javascript:alert(1)";
        data.Outlinks[0].DisplayValue = "unsafe-link";
        using var services = Services();
        await using var renderer = new HtmlRenderer(services, services.GetRequiredService<ILoggerFactory>());
        var html = await renderer.Dispatcher.InvokeAsync(async () =>
            (await renderer.RenderComponentAsync<ExaminePreDataView>(ParameterView.FromDictionary(new Dictionary<string, object?> { ["Model"] = data }))).ToHtmlString());
        Assert.That(html, Does.Contain("&lt;script&gt;").And.Not.Contain("href=\"javascript:"));
        Assert.That(html, Does.Contain("preload=\"none\"").And.Not.Contain("autoplay"));
        Assert.That(html, Does.Contain("https://media.example/picture.png").And.Contain("target=\"_blank\""));
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
        if (admin && passed is null && dependency > 0) Assert.That(html, Does.Contain("disabled").And.Contain("查看前置审核 #42"));
        if (passed is null) Assert.That(html, Does.Contain("提交内容").And.Contain("当前内容"));
        else Assert.That(html, Does.Contain("记录保存内容").And.Not.Contain("编辑后"));
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
        model.BeforeModel = model.AfterModel = new() { MainPage = "<p>unique-submission</p>" };
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
        Assert.That(renderer.AllText(root), Does.Not.Contain("通过审核"));
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
            { Id = 2, BeforeModel = new() { MainPage = "new-content" } })); }
        using var services = Services(query);
        await using var renderer = new TestRenderer(services);
        var root = await renderer.Attach<EditRecordPage>();
        var first = renderer.Render(root, new() { ["ContrastIdQuery"] = "1" });
        await started.Task.WaitAsync(TimeSpan.FromSeconds(5));
        var second = renderer.Render(root, new() { ["ContrastIdQuery"] = "2", ["SelectedIdQuery"] = "2" });
        await newerStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));
        old.SetResult(SdkResult<ExamineViewModel>.Ok(new() { Id = 1, BeforeModel = new() { MainPage = "stale-content" } }));
        await Task.WhenAll(first, second).WaitAsync(TimeSpan.FromSeconds(5));
        Assert.That(renderer.AllText(root), Does.Contain("new-content").And.Not.Contain("stale-content"));
    }

    public static ExaminePreDataModel Content(int mask) => new()
    {
        MainPage = (mask & 1) != 0 ? "<p>正文内容</p>" : "",
        Texts = (mask & 2) != 0 ? [new() { Modifier = "信息分组", Informations = [new() { DisplayName = "重复字段", DisplayValue = "第一行" }, new() { DisplayName = "重复字段", DisplayValue = "第二行" }] }] : [],
        Pictures = (mask & 4) != 0 ? [new() { Note = "图片说明", Url = "https://media.example/picture.png" }] : [],
        Relevances = (mask & 8) != 0 ? [new() { entry = new() { Id = 1, Name = "关联词条" } }] : [],
        Outlinks = (mask & 16) != 0 ? [new() { DisplayName = "官网", Link = "https://example.com", DisplayValue = "官方网站" }] : [],
        Audio = (mask & 32) != 0 ? [new() { Name = "主题曲", Url = "https://media.example/audio.mp3" }] : []
    };

    private static ExamineViewModel Review(long id, bool? passed = null, bool admin = true, long dependency = 0)
        => new() { Id = id, ObjectId = 1, ObjectName = "测试对象", UserId = "test-user", UserName = "编辑者", ApplyTime = new(2026, 11, 12),
            IsAdmin = admin, IsPassed = passed, PrepositionExamineId = dependency, Operation = Operation.EstablishMain,
            BeforeModel = new() { MainPage = "<p>旧内容</p>" }, AfterModel = new() { MainPage = "<p>新内容</p>" } };
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
