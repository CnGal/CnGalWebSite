

using CnGalWebSite.Core.Services;
using CnGalWebSite.Shared.Extentions;
using Microsoft.AspNetCore.Components;
using SixLabors.ImageSharp;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CnGalWebSite.Expo.Services
{
    public class HttpService : IHttpService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        public HttpService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;

            _jsonOptions.Converters.Add(new DateTimeConverterUsingDateTimeParse());
            _jsonOptions.Converters.Add(new DateTimeConverterUsingDateTimeNullableParse());
            _jsonOptions.Converters.Add(new JsonStringEnumConverter());
            _configuration = configuration;
        }
        public async Task<TValue> GetAsync<TValue>(string url)
        {
            var client = await GetClientAsync();

            return await client.GetFromJsonAsync<TValue>(url.Contains("://") ? url : _configuration["WebApiPath"] + url, _jsonOptions);
        }

        public async Task<TValue> PostAsync<TModel, TValue>(string url, TModel model)
        {
            var client = await GetClientAsync();
            var result = await client.PostAsJsonAsync(url.Contains("://") ? url : _configuration["WebApiPath"] + url, model);
            string jsonContent = result.Content.ReadAsStringAsync().Result;
            return JsonSerializer.Deserialize<TValue>(jsonContent, _jsonOptions);
        }

        public Task<HttpClient> GetClientAsync()
        {
            return Task.FromResult(GetClient());
        }

        public HttpClient GetClient()
        {
            return _httpClientFactory.CreateClient("AuthAPI");
        }
    }
}
