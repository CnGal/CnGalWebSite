using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.IdentityServer.Models.ViewModels.Clients
{
    public class ClientSecretViewModel
    {
        public int Id { get; set; }
        public string ClientSecret { get; set; }
        public string ClientId { get; set; }
    }
}
