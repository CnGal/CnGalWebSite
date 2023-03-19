using BootstrapBlazor.Components;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Files;
using CnGalWebSite.DataModel.ViewModel.Files.Images;
using Microsoft.AspNetCore.Components.Forms;
using System.Drawing;
using System;
using System.Net.Http.Json;
using System.Security.Policy;
using System.Text.Json;
using static System.Net.WebRequestMethods;
using Microsoft.Extensions.Logging;
using System.IO;
using CnGalWebSite.DataModel.ViewModel.Others;


namespace CnGalWebSite.Shared.Service
{
    public class FileUploadService:IFileUploadService
    {
        private readonly ILogger<FileUploadService> _logger;
        private readonly IHttpService _httpService;

        public FileUploadService(ILogger<FileUploadService> logger, IHttpService httpService)
        {
            _logger = logger;
            _httpService = httpService;
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
            var response = await client.PostAsync($"{ToolHelper.ImageApiPath}api/files/Upload?x={x}&y={y}", content);

            var newUploadResults = await response.Content
                .ReadFromJsonAsync<List<UploadResult>>();

            var result = newUploadResults.FirstOrDefault();
            if (result.Uploaded)
            {
                await AddUserLoadedFileInfor(result, UploadFileType.Image);
            }

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
            var response = await client.PostAsync($"{ToolHelper.ImageApiPath}api/files/Upload?type={UploadFileType.Audio}", content);

            var newUploadResults = await response.Content
                .ReadFromJsonAsync<List<UploadResult>>();

            var result = newUploadResults.FirstOrDefault();
            if (result.Uploaded)
            {
                await AddUserLoadedFileInfor(result, UploadFileType.Audio);
            }

            return result;
        }

        /// <summary>
        /// 向用户文件管理添加信息
        /// </summary>
        public async Task AddUserLoadedFileInfor(UploadResult infor, UploadFileType type)
        {
            try
            {
                AddUserUploadFileInforModel model = new AddUserUploadFileInforModel
                {
                    FileName = infor.Url,
                    FileSize = infor.FileSize,
                    Duration = infor.Duration,
                    Sha1 = infor.Sha1,
                    Type = type
                };

                var client = await _httpService.GetClientAsync();
                var result = await client.PostAsJsonAsync(ToolHelper.WebApiPath + "api/files/AddUserUploadFileInfor", model);
                string jsonContent = result.Content.ReadAsStringAsync().Result;
                Result obj = JsonSerializer.Deserialize<Result>(jsonContent, ToolHelper.options);
                //判断结果
                if (obj.Successful == false)
                {
                    _logger.LogError("保存上传文件信息失败：{name}", infor.Url);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "保存上传文件信息失败：{name}", infor.Url);
            }
        }
    }
}
