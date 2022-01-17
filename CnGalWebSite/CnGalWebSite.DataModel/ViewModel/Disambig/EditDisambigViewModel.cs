using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace CnGalWebSite.DataModel.ViewModel.Disambig
{
    public class EditDisambigViewModel
    {
        public int Id { get; set; }
        [Display(Name = "名称")]
        [Required(ErrorMessage = "请填写名称")]
        public string Name { get; set; }

        [Display(Name = "简介")]
        [Required(ErrorMessage = "请填写简介")]
        public string BriefIntroduction { get; set; }
        //[Required(ErrorMessage = "请上传主图")]
        [Display(Name = "主图")]
        public string MainPicture { get; set; }

        [Display(Name = "背景图")]
        public string BackgroundPicture { get; set; }

        [Display(Name = "小背景图")]
        public string SmallBackgroundPicture { get; set; }


        public string MainPicturePath { get; set; }
        public string BackgroundPicturePath { get; set; }
        public string SmallBackgroundPicturePath { get; set; }

        public List<DisambigRelevanceModel> Entries { get; set; }
        public List<DisambigRelevanceModel> Articles { get; set; }

        [Display(Name = "备注")]
        public string Note { get; set; }


    }
}
