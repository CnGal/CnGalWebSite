using System;
using System.Collections.Generic;
using System.Text;

namespace CnGalWebSite.DataModel.ViewModel.Files
{
    public class LinkToImgModel
    {
        public string msg { get; set; }

        public int code { get; set; } = 200;

        public LinkToImgData date { get; set; } = new LinkToImgData();
    }

    public class LinkToImgData
    {
        public string originalURL { get; set; }

        public string url { get; set; }
    }
}
