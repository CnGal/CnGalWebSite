using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Entries;
using System.ComponentModel.DataAnnotations;
namespace CnGalWebSite.DataModel.ViewModel
{
    public class EditMainViewModel : BaseEntryEditModel
    {
        [Display(Name = "样式模板")]
        public EntryStyleTemplate Template { get; set; }

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

        public override Result Validate()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                return new Result { Error = "必须填写词条名称" };
            }
            if (string.IsNullOrWhiteSpace(DisplayName))
            {
                return new Result { Error = "必须填写词条显示名称" };
            }
            return new Result { Successful = true };
        }
    }
}
