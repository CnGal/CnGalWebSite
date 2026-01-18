using CnGalWebSite.Kanban.Services.Configs;
using System.Collections.Generic;

namespace Live2DTest.DataRepositories
{

    /// <summary>
    /// 此接口是所有仓储的约定，此接口仅作为约定，用于标识它们
    /// </summary>
    /// <typeparam name="TEntity">当前传入的仓储的实体类型</typeparam>
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        private List<TEntity> _repository = new();

        private readonly IRemoteConfigService _remoteConfigService;

        public Repository(IRemoteConfigService remoteConfigService)
        {
            _remoteConfigService = remoteConfigService;
        }

        public async Task LoadAsync()
        {
            try
            {
                var list = await _remoteConfigService.GetRepositoryDataAsync<TEntity>();

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
