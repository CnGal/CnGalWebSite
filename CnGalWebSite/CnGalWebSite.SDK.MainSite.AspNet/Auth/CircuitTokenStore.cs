namespace CnGalWebSite.SDK.MainSite.AspNet.Auth;

/// <summary>
/// 电路级（Blazor InteractiveServer SignalR 连接）的 token 内存缓存。
///
/// 由于 InteractiveServer 的 SignalR hub 调用无法通过 Set-Cookie 将刷新后的 token
/// 持久化回客户端，本类在电路生命周期内缓存刷新后的 token，确保 AccessTokenHandler
/// 能够读取到最新 token 而非 Cookie 中的过期 token。
/// </summary>
public sealed class CircuitTokenStore
{
    private string? _accessToken;
    private string? _refreshToken;
    private DateTimeOffset _expiresAt = DateTimeOffset.MinValue;

    public string? AccessToken => _accessToken;
    public string? RefreshToken => _refreshToken;
    public DateTimeOffset ExpiresAt => _expiresAt;

    /// <summary>
    /// 判断 token 是否即将过期，需要主动刷新。
    /// </summary>
    /// <param name="refreshLeadTime">提前刷新窗口（= RefreshLeadTimeMinutes 换算的 TimeSpan）</param>
    public bool NeedsRefresh(TimeSpan refreshLeadTime)
    {
        return _accessToken is not null
            && DateTimeOffset.UtcNow + refreshLeadTime >= _expiresAt;
    }

    /// <summary>
    /// 存入刷新后的 token 集合。
    /// </summary>
    public void StoreTokens(string accessToken, string refreshToken, DateTimeOffset expiresAt)
    {
        _accessToken = accessToken;
        _refreshToken = refreshToken;
        _expiresAt = expiresAt;
    }

    /// <summary>
    /// 清空缓存（登出时调用）。
    /// </summary>
    public void Clear()
    {
        _accessToken = null;
        _refreshToken = null;
        _expiresAt = DateTimeOffset.MinValue;
    }
}
