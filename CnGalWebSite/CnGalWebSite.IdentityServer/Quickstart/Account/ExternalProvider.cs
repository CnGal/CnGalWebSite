// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace IdentityServerHost.Quickstart.UI
{
    public class ExternalProvider
    {
        public string DisplayName { get; set; }
        public string Icon
        {
            get
            {
                return DisplayName switch
                {
                    "Google" => "mdi mdi-google",
                    "GitHub" => "mdi mdi-github",
                    "Gitee" => "mdi mdi-alpha-g-circle",
                    "QQ" => "mdi mdi-qqchat",
                    _ => "mdi mdi-link"
                };
            }
        }
        public string AuthenticationScheme { get; set; }
    }
}
