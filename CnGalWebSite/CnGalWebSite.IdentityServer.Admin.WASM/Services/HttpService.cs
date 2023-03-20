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
using System.Security.Claims;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Blazored.LocalStorage;

namespace CnGalWebSite.IdentityServer.Admin.WASM.Services
{
    public class HttpService : IHttpService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<HttpService> _logger;
        private readonly NavigationManager _navigationManager;

        private readonly string _baseUrl = "https://localhost:5001/";
        private bool _isPreRender =true;

        public bool IsAuth { get; set; }

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        public HttpService(IHttpClientFactory httpClientFactory, ILogger<HttpService> logger, NavigationManager navigationManager)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _navigationManager = navigationManager;
        }
        public async Task<TValue> GetAsync<TValue>(string url)
        {
            _logger.LogInformation(IsAuth ? "AuthAPI" : "AnonymousAPI");
            var client =await GetClient();

            try
            {
                return await client.GetFromJsonAsync<TValue>(_baseUrl + url, _jsonOptions);
            }
            catch (AccessTokenNotAvailableException ex)
            {
                _navigationManager.NavigateToLogout("authentication/logout", "/");
                throw new Exception("令牌过期，请重新登入", ex);
            }
        }

        public async Task<TValue> PostAsync<TModel, TValue>(string url, TModel model)
        {
            var client =await GetClient();
            var result = await client.PostAsJsonAsync(_baseUrl + url, model);
            string jsonContent = result.Content.ReadAsStringAsync().Result;
            return JsonSerializer.Deserialize<TValue>(jsonContent, _jsonOptions);
        }

        private async Task<HttpClient> GetClient()
        {
            if (_isPreRender)
            {
                await Task.Delay(100);
            }
            _isPreRender = false;
            return _httpClientFactory.CreateClient(IsAuth ? "AuthAPI" : "AnonymousAPI");
        }

        public Task SetRefreshToken(ClaimsPrincipal user, string accessToken, string refreshToken)
        {
            throw new NotImplementedException();
        }
    }
}
