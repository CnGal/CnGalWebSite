
using System.Security.Policy;
using System.Text.Json;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using CnGalWebSite.Server.Models.Tokens;
using System.Net.Http.Json;
using System;
using CnGalWebSite.Server.Models.Shared;

namespace CnGalWebSite.Server.Services
{
    public class TokenStoreService:ITokenStoreService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TokenStoreService> _logger;

        private readonly AppUserAccessTokenType _type = AppUserAccessTokenType.Server;

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        public TokenStoreService(IHttpClientFactory httpClientFactory, ILogger<TokenStoreService> logger, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;

            _httpClient = httpClientFactory.CreateClient();
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<AppUserAccessToken> GetAsync(string id)
        {
            try
            {
                var result = await _httpClient.PostAsJsonAsync<GetTokenModel>($"{_configuration["TokenAPI"]}get", new GetTokenModel
                {
                    Secret = _configuration["TokenAPISecret"],
                    UserId = id,
                    Type= _type
                });
                string jsonContent = result.Content.ReadAsStringAsync().Result;
                var re = JsonSerializer.Deserialize<AppUserAccessToken>(jsonContent, _jsonOptions);
                return re;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取用户令牌失败");
                return null;
            }

        }

        public async Task SetAsync(AppUserAccessToken model)
        {
            try
            {
                var result = await _httpClient.PostAsJsonAsync($"{_configuration["TokenAPI"]}set", new SetTokenModel
                {
                    Secret= _configuration["TokenAPISecret"],
                    AccessToken= model.AccessToken,
                    Expiration= model.Expiration,
                    RefreshToken= model.RefreshToken,
                    Type= _type,
                    UserId= model.UserId
                });
                string jsonContent = result.Content.ReadAsStringAsync().Result;
                var re= JsonSerializer.Deserialize<Result>(jsonContent, _jsonOptions);
                if (!re.Success)
                {
                    _logger.LogError("保存用户令牌失败，错误信息：{message}", re.Message);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"保存用户令牌失败");
            }
        }

        public async Task DeleteAsync(string id)
        {
            try
            {
                var result = await _httpClient.PostAsJsonAsync($"{_configuration["TokenAPI"]}delete", new DeleteTokenModel
                {
                    UserId = id,
                    Type = _type,
                    Secret = _configuration["TokenAPISecret"],
                });
                string jsonContent = result.Content.ReadAsStringAsync().Result;
                var re = JsonSerializer.Deserialize<Result>(jsonContent, _jsonOptions);
                if (!re.Success)
                {
                    _logger.LogError("删除用户令牌失败，错误信息：{message}", re.Message);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除用户令牌失败");
            }
        }
    }
}
