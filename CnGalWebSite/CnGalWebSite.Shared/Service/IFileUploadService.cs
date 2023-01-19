using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Files.Images;
using CnGalWebSite.DataModel.ViewModel.Files;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.Shared.Service
{
    public interface IFileUploadService
    {
        Task<UploadResult> UploadImagesAsync(IBrowserFile file, ImageAspectType type);

        Task<UploadResult> UploadImagesAsync(byte[] bytes, string fileName, ImageAspectType type);

        Task AddUserLoadedFileInfor(UploadResult infor, UploadFileType type);

        Task<UploadResult> UploadAudioAsync(IBrowserFile file);
    }
}
