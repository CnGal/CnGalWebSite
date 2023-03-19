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
using CnGalWebSite.IdentityServer.Admin.Shared.Options;
using Microsoft.AspNetCore.Components.Authorization;
using CnGalWebSite.IdentityServer.Admin.Shared.Services;
using IdentityModel.Client;
using static IdentityModel.OidcConstants;

namespace CnGalWebSite.IdentityServer.Admin.WASM.Services
{
    public class HttpService : IHttpService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<HttpService> _logger;
        private readonly string _baseUrl = "https://localhost:5001/";

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
            return await client.GetFromJsonAsync<TValue>(_baseUrl+ url, _jsonOptions);
        }

        public async Task<TValue> PostAsync<TModel, TValue>(string url, TModel model)
        {
            var client = GetClient();
            var result = await client.PostAsJsonAsync(_baseUrl + url, model);
            string jsonContent = result.Content.ReadAsStringAsync().Result;
            return JsonSerializer.Deserialize<TValue>(jsonContent, _jsonOptions);
        }

        private HttpClient GetClient()
        {
            return _httpClientFactory.CreateClient(IsAuth? "AuthAPI" : "AnonymousAPI");
        }
    }
}
