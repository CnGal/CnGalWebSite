using CnGalWebSite.DrawingBed.DataReositories;
using CnGalWebSite.DrawingBed.Models.DataModels;
using CnGalWebSite.DrawingBed.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CnGalWebSite.DrawingBed.Services
{
    public class RecordService:IRecordService
    {
        private readonly IRepository<UploadRecord, long> _uploadRecordRepository;
        private readonly IHttpContextAccessor _accessor;

        public RecordService(IRepository<UploadRecord, long> uploadRecordRepository, IHttpContextAccessor accessor)
        {
            _uploadRecordRepository = uploadRecordRepository;
            _accessor = accessor;
        }

        public async Task Add(UploadResult model)
        {
            var userId = _accessor.HttpContext.User.Claims.GetUserId();

            await _uploadRecordRepository.InsertAsync(new UploadRecord
            {
                Sha1 = model.Sha1,
                Size = model.FileSize,
                Duration = model.Duration,
                Type = model.Type,
                UploadTime = DateTime.UtcNow,
                Url = model.Url,
                UserId = userId
            });
        }

        public async Task<string> Get(string sha1)
        {
            return await _uploadRecordRepository.GetAll().AsNoTracking()
                .Where(s => s.Sha1 == sha1)
                .Select(s => s.Url)
                .FirstOrDefaultAsync();
        }
    }


}
