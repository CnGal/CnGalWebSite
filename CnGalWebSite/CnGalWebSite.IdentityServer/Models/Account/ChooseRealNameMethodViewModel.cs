using IdentityServerHost.Quickstart.UI;
using System.Collections.Generic;
using System.Linq;

namespace CnGalWebSite.IdentityServer.Models.Account
{
    public class ChooseRealNameMethodViewModel
    {
        public string ReturnUrl { get; set; }

        public string Email { get; set; }

        public IEnumerable<ExternalProvider> ExternalProviders { get; set; } = Enumerable.Empty<ExternalProvider>();
    }
}
