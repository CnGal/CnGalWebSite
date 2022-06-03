using Blazored.LocalStorage;
using CnGalWebSite.PublicToolbox.DataRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.Shared.DataRepositories
{
    /// <summary>
    /// 此接口是所有仓储的约定，此接口仅作为约定，用于标识它们
    /// </summary>
    /// <typeparam name="TEntity">当前传入的仓储的实体类型</typeparam>
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        private List<TEntity> _repository = new List<TEntity>();
        private readonly ILocalStorageService _localStorage;
        private readonly string _index = typeof(TEntity).ToString().Split('.').Last().ToLower();


        public Repository(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;

            Load();
        }

        private async Task Load()
        {
            try
            {
                _repository = (await _localStorage.GetItemAsync<List<TEntity>>(_index)) ?? new List<TEntity>();

            }
            catch
            {
                _repository = new List<TEntity>();
            }
        }

        public async Task DeleteAsync(TEntity entity)
        {
            _repository.Remove(entity);
            await SaveAsync();
        }

        public List<TEntity> GetAll()
        {
            return _repository;
        }

        public async Task<TEntity> InsertAsync(TEntity entity)
        {
            _repository.Add(entity);
            await SaveAsync();
            return entity;
        }

        public async Task SaveAsync()
        {
            await _localStorage.SetItemAsync<List<TEntity>>(_index, _repository);
        }
    }
}
