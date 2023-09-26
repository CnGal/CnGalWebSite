
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Net.Http.Json;

namespace Live2DTest.DataRepositories
{
  
    /// <summary>
    /// 此接口是所有仓储的约定，此接口仅作为约定，用于标识它们
    /// </summary>
    /// <typeparam name="TEntity">当前传入的仓储的实体类型</typeparam>
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        private List<TEntity> _repository = new();

        private readonly string _index = typeof(TEntity).ToString().Split('.').Last().ToLower().Replace("model","")+ ".json";

        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public Repository(HttpClient httpClient, IConfiguration configuration)
        {
           _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task LoadAsync()
        {
            var list = await _httpClient.GetFromJsonAsync<List<TEntity>>(_configuration["Live2D_DataUrl"] + _index);
            _repository.Clear();
            _repository.AddRange(list);
        }

        public List<TEntity> GetAll()
        {
            return _repository;
        }
    }
}
