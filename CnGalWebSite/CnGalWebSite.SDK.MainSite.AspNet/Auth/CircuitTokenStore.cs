using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CnGalWebSite.SDK.MainSite.Abstractions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CnGalWebSite.SDK.MainSite.AspNet.Auth;

/// <summary>
/// 应用作用域中的认证快照。HTTP 请求仅用于初始化；circuit 后续以认证状态通知为准，
/// 不保留 HttpContext，也不让重连携带的旧票据覆盖内存中已经轮换的令牌。
/// </summary>
public sealed class CircuitTokenStore(
    IHttpContextAccessor httpContextAccessor,
    CookieOidcRefresher refresher,
    ILogger<CircuitTokenStore> logger) : IDisposable
{
    private readonly object _gate = new();
    private readonly CancellationTokenSource _lifetime = new();
    private AuthenticationSnapshot? _snapshot;
    private string? _sourcePrincipalKey;
    private TaskCompletionSource? _initialization;
    private long _identityVersion;
    private bool _isCircuit;
    private bool _initialized;
    private bool _ticketEstablished;
    private bool _disposed;

    internal AuthenticationSnapshot? CurrentSnapshot
    {
        get { lock (_gate) { return _snapshot; } }
    }

    internal async Task<AuthenticationSnapshot?> GetSnapshotAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        TaskCompletionSource? initialize = null;
        Task? initialization;
        CancellationToken lifetime;
        long version;
        lock (_gate)
        {
            if (_disposed) return null;
            lifetime = _lifetime.Token;
            version = _identityVersion;
            if (!_initialized && !_isCircuit)
            {
                _initialized = true;
                initialize = new(TaskCreationOptions.RunContinuationsAsynchronously);
                _initialization = initialize;
            }
            initialization = _initialization?.Task;
        }

        using var waiting = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, lifetime);
        if (initialize is not null)
        {
            _ = InitializeRequestAsync(httpContextAccessor.HttpContext, version, initialize);
        }
        while (initialization is not null)
        {
            await initialization.WaitAsync(waiting.Token);
            lock (_gate)
            {
                if (ReferenceEquals(initialization, _initialization?.Task)) break;
                initialization = _initialization?.Task;
            }
        }

        var snapshot = CurrentSnapshot;
        return snapshot is null ? null : await RefreshAsync(snapshot, waiting.Token, forceRefresh: false);
    }

    private async Task InitializeRequestAsync(HttpContext? context, long version, TaskCompletionSource completion)
    {
        try
        {
            var snapshot = await ReadTicketAsync(context, expectedPrincipalKey: null, version);
            lock (_gate)
            {
                if (!_disposed && !_isCircuit && version == _identityVersion)
                {
                    _snapshot = snapshot;
                    _sourcePrincipalKey = snapshot?.PrincipalKey;
                    _ticketEstablished = snapshot is not null;
                    snapshot = null;
                }
            }
            snapshot?.Invalidate();
        }
        catch (Exception)
        {
            // 认证异常可能携带敏感协议数据，不将令牌或异常正文写入日志。
            logger.LogWarning("无法从当前认证票据初始化 SDK 身份，跳过用户缓存");
        }
        finally { completion.TrySetResult(); }
    }

    internal async Task SetCircuitUserAsync(ClaimsPrincipal principal, bool retryInitialization = false)
    {
        var principalKey = AuthenticationSnapshot.GetPrincipalKey(principal);
        AuthenticationSnapshot? previous;
        TaskCompletionSource completion;
        long version;
        lock (_gate)
        {
            if (_disposed) return;
            _isCircuit = true;
            _initialized = true;
            // 仅连接恢复可重试从未建立成功的票据；失效或刷新中的旧凭据不能复活。
            if (principalKey == _sourcePrincipalKey && (_ticketEstablished || !retryInitialization || principalKey is null))
                return;
            _sourcePrincipalKey = principalKey;
            _ticketEstablished = false;
            version = ++_identityVersion;
            previous = _snapshot;
            _snapshot = null;
            completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
            _initialization?.TrySetResult();
            _initialization = completion;
        }
        previous?.Invalidate();
        AuthenticationSnapshot? snapshot = null;
        try
        {
            if (principalKey is null) return;
            snapshot = await ReadTicketAsync(httpContextAccessor.HttpContext, principalKey, version);
            lock (_gate)
            {
                if (!_disposed && version == _identityVersion)
                {
                    _snapshot = snapshot;
                    _ticketEstablished = snapshot is not null;
                    snapshot = null;
                }
            }
        }
        catch (Exception)
        {
            logger.LogWarning("无法为 circuit 身份建立可信令牌快照，跳过用户缓存");
        }
        finally
        {
            snapshot?.Invalidate();
            completion.TrySetResult();
        }
    }

    private async Task<AuthenticationSnapshot?> ReadTicketAsync(HttpContext? context, string? expectedPrincipalKey, long version)
    {
        AuthenticationSnapshot? Missing(string reason)
        {
            logger.LogDebug("SDK 身份初始化未取得可信票据。Reason={Reason}", reason);
            return null;
        }
        if (context is null) return Missing("no-http-context");
        var ticket = await context.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        if (!ticket.Succeeded || ticket.Principal is null) return Missing("no-cookie-ticket");
        var principalKey = AuthenticationSnapshot.GetPrincipalKey(ticket.Principal);
        // 同时校验当前请求主体，避免把其他身份的票据绑定到 circuit。
        if (principalKey is null || principalKey != AuthenticationSnapshot.GetPrincipalKey(context.User)
            || (expectedPrincipalKey is not null && principalKey != expectedPrincipalKey)) return Missing("principal-mismatch");
        var tokens = await refresher.GetSessionTokensAsync(ticket.Principal, null, _lifetime.Token);
        return tokens is null ? Missing("session-missing") : new AuthenticationSnapshot(tokens, version);
    }

    // 仅在 _gate 内调用；旧请求不能取得新登录代次的凭据，即使 sub 又变回原值。
    private AuthenticationSnapshot? SameIdentitySnapshot(AuthenticationSnapshot expected)
        => !_disposed && _snapshot is { } current && current.IdentityVersion == expected.IdentityVersion
            && current.ExpiresAt > DateTimeOffset.UtcNow && !current.CacheScope.IdentityChanged.IsCancellationRequested ? current : null;

    internal async Task<AuthenticationSnapshot?> RefreshAsync(AuthenticationSnapshot expected, CancellationToken cancellationToken, bool forceRefresh = true)
    {
        CancellationToken lifetime;
        lock (_gate)
        {
            if (_disposed || !ReferenceEquals(_snapshot, expected)) return SameIdentitySnapshot(expected);
            lifetime = _lifetime.Token;
        }
        using var waiting = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, lifetime);
        var tokens = await refresher.GetSessionTokensAsync(expected.Principal,
            forceRefresh ? expected.AccessToken : null, waiting.Token);
        AuthenticationSnapshot? replacement = tokens is null ? null
            : tokens.AccessToken == expected.AccessToken && !expected.CacheScope.IdentityChanged.IsCancellationRequested
                ? expected : new AuthenticationSnapshot(tokens, expected.IdentityVersion);

        AuthenticationSnapshot? current;
        var replaced = false;
        lock (_gate)
        {
            if (!_disposed && ReferenceEquals(_snapshot, expected))
            {
                // 刷新失败与取得锁之间也可能过期。
                if (ReferenceEquals(replacement, expected) && expected.ExpiresAt <= DateTimeOffset.UtcNow) replacement = null;
                replaced = !ReferenceEquals(replacement, expected);
                _snapshot = replacement;
                replacement = null;
            }
            current = SameIdentitySnapshot(expected);
        }
        replacement?.Invalidate();
        if (replaced) expected.Invalidate();
        return current;
    }

    internal void Clear()
    {
        AuthenticationSnapshot? previous;
        lock (_gate)
        {
            ++_identityVersion;
            _ticketEstablished = true;
            previous = _snapshot;
            _snapshot = null;
            _initialization?.TrySetResult();
            _initialization = null;
        }
        previous?.Invalidate();
    }

    public void Dispose()
    {
        lock (_gate)
        {
            if (_disposed) return;
            _disposed = true;
        }
        Clear();
        _lifetime.Cancel();
        _lifetime.Dispose();
    }
}

