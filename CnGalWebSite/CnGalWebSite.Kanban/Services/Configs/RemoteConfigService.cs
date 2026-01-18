using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.Kanban.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace CnGalWebSite.Kanban.Services.Configs
{
    public class RemoteConfigService : IRemoteConfigService, IDisposable
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        private readonly SemaphoreSlim _initLock = new(1, 1);
        private Task _initializeTask;

        private readonly SemaphoreSlim _cacheLock = new(1, 1);
        private readonly Dictionary<string, object> _repositoryCache = new();
        private EventGroupModel _eventGroup;

        public RemoteConfigService(IConfiguration configuration)
        {
            _configuration = configuration;
            if (ToolHelper.IsSSR)
            {
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                };
                _httpClient = new HttpClient(handler);
            }
            else
            {
                _httpClient = new HttpClient();
            }
        }

        public async Task InitializeAsync()
        {
            if (_initializeTask != null)
            {
                await _initializeTask;
                return;
            }

            await _initLock.WaitAsync();
            try
            {
                if (_initializeTask == null)
                {
                    _initializeTask = InitializeCoreAsync();
                }
            }
            finally
            {
                _initLock.Release();
            }

            await _initializeTask;
        }

        public async Task<EventGroupModel> GetEventGroupAsync()
        {
            if (_eventGroup != null)
            {
                return CloneEventGroup(_eventGroup);
            }

            await _cacheLock.WaitAsync();
            try
            {
                if (_eventGroup == null)
                {
                    _eventGroup = await FetchEventGroupAsync();
                }
            }
            finally
            {
                _cacheLock.Release();
            }

            return CloneEventGroup(_eventGroup);
        }

        public async Task<List<TEntity>> GetRepositoryDataAsync<TEntity>() where TEntity : class
        {
            var key = GetIndex<TEntity>();
            if (_repositoryCache.TryGetValue(key, out var existing))
            {
                return (List<TEntity>)existing;
            }

            await _cacheLock.WaitAsync();
            try
            {
                if (_repositoryCache.TryGetValue(key, out existing))
                {
                    return (List<TEntity>)existing;
                }

                var list = await FetchRepositoryDataAsync<TEntity>(key);
                _repositoryCache[key] = list;
                return list;
            }
            finally
            {
                _cacheLock.Release();
            }
        }

        private async Task InitializeCoreAsync()
        {
            await GetEventGroupAsync();
            await GetRepositoryDataAsync<ClothesModel>();
            await GetRepositoryDataAsync<ExpressionModel>();
            await GetRepositoryDataAsync<MotionGroupModel>();
            await GetRepositoryDataAsync<StockingsModel>();
            await GetRepositoryDataAsync<ShoesModel>();
        }

        private async Task<EventGroupModel> FetchEventGroupAsync()
        {
            var url = _configuration["Live2D_DataUrl"] + "EventGroup.json";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync();
            var model = await System.Text.Json.JsonSerializer.DeserializeAsync<EventGroupModel>(stream, ToolHelper.options);
            return model ?? new EventGroupModel();
        }

        private static EventGroupModel CloneEventGroup(EventGroupModel source)
        {
            var json = JsonSerializer.Serialize(source, ToolHelper.options);
            return JsonSerializer.Deserialize<EventGroupModel>(json, ToolHelper.options);
        }

        private async Task<List<TEntity>> FetchRepositoryDataAsync<TEntity>(string index) where TEntity : class
        {
            var url = _configuration["Live2D_DataUrl"] + index;
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync();
            var list = await System.Text.Json.JsonSerializer.DeserializeAsync<List<TEntity>>(stream, ToolHelper.options);
            return list ?? new List<TEntity>();
        }

        private static string GetIndex<TEntity>()
        {
            return typeof(TEntity).ToString().Split('.').Last().Replace("Model", "") + ".json";
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}

