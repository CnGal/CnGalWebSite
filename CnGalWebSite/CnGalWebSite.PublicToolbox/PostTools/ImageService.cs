using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Files;
using CnGalWebSite.DrawingBed.Helper.Services;
using CnGalWebSite.PublicToolbox.DataRepositories;
using CnGalWebSite.PublicToolbox.Models;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace CnGalWebSite.PublicToolbox.PostTools
{
    public class ImageService : IImageService
    {
        private readonly IFileUploadService _fileUploadService;
        private readonly IRepository<OriginalImageToDrawingBedUrl> _imageRepository;
        private readonly ILogger<ImageService> _logger;

        public ImageService(IFileUploadService fileUploadService, IRepository<OriginalImageToDrawingBedUrl> imageRepository, ILogger<ImageService> logger)
        {
            _fileUploadService = fileUploadService;
            _imageRepository = imageRepository;
            _logger = logger;
        }

        private async Task<string> UploadImageAsync(string url, double x = 0, double y = 0)
        {
            return await _fileUploadService.TransformImageAsync(url, x, y);
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
