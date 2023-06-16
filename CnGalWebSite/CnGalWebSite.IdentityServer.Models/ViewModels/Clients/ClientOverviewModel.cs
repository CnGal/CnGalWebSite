using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.IdentityServer.Models.ViewModels.Clients
{
    public class ClientOverviewModel
    {
        public int Id { get; set; }

        public bool Enabled { get; set; } = true;

        public string ClientId { get; set; }

        public string ClientName { get; set; }

        public string ClientUri { get; set; }

        public string LogoUri { get; set; }

        public string Description { get; set; }

        public bool RequirePkce { get; set; }

        public bool AllowAccessTokensViaBrowser { get; set; }
    }
}
