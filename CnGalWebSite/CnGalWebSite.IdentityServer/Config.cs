// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityModel;
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
                new IdentityResource("role", new string[]{JwtClaimTypes.Role })
            };

        public static IEnumerable<ApiScope> ApiScopes =>
            new ApiScope[]
            {
                new ApiScope("api1", "My API"),
                new ApiScope(IdentityServerConstants.LocalApi.ScopeName)
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
                },
                //Blazor Server
                new Client
                {
                    //唯一id，用来区分不同的Client
                    ClientId = "ids_admin_ssr",
                    //使用的授权方式
                    AllowedGrantTypes = GrantTypes.Code,
                    //安全码
                    ClientSecrets = { new Secret("secret".Sha256()) },
                    //Blazor运行时的URL
                    AllowedCorsOrigins =     { "http://localhost:5100" },
                    //登录成功之后将要跳转的Blazor的URL
                    RedirectUris = { "http://localhost:5100/signin-oidc" },
                    //登出之后将要跳转的Blazor的URL
                    PostLogoutRedirectUris = { "http://localhost:5100/signout-callback-oidc" },
                    //允许的Scope，openid包含用户id，profile包含用户基本资料，api为自定义的scope，也可以为其他名字
                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.LocalApi.ScopeName,
                        "role"
                    }
                },
                //Blazor WebAssembly
                new Client
                {
                    //唯一id，用来区分不同的Client
                    ClientId = "ids_admin_wasm",
                    //使用的授权方式
                    AllowedGrantTypes = GrantTypes.Code,
                    //这里设置为不需要安全码，当然也可以指定安全码
                    RequireClientSecret = false,
                    //Blazor运行时的URL
                    AllowedCorsOrigins =     { "http://localhost:5036" },
                    //登录成功之后将要跳转的Blazor的URL
                    RedirectUris = { "http://localhost:5036/authentication/login-callback" },
                    //登出之后将要跳转的Blazor的URL
                    PostLogoutRedirectUris = { "http://localhost:5036/" },
                    //允许的Scope，openid包含用户id，profile包含用户基本资料，api为自定义的scope，也可以为其他名字
                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.LocalApi.ScopeName,
                        "role"
                    }
                }
            };
    }
}
