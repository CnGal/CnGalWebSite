using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.Maui.Services
{
    public interface IOverviewService
    {
        void HideLoadingOverview();

        void Init(MainPage page);
    }
}
