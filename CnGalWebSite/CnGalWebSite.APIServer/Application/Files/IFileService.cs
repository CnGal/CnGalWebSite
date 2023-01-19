using BootstrapBlazor.Components;

using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Files;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.Files
{
    public interface IFileService
    {
        Task<PagedResultDto<ListFileAloneModel>> GetAudioPaginatedResult(PagedSortedAndFilterInput input);

        Task<PagedResultDto<ImageInforTipViewModel>> GetImagePaginatedResult(PagedSortedAndFilterInput input);

        Task<QueryData<ListFileAloneModel>> GetPaginatedResult(CnGalWebSite.DataModel.ViewModel.Search.QueryPageOptions options, ListFileAloneModel searchModel);

        Task<string> SaveImageAsync(string url, string userId, double x = 0, double y = 0);

        Task<string> TransferDepositFile(string url);

        Task TransferAllMainImages(int maxCount);

        Task AddUserUploadFileInfor(string userId, AddUserUploadFileInforModel model);

        Task<string> TransformImagesAsync(string text, string userId);

    }
}
