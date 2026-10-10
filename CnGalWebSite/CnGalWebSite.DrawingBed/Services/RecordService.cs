using CnGalWebSite.DrawingBed.DataReositories;
using CnGalWebSite.DrawingBed.Models.DataModels;
using CnGalWebSite.DrawingBed.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using CnGalWebSite.DrawingBed.Configuration;
using Microsoft.Extensions.Options;

namespace CnGalWebSite.DrawingBed.Services
{
    public class RecordService:IRecordService
    {
        private readonly IRepository<UploadRecord, long> _uploadRecordRepository;
        private readonly IHttpContextAccessor _accessor;
        private readonly string _imagePrefix;
        private readonly string _audioPrefix;
        private readonly string _galleryPrefix;

        public RecordService(IRepository<UploadRecord, long> uploadRecordRepository, IHttpContextAccessor accessor,
            IOptions<TencentCosOptions> tencentOptions, IOptions<AliyunOssOptions> aliyunOptions,
            IOptions<TucangCcOptions> tucangOptions)
        {
            _uploadRecordRepository = uploadRecordRepository;
            _accessor = accessor;
            _imagePrefix = tencentOptions.Value.PublicBaseAddress + UploadFilePolicy.ImageObjectPrefix;
            _audioPrefix = aliyunOptions.Value.PublicBaseAddress + UploadFilePolicy.AudioObjectPrefix;
            _galleryPrefix = tucangOptions.Value.PublicBaseAddress;
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
            return await GetValidatedRecords()
                .Where(s => s.Sha1 == sha1)
                .Select(s => s.Url)
                .FirstOrDefaultAsync();
        }

        public async Task<string> GetRandom(UploadFileType type)
        {
            UploadFilePolicy.ValidateType(type);
            var query = GetValidatedRecords().Where(s => s.Type == type);
            var count = await query.CountAsync();
            return count == 0 ? null : await query.OrderBy(s => s.Id).Skip(Random.Shared.Next(count))
                .Select(s => s.Url).FirstOrDefaultAsync();
        }

        private IQueryable<UploadRecord> GetValidatedRecords()
        {
            // 旧哈希记录可能指向 HTML/SVG 文件，或旧逻辑直接复用的未校验链接
            return _uploadRecordRepository.GetAll().AsNoTracking()
                .Where(s => s.Url != null && (s.Url.StartsWith(_imagePrefix) || s.Url.StartsWith(_audioPrefix) ||
                    // 图库链接在第一个问号后保存已校验的 COS 备用地址
                    (!string.IsNullOrEmpty(_galleryPrefix) && s.Url.StartsWith(_galleryPrefix) &&
                     s.Url.Contains("?") && !s.Url.Contains("#") &&
                     s.Url.Substring(s.Url.IndexOf("?") + 1).StartsWith(_imagePrefix))));
        }
    }


}
