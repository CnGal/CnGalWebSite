using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace CnGalWebSite.DataModel.ViewModel.Admin
{
    public class EditCarouselsViewModel
    {
        public List<CarouselModel> Carousels { get; set; }
    }
    public class CarouselModel
    {
        [Display(Name = "图片")]
        public string ImagePath { get; set; }

        [Display(Name = "链接")]
        [Required(ErrorMessage = "请填写链接")]
        public string Link { get; set; }
        [Display(Name = "备注")]
        public string Note { get; set; }
        [Display(Name = "优先级")]
        public int Priority { get; set; }

    }
}
