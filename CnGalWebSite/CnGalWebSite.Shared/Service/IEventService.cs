using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.Shared.Service
{
    public interface IEventService
    {
        event Action SavaTheme;
        event Action LoadTheme;
        event Action TempEffectTheme;

        void OnSavaTheme();

        void OnLoadTheme();

        Task OpenNewPage(string url);

        void OnTempEffectTheme();

    }
}
