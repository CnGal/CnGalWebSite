using System.ComponentModel.DataAnnotations;

using System;
 using System.Collections.Generic;
namespace CnGalWebSite.DataModel.ExamineModel
{
    public class DisambigMain
    {
        public string Name { get; set; }

        public string BriefIntroduction { get; set; }

        [Display(Name = "主图")]
        public string MainPicture { get; set; }

        [Display(Name = "背景图")]
        public string BackgroundPicture { get; set; }

        [Display(Name = "小背景图")]
        public string SmallBackgroundPicture { get; set; }
    }
}
