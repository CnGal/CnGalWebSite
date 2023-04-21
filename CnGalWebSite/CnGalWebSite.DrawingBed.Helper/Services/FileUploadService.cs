using Microsoft.AspNetCore.Components.Forms;
using System.Drawing;
using System;
using System.Net.Http.Json;
using System.Security.Policy;
using System.Text.Json;
using static System.Net.WebRequestMethods;
using Microsoft.Extensions.Logging;
using System.IO;
using CnGalWebSite.Core.Services;
using CnGalWebSite.DrawingBed.Models.DataModels;
using CnGalWebSite.DrawingBed.Models.ViewModels;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Net.Http;
using CnGalWebSite.Extensions;

namespace CnGalWebSite.DrawingBed.Helper.Services
{
    public class FileUploadService : IFileUploadService
    {
        private readonly ILogger<FileUploadService> _logger;
        private readonly IHttpService _httpService;
        private readonly IConfiguration _configuration;

        private readonly string _baseUrl;

        public FileUploadService(ILogger<FileUploadService> logger, IHttpService httpService, IConfiguration configuration)
        {
            _logger = logger;
            _httpService = httpService;
            _configuration = configuration;

            _baseUrl = configuration["ImageApiPath"];
        }

        public async Task<UploadResult> UploadImagesAsync(IBrowserFile file, ImageAspectType type)
        {
            using var fileContent = new StreamContent(file.OpenReadStream(file.Size));
            return await UploadImagesAsync(fileContent, file.Name, type, file.Size);
        }

        public async Task<UploadResult> UploadImagesAsync(byte[] bytes, string fileName, ImageAspectType type)
        {
            //复制数据
            using var fileContent = new StreamContent(new MemoryStream(bytes));
            return await UploadImagesAsync(fileContent, fileName, type, bytes.LongLength);
        }

        public async Task<UploadResult> UploadImagesAsync(StreamContent steam, string fileName, ImageAspectType type, long size)
        {
            using var content = new MultipartFormDataContent();

            double x, y;
            content.Add(
                content: steam,
                name: "\"files\"",
                fileName: fileName);

            switch (type)
            {
                case ImageAspectType._1_1:
                    x = y = 1;
                    break;
                case ImageAspectType._16_9:
                    x = 460;
                    y = 215;

                    break;
                case ImageAspectType._9_16:
                    x = 9;
                    y = 16;
                    break;
                case ImageAspectType._4_1A2:
                    x = 4;
                    y = 1.2;

                    break;
                default:
                    x = y = 0;
                    break;
            }
            var client = await _httpService.GetClientAsync();
            var response = await client.PostAsync($"{_baseUrl}api/files/Upload?x={x}&y={y}", content);

            var newUploadResults = await response.Content
                .ReadFromJsonAsync<List<UploadResult>>();

            var result = newUploadResults.FirstOrDefault();

            _logger.LogInformation("上传图片 {result}，url：{url}", result.Uploaded ? "成功" : "失败", result.Url);

            return result;
        }

        public async Task<UploadResult> UploadAudioAsync(IBrowserFile file)
        {
            //复制数据
            using var fileContent = new StreamContent(file.OpenReadStream(file.Size));
            using var content = new MultipartFormDataContent();

            content.Add(
                content: fileContent,
                name: "\"files\"",
                fileName: file.Name);

            var client = await _httpService.GetClientAsync();
            var response = await client.PostAsync($"{_baseUrl}api/files/Upload?type={UploadFileType.Audio}", content);

            var newUploadResults = await response.Content
                .ReadFromJsonAsync<List<UploadResult>>();

            var result = newUploadResults.FirstOrDefault();

            _logger.LogInformation("上传音频 {result}，url：{url}", result.Uploaded ? "成功" : "失败", result.Url);

            return result;
        }

        public async Task<TransformImageResult> TransformImagesAsync(string text)
        {

            if (string.IsNullOrWhiteSpace(text))
            {
                return new TransformImageResult { Text = text };
            }

            var model = new TransformImageResult();

            StringBuilder sb = new StringBuilder(text);
            sb.Replace("media.st.dl.pinyuncloud.com", "media.st.dl.eccdnx.com");
            sb.Replace("pic.cngal.top", "image.cngal.org");
            //提取全部图片
            var oldImages = sb.ToString().GetImageLinks();
            //判断外部图片
            oldImages.RemoveAll(s => s.Contains("image.cngal.org"));
            //依次转存
            //替换原图片
            foreach (var item in oldImages)
            {
                try
                {
                    var result = await _httpService.PostAsync<UploadResult, UploadResult>($"{_baseUrl}api/files/linkToImgUrl?url={item}", new UploadResult());
                    if (result.Uploaded)
                    {
                        model.UploadResults.Add(result);

                        sb.Replace(item, result.Url);
                    }

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "图片转存失败，Url：{url}", item);
                }

            }
            //保存
            sb.Replace("<br>", "\n");
            sb.Replace("\\[\\]", "[]");

            model.Text = sb.ToString();

            return model;
        }

        public async Task<string> TransformImageAsync(string url, double x = 0, double y = 0)
        {
            try
            {
                var result = await _httpService.PostAsync<UploadResult, UploadResult>($"{_baseUrl}api/files/linkToImgUrl?url={url}&x={x}&y={y}", new UploadResult());

                _logger.LogInformation("转存图片 {result}，url：{url}", result.Uploaded ? "成功" : "失败", result.Uploaded ? result.Url:result.OriginalUrl);

                if (result.Uploaded)
                {
                    return result.Url;
                }
                else
                {
                    return url;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "转存图片失败：{link}", url);
                return url;
            }

        }
    }
}
