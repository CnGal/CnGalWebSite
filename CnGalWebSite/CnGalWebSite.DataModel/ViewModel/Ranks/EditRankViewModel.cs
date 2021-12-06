using System.ComponentModel.DataAnnotations;

namespace CnGalWebSite.DataModel.ViewModel.Ranks
{
    public class EditRankViewModel
    {
        public long Id { get; set; }

        [Display(Name = "名称")]
        public string Name { get; set; }
        [Display(Name = "显示文字")]
        public string Text { get; set; }
        [Display(Name = "CSS")]
        public string CSS { get; set; }
        [Display(Name = "Styles")]
        public string Styles { get; set; }

    }
}
