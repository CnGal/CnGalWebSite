using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Files.Images;
using Microsoft.AspNetCore.Components.Forms;

namespace CnGalWebSite.DataModel.Application.Helper
{
    public interface IAppHelper
    {
         Task<UploadResult> UploadFilesAsync(IBrowserFile file, ImageAspectType type);

        Task<UploadResult> UploadFilesAsync(byte[] bytes, string fileName, ImageAspectType type);

        Task AddUserLoadedFileInfor(string url, long size);
    }
}
