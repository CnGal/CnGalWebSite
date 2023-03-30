
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CnGalWebSite.DrawingBed.Models.ViewModels;
using CnGalWebSite.DrawingBed.Models.DataModels;

namespace CnGalWebSite.DrawingBed.Helper.Services
{
    public interface IFileUploadService
    {
        Task<UploadResult> UploadImagesAsync(IBrowserFile file, ImageAspectType type);

        Task<UploadResult> UploadImagesAsync(byte[] bytes, string fileName, ImageAspectType type);

        Task<UploadResult> UploadAudioAsync(IBrowserFile file);

        Task<string> TransformImageAsync(string url, double x = 0, double y = 0);

        Task<TransformImageResult> TransformImagesAsync(string text);
    }
}
