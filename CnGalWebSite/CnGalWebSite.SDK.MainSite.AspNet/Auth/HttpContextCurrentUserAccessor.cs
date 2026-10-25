using CnGalWebSite.SDK.MainSite.Abstractions;

namespace CnGalWebSite.SDK.MainSite.AspNet.Auth;

public sealed class HttpContextCurrentUserAccessor(CircuitTokenStore tokenStore) : ICurrentUserAccessor
{
    public CurrentIdentityState GetIdentityState() => tokenStore.IdentityState;
    public long IdentityVersion => tokenStore.IdentityVersion;

    public async ValueTask<CurrentIdentityState> GetIdentityStateAsync(CancellationToken cancellationToken = default)
    {
        await tokenStore.GetSnapshotAsync(cancellationToken);
        return tokenStore.IdentityState;
    }

    public string? GetCurrentUserId() => tokenStore.CurrentSnapshot?.UserId;

    public async ValueTask<string?> GetCurrentUserIdAsync(CancellationToken cancellationToken = default)
        => (await tokenStore.GetSnapshotAsync(cancellationToken))?.UserId;

    public async ValueTask<CurrentUserCacheScope?> GetCacheScopeAsync(CancellationToken cancellationToken = default)
        => (await tokenStore.GetSnapshotAsync(cancellationToken))?.CacheScope;
}
