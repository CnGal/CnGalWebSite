using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Files;
using CnGalWebSite.DrawingBed.Services;
using CnGalWebSite.Helper.Extensions;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Text;
using System.Text.RegularExpressions;

namespace CnGalWebSite.DrawingBed.Controllers
{
    [ApiController]
    [Route("api/files/[action]")]
    public class FileController : ControllerBase
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IFileService _fileService;

        public FileController(IHttpClientFactory clientFactory, IWebHostEnvironment webHostEnvironment, IConfiguration configuration, IFileService fileService)
        {
            _clientFactory = clientFactory;
            _webHostEnvironment = webHostEnvironment;
            _configuration = configuration;
            _fileService = fileService;
        
        }

        /// <summary>
        /// 上传图片
        /// </summary>
        /// <param name="files"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<List<UploadResult>>> UploadAsync([FromForm] List<IFormFile> files, [FromQuery] double x, [FromQuery] double y)
        {
            if (files.Count == 0)
            {
                files.AddRange(HttpContext.Request.Form.Files);
            }
            var model = new List<UploadResult>();
            foreach (var item in files)
            {
                var url = await _fileService.UploadFormFile(item,x,y);
                if (string.IsNullOrWhiteSpace(url))
                {
                    model.Add(new UploadResult
                    {
                        Uploaded = false
                    });
                }
                else
                {
                    model.Add(new UploadResult
                    {
                        Uploaded = true,
                        FileName = item.FileName,
                        FileURL = url
                    });
                }
            }

            return model;
        }

        /// <summary>
        /// 转存图片
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<LinkToImgResult>> linkToImgUrlAsync([FromQuery] string url, [FromQuery] double x, [FromQuery] double y)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                // 获取请求参数
                var request = HttpContext.Request;
                request.EnableBuffering();
                var postJson = "";
                var stream = HttpContext.Request.Body;
                long? length = HttpContext.Request.ContentLength;
                if (length != null && length > 0)
                {
                    // 使用这个方式读取，并且使用异步
                    StreamReader streamReader = new StreamReader(stream, Encoding.UTF8);
                    postJson = streamReader.ReadToEndAsync().Result;
                }

                HttpContext.Request.Body.Position = 0;
                var urls = postJson.GetLinks();
                url = urls.FirstOrDefault();
            }



            string res = "";
            if (url.Contains("image.cngal.org") || url.Contains("pic.cngal.top"))
            {
                res = url;
            }
            else
            {
                res = await _fileService.TransferDepositFile(url, x, y);

            }

            if (string.IsNullOrWhiteSpace(res))
            {
                return new LinkToImgResult
                {
                    Successful = false,
                    OriginalUrl = url,

                };
            }
            else
            {
                return new LinkToImgResult
                {
                    OriginalUrl = url,
                    Url = res,
                    Successful = true,

                };
            }
        }

    }
}
