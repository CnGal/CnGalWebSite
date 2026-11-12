using System.Net.Http.Json;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace CnGalWebSite.UnitTest;

/// <summary>Opt-in tests against the real MainSite host and the local Fixtures/examine_api.py API.</summary>
[TestFixture, Explicit("Start the local fixture API on 5361 and MainSite on 5167; see Fixtures/Examine.md.")]
public class ExamineBrowserTests
{
    private IPlaywright _playwright = default!;
    private IBrowser _browser = default!;
    private IBrowserContext _context = default!;
    private IPage _page = default!;
    private readonly List<string> _errors = [];
    private const string Site = "http://127.0.0.1:5167";

    [SetUp]
    public async Task SetUp()
    {
        using var http = new HttpClient();
        var marker = await http.GetStringAsync("http://127.0.0.1:5361/__fixture");
        Assert.That(marker, Does.Contain("cngal-examine-local"));
        await http.PostAsJsonAsync("http://127.0.0.1:5361/__reset", new { });
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new() { Channel = "msedge", Headless = true });
        _context = await _browser.NewContextAsync(new() { ViewportSize = new() { Width = 1440, Height = 1000 }, Locale = "zh-CN" });
        await _context.RouteAsync("**/*", route => new Uri(route.Request.Url).Host is "127.0.0.1" or "localhost" ? route.ContinueAsync() : route.AbortAsync());
        _page = await _context.NewPageAsync();
        _errors.Clear();
        _page.PageError += (_, error) => _errors.Add(error);
    }

    [TearDown]
    public async Task TearDown()
    {
        if (_page is not null && TestContext.CurrentContext.Result.Outcome.Status == NUnit.Framework.Interfaces.TestStatus.Failed)
        {
            TestContext.Progress.WriteLine($"Browser URL: {_page.Url}");
            TestContext.Progress.WriteLine(await _page.Locator("body").InnerTextAsync());
            await _page.ScreenshotAsync(new() { Path = Path.Combine(OutputDirectory(), "failure.png"), FullPage = true });
        }
        if (_browser is not null) await _browser.DisposeAsync();
        _playwright?.Dispose();
        Assert.That(_errors, Is.Empty, "Browser JavaScript errors");
    }

    [Test]
    public async Task ResponsiveThemesAndRecordNavigation()
    {
        var output = OutputDirectory();
        foreach (var theme in new[] { "light", "dark" })
        {
            await _context.AddCookiesAsync([new() { Name = "cg-theme", Value = theme, Url = Site }]);
            foreach (var width in new[] { 375, 768, 1024, 1440 })
            {
                await _page.SetViewportSizeAsync(width, 1000);
                await _page.GotoAsync(Site + "/home/editrecord?ContrastId=20");
                await Expect(_page.Locator(".edit-record-detail")).ToBeVisibleAsync();
                await Expect(_page.Locator(".record-object h2")).ToHaveTextAsync("夏日回响 · 特别篇");
                var timeline = _page.Locator(".edit-record-timeline");
                if (width >= 1024) await Expect(timeline).ToBeVisibleAsync();
                else
                {
                    await Expect(timeline).ToBeHiddenAsync();
                    await _page.Locator(".edit-record-page__records-toggle").ClickAsync();
                    await Expect(timeline).ToBeVisibleAsync();
                    await _page.Locator(".edit-record-page__records-toggle").ClickAsync();
                }
                await NoOverflow();
                await _page.ScreenshotAsync(new() { Path = Path.Combine(output, $"record-{theme}-{width}.png"), FullPage = true });
                await _page.GetByRole(AriaRole.Link, new() { Name = "历史版本对比", Exact = true }).ClickAsync();
                await Expect(_page.Locator(".record-selector")).ToBeVisibleAsync();
                await Expect(_page.Locator(".edit-record-page__comparison-heading h2")).ToHaveTextAsync("版本 #20 → #30");
                await NoOverflow();
                await _page.ScreenshotAsync(new() { Path = Path.Combine(output, $"compare-{theme}-{width}.png"), FullPage = true });
                await _page.GotoAsync(Site + "/examine?Id=101");
                await Expect(_page.GetByRole(AriaRole.Button, new() { Name = "通过审核", Exact = true })).ToBeEnabledAsync();
                await NoOverflow();
                await _page.ScreenshotAsync(new() { Path = Path.Combine(output, $"review-{theme}-{width}.png"), FullPage = true });
                await _page.GotoAsync(Site + "/examine?Id=102");
                var history = _page.GetByRole(AriaRole.Link, new() { Name = "查看编辑记录", Exact = true });
                await Expect(_page.Locator(".record-object").GetByRole(AriaRole.Link, new() { Name = "查看编辑记录", Exact = true })).ToBeVisibleAsync();
                await Expect(history).ToHaveCountAsync(1);
                await Expect(history).ToHaveAttributeAsync("href", "/home/editrecord?ContrastId=102&Mode=record");
                await NoOverflow();
                await _page.ScreenshotAsync(new() { Path = Path.Combine(output, $"review-approved-{theme}-{width}.png"), FullPage = true });
            }
        }
        await _page.GotoAsync(Site + "/home/editrecord?ContrastId=20");
        await Expect(_page.Locator(".edit-record-detail")).ToBeVisibleAsync();
        await _page.Locator(".edit-record-timeline__item").First.ClickAsync();
        await Expect(_page.Locator(".record-meta__id")).ToHaveTextAsync("记录 #30");
        await _page.GoBackAsync();
        await Expect(_page.Locator(".record-meta__id")).ToHaveTextAsync("记录 #20");
        await _page.GoForwardAsync();
        await Expect(_page.Locator(".record-meta__id")).ToHaveTextAsync("记录 #30");
        await _page.ReloadAsync();
        await Expect(_page.Locator(".record-meta__id")).ToHaveTextAsync("记录 #30");
    }

    [Test]
    public async Task ComparisonSelectionSameVersionAndInvalidParameters()
    {
        await _page.GotoAsync(Site + "/home/editrecord?ContrastId=20&CurrentId=0");
        await Expect(_page.Locator(".edit-record-page__comparison-heading h2")).ToHaveTextAsync("版本 #20 → #30");
        var baseline = _page.GetByRole(AriaRole.Button, new() { Name = "基准版本", Exact = true });
        await baseline.FocusAsync();
        await baseline.PressAsync("Enter");
        await Expect(baseline).ToHaveAttributeAsync("aria-expanded", "true");
        await baseline.PressAsync("Escape");
        await Expect(baseline).ToHaveAttributeAsync("aria-expanded", "false");
        var selectors = _page.Locator(".record-selector .cg-select__trigger");
        await selectors.Nth(1).ClickAsync();
        await _page.Locator(".cg-select__option").Filter(new() { HasText = "#20 ·" }).ClickAsync();
        await _page.GetByRole(AriaRole.Button, new() { Name = "对比版本", Exact = true }).ClickAsync();
        await Expect(_page.Locator(".edit-record-page__notice")).ToHaveTextAsync("两个版本相同。");
        await _page.GotoAsync(Site + "/home/editrecord?ContrastId=30&CurrentId=20");
        await Expect(_page.Locator(".edit-record-page__notice")).ToContainTextAsync("基准版本不能晚于目标版本");
        await Expect(_page.Locator(".edit-record-timeline")).ToBeVisibleAsync();
        await _page.GotoAsync(Site + "/home/editrecord?ContrastId=20&CurrentId=999");
        await Expect(_page.Locator(".edit-record-page__notice")).ToContainTextAsync("同一对象");
        await _page.GotoAsync(Site + "/home/editrecord?ContrastId=not-a-number");
        await Expect(_page.Locator(".edit-record-page__state")).ToContainTextAsync("有效的编辑记录 ID");
        await _page.GotoAsync(Site + "/examine?Id=not-a-number");
        await Expect(_page.Locator(".examine-detail-page__error")).ToContainTextAsync("有效的审核记录 ID");
        await _page.GotoAsync(Site + "/home/editrecord?ContrastId=900");
        await Expect(_page.Locator(".edit-record-page__state")).ToHaveTextAsync("此对象暂无已通过的编辑记录。");
        await _page.GotoAsync(Site + "/home/editrecord?ContrastId=901");
        await Expect(_page.Locator(".edit-record-detail")).ToBeVisibleAsync();
        await _page.GetByRole(AriaRole.Link, new() { Name = "历史版本对比", Exact = true }).ClickAsync();
        await Expect(_page.Locator(".edit-record-page__notice")).ToHaveTextAsync("两个版本相同。");
    }

    [Test]
    public async Task PendingDependencyPermissionsAndFailedImages()
    {
        await _page.GotoAsync(Site + "/examine?Id=104");
        await Expect(_page.GetByRole(AriaRole.Button, new() { Name = "通过审核", Exact = true })).ToBeDisabledAsync();
        await Expect(_page.GetByRole(AriaRole.Link, new() { Name = "查看前置审核 #101 →" })).ToBeVisibleAsync();
        await _page.GotoAsync(Site + "/examine?Id=105");
        await Expect(_page.Locator(".examine-detail-panel")).ToBeVisibleAsync();
        await Expect(_page.GetByRole(AriaRole.Button, new() { Name = "通过审核", Exact = true })).ToHaveCountAsync(0);
        await _page.GotoAsync(Site + "/examine?Id=106");
        await Expect(_page.GetByText("这是一条单份评论内容。", new() { Exact = true })).ToHaveCountAsync(1);
        await _page.GotoAsync(Site + "/examine?Id=110");
        await Expect(_page.Locator(".record-object .record-image__fallback")).ToBeVisibleAsync();
        await _page.Locator(".record-comparison").ScrollIntoViewIfNeededAsync();
        await Expect(_page.Locator(".record-comparison .record-image__fallback")).ToBeVisibleAsync();
        await _page.GotoAsync(Site + "/examine?Id=111");
        await Expect(_page.Locator(".record-notes__empty")).ToHaveCountAsync(1);
        await Expect(_page.Locator(".record-comparison__empty")).ToBeVisibleAsync();
        await _page.GotoAsync(Site + "/examine?Id=102");
        await Expect(_page.Locator(".record-notes")).ToContainTextAsync(["修改说明", "资料已核对。"]);
        await Expect(_page.GetByRole(AriaRole.Button, new() { Name = "通过审核", Exact = true })).ToHaveCountAsync(0);
    }

    [Test]
    public async Task SubmissionFailureRetainsCommentsAndSuccessReturnsToContentCenter()
    {
        await _page.GotoAsync(Site + "/examine?Id=107");
        var comments = _page.GetByLabel("审核批注 （可选）");
        await comments.FillAsync("审核说明：已核对来源");
        await comments.BlurAsync();
        await _page.GetByRole(AriaRole.Button, new() { Name = "通过审核", Exact = true }).ClickAsync();
        await Expect(_page.Locator(".examine-detail-panel__error")).ToContainTextAsync("模拟网络失败");
        await Expect(comments).ToHaveValueAsync("审核说明：已核对来源");
        await _page.GetByRole(AriaRole.Button, new() { Name = "通过审核", Exact = true }).ClickAsync();
        await Expect(_page).ToHaveURLAsync(Site + "/usercenter");
        await _page.GotoAsync(Site + "/examine?Id=108");
        await _page.GetByRole(AriaRole.Button, new() { Name = "驳回", Exact = true }).ClickAsync();
        await Expect(_page.Locator(".examine-detail-panel__error")).ToContainTextAsync("此记录已被处理");
        await Expect(_page.GetByRole(AriaRole.Button, new() { Name = "通过审核", Exact = true })).ToHaveCountAsync(0);
    }

    private async Task NoOverflow()
        => Assert.That(await _page.EvaluateAsync<bool>("document.documentElement.scrollWidth <= innerWidth + 1"), Is.True, $"Horizontal overflow: {_page.Url}");

    private static string OutputDirectory()
    {
        var root = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (root.Parent is not null && !Directory.Exists(Path.Combine(root.FullName, ".git"))) root = root.Parent;
        var path = Path.Combine(root.FullName, ".cache", "examine-screenshots");
        Directory.CreateDirectory(path);
        return path;
    }
}
