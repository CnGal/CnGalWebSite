using CnGalWebSite.ProjectSite.Models.DataModels;
using CnGalWebSite.ProjectSite.Shared.Models.Stalls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.ProjectSite.Shared.Services.Stalls
{
    public interface IStallService
    {
        StallSortType SortType { get; set; }

        StallScreenType ScreenType { get; set; }

        string SearchString { get; set; }

    }
}
