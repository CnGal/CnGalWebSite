using CnGalWebSite.DataModel.Sorting;
using CnGalWebSite.MainSite.Shared.Components.DesignSystem;
using CnGalWebSite.MainSite.Shared.Components.Features.Examine;
using CnGalWebSite.MainSite.Shared.Pages.UserCenter;
using CnGalWebSite.MainSite.Shared.Services;
using CnGalWebSite.DataModel.ViewModel.EditRecords;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Search;
using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Models;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using System.Reflection;

namespace CnGalWebSite.UnitTest;

// Exercise rendered components and their actual event callbacks without a browser or backend.
#pragma warning disable BL0006 // Renderer is used only by this test harness.
[TestFixture]
public class MainSiteEditorStateTests
{
    [TestCase(null, null, null, null, null)]
    [TestCase("bad", "bad", " ", "bad", "bad")]
    [TestCase("-2", "1000", "", "Comments", "bad")]
    public void InvalidQueryValuesUseDefaults(string? page, string? size, string? search, string? sort, string? desc)
    {
        var state = UserCenterQueryState.GetTableState(page, size, search, sort, desc,
            ExamineSortField.PassedTime, ExamineSortField.Id, ExamineSortField.PassedTime, ExamineSortField.ApplyTime);
        Assert.That(state, Is.EqualTo(new CgDataTableRequest<ExamineSortField>(1, 10, ExamineSortField.PassedTime, true, null)));
    }

    [Test]
    public void QueryStatePreservesSearchAndSortAndValidatesTab()
    {
        var state = UserCenterQueryState.GetTableState("3", "20", "词条 & + #", "ApplyTime", "false",
            ExamineSortField.PassedTime, ExamineSortField.Id, ExamineSortField.PassedTime, ExamineSortField.ApplyTime);
        Assert.Multiple(() =>
        {
            Assert.That(state, Is.EqualTo(new CgDataTableRequest<ExamineSortField>(3, 20, ExamineSortField.ApplyTime, false, "词条 & + #")));
            Assert.That(UserCenterQueryState.GetTab("missing"), Is.EqualTo("overview"));
            Assert.That(UserCenterQueryState.GetTab("reviews"), Is.EqualTo("reviews"));
        });
    }

