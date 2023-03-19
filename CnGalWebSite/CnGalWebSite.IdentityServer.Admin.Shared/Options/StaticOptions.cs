using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CnGalWebSite.IdentityServer.Admin.Shared.Options
{
    public static class StaticOptions
    {
        public static string IdsApiUrl { get; set; } = "https://oauth.cngal.org/";
        public static string CnGalApiUrl { get; set; } = "https://api.cngal.org/";

        public static bool? PreSetIsSSR = null;
        public static bool IsSSR = false;
    }
}
