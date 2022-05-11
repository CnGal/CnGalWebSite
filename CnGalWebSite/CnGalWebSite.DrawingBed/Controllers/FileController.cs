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

        [HttpPost]
        public async Task<ActionResult<Result>> TransferDepositFile(TransferDepositFileModel model)
        {
            string url = "";
            if (model.Url.Contains("image.cngal.org") || model.Url.Contains("pic.cngal.top"))
            {
                url = model.Url;
            }
            else
            {
                url = await _fileService.TransferDepositFile(model);
            }

            if (string.IsNullOrWhiteSpace(url))
            {
                return new Result { Successful = false, Error = "文件上传失败" };
            }
            else
            {
                return new Result { Successful = true, Error = url };
            }
        }

        [HttpPost]
        public async Task<ActionResult<VditorUploadResult>> VditorUploadAsync([FromForm] List<IFormFile> files)
        {
            if (files.Count == 0)
            {
                files.AddRange(HttpContext.Request.Form.Files);
            }
            var model = new VditorUploadResult();
            foreach (var item in files)
            {
                var url = await _fileService.UploadFormFile(item);
                if (string.IsNullOrWhiteSpace(url))
                {
                    model.date.errFiles.Add(item.FileName);
                    model.code = 400;
                }
                else
                {
                    model.date.succMap.Add(new KeyValuePair<string, string>(item.FileName, url));
                }
            }

            return model;
        }


        [HttpPost]
        public async Task<ActionResult<LinkToImgModel>> linkToImgUrlAsync(string url)
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
                res = await _fileService.TransferDepositFile(new TransferDepositFileModel { Url = url });

            }

            if (string.IsNullOrWhiteSpace(res))
            {
                return new LinkToImgModel
                {
                    code = 400,
                    msg = "图片转存失败",
                    date = new LinkToImgData
                    {
                        originalURL = url,
                    }
                };
            }
            else
            {
                return new LinkToImgModel
                {
                    date = new LinkToImgData
                    {
                        originalURL = url,
                        url = res
                    }
                };
            }
        }

    }
}
