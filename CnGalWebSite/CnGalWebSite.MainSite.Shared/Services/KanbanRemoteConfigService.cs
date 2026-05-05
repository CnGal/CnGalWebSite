using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CnGalWebSite.MainSite.Shared.Services.KanbanModels;
using Microsoft.Extensions.Configuration;

namespace CnGalWebSite.MainSite.Shared.Services;

public class KanbanRemoteConfigService : IKanbanRemoteConfigService, IDisposable
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    private readonly SemaphoreSlim _cacheLock = new(1, 1);

    private KanbanEventGroupModel? _eventGroup;
    private List<KanbanClothesModel>? _clothes;
    private List<KanbanExpressionModel>? _expressions;
    private List<KanbanMotionGroupModel>? _motionGroups;
    private List<KanbanStockingsModel>? _stockings;
    private List<KanbanShoesModel>? _shoes;

    public KanbanRemoteConfigService(IConfiguration configuration)
    {
        _configuration = configuration;
        _httpClient = new HttpClient();
        _jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
    }

    public async Task<KanbanEventGroupModel?> GetEventGroupAsync()
    {
        if (_eventGroup is not null)
            return _eventGroup;

        await _cacheLock.WaitAsync();
        try
        {
            if (_eventGroup is not null)
                return _eventGroup;

            _eventGroup = await FetchAsync<KanbanEventGroupModel>("EventGroup.json");
        }
        finally
        {
            _cacheLock.Release();
        }

        return _eventGroup;
    }

    public Task<List<KanbanClothesModel>> GetClothesAsync()
    {
        if (_clothes is not null)
            return Task.FromResult(_clothes);

        return GetClothesSlowAsync();
    }

    private async Task<List<KanbanClothesModel>> GetClothesSlowAsync()
    {
        await _cacheLock.WaitAsync();
        try
        {
            if (_clothes is not null)
                return _clothes;

            _clothes = await FetchAsync<List<KanbanClothesModel>>("Clothes.json") ?? [];
            return _clothes;
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    public Task<List<KanbanExpressionModel>> GetExpressionsAsync()
    {
        if (_expressions is not null)
            return Task.FromResult(_expressions);

        return GetExpressionsSlowAsync();
    }

    private async Task<List<KanbanExpressionModel>> GetExpressionsSlowAsync()
    {
        await _cacheLock.WaitAsync();
        try
        {
            if (_expressions is not null)
                return _expressions;

            _expressions = await FetchAsync<List<KanbanExpressionModel>>("Expression.json") ?? [];
            return _expressions;
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    public Task<List<KanbanMotionGroupModel>> GetMotionGroupsAsync()
    {
        if (_motionGroups is not null)
            return Task.FromResult(_motionGroups);

        return GetMotionGroupsSlowAsync();
    }

    private async Task<List<KanbanMotionGroupModel>> GetMotionGroupsSlowAsync()
    {
        await _cacheLock.WaitAsync();
        try
        {
            if (_motionGroups is not null)
                return _motionGroups;

            _motionGroups = await FetchAsync<List<KanbanMotionGroupModel>>("MotionGroup.json") ?? [];
            return _motionGroups;
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    public Task<List<KanbanStockingsModel>> GetStockingsAsync()
    {
        if (_stockings is not null)
            return Task.FromResult(_stockings);

        return GetStockingsSlowAsync();
    }

    private async Task<List<KanbanStockingsModel>> GetStockingsSlowAsync()
    {
        await _cacheLock.WaitAsync();
        try
        {
            if (_stockings is not null)
                return _stockings;

            _stockings = await FetchAsync<List<KanbanStockingsModel>>("Stockings.json") ?? [];
            return _stockings;
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    public Task<List<KanbanShoesModel>> GetShoesAsync()
    {
        if (_shoes is not null)
            return Task.FromResult(_shoes);

        return GetShoesSlowAsync();
    }

    private async Task<List<KanbanShoesModel>> GetShoesSlowAsync()
    {
        await _cacheLock.WaitAsync();
        try
        {
            if (_shoes is not null)
                return _shoes;

            _shoes = await FetchAsync<List<KanbanShoesModel>>("Shoes.json") ?? [];
            return _shoes;
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    private async Task<T?> FetchAsync<T>(string fileName)
    {
        try
        {
            var baseUrl = _configuration["Live2D_DataUrl"];
            if (string.IsNullOrEmpty(baseUrl))
                return default;

            var url = baseUrl.TrimEnd('/') + "/" + fileName;
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync<T>(stream, _jsonOptions);
        }
        catch
        {
            return default;
        }
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
        _cacheLock?.Dispose();
    }
}
