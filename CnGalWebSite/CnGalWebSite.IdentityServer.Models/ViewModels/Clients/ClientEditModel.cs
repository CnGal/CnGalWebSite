using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.IdentityServer.Models.ViewModels.Clients
{
    public class ClientEditModel:ClientOverviewModel
    {
        public List<string> AllowedGrantTypes { get; set; } = new List<string> { "authorization_code" };

        public List<string> AllowedCorsOrigins { get; set; } = new List<string>();

        public List<string> RedirectUris { get; set; } = new List<string>();

        public List<string> PostLogoutRedirectUris { get; set; } = new List<string>();

        public List<string> AllowedScopes { get; set; } = new List<string> { "profile", "openid" };

        public bool RequireConsent { get; set; } = true;
    }
}
