using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.Shared.Service
{
    public interface IEventService
    {
        event Action RefreshApp;
        event Action SavaTheme;

        void OnRefreshApp();

        void OnSavaTheme();

        Task OpenNewPage(string url);

    }
}
