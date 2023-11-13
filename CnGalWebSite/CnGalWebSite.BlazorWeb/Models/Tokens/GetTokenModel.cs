using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.BlazorWeb.Models.Tokens
{
    public class GetTokenModel
    {
        public string UserId { get; set; }
        public AppUserAccessTokenType Type { get; set; }

        public string Secret { get; set; }

    }
}
