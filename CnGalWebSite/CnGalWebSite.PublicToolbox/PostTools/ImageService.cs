using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Files;
using CnGalWebSite.DataModel.ViewModel.HistoryData;
using CnGalWebSite.Helper.Helper;
using CnGalWebSite.PublicToolbox.DataRepositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CnGalWebSite.PublicToolbox.PostTools
{
    public class ImageService:IImageService
    {
        private readonly HttpClient _httpClient;
        private readonly IRepository<OriginalImageToDrawingBedUrl> _imageRepository;
        private readonly ILogger<ImageService> _logger;

        public ImageService(HttpClient client, IRepository<OriginalImageToDrawingBedUrl> imageRepository, ILogger<ImageService> logger)
        {
            _httpClient = client;
            _imageRepository = imageRepository;
            _logger = logger;
        }

        public async Task<string> UploadImageAsync(string url, double x = 0, double y = 0)
        {
            try
            {
                var result = await _httpClient.PostAsJsonAsync("https://api.cngal.top/api/files/TransferDepositFile", new TransferDepositFileModel
                {
                    Url = url,
                    X = x,
                    Y = y
                });

                var jsonContent = result.Content.ReadAsStringAsync().Result;
                var obj = System.Text.Json.JsonSerializer.Deserialize<Result>(jsonContent, ToolHelper.options);

                if (obj.Successful)
                {
                    var temp = obj.Error;
                    _logger.LogInformation("上传完成：{link}", temp);

                    return temp;
                }
                else
                {
                    _logger.LogError("上传失败：{link}", url);
                    return url;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"上传失败：{link}", url);
                return url;
            }

        }

        public async Task<string> GetImage(string url)
        {
            var image = _imageRepository.GetAll().FirstOrDefault(s => s.OldUrl == url);
            if (image == null)
            {
                var newUrl = await UploadImageAsync(url);
                await _imageRepository.InsertAsync(new OriginalImageToDrawingBedUrl
                {
                    NewUrl = newUrl,
                    OldUrl = url
                });
                return newUrl;
            }
            else
            {
                return image.NewUrl;
            }
        }
    }
}
