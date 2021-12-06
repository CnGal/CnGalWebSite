using System.ComponentModel.DataAnnotations;

namespace CnGalWebSite.DataModel.ViewModel.Files.Images
{
    public class ImagesLargeViewModel
    {
        public List<EditImageAloneModel> Pictures { get; set; }

        public int Index { get; set; }
    }

    public enum ImageAspectType
    {
        [Display(Name = "")]
        None,
        [Display(Name = "1_1")]
        _1_1,
        [Display(Name = "16_9")]
        _16_9,
        [Display(Name = "9_16")]
        _9_16,
        [Display(Name = "4_1A2")]
        _4_1A2
    }
}
