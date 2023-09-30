using CnGalWebSite.Core.Models;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Base;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace CnGalWebSite.DataModel.ViewModel.Admin
{
    public class EditCarouselsViewModel
    {
        public List<CarouselModel> Carousels { get; set; }=new List<CarouselModel>();
    }
    public class CarouselModel: BaseImageEditModel
    {
        [Display(Name = "链接")]
        [Required(ErrorMessage = "请填写链接")]
        public string Link { get; set; }

        [Display(Name = "类型")]
        public CarouselType Type { get; set; }
    }
}
