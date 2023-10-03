using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.ProjectSite.Shared.Models.Stalls
{
    public enum StallScreenType
    {
        [Display(Name = "所有")]
        None,
        [Display(Name = "配音")]
        CV,
        [Display(Name = "画师")]
        Painter,
        [Display(Name = "程序")]
        Programmer,
        [Display(Name = "剧本")]
        Writer,
        [Display(Name = "音乐")]
        Music,
        [Display(Name = "其他")]
        Other,
    }
}
