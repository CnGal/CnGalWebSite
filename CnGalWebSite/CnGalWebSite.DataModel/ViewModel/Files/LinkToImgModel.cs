using CnGalWebSite.DataModel.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace CnGalWebSite.DataModel.ViewModel.Files
{
    public class LinkToImgResult : Result
    {
        public string OriginalUrl { get; set; }

        public string Url { get; set; }
    }
}
