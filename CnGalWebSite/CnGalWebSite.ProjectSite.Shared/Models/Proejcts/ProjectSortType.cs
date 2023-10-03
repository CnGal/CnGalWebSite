using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.ProjectSite.Shared.Models.Proejcts
{
    public enum ProjectSortType
    {
        [Display(Name ="默认")]
        Default,
        [Display(Name = "创建时间")]
        CreateTime,
        [Display(Name = "更新时间")]
        UpdateTime,
        [Display(Name = "预算下限")]
        BudgetMin,
        [Display(Name = "预算上限")]
        BudgetMax,
        [Display(Name = "分成比例")]
        Percentage
    }
}
