using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.DrawingBed.Models.ViewModels
{
    public class TransformImageResult
    {
        public string Text { get; set; }

        public List<UploadResult> UploadResults { get; set; } = new List<UploadResult>();
    }
}
