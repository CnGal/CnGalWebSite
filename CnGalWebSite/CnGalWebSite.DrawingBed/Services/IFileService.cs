using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Files;
using CnGalWebSite.DataModel.ViewModel.Others;

namespace CnGalWebSite.DrawingBed.Services
{
    public interface IFileService
    {
        Task<UploadResult> TransferDepositFile(string url, double x = 0, double y = 0, UploadFileType type = UploadFileType.Image);

        Task<UploadResult> UploadFormFile(IFormFile file, double x = 0, double y = 0, UploadFileType type = UploadFileType.Image);
    }
}
