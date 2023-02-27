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
        public List<EditWebsiteImageModel> Images { get; set; } = new List<EditWebsiteImageModel>();

        [Display(Name ="介绍")]
        public string Introduction { get; set; }

        [Display(Name = "自定义Html")]
        public string Html { get; set; }

        [Display(Name = "自定义首页")]
        public string FirstPage { get; set; }

        [Display(Name = "Logo图片")]
        public string Logo { get; set; }

        [Display(Name = "副标题")]
        public string SubTitle { get; set; }

        [Display(Name = "主题语句")]
        public string Impressions { get; set; }

        [Display(Name = "主题颜色")]
        public string Color { get; set; }

        public override Result Validate()
        {
            //检查是否重复
            foreach (var item in Images)
            {
                if (Images.Count(s => s.Image == item.Image&&s.Type==item.Type) > 1)
                {
                    return new Result { Error = "相同类型的图片链接不能重复，重复的链接：" + item.Image, Successful = false };
                }
            }

            return base.Validate();
        }

    }

    public class EditWebsiteImageModel : BaseImageEditModel
    {
        [Display(Name = "类型")]
        public EntryWebsiteImageType Type { get; set; }

        [Display(Name = "大小")]
        public EntryWebsiteImageSize Size { get; set; }
    }
}
