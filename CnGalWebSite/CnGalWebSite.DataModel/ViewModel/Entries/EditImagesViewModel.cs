using CnGalWebSite.DataModel.ViewModel.Base;
using CnGalWebSite.DataModel.ViewModel.Entries;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace CnGalWebSite.DataModel.ViewModel
{
    public class EditImagesViewModel : BaseEntryEditModel
    {
        public List<EditImageAloneModel> Images { get; set; } = new List<EditImageAloneModel>();
    }

    public class EditImageAloneModel: BaseImageEditModel
    {
        [Display(Name = "分类")]
        public string Modifier { get; set; }
    }

    public class ImagesUploadAloneModel
    {
        public string Image { get; set; }
    }
}
