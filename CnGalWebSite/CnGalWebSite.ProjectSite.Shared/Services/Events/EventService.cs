using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.ProjectSite.Shared.Services.Events
{
    public class EventService:IEventService
    {
        public event Action<string> TitleChanged;

        public void OnTitleChanged(string title)
        {
            TitleChanged?.Invoke(title);
        }

    }
}
