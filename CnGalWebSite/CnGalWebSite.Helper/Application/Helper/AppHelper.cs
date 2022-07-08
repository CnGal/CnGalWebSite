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

namespace CnGalWebSite.DataModel.Application.Helper
{
    public class AppHelper : IAppHelper
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AppHelper> _logger;

        public AppHelper(HttpClient httpClient, ILogger<AppHelper> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<UploadResult> UploadFilesAsync(IBrowserFile file, ImageAspectType type)
        {
            using var content = new MultipartFormDataContent();
            using var fileContent = new StreamContent(file.OpenReadStream(file.Size));
            return await UploadFilesAsync(fileContent, file.Name, type);
        }

        public async Task<UploadResult> UploadFilesAsync(byte[] bytes, string fileName, ImageAspectType type)
        {
            //复制数据
            using var content = new MultipartFormDataContent();
            using var fileContent = new StreamContent(new MemoryStream(bytes));
            return await UploadFilesAsync(fileContent, fileName, type);
        }

        public async Task<UploadResult> UploadFilesAsync(StreamContent steam, string fileName, ImageAspectType type)
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
            var response = await _httpClient.PostAsync($"{ToolHelper.ImageApiPath}api/files/Upload?x={x}&y={y}", content);

            var newUploadResults = await response.Content
                .ReadFromJsonAsync<List<UploadResult>>();

            var result= newUploadResults.FirstOrDefault();
            if(result.Uploaded)
            {
                await AddUserLoadedFileInfor(result.FileURL, 0);
            }

            return result;
        }
        /// <summary>
        /// 向用户文件管理添加信息
        /// </summary>
        public async Task AddUserLoadedFileInfor(string url,long size)
        {
            try
            {
                ImageInforTipViewModel model = new ImageInforTipViewModel
                {
                    FileName = url,
                    FileSize = size,
                };

                var result = await _httpClient.PostAsJsonAsync(ToolHelper.WebApiPath + "api/files/AddUserLoadedFileInfor", model);
                string jsonContent = result.Content.ReadAsStringAsync().Result;
                Result obj = JsonSerializer.Deserialize<Result>(jsonContent, ToolHelper.options);
                //判断结果
                if (obj.Successful == false)
                {
                    _logger.LogError("保存上传文件信息失败：{name}",url);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"保存上传文件信息失败：{name}", url);
            }
        }

    }
}
