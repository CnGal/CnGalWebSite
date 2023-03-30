using CnGalWebSite.DrawingBed.Models.DataModels;
using CnGalWebSite.DrawingBed.Models.ViewModels;

namespace CnGalWebSite.DrawingBed.Services
{
    public interface IFileService
    {
        Task<UploadResult> TransferDepositFile(string url, double x = 0, double y = 0, UploadFileType type = UploadFileType.Image);

        Task<UploadResult> UploadFormFile(IFormFile file, double x = 0, double y = 0, UploadFileType type = UploadFileType.Image);
    }
}
