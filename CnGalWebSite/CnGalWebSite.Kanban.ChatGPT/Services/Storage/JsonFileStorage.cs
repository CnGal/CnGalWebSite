using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace CnGalWebSite.Kanban.ChatGPT.Services.Storage
{
    /// <summary>
    /// JSON文件持久化存储实现
    /// </summary>
    public class JsonFileStorage : IPersistentStorage, IDisposable
    {
        private readonly ILogger<JsonFileStorage> _logger;
        private readonly string _storageDirectory;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly ConcurrentDictionary<string, StorageMetadata> _metadata;
        private readonly Timer _cleanupTimer;
        private readonly SemaphoreSlim _fileLock = new(1, 1);

        /// <summary>
        /// 存储元数据
        /// </summary>
        private class StorageMetadata
        {
            public DateTime CreatedAt { get; set; }
            public DateTime? ExpiresAt { get; set; }
            public string FilePath { get; set; } = "";
        }

        public JsonFileStorage(IConfiguration configuration, ILogger<JsonFileStorage> logger)
        {
            _logger = logger;
            _storageDirectory = configuration["PersistentStorage:Directory"] ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "storage");

            // 确保存储目录存在
            Directory.CreateDirectory(_storageDirectory);

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            _metadata = new ConcurrentDictionary<string, StorageMetadata>();

            // 加载现有元数据
            LoadMetadataAsync().GetAwaiter().GetResult();

            // 启动定期清理任务（每小时执行一次）
            _cleanupTimer = new Timer(async _ => await CleanupExpiredAsync(), null, TimeSpan.FromHours(1), TimeSpan.FromHours(1));

            _logger.LogInformation("JSON文件存储已初始化，存储目录：{directory}", _storageDirectory);
        }

        public async Task SaveAsync<T>(string key, T data, TimeSpan? expiration = null)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key不能为空", nameof(key));

            await _fileLock.WaitAsync();
            try
            {
                var safeKey = GetSafeFileName(key);
                var filePath = Path.Combine(_storageDirectory, $"{safeKey}.json");

                var json = JsonSerializer.Serialize(data, _jsonOptions);
                await File.WriteAllTextAsync(filePath, json);

                var metadata = new StorageMetadata
                {
                    CreatedAt = DateTime.Now,
                    ExpiresAt = expiration.HasValue ? DateTime.Now.Add(expiration.Value) : null,
                    FilePath = filePath
                };

                _metadata.AddOrUpdate(key, metadata, (k, existing) => metadata);

                // 保存元数据
                await SaveMetadataAsync();

                _logger.LogDebug("数据已保存，键：{key}，文件：{filePath}", key, filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "保存数据失败，键：{key}", key);
                throw;
            }
            finally
            {
                _fileLock.Release();
            }
        }

        public async Task<T?> LoadAsync<T>(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return default;

            await _fileLock.WaitAsync();
            try
            {
                if (!_metadata.TryGetValue(key, out var metadata))
                    return default;

                // 检查是否过期
                if (metadata.ExpiresAt.HasValue && DateTime.Now > metadata.ExpiresAt.Value)
                {
                    await DeleteInternalAsync(key);
                    return default;
                }

                if (!File.Exists(metadata.FilePath))
                {
                    _metadata.TryRemove(key, out _);
                    await SaveMetadataAsync();
                    return default;
                }

                var json = await File.ReadAllTextAsync(metadata.FilePath);
                var result = JsonSerializer.Deserialize<T>(json, _jsonOptions);

                _logger.LogDebug("数据已加载，键：{key}", key);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载数据失败，键：{key}", key);
                return default;
            }
            finally
            {
                _fileLock.Release();
            }
        }

        public async Task DeleteAsync(string key)
        {
            await _fileLock.WaitAsync();
            try
            {
                await DeleteInternalAsync(key);
            }
            finally
            {
                _fileLock.Release();
            }
        }

        private async Task DeleteInternalAsync(string key)
        {
            if (!_metadata.TryRemove(key, out var metadata))
                return;

            try
            {
                if (File.Exists(metadata.FilePath))
                {
                    File.Delete(metadata.FilePath);
                }

                await SaveMetadataAsync();
                _logger.LogDebug("数据已删除，键：{key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除数据失败，键：{key}", key);
            }
        }

        public async Task<bool> ExistsAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return false;

            await _fileLock.WaitAsync();
            try
            {
                if (!_metadata.TryGetValue(key, out var metadata))
                    return false;

                // 检查是否过期
                if (metadata.ExpiresAt.HasValue && DateTime.Now > metadata.ExpiresAt.Value)
                {
                    await DeleteInternalAsync(key);
                    return false;
                }

                return File.Exists(metadata.FilePath);
            }
            finally
            {
                _fileLock.Release();
            }
        }

        public async Task CleanupExpiredAsync()
        {
            await _fileLock.WaitAsync();
            try
            {
                var expiredKeys = new List<string>();
                var now = DateTime.Now;

                foreach (var kvp in _metadata)
                {
                    if (kvp.Value.ExpiresAt.HasValue && now > kvp.Value.ExpiresAt.Value)
                    {
                        expiredKeys.Add(kvp.Key);
                    }
                }

                foreach (var key in expiredKeys)
                {
                    await DeleteInternalAsync(key);
                }

                if (expiredKeys.Count > 0)
                {
                    _logger.LogInformation("清理了 {count} 个过期数据", expiredKeys.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清理过期数据失败");
            }
            finally
            {
                _fileLock.Release();
            }
        }

        private async Task LoadMetadataAsync()
        {
            var metadataPath = Path.Combine(_storageDirectory, "_metadata.json");
            if (!File.Exists(metadataPath))
                return;

            try
            {
                var json = await File.ReadAllTextAsync(metadataPath);
                var metadata = JsonSerializer.Deserialize<Dictionary<string, StorageMetadata>>(json, _jsonOptions);

                if (metadata != null)
                {
                    foreach (var kvp in metadata)
                    {
                        _metadata.TryAdd(kvp.Key, kvp.Value);
                    }
                }

                _logger.LogDebug("加载了 {count} 条元数据", _metadata.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载元数据失败");
            }
        }

        private async Task SaveMetadataAsync()
        {
            var metadataPath = Path.Combine(_storageDirectory, "_metadata.json");

            try
            {
                var json = JsonSerializer.Serialize(_metadata.ToDictionary(kvp => kvp.Key, kvp => kvp.Value), _jsonOptions);
                await File.WriteAllTextAsync(metadataPath, json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "保存元数据失败");
            }
        }

        private static string GetSafeFileName(string key)
        {
            // 将不安全的文件名字符替换为安全字符
            var invalidChars = Path.GetInvalidFileNameChars();
            var safeKey = key;

            foreach (var invalidChar in invalidChars)
            {
                safeKey = safeKey.Replace(invalidChar, '_');
            }

            // 确保文件名不会太长
            if (safeKey.Length > 200)
            {
                safeKey = safeKey.Substring(0, 200);
            }

            return safeKey;
        }

        public void Dispose()
        {
            _cleanupTimer?.Dispose();
            _fileLock?.Dispose();
        }
    }
}
