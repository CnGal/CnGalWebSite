using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Base;
using System.ComponentModel.DataAnnotations;
namespace CnGalWebSite.DataModel.ViewModel.Tags
{
    public class EditTagMainViewModel: BaseEditModel
    {
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

        public override Result Validate()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                return new Result { Error = "请填写标签名称" };
            }
            if (Name != "游戏" && Name != "角色" && Name != "制作组" && Name != "STAFF")
            {
                if (string.IsNullOrWhiteSpace(ParentTagName))
                {
                    return new Result { Error = "除四个顶级标签外，其他标签必须关联父标签" };
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(ParentTagName) == false)
                {
                    return new Result { Error = "四个顶级标签不能关联父标签" };
                }
            }

            return new Result { Successful=true };
        }
    }
}
