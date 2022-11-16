using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Files;
using CnGalWebSite.DataModel.ViewModel.HistoryData;
using CnGalWebSite.PublicToolbox.DataRepositories;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace CnGalWebSite.PublicToolbox.PostTools
{
    public class ImageService : IImageService
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

        private async Task<string> UploadImageAsync(string url, double x = 0, double y = 0)
        {
            try
            {
                var result = await _httpClient.PostAsJsonAsync($"{ToolHelper.ImageApiPath}api/files/linkToImgUrl?url={url}&x={x}&y={y}", new TransferDepositFileModel
                {
                    Url = url,
                    X = x,
                    Y = y
                });

                var jsonContent = result.Content.ReadAsStringAsync().Result;
                var obj = System.Text.Json.JsonSerializer.Deserialize<UploadResult>(jsonContent, ToolHelper.options);

                if (obj.Uploaded)
                {
                    var temp = obj.Url;
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
                _logger.LogError(ex, "上传失败：{link}", url);
                return url;
            }

        }

        public async Task<string> GetImage(string url, double x = 0, double y = 0)
        {
            if(url.Contains("http")==false)
            {
                url = "https:" + url;
            }
            var image = _imageRepository.GetAll().FirstOrDefault(s => s.OldUrl == url && s.X == x && s.Y == y);
            if (image == null || string.IsNullOrWhiteSpace(image.NewUrl) || image.NewUrl == url)
            {
                var newUrl = await UploadImageAsync(url, x, y);
                if (newUrl != url)
                {
                    _ = await _imageRepository.InsertAsync(new OriginalImageToDrawingBedUrl
                    {
                        NewUrl = newUrl,
                        OldUrl = url,
                        X = x,
                        Y = y
                    });
                }

                return newUrl;
            }
            else
            {
                return image.NewUrl;
            }
        }
    }
}
