// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4;
using IdentityServer4.Models;
using System.Collections.Generic;

namespace CnGalWebSite.IdentityServer
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
            new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
            };

        public static IEnumerable<ApiScope> ApiScopes =>
            new ApiScope[]
            {
                new ApiScope("api1", "My API")
            };

        public static IEnumerable<ApiResource> ApiResources = new List<ApiResource>
        {
            // 本地API
            new ApiResource(IdentityServerConstants.LocalApi.ScopeName),
        };

        public static IEnumerable<Client> Clients =>
            new Client[]
            {
                 // 机器到机器客户端（来自快速入门 1 开始）
                new Client
                {
                    ClientId = "client",
                    ClientSecrets = { new Secret("secret".Sha256()) },

                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    // 客户端有权访问的范围
                    AllowedScopes = { "api1" }
                },
                // 交互式 ASP.NET Core MVC 客户端
                new Client
                {
                    ClientId = "mvc",
                    ClientSecrets = { new Secret("secret".Sha256()) },

                    AllowedGrantTypes = GrantTypes.Code,

                    // 登录后重定向到哪里
                    RedirectUris = { "https://localhost:5002/signin-oidc" },

                    // 注销后重定向到哪里
                    PostLogoutRedirectUris = { "https://localhost:5002/signout-callback-oidc" },

                    AllowOfflineAccess = true,

                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.LocalApi.ScopeName,
                        "api1"
                    }
                }
            };
    }
}
