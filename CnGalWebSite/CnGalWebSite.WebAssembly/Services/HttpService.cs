using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System.Security.Policy;
using Microsoft.AspNetCore.Components.Authorization;
using IdentityModel.Client;
using static IdentityModel.OidcConstants;
using CnGalWebSite.Shared.Service;
using Microsoft.Extensions.Logging;
using CnGalWebSite.DataModel.Helper;

namespace CnGalWebSite.WebAssembly.Services
{
    public class HttpService : IHttpService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<HttpService> _logger;

        public bool IsAuth { get; set; }

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        public HttpService(IHttpClientFactory httpClientFactory, ILogger<HttpService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }
        public async Task<TValue> GetAsync<TValue>(string url)
        {
            _logger.LogInformation(IsAuth ? "AuthAPI" : "AnonymousAPI");
            var client = GetClient();
            return await client.GetFromJsonAsync<TValue>(ToolHelper.WebApiPath + url, _jsonOptions);
        }

        public async Task<TValue> PostAsync<TModel, TValue>(string url, TModel model)
        {
            var client = GetClient();
            var result = await client.PostAsJsonAsync(ToolHelper.WebApiPath + url, model);
            string jsonContent = result.Content.ReadAsStringAsync().Result;
            return JsonSerializer.Deserialize<TValue>(jsonContent, _jsonOptions);
        }

        public HttpClient GetClient()
        {
            return _httpClientFactory.CreateClient(IsAuth ? "AuthAPI" : "AnonymousAPI");
        }

        public async Task<HttpClient> GetClientAsync()
        {
            return await Task.FromResult(_httpClientFactory.CreateClient(IsAuth ? "AuthAPI" : "AnonymousAPI"));
        }
    }
}
