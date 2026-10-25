using Blazored.LocalStorage;

namespace CnGalWebSite.MainSite.Shared.Services;

/// <summary>Preserves the existing LocalStorage serializer, keys and payloads.</summary>
public sealed class EditDraftStore(ILocalStorageService storage)
{
    public ValueTask<T?> ReadAsync<T>(string key, CancellationToken token = default)
        => storage.GetItemAsync<T>(key, token);
    public ValueTask WriteAsync<T>(string key, T value, CancellationToken token = default)
        => storage.SetItemAsync(key, value, token);
    public ValueTask RemoveAsync(string key, CancellationToken token = default)
        => storage.RemoveItemAsync(key, token);
}
