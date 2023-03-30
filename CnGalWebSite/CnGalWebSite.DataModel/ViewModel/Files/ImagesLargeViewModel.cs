using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace CnGalWebSite.DataModel.ViewModel.Files.Images
{
    public class ImagesLargeViewModel
    {
        public List<EditImageAloneModel> Pictures { get; set; }

        public int Index { get; set; }
    }
}
