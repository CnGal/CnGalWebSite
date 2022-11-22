using BootstrapBlazor.Components;
using CnGalWebSite.DataModel.ExamineModel.Peripheries;
using CnGalWebSite.DataModel.ExamineModel.Shared;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Peripheries;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.Peripheries
{
    public interface IPeripheryService
    {
        Task<QueryData<ListPeripheryAloneModel>> GetPaginatedResult(CnGalWebSite.DataModel.ViewModel.Search.QueryPageOptions options, ListPeripheryAloneModel searchModel);

        void UpdatePeripheryDataMain(Periphery periphery, PeripheryMain_1_0 examine);

        void UpdatePeripheryDataMain(Periphery periphery, ExamineMain examine);

        void UpdatePeripheryDataImages(Periphery periphery, PeripheryImages examine);

        Task UpdatePeripheryDataAsync(Periphery periphery, Examine examine);

        Task UpdatePeripheryDataRelatedEntries(Periphery periphery, PeripheryRelatedEntries examine);

        Task UpdatePeripheryDataRelatedPeripheriesAsync(Periphery periphery, PeripheryRelatedPeripheries examine);

        GameOverviewPeripheriesModel GetGameOverViewPeripheriesModel(ApplicationUser user, Entry entry, List<long> ownedPeripheries, bool showUserPhoto);

        GameOverviewPeripheriesModel GetGameOverViewPeripheriesModel(ApplicationUser user, Periphery periphery, List<long> ownedPeripheries, bool showUserPhoto);

        Task<PeripheryEditState> GetPeripheryEditState(ApplicationUser user, long peripheryId);

        Task<List<long>> GetPeripheryIdsFromNames(List<string> names);

       PeripheryViewModel GetPeripheryViewModel(Periphery periphery);

        List<KeyValuePair<object, Operation>> ExaminesCompletion(Periphery currentPeriphery, Periphery newPeriphery);

       List<PeripheryViewModel> ConcompareAndGenerateModel(Periphery currentPeriphery, Periphery newPeriphery);

        void SetDataFromEditPeripheryMainViewModel(Periphery newPeriphery, EditPeripheryMainViewModel model);

        void SetDataFromEditPeripheryImagesViewModel(Periphery newPeriphery, EditPeripheryImagesViewModel model);

        void SetDataFromEditPeripheryRelatedEntriesViewModel(Periphery newPeriphery, EditPeripheryRelatedEntriesViewModel model, List<Entry> entries);

        void SetDataFromEditPeripheryRelatedPerpheriesViewModel(Periphery newPeriphery, EditPeripheryRelatedPeripheriesViewModel model, List<Periphery> peripheries);
    }
}
