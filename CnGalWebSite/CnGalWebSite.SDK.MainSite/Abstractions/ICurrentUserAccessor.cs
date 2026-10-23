namespace CnGalWebSite.SDK.MainSite.Abstractions;

/// <summary>
/// 提供当前用户标识的抽象访问器。
/// SDK.MainSite 纯核心层定义此接口，避免直接依赖 ASP.NET 的 IHttpContextAccessor。
/// ASP.NET 宿主项目通过 SDK.MainSite.AspNet 提供实现。
/// 控制台/非 Web 场景可提供空实现（始终返回 null），此时跳过用户相关缓存。
/// </summary>
public interface ICurrentUserAccessor
{
    string? GetCurrentUserId();

    ValueTask<string?> GetCurrentUserIdAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult(GetCurrentUserId());
    }

    ValueTask<CurrentUserCacheScope?> GetCacheScopeAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult<CurrentUserCacheScope?>(null);
    }
}

/// <summary>
/// 用户缓存隔离作用域。AuthorizationKey 应是可信身份及权限上下文的摘要，不得使用原始令牌。
/// </summary>
public sealed record CurrentUserCacheScope(string UserId, string AuthorizationKey)
{
    /// <summary>身份或令牌改变后取消；旧身份的响应不得再返回或回填。</summary>
    public CancellationToken IdentityChanged { get; init; }
}
