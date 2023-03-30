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
using IdentityModel.AspNetCore.AccessTokenManagement;
using IdentityModel.Client;
using CnGalWebSite.Shared.Service;
using CnGalWebSite.Core.Services;

namespace CnGalWebSite.Server.Services
{
    public class HttpService: IHttpService
    {
        private readonly HttpClient _client;
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        private readonly IUserAccessTokenManagementService _tokenManagementService;

        public bool IsAuth { get; set; }

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        public HttpService(
               HttpClient client,
               AuthenticationStateProvider authenticationStateProvider,
               IUserAccessTokenManagementService tokenManagementService)
        {
            _client = client;
            _authenticationStateProvider = authenticationStateProvider;
            _tokenManagementService = tokenManagementService;
        }
        public async Task<TValue> GetAsync<TValue>(string url)
        {
            var client =await GetClientAsync();
            return await client.GetFromJsonAsync<TValue>( url, _jsonOptions);
        }

        public async Task<TValue> PostAsync<TModel,TValue>(string url, TModel model)
        {
            var client =await GetClientAsync();
            var result = await client.PostAsJsonAsync(url, model);
            string jsonContent = result.Content.ReadAsStringAsync().Result;
            return JsonSerializer.Deserialize<TValue>(jsonContent, _jsonOptions);
        }

        public async Task<HttpClient> GetClientAsync()
        {
            var state = await _authenticationStateProvider.GetAuthenticationStateAsync();
            var token = await _tokenManagementService.GetUserAccessTokenAsync(state.User);

            _client.SetBearerToken(token);
            return _client;
        }

        public HttpClient GetClient()
        {
            var state = _authenticationStateProvider.GetAuthenticationStateAsync().GetAwaiter().GetResult();
            var token = _tokenManagementService.GetUserAccessTokenAsync(state.User).GetAwaiter().GetResult();

            _client.SetBearerToken(token);
            return _client;

        }
    }
}
