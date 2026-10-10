using CnGalWebSite.DrawingBed.Models.ViewModels;

using CnGalWebSite.DrawingBed.Models.DataModels;

namespace CnGalWebSite.DrawingBed.Services
{
    public interface IRecordService
    {
        Task Add(UploadResult model);

        Task<string> Get(string sha1);

        Task<string> GetRandom(UploadFileType type);
    }
}
