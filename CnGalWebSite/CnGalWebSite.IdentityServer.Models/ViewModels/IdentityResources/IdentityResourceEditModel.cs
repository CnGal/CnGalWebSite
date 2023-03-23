using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.IdentityServer.Models.ViewModels.IdentityResources
{
    public class IdentityResourceEditModel:IdentityResourceOverviewModel
    {
        public List<string> UserClaims { get; set; } = new List<string>();
    }
}
