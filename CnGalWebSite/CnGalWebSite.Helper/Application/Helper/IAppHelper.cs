using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Files;
using CnGalWebSite.DataModel.ViewModel.Files.Images;
using Microsoft.AspNetCore.Components.Forms;

namespace CnGalWebSite.DataModel.Application.Helper
{
    public interface IAppHelper
    {
         Task<UploadResult> UploadImagesAsync(IBrowserFile file, ImageAspectType type);

        Task<UploadResult> UploadImagesAsync(byte[] bytes, string fileName, ImageAspectType type);

        Task AddUserLoadedFileInfor(UploadResult infor, UploadFileType type);

        Task<UploadResult> UploadAudioAsync(IBrowserFile file);
    }
}
