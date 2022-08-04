using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Xml.Linq;

namespace CnGalWebSite.DataModel.ViewModel.Base
{
    public class BaseImageEditModel
    {
        [Display(Name = "备注")]
        public string Note { get; set; }
        [Display(Name = "图片")]
        [Required(ErrorMessage = "请填写图片链接")]
        public string Image { get; set; }
        [Display(Name = "优先级")]
        public int Priority { get; set; }

    }
}
