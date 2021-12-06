using CnGalWebSite.DataModel.Model;
using System.ComponentModel.DataAnnotations;

namespace CnGalWebSite.DataModel.ViewModel
{
    public class EditMainViewModel
    {
        public int Id { get; set; }
        [Display(Name = "唯一名称")]
        [Required(ErrorMessage = "请填写唯一名称")]
        public string Name { get; set; }
        [Display(Name = "显示名称")]
        [Required(ErrorMessage = "请填写显示名称")]
        public string DisplayName { get; set; }
        [Display(Name = "别称")]
        public string AnotherName { get; set; }

        [Display(Name = "简介")]
        public string BriefIntroduction { get; set; }

        [Display(Name = "主图")]
        public string MainPicture { get; set; }
        [Display(Name = "缩略图")]
        public string Thumbnail { get; set; }
        [Display(Name = "背景图")]
        public string BackgroundPicture { get; set; }
        [Display(Name = "小背景图")]
        public string SmallBackgroundPicture { get; set; }


        [Display(Name = "类别")]
        [Required(ErrorMessage = "请选择类别")]
        public EntryType Type { get; set; }

        [Display(Name = "主图")]
        public string MainPicturePath { get; set; }

        public string ThumbnailPath { get; set; }

        public string BackgroundPicturePath { get; set; }

        public string SmallBackgroundPicturePath { get; set; }

        [Display(Name = "备注")]
        public string Note { get; set; }

    }
}
