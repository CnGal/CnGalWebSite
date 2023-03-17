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

namespace CnGalWebSite.IdentityServer.Admin.Shared.Services
{
    public class HttpService: IHttpService
    {
        private readonly HttpClient _httpClient;

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        public HttpService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<TValue> GetAsync<TValue>(string url)
        {
            var client = GetClient();
            return await client.GetFromJsonAsync<TValue>( url, _jsonOptions);
        }

        public async Task<TValue> PostAsync<TModel,TValue>(string url, TModel model)
        {
            var client = GetClient();
            var result = await client.PostAsJsonAsync( url, model);
            string jsonContent = result.Content.ReadAsStringAsync().Result;
            return JsonSerializer.Deserialize<TValue>(jsonContent, _jsonOptions);
        }

        public HttpClient GetClient()
        {  
            return _httpClient;
        }
    }
}
