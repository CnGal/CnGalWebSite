using Microsoft.AspNetCore.Components.Forms;

namespace CnGalWebSite.DataModel.Model
{
    public class UploadModel
    {
        public int x { get; set; }

        public int y { get; set; }

        public IBrowserFile File { get; set; }
    }
}
