using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System.Text.Json;

namespace CnGalWebSite.SDK.MainSite.Infrastructure;

/// <summary>按用户隔离的有界审核缓存；分区失效使已有缓存过期并阻止在途结果回填，不中止 HTTP 请求。</summary>
public sealed class ExamineUserCache(ILogger<ExamineUserCache> logger) : IDisposable
{
    private const int UserPartitionLimit = 256;
    private const long ResponseSizeLimit = 64 * 1024 * 1024;
    private const long MinimumResponseSize = 8 * 1024;
    private static readonly TimeSpan ResponseDuration = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan UserIdleDuration = TimeSpan.FromMinutes(10);
    private readonly MemoryCache _users = new(new MemoryCacheOptions { SizeLimit = UserPartitionLimit });
    private readonly MemoryCache _responses = new(new MemoryCacheOptions { SizeLimit = ResponseSizeLimit });

    private readonly object _usersLock = new();
    private readonly object _generationsLock = new();

    internal async Task<SdkResult<T>> GetOrCreateAsync<T>(
        Uri? apiBase,
        CurrentUserCacheScope? scope,
        string requestKey,
        Func<CancellationToken, Task<SdkResult<T>>> load,
        CancellationToken cancellationToken,
        DataGeneration? generation)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (apiBase is null || scope is null || string.IsNullOrWhiteSpace(scope.UserId)
            || string.IsNullOrWhiteSpace(scope.AuthorizationKey))
        {
            return await load(cancellationToken);
        }

        var partition = (apiBase.AbsoluteUri, scope.UserId);
        Bucket bucket;
        lock (_usersLock)
        {
            if (!_users.TryGetValue(partition, out bucket!))
            {
                bucket = new Bucket();
                _users.Set(partition, bucket, new MemoryCacheEntryOptions
                {
                    Size = 1,
                    SlidingExpiration = UserIdleDuration
                }.RegisterPostEvictionCallback(static (_, value, _, _) => ((Bucket)value!).Dispose()));
            }
        }

        var request = new RequestKey(scope.AuthorizationKey, requestKey, typeof(T), generation?.Value);
        var key = (bucket, request);
        if (TryRead<T>(key, out var cached))
        {
            return cached;
        }

        var result = await load(cancellationToken);
        if (result.Success && result.Data is not null && !cancellationToken.IsCancellationRequested
            && !bucket.Token.IsCancellationRequested)
        {
            TryWrite(key, bucket.Token, scope.IdentityChanged, generation, result.Data);
        }
        return result;
    }

    private bool TryRead<T>(object key, out SdkResult<T> result)
    {
        result = null!;
        if (!_responses.TryGetValue(key, out byte[]? payload) || payload is null)
        {
            return false;
        }

        try
        {
            var data = JsonSerializer.Deserialize<T>(payload, SdkJsonSerializerOptions.Default);
            if (data is not null)
            {
                result = SdkResult<T>.Ok(data);
                return true;
            }
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException)
        {
            logger.LogWarning("审核缓存读取失败，将移除并回源。ResultType={ResultType}; ErrorType={ErrorType}",
                typeof(T).FullName, ex.GetType().Name);
        }

        _responses.Remove(key);
        return false;
    }

    private void TryWrite<T>(object key, CancellationToken expirationToken, CancellationToken identityChanged, DataGeneration? generation, T data)
    {
        byte[] payload;
        try
        {
            payload = JsonSerializer.SerializeToUtf8Bytes(data, SdkJsonSerializerOptions.Default);
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException)
        {
            logger.LogWarning("审核缓存序列化失败，将返回原始查询结果。ResultType={ResultType}; ErrorType={ErrorType}",
                typeof(T).FullName, ex.GetType().Name);
            return;
        }

        if (payload.LongLength > ResponseSizeLimit)
        {
            return;
        }

        lock (_generationsLock)
        {
            identityChanged.ThrowIfCancellationRequested();
            if (generation is not null && !IsCurrentGeneration(generation.Value)) return;
            _responses.Set(key, payload, new MemoryCacheEntryOptions
            {
                // 预算是计费大小，不是进程内存上限；最小计费同时将条目数限制在 8192 项以内。
                Size = Math.Max(MinimumResponseSize, payload.LongLength),
                AbsoluteExpirationRelativeToNow = ResponseDuration
            }.AddExpirationToken(new CancellationChangeToken(expirationToken)));
        }
    }

    internal DataGeneration? GetReviewQueueGeneration(Uri? apiBase) => GetGeneration(apiBase, null);

    internal DataGeneration? GetExamineGeneration(Uri? apiBase, long examineId) => GetGeneration(apiBase, examineId);

    public void InvalidateReviewQueue(Uri? apiBase) => AdvanceGeneration(apiBase, null);

    public void InvalidateExamine(Uri? apiBase, long examineId) => AdvanceGeneration(apiBase, examineId);

    private DataGeneration? GetGeneration(Uri? apiBase, long? examineId)
    {
        if (apiBase is null) return null;
        var key = new GenerationKey(apiBase.AbsoluteUri, examineId);
        lock (_generationsLock)
        {
            if (!_responses.TryGetValue(key, out Guid value)) value = SetGeneration(key);
            return new DataGeneration(key.ApiBase, examineId, value);
        }
    }

    private void AdvanceGeneration(Uri? apiBase, long? examineId)
    {
        if (apiBase is null) return;
        lock (_generationsLock) { SetGeneration(new GenerationKey(apiBase.AbsoluteUri, examineId)); }
    }

    private Guid SetGeneration(GenerationKey key)
    {
        // 元数据和响应共用计费预算；淘汰后生成全新 GUID，不能复用固定的默认版本。
        var value = Guid.NewGuid();
        _responses.Set(key, value, new MemoryCacheEntryOptions
        {
            Size = MinimumResponseSize,
            SlidingExpiration = UserIdleDuration
        });
        return value;
    }

    private bool IsCurrentGeneration(DataGeneration generation)
        => _responses.TryGetValue(new GenerationKey(generation.ApiBase, generation.ExamineId), out Guid value)
            && value == generation.Value;

    public void Invalidate(Uri? apiBase, string? userId)
    {
        if (apiBase is null || string.IsNullOrWhiteSpace(userId))
        {
            return;
        }

        Bucket? bucket;
        lock (_usersLock)
        {
            var partition = (apiBase.AbsoluteUri, userId);
            if (!_users.TryGetValue(partition, out bucket))
            {
                return;
            }
            _users.Remove(partition);
        }
        bucket?.Dispose();
    }

    public void Dispose()
    {
        _users.Compact(1);
        _users.Dispose();
        _responses.Dispose();
    }

    internal readonly record struct DataGeneration(string ApiBase, long? ExamineId, Guid Value);
    private readonly record struct GenerationKey(string ApiBase, long? ExamineId);
    private readonly record struct RequestKey(string AuthorizationKey, string Request, Type ResultType, Guid? DataGeneration);

    private sealed class Bucket : IDisposable
    {
        private readonly CancellationTokenSource _expiration = new();
        private int _disposed;
        public CancellationToken Token { get; }

        public Bucket() => Token = _expiration.Token;

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 0)
            {
                _expiration.Cancel();
                _expiration.Dispose();
            }
        }
    }
}
