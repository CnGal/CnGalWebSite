using System.Collections.Concurrent;
using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Models;
using Microsoft.Extensions.Caching.Memory;

namespace CnGalWebSite.SDK.MainSite.Infrastructure;

/// <summary>Anonymous-only public cache. Identity and invalidation generations are captured per query, never per circuit.</summary>
public sealed class PublicDataCache(IMemoryCache storage, ICurrentUserAccessor user)
{
    private static readonly ConcurrentDictionary<string, Generation> Generations = new();
    private string? _source;
    private Generation? _generation;

    public PublicDataCache ForSource(Uri? source)
    {
        var cache = new PublicDataCache(storage, user);
        if (source is null) return cache;
        cache._source = source.AbsoluteUri;
        cache._generation = Generations.GetOrAdd(cache._source, static _ => new());
        return cache;
    }

    public async ValueTask<ReadScope> BeginQueryAsync(CancellationToken cancellationToken)
    {
        var state = await user.GetIdentityStateAsync(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        return new(_generation is null ? 0 : Volatile.Read(ref _generation.Value),
            user.IdentityVersion, state == CurrentIdentityState.Anonymous);
    }

    private bool CanReadOrWrite(ReadScope scope) => scope.Anonymous && _generation is not null
        && scope.CacheVersion == Volatile.Read(ref _generation.Value)
        && scope.IdentityVersion == user.IdentityVersion
        && user.GetIdentityState() == CurrentIdentityState.Anonymous;

    private object Key(object key, ReadScope scope) => (_source, scope.CacheVersion, key);

    private bool IsGenerationCurrent(ReadScope scope) => _generation is null
        ? scope.CacheVersion == 0
        : scope.CacheVersion == Volatile.Read(ref _generation.Value);

    private bool IsCompletionValid(ReadScope scope)
    {
        if (scope.IdentityVersion != user.IdentityVersion)
        {
            return false;
        }

        // Anonymous responses can outlive a write. Refuse to surface data that
        // was fetched before an invalidation; callers show a retryable error.
        return !scope.Anonymous
            || (IsGenerationCurrent(scope) && user.GetIdentityState() == CurrentIdentityState.Anonymous);
    }

    public SdkResult<T> Complete<T>(T value, ReadScope scope)
    {
        if (scope.IdentityVersion != user.IdentityVersion)
        {
            return SdkResult<T>.Fail("SDK_IDENTITY_CHANGED", "会话身份已改变，请重新加载页面");
        }

        return IsCompletionValid(scope)
            ? SdkResult<T>.Ok(value)
            : SdkResult<T>.Fail("SDK_DATA_CHANGED", "数据已在请求期间更新，请重新加载页面");
    }

    public SdkResult<T> CompleteResult<T>(SdkResult<T> result, ReadScope scope)
    {
        if (scope.IdentityVersion != user.IdentityVersion)
        {
            return SdkResult<T>.Fail("SDK_IDENTITY_CHANGED", "会话身份已改变，请重新加载页面");
        }

        return IsCompletionValid(scope)
            ? result
            : SdkResult<T>.Fail("SDK_DATA_CHANGED", "数据已在请求期间更新，请重新加载页面");
    }

    public bool TryGetValue<T>(object key, out T? value, ReadScope scope)
    {
        value = default;
        return CanReadOrWrite(scope) && storage.TryGetValue(Key(key, scope), out value) && CanReadOrWrite(scope);
    }

    public T Set<T>(object key, T value, TimeSpan duration, ReadScope scope)
    {
        if (CanReadOrWrite(scope)) storage.Set(Key(key, scope), value, duration);
        return value;
    }

    public void Invalidate()
    {
        // All related lists and in-flight responses expire together; old keys age out through their short TTL.
        if (_generation is not null) Interlocked.Increment(ref _generation.Value);
    }

    public readonly record struct ReadScope(long CacheVersion, long IdentityVersion, bool Anonymous);
    private sealed class Generation { public long Value; }
}
