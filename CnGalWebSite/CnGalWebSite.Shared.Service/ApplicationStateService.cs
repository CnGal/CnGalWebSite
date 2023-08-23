using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.Shared.Service
{
    public class ApplicationStateService:IApplicationStateService
    {
        public bool InPreRendered { get; set; } = true;
    }
}