    [Test]
    public void TabLinksResetListStateAndTableChangesKeepEncodedSearchInHistory()
    {
        var navigation = new TestNavigation();
        var page = new UserCenterPage();
        typeof(UserCenterPage).GetProperty("NavigationManager", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(page, navigation);
        typeof(UserCenterPage).GetProperty("Tab")!.SetValue(page, "examines");
        var template = (string)typeof(UserCenterPage).GetMethod("GetTabHref", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(page, null)!;
        var query = QueryHelpers.ParseQuery(new Uri(string.Format(template, "reviews")).Query);
        Assert.That(query.Keys, Is.EquivalentTo(new[] { "tab", "unrelated" }));
        Assert.That(query["tab"].ToString(), Is.EqualTo("reviews"));
        Assert.That(query["unrelated"].ToString(), Is.EqualTo("{keep}"));

        var state = new CgDataTableRequest<ExamineSortField>(2, 20, ExamineSortField.ApplyTime, false, "词条 & + #");
        typeof(UserCenterPage).GetMethod("SetTableState", BindingFlags.Instance | BindingFlags.NonPublic)!
            .MakeGenericMethod(typeof(ExamineSortField)).Invoke(page, new object[] { state });
        query = QueryHelpers.ParseQuery(new Uri(navigation.Uri).Query);
        Assert.Multiple(() =>
        {
            Assert.That(query["search"].ToString(), Is.EqualTo(state.SearchText));
            Assert.That(query["sort"].ToString(), Is.EqualTo("ApplyTime"));
            Assert.That(query["desc"].ToString(), Is.EqualTo("False").IgnoreCase);
            Assert.That(query["page"].ToString(), Is.EqualTo("2"));
            Assert.That(navigation.LastReplace, Is.False);
        });
    }

    [TestCase(2)]
    [TestCase(99)]
    public async Task OverviewRestoresAllThreePagesAndCorrectsPageAfterLastItemIsRead(int page)
    {
        var model = new UserContentCenterViewModel
        {
            UnReviewExamines = Enumerable.Range(1, 11).Select(i => new ExaminedNormalListModel { Id = i, RelatedName = $"未读{i}" }).ToList(),
            PendingExamines = Enumerable.Range(1, 11).Select(i => new ExaminedNormalListModel { Id = i, RelatedName = $"待审核{i}" }).ToList(),
            UnReviewEntries = Enumerable.Range(1, 11).Select(i => new EntryInforTipViewModel { Id = i, Name = $"最近编辑{i}" }).ToList(),
        };
        var query = DispatchProxy.Create<IExamineQueryService, ServiceProxy>();
        ((ServiceProxy)(object)query).Handler = (_, _) => Task.FromResult(SdkResult<UserContentCenterViewModel>.Ok(model));
        var command = DispatchProxy.Create<IExamineCommandService, ServiceProxy>();
        ((ServiceProxy)(object)command).Handler = (_, _) => Task.FromResult(SdkResult<bool>.Ok(true));
        using var services = new ServiceCollection().AddLogging()
            .AddSingleton(query).AddSingleton(command)
            .AddSingleton<ICgToastService, CgToastService>()
            .AddSingleton<NavigationManager, TestNavigation>().BuildServiceProvider();
        await using var renderer = new ComponentRenderer(services);
        var corrected = new List<(int, int, int)>();
        var (_, root) = await renderer.MountAsync<UserContentOverview>(new Dictionary<string, object?>
        {
            ["UnReviewPage"] = page, ["PendingPage"] = page, ["EntryPage"] = page,
            ["PagesCorrected"] = EventCallback.Factory.Create<(int, int, int)>(corrected, value => corrected.Add(value)),
        });
        Assert.That(renderer.Text(root), Does.Contain("未读11").And.Not.Contain("未读10")
            .And.Contain("待审核11").And.Not.Contain("待审核10")
            .And.Contain("最近编辑11").And.Not.Contain("最近编辑10"));
        Assert.That(renderer.Attribute(root, "href")?.ToString(), Does.Contain("11"));
        Assert.That(corrected, Is.EqualTo(page == 99 ? new[] { (2, 2, 2) } : Array.Empty<(int, int, int)>()));
        corrected.Clear();
        // Mark the only item on page 2 read (the "all read" button is a separate child component).
        await renderer.EventAsync(root, "onclick", new MouseEventArgs());
        Assert.That(model.UnReviewExamines.Count, Is.EqualTo(10));
        Assert.That(corrected, Is.EqualTo(new[] { (1, 2, 2) }));
    }

    [TestCase("词", "")]
    [TestCase("不存在", "")]
    [TestCase("", "")]
    [TestCase("ITEM24", "Item24")]
    public async Task RestrictedInputKeepsDraftButNeverPublishesInvalidText(string input, string expected)
    {
        using var services = new ServiceCollection().AddLogging().BuildServiceProvider();
        await using var renderer = new ComponentRenderer(services);
        var (host, root) = await renderer.MountAsync<AutoCompleteHost>();
        var component = renderer.ChildId<CgAutoComplete>(root);
        await renderer.EventAsync(component, "oncginputcommit", new ChangeEventArgs { Value = input });
        Assert.Multiple(() =>
        {
            Assert.That(host.Value, Is.EqualTo(expected), "The bound model must already be safe before blur or submit.");
            Assert.That(renderer.Attribute(component, "value"), Is.EqualTo(input));
        });

        await renderer.Dispatcher.InvokeAsync(host.Redraw);
        Assert.That(renderer.Attribute(component, "value"), Is.EqualTo(input), "Parent rerenders must preserve the draft.");
        await renderer.EventAsync(component, "oncgfocusleave", new CgFocusLeaveEventArgs());
        Assert.That(renderer.Attribute(component, "value")?.ToString() ?? "", Is.EqualTo(expected));
        Assert.That(host.Value, Is.EqualTo(expected));
    }

    [Test]
    public async Task MovingFocusToCandidateDoesNotClearSearchAndClickConfirmsIt()
    {
        using var services = new ServiceCollection().AddLogging().BuildServiceProvider();
        await using var renderer = new ComponentRenderer(services);
        var (host, root) = await renderer.MountAsync<AutoCompleteHost>();
        var component = renderer.ChildId<CgAutoComplete>(root);
        await renderer.EventAsync(component, "oncginputcommit", new ChangeEventArgs { Value = "词" });
        await renderer.EventAsync(component, "oncgfocusleave", new CgFocusLeaveEventArgs { IsInternal = true });
        Assert.That(renderer.Attribute(component, "value"), Is.EqualTo("词"));
        await renderer.EventAsync(component, "onclick", new MouseEventArgs());
        Assert.That(host.Value, Is.EqualTo("词条"));
        Assert.That(renderer.Attribute(component, "value"), Is.EqualTo("词条"));
    }

    [Test]
    public async Task LoadingCandidatesDoesNotClearExistingValueAndFreeInputStillWorks()
    {
        using var services = new ServiceCollection().AddLogging().BuildServiceProvider();
        await using var renderer = new ComponentRenderer(services);
        var (host, root) = await renderer.MountAsync<AutoCompleteHost>();
        var component = renderer.ChildId<CgAutoComplete>(root);
        await renderer.Dispatcher.InvokeAsync(() => { host.Items = []; host.Value = "尚未加载"; host.Redraw(); });
        await renderer.EventAsync(component, "oncgfocusleave", new CgFocusLeaveEventArgs());
        Assert.That(host.Value, Is.EqualTo("尚未加载"));
        await renderer.Dispatcher.InvokeAsync(() => { host.Restrict = false; host.Redraw(); });
        await renderer.EventAsync(component, "oncginputcommit", new ChangeEventArgs { Value = "任意文本" });
        await renderer.EventAsync(component, "oncgfocusleave", new CgFocusLeaveEventArgs());
        Assert.That(host.Value, Is.EqualTo("任意文本"));
    }

    [Test]
    public async Task TableRestoresExternalStateAndDoesNotReloadItsOwnEcho()
    {
        using var services = new ServiceCollection().AddLogging().BuildServiceProvider();
        await using var renderer = new ComponentRenderer(services);
        var (host, root) = await renderer.MountAsync<TableHost>();
        var component = renderer.ChildId<CgDataTable<string, ExamineSortField>>(root);
        var original = host.State;
        Assert.That(host.Requests, Is.EqualTo(new[] { original }));

        await renderer.EventAsync(component, "oncginputcommit", new ChangeEventArgs { Value = "新搜索" });
        Assert.Multiple(() =>
        {
            Assert.That(host.Requests.Count, Is.EqualTo(2), "Echoed State must not duplicate server queries.");
            Assert.That(host.State, Is.EqualTo(original with { Page = 1, SearchText = "新搜索" }));
        });
        await renderer.Dispatcher.InvokeAsync(() => { host.State = original; host.Redraw(); });
        Assert.That(host.Requests.Last(), Is.EqualTo(original), "Browser back must restore the entire query.");
        Assert.That(renderer.Attribute(component, "value"), Is.EqualTo("原搜索"));

        await renderer.EventAsync(component, "onclick", new MouseEventArgs());
        Assert.That(host.State, Is.EqualTo(original with { Page = 1, SortDesc = false }));
        var select = renderer.ChildId<CgSelect>(component);
        await renderer.EventAsync(select, "onclick", new MouseEventArgs());
        // Trigger, backdrop, then the 5/10/20/50 options.
        await renderer.EventAsync(select, "onclick", new MouseEventArgs(), 4);
        Assert.That(host.State.PageSize, Is.EqualTo(20));
        Assert.That(host.State.Page, Is.EqualTo(1));
    }

    [Test]
    public async Task PageCorrectionNotifiesUrlOwnerAfterDataShrinks()
    {
        using var services = new ServiceCollection().AddLogging().BuildServiceProvider();
        await using var renderer = new ComponentRenderer(services);
        var (host, _) = await renderer.MountAsync<TableHost>();
        host.Query = request => Task.FromResult(new CgDataTableResult<string>(["remaining"], 1));
        await renderer.Dispatcher.InvokeAsync(() => host.Table!.RefreshAsync());
        Assert.That(host.State.Page, Is.EqualTo(1));
        Assert.That(host.Requests.TakeLast(2).Select(r => r.Page), Is.EqualTo(new[] { 3, 1 }));
    }

    [Test]
    public async Task OlderSearchResultsCannotOverwriteNewerSearch()
    {
        using var services = new ServiceCollection().AddLogging().BuildServiceProvider();
        await using var renderer = new ComponentRenderer(services);
        var (host, root) = await renderer.MountAsync<TableHost>();
        var component = renderer.ChildId<CgDataTable<string, ExamineSortField>>(root);
        var old = new TaskCompletionSource<CgDataTableResult<string>>(TaskCreationOptions.RunContinuationsAsynchronously);
        host.Query = request => request.SearchText == "old" ? old.Task
            : Task.FromResult(new CgDataTableResult<string>(["new result"], 1));
        var first = renderer.EventAsync(component, "oncginputcommit", new ChangeEventArgs { Value = "old" });
        await renderer.EventAsync(component, "oncginputcommit", new ChangeEventArgs { Value = "new" });
        old.SetResult(new CgDataTableResult<string>(["stale result"], 1));
        await first;
        Assert.That(renderer.Text(component), Does.Contain("new result").And.Not.Contain("stale result"));
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task TablesWithoutExternalStateContinueToSearch(bool clientMode)
    {
        using var services = new ServiceCollection().AddLogging().BuildServiceProvider();
        await using var renderer = new ComponentRenderer(services);
        var (host, root) = await renderer.MountAsync<TableHost>(new Dictionary<string, object?>
        {
            ["BindState"] = false, ["ClientMode"] = clientMode,
        });
        var component = renderer.ChildId<CgDataTable<string, ExamineSortField>>(root);
        await renderer.EventAsync(component, "oncginputcommit", new ChangeEventArgs { Value = "row" });
        Assert.That(renderer.Text(component), Does.Contain("row").And.Not.Contain("other"));
        if (!clientMode)
        {
            Assert.That(host.Requests.Last().SearchText, Is.EqualTo("row"));
            Assert.That(host.Requests.Last().Page, Is.EqualTo(1));
        }
    }

    public sealed class AutoCompleteHost : ComponentBase
    {
        public string Value = "词条";
        public bool Restrict = true;
        public IEnumerable<string> Items = Enumerable.Range(0, 30).Select(i => $"Item{i}").Append("词条");
        public void Redraw() => StateHasChanged();
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenComponent<CgAutoComplete>(0);
            builder.AddAttribute(1, "Value", Value);
            builder.AddAttribute(2, "Items", Items);
            builder.AddAttribute(3, "RestrictToItems", Restrict);
            builder.AddAttribute(4, "ValueChanged", EventCallback.Factory.Create<string?>(this, value => Value = value ?? ""));
            builder.CloseComponent();
        }
    }

    public sealed class TableHost : ComponentBase
    {
        [Parameter] public bool BindState { get; set; } = true;
        [Parameter] public bool ClientMode { get; set; }
        public CgDataTableRequest<ExamineSortField> State = new(3, 10, ExamineSortField.Id, true, "原搜索");
        public readonly List<CgDataTableRequest<ExamineSortField>> Requests = [];
        public Func<CgDataTableRequest<ExamineSortField>, Task<CgDataTableResult<string>>> Query =
            _ => Task.FromResult(new CgDataTableResult<string>(["row"], 100));
        public CgDataTable<string, ExamineSortField>? Table;
        public void Redraw() => StateHasChanged();
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenComponent<CgDataTable<string, ExamineSortField>>(0);
            if (BindState)
            {
                builder.AddAttribute(1, "State", State);
                builder.AddAttribute(2, "StateChanged", EventCallback.Factory.Create<CgDataTableRequest<ExamineSortField>>(this, state => State = state));
            }
            if (ClientMode)
                builder.AddAttribute(3, "Items", new[] { "row", "other" });
            else
                builder.AddAttribute(3, "OnServerQuery", (Func<CgDataTableRequest<ExamineSortField>, Task<CgDataTableResult<string>>>)(request =>
                {
                    Requests.Add(request);
                    return Query(request);
                }));
            builder.AddAttribute(4, "ChildContent", (RenderFragment)(columns =>
            {
                columns.OpenComponent<CgDataTableColumn<string, ExamineSortField>>(0);
                columns.AddAttribute(1, "SortField", ExamineSortField.Id);
                columns.AddAttribute(2, "Value", (Func<string, object?>)(item => item));
                columns.CloseComponent();
            }));
            builder.AddComponentReferenceCapture(5, instance => Table = (CgDataTable<string, ExamineSortField>)instance);
            builder.CloseComponent();
        }
    }

    public class ServiceProxy : DispatchProxy
    {
        public Func<MethodInfo, object?[]?, object?> Handler = (_, _) => throw new NotSupportedException();
        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args) => Handler(targetMethod!, args);
    }

