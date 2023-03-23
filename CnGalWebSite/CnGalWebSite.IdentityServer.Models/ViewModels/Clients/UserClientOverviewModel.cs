using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.IdentityServer.Models.ViewModels.Clients
{
    public class UserClientOverviewModel:ClientOverviewModel
    {
        public bool? IsPassed { get; set; }
    }
}
