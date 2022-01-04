using System;
 using System.Collections.Generic;
namespace CnGalWebSite.DataModel.Model
{
    public class Result
    {
        public bool Successful { get; set; }
        public string Error { get; set; }
    }
    public class UploadImageResult
    {
        public int code { get; set; }
        public string message { get; set; }
        public string url { get; set; }
    }
}
