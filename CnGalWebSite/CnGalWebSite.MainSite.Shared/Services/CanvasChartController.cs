using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace CnGalWebSite.MainSite.Shared.Services;

/// <summary>Owns the module and canvas controller for one chart component.</summary>
public sealed class CanvasChartController(IJSRuntime js, string modulePath) : IAsyncDisposable
{
    private readonly SemaphoreSlim _gate = new(1, 1);
    private IJSObjectReference? _module, _controller;
    private bool _disposed;
    private Task? _disposal;
    public string? ErrorMessage { get; private set; }

    /// <returns>Whether a newly displayed error requires a component render.</returns>
    public async Task<bool> UpdateAsync(ElementReference root, ElementReference canvas, params object?[] data)
    {
        await _gate.WaitAsync();
        try
        {
            if (_disposed || ErrorMessage is not null) return false;
            _module ??= await js.InvokeAsync<IJSObjectReference>("import", modulePath);
            if (_disposed) return false;
            _controller ??= await _module.InvokeAsync<IJSObjectReference>("create", root, canvas);
            if (_disposed) return false;
            await _controller.InvokeVoidAsync("update", data);
            return false;
        }
        catch (JSException ex)
        {
            Console.Error.WriteLine($"{modulePath} render: {ex.Message}");
            if (_disposed) return false;
            ErrorMessage = "图表加载失败";
            return true;
        }
        catch (OperationCanceledException) when (_disposed) { return false; }
        finally { _gate.Release(); }
    }

    public async Task RetryAsync()
    {
        await _gate.WaitAsync();
        try
        {
            if (_disposed) return;
            await ReleaseControllerAsync();
            ErrorMessage = null;
        }
        finally { _gate.Release(); }
    }

    private async Task ReleaseControllerAsync()
    {
        var controller = _controller;
        _controller = null;
        if (controller is null) return;
        try { await controller.InvokeVoidAsync("dispose"); }
        finally { await controller.DisposeAsync(); }
    }

    public ValueTask DisposeAsync() => new(_disposal ??= DisposeCoreAsync());

    private async Task DisposeCoreAsync()
    {
        _disposed = true;
        await _gate.WaitAsync();
        try
        {
            try { await ReleaseControllerAsync(); }
            finally { if (_module is not null) await _module.DisposeAsync(); }
        }
        catch (JSDisconnectedException) { }
        catch (TaskCanceledException) { }
        finally { _module = null; _gate.Release(); }
    }
}
