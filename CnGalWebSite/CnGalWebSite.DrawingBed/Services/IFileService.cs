using CnGalWebSite.DataModel.ViewModel.Files;

namespace CnGalWebSite.DrawingBed.Services
{
    public interface IFileService
    {
        Task<string> TransferDepositFile(TransferDepositFileModel model);

        Task<string> UploadFormFile(IFormFile file);
    }
}
