using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.GameSite.Shared.Services.Events
{
    public interface IEventService
    {
        event Action<string> TitleChanged;

        void OnTitleChanged(string title);
    }
}
