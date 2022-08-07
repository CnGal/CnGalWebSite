using CnGalWebSite.DataModel.Model;
using System.ComponentModel.DataAnnotations;
namespace CnGalWebSite.DataModel.ViewModel.Ranks
{
    public class CreateRankModel
    {
        [Display(Name = "名称")]
        public string Name { get; set; }
        [Display(Name = "显示文字")]
        public string Text { get; set; }
        [Display(Name = "CSS")]
        public string CSS { get; set; }
        [Display(Name = "Styles")]
        public string Styles { get; set; }
        [Display(Name = "图片")]
        public string Image { get; set; }
        [Display(Name = "类型")]
        public RankType Type { get; set; }

    }
}
