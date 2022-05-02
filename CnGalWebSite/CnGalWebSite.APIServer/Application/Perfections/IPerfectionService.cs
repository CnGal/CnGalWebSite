using BootstrapBlazor.Components;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Perfections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.Perfections
{
    public interface IPerfectionService
    {
        Task UpdateEntryPerfectionResultAsync(int entryId);

        Task<PerfectionInforTipViewModel> GetEntryPerfection(int entryId);

        Task<List<PerfectionCheckViewModel>> GetEntryPerfectionCheckList(long perfectionId);

        Task UpdatePerfectionCountAndVictoryPercentage();

        Task UpdateAllEntryPerfectionsAsync();

        Task<List<PerfectionInforTipViewModel>> GetPerfectionLevelRadomListAsync(PerfectionLevel level);

        Task<PerfectionLevelOverviewModel> GetPerfectionLevelOverview();

        Task UpdatePerfectionOverview();

        Task<List<PerfectionCheckViewModel>> GetPerfectionCheckLevelRadomListAsync(PerfectionCheckLevel level);

        Task<QueryData<ListPerfectionAloneModel>> GetPaginatedResult(CnGalWebSite.DataModel.ViewModel.Search.QueryPageOptions options, ListPerfectionAloneModel searchModel);

        Task<QueryData<ListPerfectionCheckAloneModel>> GetPaginatedResult(CnGalWebSite.DataModel.ViewModel.Search.QueryPageOptions options, ListPerfectionCheckAloneModel searchModel);
    }
}
