using CnGalWebSite.DataModel.ViewModel.Files;

namespace CnGalWebSite.DrawingBed.Services
{
    public interface IFileService
    {
        Task<string> TransferDepositFile(string url, double x = 0, double y = 0);

        Task<string> UploadFormFile(IFormFile file, double x = 0, double y = 0);
    }
}
