using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.PublicToolbox.PostTools
{
    public interface IImageService
    {
        Task<string> UploadImageAsync(string url, double x = 0, double y = 0);

        Task<string> GetImage(string url);
    }
}
