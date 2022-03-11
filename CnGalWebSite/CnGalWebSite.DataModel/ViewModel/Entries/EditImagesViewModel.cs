using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Entries;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace CnGalWebSite.DataModel.ViewModel
{
    public class EditImagesViewModel : BaseEntryEditModel
    {
        public List<EditImageAloneModel> Images { get; set; } = new List<EditImageAloneModel>();
    }

    public class EditImageAloneModel
    {
        [Display(Name = "分类")]
        public string Modifier { get; set; }
        [Display(Name = "备注")]
        public string Note { get; set; }
        [Display(Name = "链接")]
        [Required(ErrorMessage = "请输入链接")]
        public string Url { get; set; }
    }

    public class ImagesUploadAloneModel
    {
        public string Image { get; set; }

        public string ImagePath { get; set; }
    }
}
