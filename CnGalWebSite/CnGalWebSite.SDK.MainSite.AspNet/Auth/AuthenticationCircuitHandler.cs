using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.Extensions.Logging;

namespace CnGalWebSite.SDK.MainSite.AspNet.Auth;

internal sealed class AuthenticationCircuitHandler(
    AuthenticationStateProvider authenticationStateProvider,
    CircuitTokenStore tokenStore,
    ILogger<AuthenticationCircuitHandler> logger) : CircuitHandler, IDisposable
{
    private readonly CancellationTokenSource _lifetime = new();
    private long _changeVersion;
    private bool _disposed;

    public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        authenticationStateProvider.AuthenticationStateChanged += AuthenticationChanged;
        return Task.CompletedTask;
    }

    public override Task OnConnectionUpAsync(Circuit circuit, CancellationToken cancellationToken)
        => UpdateAsync(authenticationStateProvider.GetAuthenticationStateAsync(), retryInitialization: true);

    private void AuthenticationChanged(Task<AuthenticationState> state)
        => _ = UpdateAsync(state);

    private async Task UpdateAsync(Task<AuthenticationState> stateTask, bool retryInitialization = false)
    {
        var version = Interlocked.Increment(ref _changeVersion);
        try
        {
            var state = await stateTask.WaitAsync(_lifetime.Token);
            if (!_disposed && version == Volatile.Read(ref _changeVersion)) await tokenStore.SetCircuitUserAsync(state.User, retryInitialization);
        }
        catch (OperationCanceledException) when (_disposed) { }
        catch (Exception)
        {
            if (!_disposed && version == Volatile.Read(ref _changeVersion)) tokenStore.Clear();
            logger.LogWarning("无法取得 circuit 认证状态，旧 SDK 身份已失效");
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Interlocked.Increment(ref _changeVersion);
        authenticationStateProvider.AuthenticationStateChanged -= AuthenticationChanged;
        _lifetime.Cancel();
        _lifetime.Dispose();
    }
}
