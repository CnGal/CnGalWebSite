using System.ComponentModel.DataAnnotations;

using System;
 using System.Collections.Generic;
namespace CnGalWebSite.DataModel.ViewModel.Tags
{
    public class EditTagMainViewModel
    {
        public int Id { get; set; }

        [Display(Name = "名称")]
        [Required(ErrorMessage = "请填写名称")]
        public string Name { get; set; }

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

        [Display(Name = "父标签")]
        public string ParentTagName { get; set; }


        [Display(Name = "备注")]
        public string Note { get; set; }
    }
}
