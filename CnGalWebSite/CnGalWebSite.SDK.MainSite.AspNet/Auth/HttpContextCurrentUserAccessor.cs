using CnGalWebSite.SDK.MainSite.Abstractions;

namespace CnGalWebSite.SDK.MainSite.AspNet.Auth;

public sealed class HttpContextCurrentUserAccessor(CircuitTokenStore tokenStore) : ICurrentUserAccessor
{
    public string? GetCurrentUserId() => tokenStore.CurrentSnapshot?.UserId;

    public async ValueTask<string?> GetCurrentUserIdAsync(CancellationToken cancellationToken = default)
        => (await tokenStore.GetSnapshotAsync(cancellationToken))?.UserId;

    public async ValueTask<CurrentUserCacheScope?> GetCacheScopeAsync(CancellationToken cancellationToken = default)
        => (await tokenStore.GetSnapshotAsync(cancellationToken))?.CacheScope;
}
