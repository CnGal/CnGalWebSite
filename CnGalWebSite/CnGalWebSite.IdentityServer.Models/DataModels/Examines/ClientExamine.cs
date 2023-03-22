using CnGalWebSite.IdentityServer.Models.DataModels.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.IdentityServer.Models.DataModels.Examines
{
    public class ClientExamine
    {
        public long Id { get; set; }

        public string ClientName { get; set; }

        public string ClientUri { get; set; }

        public string LogoUri { get; set; }

        public string Description { get; set; }
    }
}
