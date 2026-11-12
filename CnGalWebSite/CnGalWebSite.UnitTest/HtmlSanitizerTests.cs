using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Markdig;
using NUnit.Framework;
using CnGalWebSite.APIServer.Application.Helper;

namespace CnGalWebSite.UnitTest;

[TestFixture]
public class HtmlSanitizerTests
{
    [TestCase("article"), TestCase("aside"), TestCase("footer"), TestCase("header")]
    [TestCase("main"), TestCase("nav"), TestCase("section")]
    public void PreservesSemanticContainerContent(string tag)
    {
        var body = Clean($"<{tag}><h2>Title</h2><p>Body &amp; text</p>"
            + $"<img src='/ok.png' onerror='alert(1)'><script>alert(1)</script></{tag}>");
        Assert.That(body.QuerySelector(tag)!.TextContent, Is.EqualTo("TitleBody & text"));
        Assert.That(body.QuerySelector("img")!.GetAttribute("src"), Is.EqualTo("/ok.png"));
        Assert.That(body.QuerySelectorAll("script,[onerror]"), Is.Empty);
    }

    [TestCase("HTTPS://PLAYER.BILIBILI.COM/player.html", true)]
    [TestCase("//player.bilibili.com/player.html", true)]
    [TestCase("https://b23.tv/example", true)]
    [TestCase("https://bilibili.com:443/", true)]
    [TestCase("https://evilbilibili.com/", false)]
    [TestCase("https://bilibili.com.evil.example/", false)]
    [TestCase("https://bilibili.com@evil.example/", false)]
    [TestCase("https://evil.example@bilibili.com/", false)]
    [TestCase("https://bilibili.com:444/", false)]
    [TestCase("https://bilibili.com./", false)]
    [TestCase("https://bilibili.com\\@evil.example/", false)]
    [TestCase("https://bili&#9;bili.com/", false)]
    [TestCase("https://bilibili.com&#10;@evil.example/", false)]
    [TestCase("https://%62ilibili.com/", false)]
    public void EnforcesInitialIframeOrigin(string url, bool allowed)
    {
        var frame = Clean($"<iframe src='{url}'></iframe>").QuerySelector("iframe");
        Assert.That(frame != null, Is.EqualTo(allowed));
        if (!allowed) return;
        Assert.That(frame!.GetAttribute("loading"), Is.EqualTo("lazy"));
        Assert.That(frame.GetAttribute("allow"), Is.EqualTo("fullscreen"));
        Assert.That(frame.GetAttribute("referrerpolicy"), Is.EqualTo("strict-origin-when-cross-origin"));
    }

    private static IElement Clean(string html)
    {
        var result = HtmlSanitizerHelper.Sanitize(html);
        Assert.That(HtmlSanitizerHelper.Sanitize(result), Is.EqualTo(result), "Idempotence");
        return new HtmlParser().ParseDocument(result).Body!;
    }

    [TestCase("line-through")]
    [TestCase("underline")]
    public void PreservesTextDecoration(string value)
    {
        var body = Clean($"<span style='text-decoration:{value};position:fixed'>text</span>");
        Assert.That(body.QuerySelector("span")!.GetAttribute("style"), Does.Contain(value).And.Not.Contain("position"));
    }

    [Test]
    public void PreservesMarkdownAndSafeFormatting()
    {
        var markdown = "~~deleted~~\n\n- [x] done\n- [ ] todo\n\n| A | B |\n| :--- | ---: |\n| a | b |";
        var body = Clean(Markdown.ToHtml(markdown, new MarkdownPipelineBuilder().UseAdvancedExtensions().Build()));
        Assert.That(body.QuerySelector("del")!.TextContent, Is.EqualTo("deleted"));
        Assert.That(body.QuerySelectorAll("input[disabled]"), Has.Length.EqualTo(2));
        Assert.That(body.QuerySelectorAll("input[checked]"), Has.Length.EqualTo(1));
        Assert.That(body.QuerySelector("th")!.GetAttribute("style"), Does.Contain("left"));
        var colors = Clean("<span style='color:red;background-color:yellow;font-weight:bold;font-style:italic'>text</span>");
        Assert.That(colors.QuerySelector("span")!.GetAttribute("style"),
            Does.Contain("color").And.Contain("background-color").And.Contain("bold").And.Contain("italic"));
    }

    [TestCase("/small.png 1x, //cdn.example/large.png 2x")]
    [TestCase("small.png 400w, https://cdn.example/large.png 800w")]
    [TestCase("https://cdn.example/a,b.png 1x, /large.png 2x")]
    public void KeepsFallbackWhileSrcsetSupportIsDeferred(string value)
    {
        var body = Clean($"<picture><source srcset='{value}' sizes='50vw'><img src='/fallback.png'></picture>");
        var source = body.QuerySelector("source")!;
        Assert.That(source.HasAttribute("srcset"), Is.False);
        Assert.That(source.HasAttribute("sizes"), Is.False);
        Assert.That(body.QuerySelector("img")!.GetAttribute("src"), Is.EqualTo("/fallback.png"));
    }

