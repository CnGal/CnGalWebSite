using IdentityServerHost.Quickstart.UI;
using System.Collections.Generic;
using System.Linq;

namespace CnGalWebSite.IdentityServer.Models.Account
{
    public class SelectRealNameMethodViewModel
    {
        public string ReturnUrl { get; set; }

        public string UserId { get; set; }

        public string Code { get; set; }

        public bool  ShowCpltToast { get; set; }

        public IEnumerable<ExternalProvider> ExternalProviders { get; set; } = Enumerable.Empty<ExternalProvider>();
    }
}
