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
        public static string IdsUrl { get; set; } = "https://oauth2.cngal.org/";

        public static bool? PreSetIsSSR = null;
        public static bool IsSSR = false;
    }
}
