using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.IdentityServer.Models.ViewModels.ApiScopes
{
    public class ApiScopeEditModel:ApiScopeOverviewModel
    {
        public List<string> UserClaims { get; set; } = new List<string>();
    }
}
