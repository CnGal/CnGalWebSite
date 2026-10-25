using CnGalWebSite.DrawingBed.Models.Sorting;
using CnGalWebSite.Core.Models;
using CnGalWebSite.Core.Services.Query;
using CnGalWebSite.DrawingBed.DataReositories;
using CnGalWebSite.DrawingBed.Models.DataModels;
using CnGalWebSite.DrawingBed.Models.ViewModels;
using CnGalWebSite.DrawingBed.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using System.IO;
using System.Text;

namespace CnGalWebSite.DrawingBed.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/files/[action]")]
    public class FileController : ControllerBase
    {
        private static readonly SortMap<UploadRecord, DrawingBedUploadRecordSortField> ListSorting =
            new SortMap<UploadRecord, DrawingBedUploadRecordSortField>(DrawingBedUploadRecordSortField.Id, DrawingBedUploadRecordSortField.Id)
            .Add(DrawingBedUploadRecordSortField.Id, s => s.Id)
            .Add(DrawingBedUploadRecordSortField.Sha1, s => s.Sha1)
            .Add(DrawingBedUploadRecordSortField.Size, s => s.Size)
            .Add(DrawingBedUploadRecordSortField.Duration, s => s.Duration)
            .Add(DrawingBedUploadRecordSortField.Type, s => s.Type)
            .Add(DrawingBedUploadRecordSortField.UploadTime, s => s.UploadTime)
            .Add(DrawingBedUploadRecordSortField.Url, s => s.Url)
            .Add(DrawingBedUploadRecordSortField.UserId, s => s.UserId);


        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IFileService _fileService;
        private readonly IUploadService _uploadService;
        private readonly IRecordService _recordService;
        private readonly IQueryService _queryService;
        private readonly IRepository<UploadRecord, long> _uploadRecordRepository;
        private readonly ILogger<FileController> _logger;

        public FileController(IHttpClientFactory clientFactory, IWebHostEnvironment webHostEnvironment, IConfiguration configuration, IFileService fileService, IQueryService queryService, IRepository<UploadRecord, long> uploadRecordRepository, IUploadService uploadService, ILogger<FileController> logger, IRecordService recordService)
        {
            _clientFactory = clientFactory;
            _webHostEnvironment = webHostEnvironment;
            _configuration = configuration;
            _fileService = fileService;
            _queryService = queryService;
            _uploadRecordRepository = uploadRecordRepository;
            _uploadService = uploadService;
            _recordService = recordService;
            _logger = logger;
        }

        /// <summary>
        /// 上传文件 图片或音频
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [EnableRateLimiting("uploads")]
        public async Task<ActionResult<List<UploadResult>>> UploadAsync([FromForm] List<IFormFile> files, [FromQuery] double x, [FromQuery] double y, [FromQuery] UploadFileType type, [FromQuery] bool gallery = false, [FromQuery] double cropX = 0, [FromQuery] double cropY = 0, [FromQuery] double cropW = 0, [FromQuery] double cropH = 0)
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
                    model.Add(await _fileService.UploadFormFile(item, gallery, x, y, type, cropX, cropY, cropW, cropH));
                }
                catch (InvalidDataException ex)
                {
                    // 文件校验异常的消息由 UploadFilePolicy 生成，不包含服务器路径，可安全返回给用户。
                    model.Add(new UploadResult
                    {
                        Uploaded = false,
                        FileName = item.Name,
                        Error = ex.Message
                    });
                }
                catch (Exception)
                {
                    // 其他异常可能泄露服务器路径（如 ffmpeg 输出），只记日志不回显。
                    model.Add(new UploadResult
                    {
                        Uploaded = false,
                        FileName = item.Name,
                        Error = "上传失败，请检查文件是否有效"
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
        [EnableRateLimiting("uploads")]
        public async Task<ActionResult<UploadResult>> linkToImgUrlAsync([FromQuery] string url, [FromQuery] double x, [FromQuery] double y, [FromQuery] UploadFileType type, [FromQuery] bool gallery = false, [FromQuery] double cropX = 0, [FromQuery] double cropY = 0, [FromQuery] double cropW = 0, [FromQuery] double cropH = 0)
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
                var result = await _fileService.TransferDepositFile(url, gallery, x, y, type, cropX, cropY, cropW, cropH, HttpContext.RequestAborted);

                return result;
            }
            catch (Exception ex)
            {
                //错误细节可能泄露内网探测信息（连接错误/目标地址），只记日志不回显
                return new UploadResult
                {
                    Uploaded = false,
                    OriginalUrl = url,
                    Error = ex is HttpRequestException { StatusCode: System.Net.HttpStatusCode.RequestEntityTooLarge }
                        ? "远程文件不能超过20 MiB" : "转存失败，请稍后重试"
                };
            }

        }


        /// <summary>
        /// 转存图片到TucangCC
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        [EnableRateLimiting("uploads")]
        public async Task<ActionResult<UploadResult>> TransferDepositToTucangCCAsync([FromQuery] string url)
        {
            string path = "";
            try
            {
                path = await _fileService.SaveFileFromUrl(url, UploadFileType.Image, HttpContext.RequestAborted);
                var sha1 = _fileService.GetSHA1(path);
                var result = await _uploadService.UploadToTucangCC(path, $"{sha1}{Path.GetExtension(path)}");
                _fileService.DeleteFile(path);

                return new UploadResult
                {
                    Url = result,
                    OriginalUrl = url,
                    Uploaded = true
                };
            }
            catch (UnknownImageFormatException)
            {
                _fileService.DeleteFile(path);
                return new UploadResult { Uploaded = false, OriginalUrl = url, Error = "不支持的图片格式，保留原地址" };
            }
            catch (Exception ex)
            {
                _fileService.DeleteFile(path);
                _logger.LogError(ex, "转存图片到TucangCC失败");
                //错误细节可能泄露内网探测信息（连接错误/目标地址），只记日志不回显
                return new UploadResult
                {
                    Uploaded = false,
                    OriginalUrl = url,
                    Error = ex is HttpRequestException { StatusCode: System.Net.HttpStatusCode.RequestEntityTooLarge }
                        ? "远程文件不能超过20 MiB" : "转存失败，请稍后重试"
                };
            }

        }


        [HttpPost]
        public async Task<QueryResultModel<UploadRecordOverviewModel, DrawingBedUploadRecordSortField>> List(QueryParameterModel<DrawingBedUploadRecordSortField> model)
        {
            var (items, total) = await _queryService.QueryAsync<UploadRecord, DrawingBedUploadRecordSortField>(_uploadRecordRepository.GetAll().AsSingleQuery(), model,
                s => string.IsNullOrWhiteSpace(model.SearchText) || (s.Sha1.Contains(model.SearchText) || s.Url.Contains(model.SearchText) || s.UserId.Contains(model.SearchText)), ListSorting);

            return new QueryResultModel<UploadRecordOverviewModel, DrawingBedUploadRecordSortField>
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

        /// <summary>
        /// 获取相同sha1的图片
        /// </summary>
        /// <param name="sha1"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<Result>> GetSameFileAsync([FromQuery] string sha1)
        {
            if (string.IsNullOrWhiteSpace(sha1))
            {
                return new Result { Success = false };
            }

            var url = await _recordService.Get(sha1);
            if (url == null)
            {
                return new Result { Success = false };
            }

            return new Result { Success = true, Message = url };

        }

        /// <summary>
        /// 获取随机类型的图片或音频
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<Result>> GetRandomFileAsync([FromQuery] UploadFileType type)
        {
            if (type is not (UploadFileType.Image or UploadFileType.Audio))
                return new Result { Success = false };
            var url = await _recordService.GetRandom(type);
            return new Result { Success = url != null, Message = url };
        }
    }
}
