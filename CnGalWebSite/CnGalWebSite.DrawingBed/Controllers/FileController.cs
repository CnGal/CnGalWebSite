using CnGalWebSite.Core.Models;
using CnGalWebSite.Core.Services.Query;
using CnGalWebSite.DrawingBed.DataReositories;
using CnGalWebSite.DrawingBed.Models.DataModels;
using CnGalWebSite.DrawingBed.Models.ViewModels;
using CnGalWebSite.DrawingBed.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Text;

namespace CnGalWebSite.DrawingBed.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/files/[action]")]
    public class FileController : ControllerBase
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IFileService _fileService;
        private readonly IUploadService _uploadService;
        private readonly IQueryService _queryService;
        private readonly IRepository<UploadRecord, long> _uploadRecordRepository;

        public FileController(IHttpClientFactory clientFactory, IWebHostEnvironment webHostEnvironment, IConfiguration configuration, IFileService fileService, IQueryService queryService, IRepository<UploadRecord, long> uploadRecordRepository, IUploadService uploadService)
        {
            _clientFactory = clientFactory;
            _webHostEnvironment = webHostEnvironment;
            _configuration = configuration;
            _fileService = fileService;
            _queryService = queryService;
            _uploadRecordRepository = uploadRecordRepository;
            _uploadService = uploadService;
        }

        /// <summary>
        /// 上传图片
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<List<UploadResult>>> UploadAsync([FromForm] List<IFormFile> files, [FromQuery] double x, [FromQuery] double y, [FromQuery] UploadFileType type, [FromQuery] bool gallery=false)
        {
            if (files.Count == 0)
            {
                files.AddRange(HttpContext.Request.Form.Files);
            }
            var model = new List<UploadResult>();
            foreach (var item in files)
            {
                try
                {
                    model.Add(await _fileService.UploadFormFile(item,gallery, x, y, type));
                }
                catch (Exception ex)
                {
                    model.Add(new UploadResult
                    {
                        Uploaded = false,
                        FileName=item.Name,
                        Error = ex.Message
                    });
                }

            }

            return model;
        }

        /// <summary>
        /// 转存图片
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<UploadResult>> linkToImgUrlAsync([FromQuery] string url, [FromQuery] double x, [FromQuery] double y, [FromQuery] UploadFileType type, [FromQuery] bool gallery = false)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                // 获取请求参数
                var request = HttpContext.Request;
                request.EnableBuffering();
                var postJson = "";
                var stream = HttpContext.Request.Body;
                var length = HttpContext.Request.ContentLength;
                if (length is not null and > 0)
                {
                    // 使用这个方式读取，并且使用异步
                    var streamReader = new StreamReader(stream, Encoding.UTF8);
                    postJson = streamReader.ReadToEndAsync().Result;
                }

                HttpContext.Request.Body.Position = 0;
                var urls = postJson.GetLinks();
                url = urls.FirstOrDefault();
            }


            try
            {
               var result= await _fileService.TransferDepositFile(url,gallery, x, y, type);

                return result;
            }
            catch (Exception ex)
            {
                return new UploadResult
                {
                    Uploaded = false,
                    OriginalUrl = url,
                    Error = ex.Message
                };
            }

        }


        /// <summary>
        /// 转存图片到TucangCC
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<UploadResult>> TransferDepositToTucangCCAsync([FromQuery] string url)
        {
            string path="";
            try
            {
                path = await _fileService.SaveFileFromUrl(url, UploadFileType.Image);
                var result = await _uploadService.UploadToTucangCC(path);
                _fileService.DeleteFile(path);

                return new UploadResult
                {
                    Url = result,
                    OriginalUrl = url,
                    Uploaded=true
                };
            }
            catch (Exception ex)
            {
                _fileService.DeleteFile(path);
                return new UploadResult
                {
                    Uploaded = false,
                    OriginalUrl = url,
                    Error = ex.Message
                };
            }

        }


        [HttpPost]
        public async Task<QueryResultModel<UploadRecordOverviewModel>> List(QueryParameterModel model)
        {
            var (items, total) = await _queryService.QueryAsync<UploadRecord, long>(_uploadRecordRepository.GetAll().AsSingleQuery(), model,
                s => string.IsNullOrWhiteSpace(model.SearchText) || (s.Sha1.Contains(model.SearchText) || s.Url.Contains(model.SearchText) || s.UserId.Contains(model.SearchText)));

            return new QueryResultModel<UploadRecordOverviewModel>
            {
                Items = await items.Select(s => new UploadRecordOverviewModel
                {
                    Id = s.Id,
                    Sha1 = s.Sha1,
                    Size = s.Size,
                    Duration = s.Duration,
                    Type = s.Type,
                    UploadTime = s.UploadTime,
                    Url = s.Url,
                    UserId = s.UserId,
                }).ToListAsync(),
                Total = total,
                Parameter = model
            };
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<Result>> GetSameFileAsync([FromQuery] string sha1)
        {
            if (string.IsNullOrWhiteSpace(sha1))
            {
                return new Result { Success = false };
            }

            var file = await _uploadRecordRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(s => s.Sha1 == sha1);
            if (file == null)
            {
                return new Result { Success = false };
            }

            return new Result { Success = true, Message = file.Url };

        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<Result>> GetRandomFileAsync([FromQuery] UploadFileType type)
        {
            var random = new Random();
            var length = await _uploadRecordRepository.CountAsync(s => string.IsNullOrWhiteSpace(s.Url) == false&&s.Type==type);
            if (length > 0)
            {
                var p = random.Next(1, length - 1);
                var temp = await _uploadRecordRepository.GetAll().Where(s => string.IsNullOrWhiteSpace(s.Url) == false && s.Type == type).OrderBy(s => s.Id).Skip(p).Take(1).ToListAsync();
                if (temp.Any())
                {
                    return new Result { Success = true, Message = temp[0].Url };
                }
                else
                {
                    return new Result { Success = false };
                }
            }
            else
            {
                return new Result { Success = false };
            }
        }
    }
}
