using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.Maui.Services
{
    internal class OverviewService: IOverviewService
    {
        private MainPage _page { get; set; }

        public void Init(MainPage page)
        {
            _page = page;
        }

        public void HideLoadingOverview()
        {
            _page.HideOverviewGrid();
        }

    }
}