    private sealed class TestNavigation : NavigationManager
    {
        public bool LastReplace;
        public TestNavigation() => Initialize("https://test.example/", "https://test.example/usercenter?tab=examines&page=3&pageSize=20&search=old&sort=Id&desc=true&unReviewPage=2&pendingPage=3&entryPage=4&unrelated=%7Bkeep%7D");
        protected override void NavigateToCore(string uri, bool forceLoad) => NavigateToCore(uri, new NavigationOptions { ForceLoad = forceLoad });
        protected override void NavigateToCore(string uri, NavigationOptions options)
        {
            LastReplace = options.ReplaceHistoryEntry;
            Uri = ToAbsoluteUri(uri).ToString();
            NotifyLocationChanged(false);
        }
    }

    private sealed class ComponentRenderer(IServiceProvider services)
        : Renderer(services, services.GetRequiredService<ILoggerFactory>())
    {
        public override Dispatcher Dispatcher { get; } = Dispatcher.CreateDefault();
        protected override Task UpdateDisplayAsync(in RenderBatch renderBatch) => Task.CompletedTask;
        protected override void HandleException(Exception exception) => throw new InvalidOperationException("Component render failed", exception);
        public Task<(T Component, int Id)> MountAsync<T>(Dictionary<string, object?>? parameters = null) where T : IComponent => Dispatcher.InvokeAsync(async () =>
        {
            var component = (T)InstantiateComponent(typeof(T));
            var id = AssignRootComponentId(component);
            await RenderRootComponentAsync(id, ParameterView.FromDictionary(parameters ?? new()));
            return (component, id);
        });
        private RenderTreeFrame[] Frames(int id)
        {
            var frames = GetCurrentRenderTreeFrames(id);
            return frames.Array.Take(frames.Count).ToArray();
        }
        public int ChildId<T>(int id) => Frames(id).Single(frame => frame.FrameType == RenderTreeFrameType.Component && frame.ComponentType == typeof(T)).ComponentId;
        public object? Attribute(int id, string name) => Frames(id).Where(frame => frame.FrameType == RenderTreeFrameType.Attribute && frame.AttributeName == name).Select(frame => frame.AttributeValue).FirstOrDefault();
        public string Text(int id) => string.Join(" ", Frames(id).Where(frame => frame.FrameType == RenderTreeFrameType.Text).Select(frame => frame.TextContent));
        public Task EventAsync(int id, string name, EventArgs args, int index = 0) => Dispatcher.InvokeAsync(() =>
        {
            var handler = Frames(id).Where(frame => frame.FrameType == RenderTreeFrameType.Attribute && frame.AttributeName == name).ElementAt(index).AttributeEventHandlerId;
            return DispatchEventAsync(handler, null, args);
        });
    }
}
#pragma warning restore BL0006
