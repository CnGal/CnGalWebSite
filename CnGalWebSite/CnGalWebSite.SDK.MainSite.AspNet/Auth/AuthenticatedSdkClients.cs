using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;

namespace CnGalWebSite.SDK.MainSite.AspNet.Auth;

/// <summary>The application scope owns authentication handlers; HttpClientFactory owns transport handlers.</summary>
internal sealed class AuthenticatedSdkClients(
    CircuitTokenStore tokens,
    IHttpMessageHandlerFactory handlers,
    IOptionsMonitor<HttpClientFactoryOptions> options) : IDisposable
{
    private readonly List<HttpClient> _clients = [];
    private int _disposed;

    public HttpClient Create(string name)
    {
        ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposed) != 0, this);
        var auth = new AccessTokenHandler(tokens)
        {
            InnerHandler = new BorrowedTransport(handlers.CreateHandler(name))
        };
        var client = new HttpClient(auth, disposeHandler: true);
        foreach (var configure in options.Get(name).HttpClientActions) configure(client);
        _clients.Add(client);
        return client;
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0) return;
        foreach (var client in _clients) client.Dispose();
        _clients.Clear();
    }

    private sealed class BorrowedTransport(HttpMessageHandler inner) : HttpMessageHandler
    {
        private readonly HttpMessageInvoker _invoker = new(inner, disposeHandler: false);
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => _invoker.SendAsync(request, cancellationToken);
        protected override void Dispose(bool disposing)
        {
            if (disposing) _invoker.Dispose();
            base.Dispose(disposing);
        }
    }
}
