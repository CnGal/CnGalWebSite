using System;
 using System.Collections.Generic;
namespace CnGalWebSite.DataModel.ViewModel.Files
{
    public class ImageInforTipViewModel
    {
        public int Id { get; set; }

        public string FileName { get; set; }

        public long? FileSize { get; set; }

        public string UserName { get; set; }

        public string UserId { get; set; }

        public string Sha1 { get; set; }

        public DateTime UploadTime { get; set; }
    }
}
