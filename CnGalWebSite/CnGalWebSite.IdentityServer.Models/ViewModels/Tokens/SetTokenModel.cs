using CnGalWebSite.IdentityServer.Admin.SSR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.IdentityServer.Models.ViewModels.Tokens
{
    public class SetTokenModel
    {
        public AppUserAccessTokenType Type { get; set; }
        public string UserId { get; set; }
        public string AccessToken { get; set; }
        public DateTimeOffset? Expiration { get; set; }
        public string RefreshToken { get; set; }

        public string Secret { get; set; }

    }
}
