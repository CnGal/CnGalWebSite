using BootstrapBlazor.Components;
using CnGalWebSite.DataModel.ExamineModel;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Peripheries;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.Peripheries
{
    public interface IPeripheryService
    {
        Task<QueryData<ListPeripheryAloneModel>> GetPaginatedResult(QueryPageOptions options, ListPeripheryAloneModel searchModel);

        void UpdatePeripheryDataMain(Periphery periphery, PeripheryMain examine);

        void UpdatePeripheryDataImages(Periphery periphery, PeripheryImages examine);

        void UpdatePeripheryData(Periphery periphery, Examine examine);

        void UpdatePeripheryDataRelatedEntries(Periphery periphery, PeripheryRelatedEntries examine);

        Task UpdatePeripheryDataRelatedPeripheriesAsync(Periphery periphery, PeripheryRelatedPeripheries examine);

        Task RealUpdateRelevances(Periphery periphery);

        GameOverviewPeripheriesModel GetGameOverViewPeripheriesModel(ApplicationUser user, Entry entry, List<long> ownedPeripheries, bool showUserPhoto);

        GameOverviewPeripheriesModel GetGameOverViewPeripheriesModel(ApplicationUser user, Periphery periphery, List<long> ownedPeripheries, bool showUserPhoto);

        Task<PeripheryEditState> GetPeripheryEditState(ApplicationUser user, long peripheryId);
    }
}
