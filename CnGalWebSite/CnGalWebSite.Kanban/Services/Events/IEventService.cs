using CnGalWebSite.Kanban.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.Kanban.Services.Events
{
    public interface IEventService
    {
        Task RunAsync(CancellationToken token);

        Task TriggerCustomEventAsync(string name);
    }
}
