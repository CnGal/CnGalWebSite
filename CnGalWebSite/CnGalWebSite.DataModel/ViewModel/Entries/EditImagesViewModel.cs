using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Base;
using CnGalWebSite.DataModel.ViewModel.Entries;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace CnGalWebSite.DataModel.ViewModel
{
    public class EditImagesViewModel : BaseEntryEditModel
    {
        public List<EditImageAloneModel> Images { get; set; } = new List<EditImageAloneModel>();

        public override Result Validate()
        {
            foreach (var item in Images)
            {
                if (item.Image.Contains("image.cngal.org") == false && item.Image.Contains("pic.cngal.top") == false)
                {
                    return new Result { Successful = false, Error = "相册中不能添加外部图片：" + item.Image };
                }
            }
            //检查是否重复
            foreach (var item in Images)
            {
                if (Images.Count(s => s.Image == item.Image) > 1)
                {
                    return new Result { Error = "图片链接不能重复，重复的链接：" + item.Image, Successful = false };

                }
            }

            return base.Validate();
        }
    }

    public class EditImageAloneModel: BaseImageEditModel
    {
        [Display(Name = "分类")]
        public string Modifier { get; set; }
    }

    public class ImagesUploadAloneModel
    {
        public string Image { get; set; }
    }
}
