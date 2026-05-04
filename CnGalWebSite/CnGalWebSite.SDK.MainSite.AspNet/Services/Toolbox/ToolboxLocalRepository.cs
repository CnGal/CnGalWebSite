using System.Text.Json;
using CnGalWebSite.SDK.MainSite.Services.Toolbox;
using Microsoft.JSInterop;

namespace CnGalWebSite.SDK.MainSite.AspNet.Services.Toolbox;

public sealed class ToolboxLocalRepository<TEntity>(IJSRuntime jsRuntime) : IToolboxLocalRepository<TEntity>
    where TEntity : class
{
    private readonly string _storageKey = $"main-site-toolbox:{typeof(TEntity).Name.ToLowerInvariant()}";
    private readonly SemaphoreSlim _loadLock = new(1, 1);
    private List<TEntity>? _items;

    public async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await EnsureLoadedAsync(cancellationToken);
        return _items!;
    }

    public async Task<TEntity> InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await EnsureLoadedAsync(cancellationToken);
        _items!.Add(entity);
        await SaveAsync(cancellationToken);
        return entity;
    }

    public async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        await EnsureLoadedAsync(cancellationToken);
        var json = JsonSerializer.Serialize(_items);
        await jsRuntime.InvokeVoidAsync("localStorage.setItem", cancellationToken, _storageKey, json);
    }

    public async Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await EnsureLoadedAsync(cancellationToken);
        _items!.Remove(entity);
        await SaveAsync(cancellationToken);
    }

    private async Task EnsureLoadedAsync(CancellationToken cancellationToken)
    {
        if (_items is not null)
        {
            return;
        }

        await _loadLock.WaitAsync(cancellationToken);
        try
        {
            if (_items is not null)
            {
                return;
            }

            try
            {
                var json = await jsRuntime.InvokeAsync<string?>("localStorage.getItem", cancellationToken, _storageKey);
                _items = string.IsNullOrWhiteSpace(json) ? [] : JsonSerializer.Deserialize<List<TEntity>>(json) ?? [];
            }
            catch
            {
                _items = [];
            }
        }
        finally
        {
            _loadLock.Release();
        }
    }
}
