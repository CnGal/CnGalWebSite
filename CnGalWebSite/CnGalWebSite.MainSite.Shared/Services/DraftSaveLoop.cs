namespace CnGalWebSite.MainSite.Shared.Services;

/// <summary>A single awaited save loop. Submit/discard share its gate so a save cannot resurrect a removed draft.</summary>
public sealed class DraftSaveLoop : IAsyncDisposable
{
    private readonly CancellationTokenSource _lifetime = new();
    private readonly SemaphoreSlim _gate = new(1, 1);
    private Task _loop = Task.CompletedTask;

    public void Start(Func<CancellationToken, Task> save)
    {
        if (!_loop.IsCompleted) throw new InvalidOperationException("Draft save loop already running");
        _loop = RunAsync(save);
    }

    private async Task RunAsync(Func<CancellationToken, Task> save)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(10));
        try
        {
            while (await timer.WaitForNextTickAsync(_lifetime.Token))
            {
                using var lease = await EnterAsync(_lifetime.Token);
                await save(_lifetime.Token);
            }
        }
        catch (OperationCanceledException) when (_lifetime.IsCancellationRequested) { }
    }

    public async Task<IDisposable> EnterAsync(CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);
        return new Lease(_gate);
    }

    private Task? _disposal;
    public ValueTask DisposeAsync() => new(_disposal ??= DisposeCoreAsync());
    private async Task DisposeCoreAsync()
    {
        await _lifetime.CancelAsync();
        try { await _loop; }
        finally { _lifetime.Dispose(); }
    }

    private sealed class Lease(SemaphoreSlim gate) : IDisposable
    {
        private SemaphoreSlim? _gate = gate;
        public void Dispose() => Interlocked.Exchange(ref _gate, null)?.Release();
    }
}
