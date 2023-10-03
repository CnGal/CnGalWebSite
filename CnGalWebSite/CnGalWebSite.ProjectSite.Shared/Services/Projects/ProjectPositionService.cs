using CnGalWebSite.ProjectSite.Shared.Models.Proejcts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.ProjectSite.Shared.Services.Projects
{
    public class ProjectPositionService:IProjectPositionService
    {
        public ProjectPositionSortType SortType { get; set; }

        public ProjectPositionUrgencyScreenType UrgencyScreenType { get; set; }

        public ProjectPositionBudgetScreenType BudgetScreenType { get; set; }

        public ProjectPositionTypeScreenType TypeScreenType { get; set; }

        public string SearchString { get; set; }
    }
}
