using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using IdentityModel.Client;
using CnGalWebSite.Core.Services;
using System.Text.Json.Serialization;
using CnGalWebSite.Kanban.ChatGPT.Extensions;
using Microsoft.Extensions.Configuration;

namespace CnGalWebSite.Kanban.ChatGPT.Services
{
    public class HttpService : IHttpService
    {
        private readonly HttpClient _client;
        private readonly IConfiguration _configuration;

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        public bool IsAuth { get; set; }

        public HttpService(HttpClient client, IConfiguration configuration)
        {
            _client = client;
            _configuration = configuration;

            _jsonOptions.Converters.Add(new DateTimeConverterUsingDateTimeParse());
            _jsonOptions.Converters.Add(new DateTimeConverterUsingDateTimeNullableParse());
            _jsonOptions.Converters.Add(new JsonStringEnumConverter());

            // 设置 ChatGPT API 授权头
            var apiKey = _configuration["ChatGPTApiKey"];
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                _client.DefaultRequestHeaders.Add("Authorization", "Bearer " + apiKey);
            }
        }
        public async Task<TValue?> GetAsync<TValue>(string url)
        {
            var client = await GetClientAsync();
            return await client.GetFromJsonAsync<TValue>(url, _jsonOptions);
        }

        public async Task<TValue?> PostAsync<TModel, TValue>(string url, TModel model)
        {
            var client = await GetClientAsync();
            var result = await client.PostAsJsonAsync(url, model);
            var jsonContent = result.Content.ReadAsStringAsync().Result;
            return JsonSerializer.Deserialize<TValue>(jsonContent, _jsonOptions);
        }

        public async Task<HttpClient> GetClientAsync()
        {
            return await Task.FromResult(_client);
        }

        public HttpClient GetClient()
        {
            return _client;

        }
    }
}
