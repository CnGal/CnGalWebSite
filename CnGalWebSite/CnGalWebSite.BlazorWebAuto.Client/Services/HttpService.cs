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
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using CnGalWebSite.Core.Services;
using CnGalWebSite.Shared.Extentions;
using System.Text.Json.Serialization;

namespace CnGalWebSite.BlazorWebAuto.Client.Services
{
    public class HttpService : IHttpService
    {
        private readonly HttpClient _client;
        private readonly AuthenticationStateProvider _authenticationStateProvider;

        public bool IsAuth { get; set; }

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        public HttpService(
               HttpClient client,
               AuthenticationStateProvider authenticationStateProvider)
        {
            _client = client;
            _authenticationStateProvider = authenticationStateProvider;

            _jsonOptions.Converters.Add(new DateTimeConverterUsingDateTimeParse());
            _jsonOptions.Converters.Add(new DateTimeConverterUsingDateTimeNullableParse());
            _jsonOptions.Converters.Add(new JsonStringEnumConverter());

        }
        public async Task<TValue> GetAsync<TValue>(string url)
        {
            var client = await GetClientAsync();
            return await client.GetFromJsonAsync<TValue>(url, _jsonOptions);
        }

        public async Task<TValue> PostAsync<TModel, TValue>(string url, TModel model)
        {
            var client = await GetClientAsync();
            var result = await client.PostAsJsonAsync(url, model);
            string jsonContent = result.Content.ReadAsStringAsync().Result;
            return JsonSerializer.Deserialize<TValue>(jsonContent, _jsonOptions);
        }

        public async Task<HttpClient> GetClientAsync()
        {
            var state = await _authenticationStateProvider.GetAuthenticationStateAsync();

            return _client;
        }
    }
}
