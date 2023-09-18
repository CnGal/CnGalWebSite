

using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Files;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.Files
{
    public interface IFileService
    {
        Task<string> TransferDepositFile(string url);

        Task TransferMainImagesToPublic(int maxCount);

    }
}
