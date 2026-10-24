using System.Security.Claims;
using System.Security.Cryptography;
using CnGalWebSite.SDK.MainSite.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CnGalWebSite.SDK.MainSite.AspNet.Auth;

/// <summary>单实例内的登录会话令牌。仅登录创建；记录丢失或进程重启后必须重新登录。</summary>
public sealed class SessionTokenStore(IOptions<MainSiteOidcOptions> options, ILogger<SessionTokenStore> logger) : IDisposable
{
    private const string SessionClaim = "urn:cngal:main-site:session";
    private readonly MemoryCache _sessions = new(new MemoryCacheOptions());
    private readonly CancellationTokenSource _lifetime = new();

    internal void Create(ClaimsPrincipal principal, AuthenticationProperties properties, ClaimsPrincipal? previous = null)
    {
        var accessToken = properties.GetTokenValue("access_token");
        if (principal.Identity?.IsAuthenticated != true || string.IsNullOrWhiteSpace(principal.FindFirst("sub")?.Value)
            || !DateTimeOffset.TryParse(properties.GetTokenValue("expires_at"), out var expiresAt)
            || string.IsNullOrWhiteSpace(accessToken) || expiresAt <= DateTimeOffset.UtcNow)
            throw new InvalidOperationException("登录票据缺少有效的用户令牌");

        Session? previousSession;
        lock (_sessions)
        {
            var limit = Math.Max(1, options.Value.MaxSessions);
            var previousKey = previous?.FindFirst(SessionClaim)?.Value;
            previousSession = previousKey is not null && _sessions.TryGetValue(previousKey, out Session? existing) ? existing : null;
            var subject = principal.FindFirst("sub")!;
            var userKey = (subject.Value, subject.Issuer);
            var userSessions = 0;
            // 按实际记录计数并清理过期项；替换只抵扣属于同一用户的旧会话。
            foreach (var sessionKey in _sessions.Keys)
                if (_sessions.TryGetValue(sessionKey, out Session? session)
                    && !ReferenceEquals(session, previousSession) && session!.UserKey == userKey) userSessions++;
            if (userSessions >= Math.Max(1, options.Value.MaxSessionsPerUser))
                throw new InvalidOperationException("当前账号的登录会话数量已达上限，请先退出其他设备或浏览器中的登录。");
            if (_sessions.Count >= limit && previousSession is null)
                throw new InvalidOperationException("当前登录会话容量已满，暂时无法建立新会话。");

            CreateSession(principal, properties, accessToken, expiresAt);
            // 新会话成功保存后才移除旧会话；容量检查和替换不得被并发登录穿透。
            if (previousSession is not null) _sessions.Remove(previousKey!);
        }
        previousSession?.Invalidate();
    }

    private void CreateSession(ClaimsPrincipal principal, AuthenticationProperties properties, string accessToken, DateTimeOffset expiresAt)
    {
        // 会话编号由本应用签发，不能采用 IdP 提供的同名声明，也不能按 sub 合并独立登录。
        foreach (var identity in principal.Identities)
            foreach (var claim in identity.FindAll(SessionClaim).ToArray()) identity.RemoveClaim(claim);
        var key = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        ((ClaimsIdentity)principal.Identity!).AddClaim(new Claim(SessionClaim, key));
        var tokens = new Tokens(principal, accessToken, properties.GetTokenValue("refresh_token") ?? string.Empty,
            properties.GetTokenValue("id_token") ?? string.Empty, expiresAt, _lifetime.Token);
        var sessionExpiresAt = properties.ExpiresUtc ?? DateTimeOffset.UtcNow.AddDays(Math.Max(1, options.Value.PersistentLoginDays));
        _sessions.Set(key, new Session(tokens, sessionExpiresAt), new MemoryCacheEntryOptions
        {
            AbsoluteExpiration = sessionExpiresAt
        }.RegisterPostEvictionCallback((_, value, _, _) => ((Session)value!).Invalidate()));

        // Cookie 保留登录及登出所需信息；访问令牌和刷新令牌只保存在服务器。
        properties.StoreTokens(properties.GetTokens().Where(token => token.Name is not ("access_token" or "refresh_token")).ToArray());
    }

    internal void Remove(ClaimsPrincipal? principal)
    {
        var key = principal?.FindFirst(SessionClaim)?.Value;
        if (key is null || !_sessions.TryGetValue(key, out Session? session)) return;
        session!.Invalidate();
        _sessions.Remove(key);
    }

