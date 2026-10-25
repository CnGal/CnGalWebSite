using System.Text.Json;
using Blazored.LocalStorage;

namespace CnGalWebSite.MainSite.Shared.Services.Editing;

/// <summary>Draft state and serialized operations for a single domain editing session.</summary>
public sealed class EditDraftController<TDraft>(string key, ILocalStorageService storage,
    ICgToastService toast, Func<TDraft?> getData, Action<TDraft> setData) : IAsyncDisposable
    where TDraft : class
{
    private readonly EditDraftStore _store = new(storage);
    private readonly DraftSaveLoop _saveLoop = new();
    private readonly CancellationTokenSource _lifetime = new();
    private Task _initialization = Task.CompletedTask, _loading = Task.CompletedTask,
        _submission = Task.CompletedTask, _discard = Task.CompletedTask, _leaving = Task.CompletedTask;
    private Task? _disposal;
    private string? _lastSavedSnapshot;
    private bool _initialized, _disposed, _submitPending, _submitted, _readable = true;

    public event Action? Changed;
    public Func<Task> FlushInputs { get; set; } = () => Task.CompletedTask;
    public Func<Func<Task>, Task> Dispatch { get; set; } = work => work();
    public CancellationToken Token => _lifetime.Token;
    public bool IsDisposed => _disposed;
    public bool IsLoading { get; private set; } = true;
    public bool IsSubmitting { get; private set; }
    public bool HasDraftRecovered { get; private set; }
    public TDraft? OriginalData { get; private set; }

    public Task InitializeAsync(Func<Task> load)
    {
        if (_initialized || _disposed) return _initialization;
        _initialized = true;
        return _initialization = InitializeCoreAsync(load);
    }

    private async Task InitializeCoreAsync(Func<Task> load)
    {
        await LoadAsync(load);
        if (_disposed) return;
        _lastSavedSnapshot = Snapshot(getData());
        _saveLoop.Start(token => Dispatch(() => SaveCoreAsync(token)));
    }

    public Task LoadAsync(Func<Task> load) => _disposed ? Task.CompletedTask
        : !_loading.IsCompleted ? _loading : _loading = LoadCoreAsync(load);

    private async Task LoadCoreAsync(Func<Task> load)
    {
        IsLoading = true;
        NotifyChanged();
        try { await load(); }
        catch (OperationCanceledException) when (_lifetime.IsCancellationRequested) { }
        finally { IsLoading = false; NotifyChanged(); }
    }

    public async Task<TDraft> RestoreAsync(TDraft original)
    {
        OriginalData = Clone(original);
        HasDraftRecovered = false;
        _readable = true;
        var data = original;
        try
        {
            var draft = await _store.ReadAsync<TDraft>(key, Token);
            Token.ThrowIfCancellationRequested();
            if (draft is not null) { data = draft; HasDraftRecovered = true; }
        }
        catch (OperationCanceledException) when (_lifetime.IsCancellationRequested) { throw; }
        catch (Exception ex)
        {
            _readable = false;
            toast.Show("草稿读取失败，已暂停自动保存", ex.Message, "warning");
        }
        _lastSavedSnapshot = Snapshot(data);
        return data;
    }

    public Task DiscardAsync() => _disposed ? Task.CompletedTask
        : !_discard.IsCompleted ? _discard : _discard = DiscardCoreAsync();

    private async Task DiscardCoreAsync()
    {
        using var lease = await _saveLoop.EnterAsync(Token);
        if (OriginalData is null || getData() is null || _disposed || _submitted) return;
        var data = Clone(OriginalData);
        setData(data);
        await _store.RemoveAsync(key, Token);
        HasDraftRecovered = false;
        _lastSavedSnapshot = Snapshot(data);
        _readable = true;
        NotifyChanged();
    }

    public Task SubmitAsync(Func<Task> submit) => _submitPending || _submitted || _disposed
        ? _submission : _submission = SubmitCoreAsync(submit);

    private async Task SubmitCoreAsync(Func<Task> submit)
    {
        _submitPending = true;
        try
        {
            using var lease = await _saveLoop.EnterAsync(Token);
            await FlushInputs();
            Token.ThrowIfCancellationRequested();
            IsSubmitting = true;
            NotifyChanged();
            await submit();
        }
        catch (OperationCanceledException) when (_lifetime.IsCancellationRequested) { }
        catch (Exception ex) { toast.Show("提交失败，草稿已保留", ex.Message, "error"); }
        finally { _submitPending = false; IsSubmitting = false; NotifyChanged(); }
    }

    /// <summary>Called by the domain submit operation while it owns the shared gate.</summary>
    public async Task CompleteAsync()
    {
        _submitted = true;
        await _store.RemoveAsync(key, Token);
    }

    public Task SaveAndLeaveAsync(Action navigate) => _disposed ? Task.CompletedTask
        : !_leaving.IsCompleted ? _leaving : _leaving = SaveAndLeaveCoreAsync(navigate);

    private async Task SaveAndLeaveCoreAsync(Action navigate)
    {
        using var lease = await _saveLoop.EnterAsync(Token);
        await SaveCoreAsync(Token);
        if (!_disposed && !Token.IsCancellationRequested) navigate();
    }

    private async Task SaveCoreAsync(CancellationToken token)
    {
        if (_disposed || _submitted || !_readable || getData() is null || IsLoading || IsSubmitting) return;
        try
        {
            await FlushInputs();
            token.ThrowIfCancellationRequested();
            Token.ThrowIfCancellationRequested();
            var data = getData();
            var snapshot = Snapshot(data);
            if (data is null || snapshot == _lastSavedSnapshot) return;
            await _store.WriteAsync(key, data, token);
            _lastSavedSnapshot = snapshot;
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested || _lifetime.IsCancellationRequested) { }
        catch (Exception ex) { toast.Show("草稿保存失败", ex.Message, "warning"); }
    }

    private static string? Snapshot(TDraft? data) => data is null ? null : JsonSerializer.Serialize(data);
    private static TDraft Clone(TDraft data) => JsonSerializer.Deserialize<TDraft>(JsonSerializer.Serialize(data))!;
    private void NotifyChanged() { if (!_disposed) Changed?.Invoke(); }

    public ValueTask DisposeAsync() => new(_disposal ??= DisposeCoreAsync());

    private async Task DisposeCoreAsync()
    {
        _disposed = true;
        await _lifetime.CancelAsync();
        try
        {
            await Task.WhenAll(_saveLoop.DisposeAsync().AsTask(), _initialization, _loading,
                _submission, _discard, _leaving);
        }
        catch (OperationCanceledException) when (_lifetime.IsCancellationRequested) { }
        finally { _lifetime.Dispose(); }
    }
}
