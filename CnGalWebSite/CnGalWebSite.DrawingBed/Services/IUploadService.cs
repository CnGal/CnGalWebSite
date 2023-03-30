namespace CnGalWebSite.DrawingBed.Services
{
    public interface IUploadService
    {
        Task<string> UploadToTencentOSS(string filePath, string shar1);

        Task<string> UploadToAliyunOSS(string filePath, string shar1);
    }
}
