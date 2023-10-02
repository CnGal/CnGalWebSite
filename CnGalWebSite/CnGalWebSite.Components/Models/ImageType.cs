using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.Components.Models
{
    public enum ImageType
    {
        [Display(Name = "无")]
        None,
        [Display(Name = "头像")]
        Avatar,
        [Display(Name = "横图")]
        Horizontal,
        [Display(Name = "竖图")]
        Vertical,
        [Display(Name = "背景")]
        Background,
        [Display(Name = "音乐")]
        Audio,
    }
}
