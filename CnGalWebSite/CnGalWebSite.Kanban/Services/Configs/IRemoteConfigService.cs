using CnGalWebSite.Kanban.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CnGalWebSite.Kanban.Services.Configs
{
    public interface IRemoteConfigService
    {
        Task InitializeAsync();

        Task<EventGroupModel> GetEventGroupAsync();

        Task<List<TEntity>> GetRepositoryDataAsync<TEntity>() where TEntity : class;
    }
}

