using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.Helper.ViewModel.Files
{
    public class TransformImageResult
    {
        public string Text { get; set; }

        public List<UploadResult> UploadResults { get; set; } = new List<UploadResult>();
    }
}
