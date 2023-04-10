using CnGalWebSite.DataModel.ViewModel.Others;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.Tables
{
    public interface ITableService
    {
        Task UpdateAllInforListAsync();

        Task<EChartsTreeMapOptionModel> GetGroupGameRoleTreeMap();

        Task<EChartsGraphOptionModel> GetEntryGraph();
    }
}
