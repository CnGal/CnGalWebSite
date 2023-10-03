using CnGalWebSite.ProjectSite.Shared.Models.Proejcts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.ProjectSite.Shared.Services.Projects
{
    public interface IProjectService
    {
        ProjectSortType SortType { get; set; }

        string SearchString { get; set; }
    }
}
