using Microsoft.AspNetCore.Components.Forms;

using System;
 using System.Collections.Generic;
namespace CnGalWebSite.DataModel.Model
{
    public class UploadModel
    {
        public int x { get; set; }

        public int y { get; set; }

        public IBrowserFile File { get; set; }
    }
}