    [TestCase("javascript:alert(1)")]
    [TestCase("JaVaScRiPt:alert(1)")]
    [TestCase("java&#x09;script:alert(1)")]
    [TestCase("java&#x0a;script:alert(1)")]
    [TestCase("&#106;avascript:alert(1)")]
    [TestCase("data:image/png;base64,AAAA")]
    [TestCase("blob:https://example.com/id")]
    [TestCase("mailto:test@example.com")]
    [TestCase("file:///test.png")]
    public void RemovesUnsafeCandidatesInEveryPosition(string url)
    {
        foreach (var candidates in new[] { $"{url} 1x, /ok.png 2x", $"/ok.png 1x, {url} 2x" })
        {
            var image = Clean($"<img srcset='{candidates}'>").QuerySelector("img")!;
            Assert.That(image.HasAttribute("srcset"), Is.False);
        }
        var body = Clean($"<picture><source srcset='{url} 1x'><img src='/fallback.png'></picture>");
        Assert.That(body.QuerySelector("source")!.HasAttribute("srcset"), Is.False);
        Assert.That(body.QuerySelector("img")!.GetAttribute("src"), Is.EqualTo("/fallback.png"));
    }

    [Test]
    public void RejectsMalformedDescriptorsAndRestrictsAttributeOwners()
    {
        var body = Clean("<img srcset='/bad.png 2q, /ok.png 2x'>"
            + "<div srcset='/bad.png' sizes='100vw' crossorigin='anonymous'>text</div>"
            + "<video><source src='/ok.mp4' srcset='/bad.png' sizes='100vw'></video>");
        Assert.That(body.QuerySelector("img")!.HasAttribute("srcset"), Is.False);
        Assert.That(body.QuerySelectorAll("div[srcset],div[sizes],div[crossorigin],video source[srcset],video source[sizes]"), Is.Empty);
    }

    [TestCase("img")]
    [TestCase("source")]
    [TestCase("audio")]
    [TestCase("video")]
    public void MediaSchemesAreNarrowerThanLinkSchemes(string tag)
    {
        foreach (var url in new[] { "mailto:test@example.com", "javascript:alert(1)", "java&#x09;script:alert(1)",
            "java&#x0a;script:alert(1)", "&#106;avascript:alert(1)", "data:text/plain,test", "blob:https://example.com/id" })
            Assert.That(Clean($"<{tag} src='{url}' poster='{url}'></{tag}>").QuerySelectorAll("[src],[poster]"), Is.Empty);
        foreach (var url in new[] { "/ok.png", "../ok.png", "//cdn.example/ok.png", "https://cdn.example/ok.png", "http://cdn.example/ok.png" })
            Assert.That(Clean($"<{tag} src='{url}'></{tag}>").QuerySelector(tag)!.GetAttribute("src"), Is.EqualTo(url));
        Assert.That(Clean("<a href='mailto:test@example.com'>mail</a>").QuerySelector("a")!.GetAttribute("href"), Is.EqualTo("mailto:test@example.com"));
    }

    [TestCase("")]
    [TestCase("anonymous")]
    [TestCase("ANONYMOUS")]
    public void NormalizesAnonymousCrossOrigin(string value)
    {
        foreach (var tag in new[] { "img", "audio", "video" })
            Assert.That(Clean($"<{tag} crossorigin='{value}'></{tag}>").QuerySelector(tag)!.GetAttribute("crossorigin"), Is.EqualTo("anonymous"));
    }

    [TestCase("use-credentials")]
    [TestCase("invalid")]
    [TestCase(" anonymous ")]
    public void RemovesUnsupportedCrossOrigin(string value)
    {
        Assert.That(Clean($"<video crossorigin='{value}'></video>").QuerySelectorAll("[crossorigin]"), Is.Empty);
    }

    [Test]
    public void PreservesSecurityBoundaries()
    {
        var body = Clean("<script>alert(1)</script><img src='/ok.png' onerror='alert(1)'>"
            + "<span style='color:red;background-image:url(javascript:alert(1));position:fixed'>text</span>"
            + "<iframe src='https://evilbilibili.com/player' srcdoc='<script>alert(1)</script>'></iframe>"
            + "<iframe src='//player.bilibili.com/player.html' srcdoc='bad'></iframe>"
            + "<input type='text'><input type='checkbox' checked onclick='alert(1)'>"
            + "<video aria-hidden='true'></video><a href='https://example.com' target='_blank'>link</a>");
        Assert.That(body.QuerySelectorAll("script,[onerror],[onclick],[srcdoc],[aria-hidden],input[type=text]"), Is.Empty);
        Assert.That(body.QuerySelectorAll("iframe"), Has.Length.EqualTo(1));
        Assert.That(body.QuerySelector("iframe")!.GetAttribute("src"), Does.StartWith("https://player.bilibili.com/"));
        Assert.That(body.QuerySelector("span")!.GetAttribute("style"), Does.Not.Contain("url").And.Not.Contain("position"));
        Assert.That(body.QuerySelector("a")!.GetAttribute("rel"), Is.EqualTo("noopener noreferrer"));
    }
}
