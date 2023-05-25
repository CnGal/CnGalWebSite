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

namespace CnGalWebSite.WebAssembly.Services
{
    public class HttpService : IHttpService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<HttpService> _logger;
        private readonly NavigationManager _navigationManager;

        private readonly string _baseUrl = ToolHelper.WebApiPath;
        private bool _isPreRender = true;

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

            _jsonOptions.Converters.Add(new DateTimeConverterUsingDateTimeParse());
            _jsonOptions.Converters.Add(new DateTimeConverterUsingDateTimeNullableParse());
            _jsonOptions.Converters.Add(new JsonStringEnumConverter());
        }
        public async Task<TValue> GetAsync<TValue>(string url)
        {
            _logger.LogInformation(IsAuth ? "AuthAPI" : "AnonymousAPI");
            var client = await GetClientAsync();

            try
            {
                return await client.GetFromJsonAsync<TValue>(url.Contains("://")?url: _baseUrl + url, _jsonOptions);
            }
            catch (AccessTokenNotAvailableException ex)
            {
                InteractiveRequestOptions requestOptions = new()
                {
                    Interaction = InteractionType.SignIn,
                    ReturnUrl = _navigationManager.Uri,
                };
                _navigationManager.NavigateToLogin("authentication/login", requestOptions);

                throw new Exception("令牌过期，请重新登入", ex);
            }
        }

        public async Task<TValue> PostAsync<TModel, TValue>(string url, TModel model)
        {
            _logger.LogInformation(IsAuth ? "AuthAPI" : "AnonymousAPI");
            try
            {
                var client = await GetClientAsync();
                var result = await client.PostAsJsonAsync(url.Contains("://") ? url : _baseUrl + url, model);
                string jsonContent = result.Content.ReadAsStringAsync().Result;
                return JsonSerializer.Deserialize<TValue>(jsonContent, _jsonOptions);
            }
            catch (AccessTokenNotAvailableException ex)
            {
                InteractiveRequestOptions requestOptions = new()
                {
                    Interaction = InteractionType.SignIn,
                    ReturnUrl = _navigationManager.Uri,
                };
                _navigationManager.NavigateToLogin("authentication/login", requestOptions);
                
                throw new Exception("令牌过期，请重新登入", ex);
            }
        }

        public async Task<HttpClient> GetClientAsync()
        {
            if (_isPreRender)
            {
                await Task.Delay(100);
            }
            _isPreRender = false;
            return _httpClientFactory.CreateClient(IsAuth ? "AuthAPI" : "AnonymousAPI");
        }

        public HttpClient GetClient()
        {
            return _httpClientFactory.CreateClient(IsAuth ? "AuthAPI" : "AnonymousAPI");
        }
    }
}
