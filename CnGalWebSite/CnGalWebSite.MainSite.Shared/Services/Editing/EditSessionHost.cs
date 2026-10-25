namespace CnGalWebSite.MainSite.Shared.Services.Editing;

/// <summary>Owns the session for one editing page, including overlapping route changes.</summary>
public sealed class EditSessionHost<TSession>(Action changed) : IAsyncDisposable
    where TSession : class, IEditSession
{
    private TSession? _current;
    private string? _key;
    private long _generation;
    private bool _disposed;
    private Task? _disposal;

    public TSession Current => _current ?? throw new InvalidOperationException("No edit session has been created");

    public async Task SetAsync(string key, Func<TSession> create)
    {
        if (_disposed || _key == key) return;
        _key = key;
        var generation = ++_generation;
        if (_current is not null)
        {
            _current.Changed -= changed;
            await _current.DisposeAsync();
        }
        if (_disposed || generation != _generation) return;
        _current = create();
        _current.Changed += changed;
        await _current.InitializeAsync();
    }

    public ValueTask DisposeAsync() => new(_disposal ??= DisposeCoreAsync());

    private async Task DisposeCoreAsync()
    {
        _disposed = true;
        ++_generation;
        if (_current is not null)
        {
            _current.Changed -= changed;
            await _current.DisposeAsync();
        }
    }
}
