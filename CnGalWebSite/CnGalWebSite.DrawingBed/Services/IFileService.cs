using CnGalWebSite.DrawingBed.Models.DataModels;
using CnGalWebSite.DrawingBed.Models.ViewModels;

namespace CnGalWebSite.DrawingBed.Services
{
    public interface IFileService
    {
        Task<UploadResult> TransferDepositFile(string url, bool gallery,double x = 0, double y = 0,  UploadFileType type = UploadFileType.Image, double cropX = 0, double cropY = 0, double cropW = 0, double cropH = 0);

        Task<UploadResult> UploadFormFile(IFormFile file,bool gallery, double x = 0, double y = 0,  UploadFileType type = UploadFileType.Image, double cropX = 0, double cropY = 0, double cropW = 0, double cropH = 0);

        Task<string> SaveFileFromUrl(string url, UploadFileType type);

        void DeleteFile(string path);

        string GetSHA1(string path);
    }
}
