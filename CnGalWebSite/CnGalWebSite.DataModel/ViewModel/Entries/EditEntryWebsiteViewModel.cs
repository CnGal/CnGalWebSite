using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace CnGalWebSite.DataModel.ViewModel.Entries
{
    public class EditEntryWebsiteViewModel : BaseEntryEditModel
    {
        public List<EditWebsiteImageModel> Carousels { get; set; } = new List<EditWebsiteImageModel>();
        public List<EditWebsiteImageModel> BackgroundImages { get; set; } = new List<EditWebsiteImageModel>();

        [Display(Name ="介绍")]
        public string Introduction { get; set; }

        [Display(Name = "标题")]
        public string Title { get; set; }

        [Display(Name = "自定义Html")]
        public string Html { get; set; }

        public override Result Validate()
        {
            //检查是否重复
            foreach (var item in Carousels)
            {
                if (Carousels.Count(s => s.Image == item.Image) > 1)
                {
                    return new Result { Error = "图片链接不能重复，重复的链接：" + item.Image, Successful = false };
                }
            }
            foreach (var item in BackgroundImages)
            {
                if (BackgroundImages.Count(s => s.Image == item.Image) > 1)
                {
                    return new Result { Error = "图片链接不能重复，重复的链接：" + item.Image, Successful = false };
                }
            }


            return base.Validate();
        }

    }

    public class EditWebsiteImageModel : BaseImageEditModel
    {
        [Display(Name = "类型")]
        public EntryWebsiteImageType Type { get; set; }
    }
}
