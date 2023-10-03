using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.ProjectSite.Shared.Models.Proejcts
{
    public enum ProjectPositionUrgencyScreenType
    {
        [Display(Name = "所有")]
        Null,
        [Display(Name = "未设置")]
        None,
        [Display(Name = "低")]
        Low,
        [Display(Name = "高")]
        High
    }
}
