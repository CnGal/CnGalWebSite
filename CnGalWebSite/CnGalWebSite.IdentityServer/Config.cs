// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityModel;
using IdentityServer4;
using IdentityServer4.Models;
using System.Collections.Generic;
using System.Security.Claims;

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
              new ApiScope(IdentityServerConstants.LocalApi.ScopeName,new List<string>(){JwtClaimTypes.Role,ClaimTypes.Role,ClaimTypes.Name,JwtClaimTypes.Name,ClaimTypes.Email,JwtClaimTypes.Email}),
              new ApiScope("CnGalAPI", "CnGal V3 API",new List<string>(){JwtClaimTypes.Role,ClaimTypes.Role,ClaimTypes.Name,JwtClaimTypes.Name,ClaimTypes.Email,JwtClaimTypes.Email}),
          };

        public static IEnumerable<ApiResource> ApiResources = new List<ApiResource>
        {
            new ApiResource(IdentityServerConstants.LocalApi.ScopeName),
            new ApiResource("CnGalAPI", "CnGal V3 API"),
        };

        public static IEnumerable<Client> Clients =>
            new Client[]
            {
                //IdentityServer.Addmin.SSR
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
                    },
                    AlwaysSendClientClaims = true,
                    AlwaysIncludeUserClaimsInIdToken = true,
                },
                //IdentityServer.Addmin.WASM
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
                    },
                    AlwaysSendClientClaims = true,
                    AlwaysIncludeUserClaimsInIdToken = true,
                },
                //Server
                new Client
                {
                    //唯一id，用来区分不同的Client
                    ClientId = "ssr",
                    //使用的授权方式
                    AllowedGrantTypes = GrantTypes.Code,
                    //安全码
                    ClientSecrets = { new Secret("secret".Sha256()) },
                    //Blazor运行时的URL
                    AllowedCorsOrigins =     { "http://localhost:45159" },
                    //登录成功之后将要跳转的Blazor的URL
                    RedirectUris = { "http://localhost:45159/signin-oidc" },
                    //登出之后将要跳转的Blazor的URL
                    PostLogoutRedirectUris = { "http://localhost:45159/signout-callback-oidc" },
                    //允许的Scope，openid包含用户id，profile包含用户基本资料，api为自定义的scope，也可以为其他名字
                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "CnGalAPI",
                        "role"
                    },
                    AlwaysSendClientClaims = true,
                    AlwaysIncludeUserClaimsInIdToken = true,
                },
                //WebAsseembly
                new Client
                {
                    //唯一id，用来区分不同的Client
                    ClientId = "wasm",
                    //使用的授权方式
                    AllowedGrantTypes = GrantTypes.Code,
                    //这里设置为不需要安全码，当然也可以指定安全码
                    RequireClientSecret = false,
                    //Blazor运行时的URL
                    AllowedCorsOrigins =     { "http://localhost:5000" },
                    //登录成功之后将要跳转的Blazor的URL
                    RedirectUris = { "http://localhost:5000/authentication/login-callback" },
                    //登出之后将要跳转的Blazor的URL
                    PostLogoutRedirectUris = { "http://localhost:5000/" },
                    //允许的Scope，openid包含用户id，profile包含用户基本资料，api为自定义的scope，也可以为其他名字
                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "CnGalAPI",
                        "role"
                    },
                    AlwaysSendClientClaims = true,
                    AlwaysIncludeUserClaimsInIdToken = true,
                },
            };
    }
}