internal sealed class AuthenticationSnapshot
{
    private readonly CancellationTokenSource _changed;
    private int _invalidated;

    internal AuthenticationSnapshot(SessionTokenStore.Tokens tokens, long identityVersion)
    {
        _changed = CancellationTokenSource.CreateLinkedTokenSource(tokens.Changed);
        Principal = new ClaimsPrincipal(tokens.Principal.Identities.Select(identity => identity.Clone()));
        AccessToken = tokens.AccessToken;
        ExpiresAt = tokens.ExpiresAt;
        IdentityVersion = identityVersion;
        UserId = Principal.FindFirst("sub")!.Value;
        PrincipalKey = GetPrincipalKey(Principal)!;
        // 相同票据跨 HTTP 作用域仍可命中；令牌轮换改变代次，仅在快照更新时哈希。
        var authorizationKey = Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(new
        {
            PrincipalKey,
            TokenGeneration = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(AccessToken)))
        })));
        CacheScope = new(UserId, authorizationKey) { IdentityChanged = _changed.Token };
    }

    internal ClaimsPrincipal Principal { get; }
    internal string UserId { get; }
    internal string PrincipalKey { get; }
    internal string AccessToken { get; }
    internal DateTimeOffset ExpiresAt { get; }
    internal long IdentityVersion { get; }
    internal CurrentUserCacheScope CacheScope { get; }

    internal void Invalidate()
    {
        if (Interlocked.Exchange(ref _invalidated, 1) != 0) return;
        try { _changed.Cancel(); }
        finally { _changed.Dispose(); }
    }

    internal static string? GetPrincipalKey(ClaimsPrincipal principal)
    {
        if (principal.Identity?.IsAuthenticated != true || string.IsNullOrWhiteSpace(principal.FindFirst("sub")?.Value)) return null;
        return Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(
            principal.Identities.Select(identity => new
            {
                identity.AuthenticationType,
                identity.IsAuthenticated,
                identity.NameClaimType,
                identity.RoleClaimType,
                Claims = identity.Claims.Select(claim => new
                {
                    claim.Type, claim.Value, claim.ValueType, claim.Issuer, claim.OriginalIssuer,
                    Properties = claim.Properties.OrderBy(item => item.Key, StringComparer.Ordinal).ToArray()
                }).OrderBy(claim => JsonSerializer.Serialize(claim), StringComparer.Ordinal).ToArray()
            }))));
    }
}
