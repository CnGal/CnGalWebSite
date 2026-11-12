using System.Text;
using System.Net;
using System.Net.Sockets;
using Microsoft.Playwright;
using NUnit.Framework;
using CnGalWebSite.Helper.Security;

namespace CnGalWebSite.UnitTest;

[TestFixture]
[Category("Browser")]
public class HtmlSanitizerBrowserTests
{
    [SetUp]
    public void RequireExplicitOptIn()
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("RUN_HTML_SANITIZER_BROWSER_TESTS"), "1", StringComparison.Ordinal))
            Assert.Ignore("Set RUN_HTML_SANITIZER_BROWSER_TESTS=1 to run Playwright compatibility tests.");
    }

    private sealed class FixtureServer : IAsyncDisposable
    {
        private readonly TcpListener _listener = new(IPAddress.Loopback, 0);
        private readonly CancellationTokenSource _stop = new();
        private readonly Task _loop;
        private readonly List<Task> _connections = new();
        public string Origin { get; }

        public FixtureServer()
        {
            _listener.Start();
            Origin = $"http://127.0.0.1:{((IPEndPoint)_listener.LocalEndpoint).Port}";
            _loop = RunAsync();
        }

        private async Task RunAsync()
        {
            try
            {
                while (!_stop.IsCancellationRequested)
                {
                    var client = await _listener.AcceptTcpClientAsync(_stop.Token);
                    _connections.Add(HandleAsync(client));
                }
            }
            catch (OperationCanceledException) { }
        }

        private async Task HandleAsync(TcpClient client)
        {
            using (client)
            {
                try
                {
                    await using var stream = client.GetStream();
                    using var reader = new StreamReader(stream, leaveOpen: true);
                    var request = await reader.ReadLineAsync(_stop.Token);
                    if (request == null)
                        return;
                    while (!string.IsNullOrEmpty(await reader.ReadLineAsync(_stop.Token))) { }
                    var path = request.Split(' ')[1];
                    var image = path.EndsWith(".png");
                    var audio = path.EndsWith(".wav");
                    var body = image
                        ? Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAIAAAACCAIAAAD91JpzAAAAFklEQVR4nGO4Y2QktyCFwSjlzod9GgAkqAWrQxfoeAAAAABJRU5ErkJggg==")
                        : audio ? Tone() : Encoding.UTF8.GetBytes("<!doctype html><html><body></body></html>");
                    var type = image ? "image/png" : audio ? "audio/wav" : "text/html";
                    var cors = path.Contains("deny") ? "" : "Access-Control-Allow-Origin: *\r\n";
                    var headers = Encoding.ASCII.GetBytes($"HTTP/1.1 200 OK\r\nContent-Type: {type}\r\nContent-Length: {body.Length}\r\n{cors}Connection: close\r\n\r\n");
                    await stream.WriteAsync(headers, _stop.Token);
                    await stream.WriteAsync(body, _stop.Token);
                }
                catch (OperationCanceledException) { }
                catch (IOException) { } // Browsers may cancel speculative or CORS-denied requests.
            }
        }

        public async ValueTask DisposeAsync()
        {
            await _stop.CancelAsync();
            await _loop;
            await Task.WhenAll(_connections);
            _listener.Stop();
            _stop.Dispose();
        }
    }

    private static byte[] Tone()
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);
        writer.Write(Encoding.ASCII.GetBytes("RIFF"));
        writer.Write(36 + 16000);
        writer.Write(Encoding.ASCII.GetBytes("WAVEfmt "));
        writer.Write(16);
        writer.Write((short)1);
        writer.Write((short)1);
        writer.Write(8000);
        writer.Write(16000);
        writer.Write((short)2);
        writer.Write((short)16);
        writer.Write(Encoding.ASCII.GetBytes("data"));
        writer.Write(16000);
        for (var i = 0; i < 8000; i++)
            writer.Write((short)(Math.Sin(i * Math.PI * 440 / 4000) * 1000));
        return stream.ToArray();
    }

    [TestCase("chromium", 1280)]
    [TestCase("chromium", 390)]
    [TestCase("firefox", 1280)]
    [TestCase("firefox", 390)]
    [TestCase("webkit", 1280)]
    [TestCase("webkit", 390)]
    public async Task IsolatedMediaAndFormatting(string engine, int width)
    {
        using var playwright = await Playwright.CreateAsync();
        await using var site = new FixtureServer();
        await using var media = new FixtureServer();
        var browserType = engine switch
        {
            "firefox" => playwright.Firefox,
            "webkit" => playwright.Webkit,
            _ => playwright.Chromium
        };
        await using var browser = await browserType.LaunchAsync(new() { Headless = true });
        await using var context = await browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = width, Height = 900 },
            DeviceScaleFactor = 1,
            ServiceWorkers = ServiceWorkerPolicy.Block
        });
        var requests = new List<string>();
        await context.RouteAsync("**/*", async route =>
        {
            var uri = new Uri(route.Request.Url);
            requests.Add(uri.AbsolutePath);
            if (uri.GetLeftPart(UriPartial.Authority) != site.Origin
                && uri.GetLeftPart(UriPartial.Authority) != media.Origin)
            {
                await route.AbortAsync();
                return;
            }
            await route.ContinueAsync();
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(site.Origin);
        var html = """
            <h1>Media compatibility audit</h1>
            <p id="decoration" style="text-decoration:line-through;color:red">Entry 42: text decoration</p>
            <picture><source media="(max-width:600px)" srcset="/small.png 1x, /large.png 2x">
              <img id="responsive" src="/fallback.png" width="180" height="100" alt="Entry 6445 responsive image"></picture>
            <picture><source srcset="javascript:alert(1) 1x">
              <img id="fallback" src="/fallback.png" width="180" height="100" alt="Fallback image"></picture>
            <img id="cors-ok" crossorigin="" src="http://media.test/ok.png" alt="Anonymous CORS allowed">
            <img id="cors-deny" crossorigin="anonymous" src="http://media.test/deny.png" alt="Anonymous CORS denied">
            <video id="media-ok" controls preload="auto" crossorigin="anonymous" aria-hidden="true" src="http://media.test/ok.wav"></video>
            <video id="media-deny" controls preload="auto" crossorigin="anonymous" src="http://media.test/deny.wav"></video>
            """;
        var clean = HtmlSanitizerHelper.Sanitize(html.Replace("http://media.test", media.Origin));
        Assert.That(HtmlSanitizerHelper.Sanitize(clean), Is.EqualTo(clean));
        await page.SetContentAsync(clean);
        await page.AddStyleTagAsync(new()
        {
            Content = "body{font:16px sans-serif;margin:16px}h1{font-size:24px}img,video{display:block;margin:12px 0;max-width:100%}img{image-rendering:pixelated}"
        });
        await page.WaitForFunctionAsync("document.querySelector('#responsive').naturalWidth > 0 && document.querySelector('#fallback').naturalWidth > 0");
        Assert.That(await page.Locator("#responsive").EvaluateAsync<string>("e => e.currentSrc"),
            Does.EndWith("/fallback.png"));
        Assert.That(await page.Locator("#fallback").EvaluateAsync<string>("e => e.currentSrc"), Does.EndWith("/fallback.png"));
        Assert.That(await page.Locator("#decoration").EvaluateAsync<string>("e => getComputedStyle(e).textDecorationLine"), Is.EqualTo("line-through"));
        await page.WaitForFunctionAsync("document.querySelector('#cors-ok').complete && document.querySelector('#cors-deny').complete");
        Assert.That(await page.Locator("#cors-ok").EvaluateAsync<int>("e => e.naturalWidth"), Is.GreaterThan(0));
        Assert.That(await page.Locator("#cors-deny").EvaluateAsync<int>("e => e.naturalWidth"), Is.Zero);
        const string mediaSettled = "document.querySelector('#media-ok').readyState >= 1 && (document.querySelector('#media-deny').error !== null || document.querySelector('#media-deny').readyState >= 1)";
        const string mediaState = "JSON.stringify([...document.querySelectorAll('video')].map(e => ({loaded:e.readyState >= 1, error:e.error?.code ?? null})))";
        await page.WaitForFunctionAsync(mediaSettled);
        var baseline = await context.NewPageAsync();
        await baseline.GotoAsync(site.Origin);
        await baseline.SetContentAsync($"""
            <video id="media-ok" preload="auto" crossorigin="anonymous" src="{media.Origin}/ok.wav"></video>
            <video id="media-deny" preload="auto" crossorigin="anonymous" src="{media.Origin}/deny.wav"></video>
            """);
        await baseline.WaitForFunctionAsync(mediaSettled);
        var baselineState = await baseline.EvaluateAsync<string>(mediaState);
        var actualState = await page.EvaluateAsync<string>(mediaState);
        Assert.That(actualState, Is.EqualTo(baselineState), "Sanitization must preserve native media CORS behavior");
        // Some Windows WebKit builds load cross-origin media even with anonymous CORS.
        // Compare against raw markup, and check the fetch/image CORS boundary independently.
        Assert.That(await page.EvaluateAsync<bool>("url => fetch(url).then(() => false, () => true)", $"{media.Origin}/deny.wav"), Is.True);
        if (engine != "webkit")
            Assert.That(await page.Locator("#media-deny").EvaluateAsync<bool>("e => e.error !== null"), Is.True);
        TestContext.Progress.WriteLine($"{engine}/{width}: raw={baselineState}; sanitized={actualState}");
        Assert.That(await page.Locator("#media-ok").EvaluateAsync<string>("e => e.crossOrigin"), Is.EqualTo("anonymous"));
        Assert.That(await page.Locator("#media-ok").GetAttributeAsync("aria-hidden"), Is.Null);
        await page.Locator("#media-ok").EvaluateAsync("e => { e.muted = true; return e.play(); }");
        await page.WaitForFunctionAsync("document.querySelector('#media-ok').currentTime > 0");
        var screenshots = Environment.GetEnvironmentVariable("HTML_SANITIZER_SCREENSHOTS");
        if (!string.IsNullOrEmpty(screenshots))
        {
            Directory.CreateDirectory(screenshots);
            await page.ScreenshotAsync(new() { Path = Path.Combine(screenshots, $"{engine}-{width}.png"), FullPage = true });
            await File.WriteAllTextAsync(Path.Combine(screenshots, $"{engine}-{width}-aria.txt"),
                await page.Locator("body").AriaSnapshotAsync());
            await File.WriteAllTextAsync(Path.Combine(screenshots, $"{engine}-{width}-media.txt"),
                $"raw={baselineState}\nsanitized={actualState}\n");
        }
        foreach (var candidate in new[] { "java&#x09;script:alert(1) 1x, /large.png 2x", "/bad.png 2q, /large.png 2x" })
        {
            requests.Clear();
            await page.SetContentAsync(HtmlSanitizerHelper.Sanitize($"<img srcset='{candidate}' src='/fallback.png'>"));
            await page.WaitForFunctionAsync("document.querySelector('img').complete && document.querySelector('img').naturalWidth > 0");
            Assert.That(await page.Locator("img").EvaluateAsync<string>("e => e.currentSrc"), Does.EndWith("/fallback.png"));
            Assert.That(requests, Does.Not.Contain("/1x"));
        }
    }
}
