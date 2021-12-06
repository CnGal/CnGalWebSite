using System.ComponentModel.DataAnnotations;

namespace CnGalWebSite.DataModel.ViewModel.Entries
{
    public class EntryAddNewsViewModel
    {
        [Required(ErrorMessage = "请输入标题")]
        [Display(Name = "标题")]
        public string Name { get; set; }
        [Display(Name = "动态真实发生时间")]
        public DateTime? RealTime { get; set; }
        [Required(ErrorMessage = "请输入简介")]
        [Display(Name = "简介")]
        public string BriefIntroduction { get; set; }
        [Required(ErrorMessage = "请输入正文")]
        [Display(Name = "正文")]
        public string Text { get; set; }
    }
}
