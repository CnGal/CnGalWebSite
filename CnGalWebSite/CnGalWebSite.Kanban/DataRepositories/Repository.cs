using CnGalWebSite.DataModel.Helper;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Net.Security;

namespace Live2DTest.DataRepositories
{

    /// <summary>
    /// 此接口是所有仓储的约定，此接口仅作为约定，用于标识它们
    /// </summary>
    /// <typeparam name="TEntity">当前传入的仓储的实体类型</typeparam>
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        private List<TEntity> _repository = new();

        private readonly string _index = typeof(TEntity).ToString().Split('.').Last().Replace("Model","")+ ".json";

        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public Repository(HttpClient httpClient, IConfiguration configuration)
        {
            if (ToolHelper.IsSSR)
            {
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                };
                _httpClient = new HttpClient(handler);
            }
            else
            {
                _httpClient = httpClient;
            }
            _configuration = configuration;
        }

        public async Task LoadAsync()
        {
            try
            {
                // 使用 HttpClient 获取响应
                var response = await _httpClient.GetAsync(_configuration["Live2D_DataUrl"] + _index);
                response.EnsureSuccessStatusCode();

                // 使用 Stream 读取内容
                using var stream = await response.Content.ReadAsStreamAsync();
                var list = await System.Text.Json.JsonSerializer.DeserializeAsync<List<TEntity>>(stream, ToolHelper.options);

                _repository.Clear();
                _repository.AddRange(list);
            }
            catch (Exception ex)
            {
                // 如果加载失败，清空仓库
                _repository.Clear();
                throw;
            }
        }

        public List<TEntity> GetAll()
        {
            return _repository;
        }
    }
}
