using CnGalWebSite.ProjectSite.Shared.Models.Proejcts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.ProjectSite.Shared.Services.Projects
{
    public interface IProjectPositionService
    {
        ProjectPositionSortType SortType { get; set; }

        ProjectPositionUrgencyScreenType UrgencyScreenType { get; set; }

        ProjectPositionBudgetScreenType BudgetScreenType { get; set; }

        ProjectPositionTypeScreenType TypeScreenType { get; set; }

        string SearchString { get; set; }
    }
}
