using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Steam;

namespace CnGalWebSite.APIServer.Application.Stores
{
    public interface IStoreInfoService
    {
        Task BatchUpdate(int max);

        Task<StoreInfoViewModel> Get(PublishPlatformType platformType, string platformName, string link,string name, int entryId);

    }
}
