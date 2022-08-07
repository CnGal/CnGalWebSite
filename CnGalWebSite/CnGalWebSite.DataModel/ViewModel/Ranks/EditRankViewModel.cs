using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Base;
using System.ComponentModel.DataAnnotations;
namespace CnGalWebSite.DataModel.ViewModel.Ranks
{
    public class EditRankViewModel:BaseEditModel
    {
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

        public override Result Validate()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                return new Result { Error = "必须填写头衔名称" };
            }

            return new Result { Successful = true };
        }
    }
}
