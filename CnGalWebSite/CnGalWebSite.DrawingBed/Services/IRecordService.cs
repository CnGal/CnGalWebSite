using CnGalWebSite.DrawingBed.Models.ViewModels;

namespace CnGalWebSite.DrawingBed.Services
{
    public interface IRecordService
    {
        Task Add(UploadResult model);

        Task<string> Get(string sha1);
    }
}