    internal async Task<Tokens?> GetAsync(ClaimsPrincipal principal, string? rejectedAccessToken,
        Func<string, CancellationToken, Task<TokenRefreshResult?>> refresh, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var key = principal.FindFirst(SessionClaim)?.Value;
        if (key is null || !_sessions.TryGetValue(key, out Session? session)) return null;
        TaskCompletionSource<Tokens?>? completion = null;
        Task<Tokens?> pending;
        Tokens expected;
        lock (session!)
        {
            if (session.Current is not { } current || current.Changed.IsCancellationRequested
                || !SameSubject(principal, current.Principal)) return null;
            expected = current;
            if (session.Refresh is not null) pending = session.Refresh;
            else
            {
                var now = DateTimeOffset.UtcNow;
                if (current.ExpiresAt > now && (rejectedAccessToken is null
                    ? now.AddMinutes(Math.Max(1, options.Value.RefreshLeadTimeMinutes)) < current.ExpiresAt
                    : rejectedAccessToken != current.AccessToken)) return current;
                completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
                pending = session.Refresh = completion.Task;
            }
        }
        if (completion is not null) _ = RefreshAsync(key, session, expected, refresh, completion);
        // 单个请求或页面关闭只退出等待，不取消其他持有者共用的刷新。
        var tokens = await pending.WaitAsync(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        return tokens;
    }

    private async Task RefreshAsync(string key, Session session, Tokens expected,
        Func<string, CancellationToken, Task<TokenRefreshResult?>> refresh, TaskCompletionSource<Tokens?> completion)
    {
        Tokens? replacement = null;
        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(expected.Changed);
            timeout.CancelAfter(TimeSpan.FromSeconds(30));
            var result = await refresh(expected.RefreshToken, timeout.Token);
            if (result is not null)
            {
                var principal = new ClaimsPrincipal(result.Value.ClaimsIdentity.Clone());
                if (SameSubject(principal, expected.Principal))
                {
                    foreach (var claim in principal.FindAll(SessionClaim).ToArray()) ((ClaimsIdentity)principal.Identity!).RemoveClaim(claim);
                    ((ClaimsIdentity)principal.Identity!).AddClaim(new Claim(SessionClaim, key));
                    replacement = new Tokens(principal, result.Value.AccessToken, result.Value.RefreshToken,
                        result.Value.IdToken, DateTimeOffset.UtcNow.AddSeconds(result.Value.ExpiresIn), _lifetime.Token);
                }
            }
        }
        catch (Exception ex) when (CookieOidcRefresher.IsTransient(ex)
            && !expected.Changed.IsCancellationRequested && expected.ExpiresAt > DateTimeOffset.UtcNow)
        {
            replacement = expected;
            logger.LogWarning("会话令牌刷新暂时失败，保留仍有效的令牌");
        }
        catch (OperationCanceledException) { }
        catch (Exception) { logger.LogWarning("会话令牌刷新失败，需要重新登录"); }

        Tokens? current;
        Tokens? discarded = null;
        lock (session)
        {
            if (ReferenceEquals(session.Current, expected))
            {
                if (replacement?.ExpiresAt <= DateTimeOffset.UtcNow || session.ExpiresAt <= DateTimeOffset.UtcNow
                    || expected.Changed.IsCancellationRequested)
                {
                    discarded = replacement;
                    replacement = null;
                }
                session.Current = replacement;
                replacement = null;
                session.Refresh = null;
            }
            current = session.Current;
        }
        replacement?.Invalidate();
        discarded?.Invalidate();
        if (!ReferenceEquals(current, expected)) expected.Invalidate();
        try { if (current is null) _sessions.Remove(key); }
        catch (ObjectDisposedException) { }
        completion.TrySetResult(current);
    }

    private static bool SameSubject(ClaimsPrincipal principal, ClaimsPrincipal expected)
    {
        var subject = principal.FindFirst("sub");
        var previous = expected.FindFirst("sub");
        return principal.Identity?.IsAuthenticated == true && !string.IsNullOrWhiteSpace(subject?.Value)
            && subject.Value == previous?.Value && subject.Issuer == previous.Issuer;
    }

    public void Dispose()
    {
        _lifetime.Cancel();
        _sessions.Dispose();
        _lifetime.Dispose();
    }

    private sealed class Session(Tokens tokens, DateTimeOffset expiresAt)
    {
        internal (string Subject, string Issuer) UserKey { get; } =
            (tokens.Principal.FindFirst("sub")!.Value, tokens.Principal.FindFirst("sub")!.Issuer);
        internal DateTimeOffset ExpiresAt { get; } = expiresAt;
        internal Tokens? Current = tokens;
        internal Task<Tokens?>? Refresh;

        internal void Invalidate()
        {
            Tokens? previous;
            lock (this) { previous = Current; Current = null; }
            previous?.Invalidate();
        }
    }

    internal sealed class Tokens
    {
        private readonly CancellationTokenSource _changed;
        private int _invalidated;
        internal Tokens(ClaimsPrincipal principal, string accessToken, string refreshToken,
            string idToken, DateTimeOffset expiresAt, CancellationToken lifetime)
        {
            _changed = CancellationTokenSource.CreateLinkedTokenSource(lifetime);
            Changed = _changed.Token;
            Principal = new ClaimsPrincipal(principal.Identities.Select(identity => identity.Clone()));
            AccessToken = accessToken;
            RefreshToken = refreshToken;
            IdToken = idToken;
            ExpiresAt = expiresAt;
        }
        internal ClaimsPrincipal Principal { get; }
        internal string AccessToken { get; }
        internal string RefreshToken { get; }
        internal string IdToken { get; }
        internal DateTimeOffset ExpiresAt { get; }
        internal CancellationToken Changed { get; }

        internal void Invalidate()
        {
            if (Interlocked.Exchange(ref _invalidated, 1) != 0) return;
            try { _changed.Cancel(); }
            finally { _changed.Dispose(); }
        }
    }
}
