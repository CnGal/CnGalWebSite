using CnGalWebSite.ProjectSite.Shared.Models.Proejcts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.ProjectSite.Shared.Services.Projects
{
    public class ProjectService:IProjectService
    {
        public ProjectSortType SortType { get; set; }

        public string SearchString { get; set; }
    }
}
