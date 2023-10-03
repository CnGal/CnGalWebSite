using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.ProjectSite.Shared.Models.Proejcts
{
    public enum ProjectPositionBudgetScreenType
    {
        [Display(Name ="无")]
        None,
        [Display(Name = "销售分成")]
        Divide,
        [Display(Name = "一次性报酬+销售分成")]
        IntervalAndDivide,
        [Display(Name = "一次性报酬")]
        Interval,
        [Display(Name = "用爱发电")]
        Afadian,
    }
}
