using CnGalWebSite.ProjectSite.Models.DataModels;
using CnGalWebSite.ProjectSite.Shared.Models.Proejcts;
using CnGalWebSite.ProjectSite.Shared.Models.Stalls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.ProjectSite.Shared.Services.Stalls
{
    public class StallService:IStallService
    {
        public StallSortType SortType { get; set; }

        public StallScreenType ScreenType { get; set; }

        public string SearchString { get; set; }
    }
}
