using CnGalWebSite.MainSite.Shared.Components.Features.Examine;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using AngleSharp.Html.Parser;

namespace CnGalWebSite.UnitTest;

public class RecordRawDiffTests
{
    [Test]
    public void ReadsChangesAndDecodesTransportEntitiesOnce()
    {
        var segments = RecordRawDiff.Parse("# 标题<br>![封面](https://example.com/a.png) "
            + "<del class='diffmod'>旧 &amp; 内容</del><ins class='diffmod'>新 &amp;lt; 内容</ins>");
        Assert.That(string.Concat(segments.Select(s => s.Text)),
            Is.EqualTo("# 标题\n![封面](https://example.com/a.png) 旧 & 内容新 &lt; 内容"));
        Assert.That(segments.Where(s => s.Change == RecordTextChangeKind.Removed).Select(s => s.Text), Is.EqualTo(new[] { "旧 & 内容" }));
        Assert.That(segments.Where(s => s.Change == RecordTextChangeKind.Added).Select(s => s.Text), Is.EqualTo(new[] { "新 &lt; 内容" }));
    }

    [TestCase("<a href='https://example.com/?q=a>b'><del class='diffmod'>旧</del><ins class='diffmod'>新</ins></a>", "<a href='https://example.com/?q=a>b'>旧新</a>")]
    [TestCase("<del>原有删除线</del><ins>原有下划线</ins>", "<del>原有删除线</del><ins>原有下划线</ins>")]
    [TestCase("<p><strong>源码</strong><img src='https://example.com/raw.png'></p>", "<p><strong>源码</strong><img src='https://example.com/raw.png'></p>")]
    [TestCase("<div><ins class='diffins'>未闭合", "<div>未闭合")]
    [TestCase("<div><span>x</div>", "<div><span>x</div>")]
    [TestCase("<!-- 注释 --><h1>源码</h1>", "<!-- 注释 --><h1>源码</h1>")]
    [TestCase("<pre><code class=\"language-json\">{\n  &quot;name&quot;: &quot;&lt;img&gt;&quot;\n}\n</code></pre>\n", "{\n  \"name\": \"<img>\"\n}\n\n")]
    [TestCase("", "")]
    public void PreservesSourceMarkupAndUnwrapsFormattedJson(string input, string expected)
        => Assert.That(string.Concat(RecordRawDiff.Parse(input).Select(s => s.Text)), Is.EqualTo(expected));

    [Test]
    public async Task RendersOnlyTextAndDiffMarkers()
    {
        using var services = new ServiceCollection().AddLogging().BuildServiceProvider();
        await using var renderer = new HtmlRenderer(services, services.GetRequiredService<ILoggerFactory>());
        const string source = "# 标题 **加粗** [链接](https://example.com/)\n"
            + "<img src='https://example.com/raw.png'><iframe src='https://example.com/'></iframe>"
            + "<script>alert('source')</script><a href='https://example.com/'><del class='diffdel'>旧</del><ins class='diffins'>新</ins></a>";
        var html = await renderer.Dispatcher.InvokeAsync(async () =>
            (await renderer.RenderComponentAsync<RecordRawData>(ParameterView.FromDictionary(
                new Dictionary<string, object?> { ["Content"] = source }))).ToHtmlString());
        var body = new HtmlParser().ParseDocument(html).Body!;
        Assert.That(body.QuerySelectorAll("img, iframe, script, a, h1, strong"), Is.Empty);
        Assert.That(body.QuerySelector("pre")!.TextContent, Does.Contain("# 标题 **加粗** [链接]")
            .And.Contain("<img src=").And.Contain("<script>alert('source')</script>"));
        Assert.That(body.QuerySelector("del")!.TextContent, Is.EqualTo("旧"));
        Assert.That(body.QuerySelector("ins")!.TextContent, Is.EqualTo("新"));
    }

    [Test]
    public async Task ExplicitRawContentPreservesSourceTagsAndEntitiesAsText()
    {
        using var services = new ServiceCollection().AddLogging().BuildServiceProvider();
        await using var renderer = new HtmlRenderer(services, services.GetRequiredService<ILoggerFactory>());
        const string source = "<del>删除线</del><ins class='diffmod'>作者标签</ins><br>\n"
            + "&amp; &lt;img&gt; </pre><script>alert('source')</script>";
        var html = await renderer.Dispatcher.InvokeAsync(async () =>
            (await renderer.RenderComponentAsync<RecordRawData>(ParameterView.FromDictionary(
                new Dictionary<string, object?> { ["Content"] = "<del class='diffdel'>旧接口内容</del>",
                    ["RawBeforeContent"] = source, ["RawContent"] = source }))).ToHtmlString());
        var body = new HtmlParser().ParseDocument(html).Body!;
        Assert.Multiple(() =>
        {
            Assert.That(body.QuerySelectorAll("script, img, ins, del, br"), Is.Empty);
            Assert.That(body.QuerySelector("pre")!.TextContent, Is.EqualTo(source));
        });
    }

    [TestCase("")]
    [TestCase("{\"Html\":\"<br><del>源码</del>&amp;\"}")]
    public async Task ExplicitSubmissionDoesNotFallBackToLegacyHtml(string source)
    {
        using var services = new ServiceCollection().AddLogging().BuildServiceProvider();
        await using var renderer = new HtmlRenderer(services, services.GetRequiredService<ILoggerFactory>());
        var html = await renderer.Dispatcher.InvokeAsync(async () =>
            (await renderer.RenderComponentAsync<RecordRawData>(ParameterView.FromDictionary(
                new Dictionary<string, object?> { ["Content"] = "旧接口内容", ["RawContent"] = source }))).ToHtmlString());
        Assert.That(new HtmlParser().ParseDocument(html).QuerySelector("pre")!.TextContent, Is.EqualTo(source));
    }

    [Test]
    public void RawTextDiffPreservesBothSourcesIncludingLineEndingsAndMarkup()
    {
        const string before = "<del>原有删除线</del><br>\r\n&amp; 旧";
        const string after = "<del>原有删除线</del><br>\n&amp; 新";
        var segments = RecordTextDiff.Build(before, after, normalizeLineEndings: false);
        Assert.Multiple(() =>
        {
            Assert.That(string.Concat(segments.Where(s => s.Change != RecordTextChangeKind.Added).Select(s => s.Text)), Is.EqualTo(before));
            Assert.That(string.Concat(segments.Where(s => s.Change != RecordTextChangeKind.Removed).Select(s => s.Text)), Is.EqualTo(after));
            Assert.That(RecordTextDiff.Build(before, before, normalizeLineEndings: false),
                Is.EqualTo(new[] { new RecordTextSegment(before, RecordTextChangeKind.Unchanged) }));
        });
    }
}
