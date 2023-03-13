// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace IdentityServerHost.Quickstart.UI
{
    public class SecurityHeadersAttribute : ActionFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext context)
        {
            var result = context.Result;
            if (result is ViewResult)
            {
                // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/X-Content-Type-Options
                if (!context.HttpContext.Response.Headers.ContainsKey("X-Content-Type-Options"))
                {
                    context.HttpContext.Response.Headers.Add("X-Content-Type-Options", "nosniff");
                }

                // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/X-Frame-Options
                if (!context.HttpContext.Response.Headers.ContainsKey("X-Frame-Options"))
                {
                    context.HttpContext.Response.Headers.Add("X-Frame-Options", "SAMEORIGIN");
                }

                // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Content-Security-Policy
                var csp = @"base-uri'self';upgrade-insecure-requests;default-src'self';img-srcdata:https:blob:;object-src'none';script-srchttps://*.cngal.org/https://*.bytecdntp.com/https://*.cngal.top/https://*.geetest.com/https://dn-staticdown.qbox.me/https://cdn.masastack.com/https://*.clarity.ms/'sha256-djT2W42mSgDXuWiBkiSkizYu3s+jgEflEd0x/HANgTE=''self';script-src-elemhttps://code.getmdl.io/'self';style-src-elemhttps://fonts.googleapis.com/https://code.getmdl.io'self';style-srchttps://*.cngal.org/https://*.bytecdntp.com/https://dn-staticdown.qbox.me/https://static.geetest.com/https://cdn.jsdelivr.net/https://fonts.googleapis.com/https://use.fontawesome.com/https://cdn.masastack.com/'self''unsafe-inline';font-srchttps://*.cngal.org/https://*.bytecdntp.com/https://cdn.jsdelivr.net/https://fonts.googleapis.com/https://use.fontawesome.com/https://fonts.gstatic.com/'self';connect-srchttps://*.cngal.top/https://*.cngal.org/https://cdn.masastack.com/https://*.clarity.ms/'self';media-srchttps://*.cngal.org/'self';frame-srchttps://player.bilibili.com/";
                // also consider adding upgrade-insecure-requests once you have HTTPS in place for production
                //csp += "upgrade-insecure-requests;";
                // also an example if you need client images to be displayed from twitter
                // csp += "img-src 'self' https://pbs.twimg.com;";

                // once for standards compliant browsers
                if (!context.HttpContext.Response.Headers.ContainsKey("Content-Security-Policy"))
                {
                    context.HttpContext.Response.Headers.Add("Content-Security-Policy", csp);
                }
                // and once again for IE
                if (!context.HttpContext.Response.Headers.ContainsKey("X-Content-Security-Policy"))
                {
                    context.HttpContext.Response.Headers.Add("X-Content-Security-Policy", csp);
                }

                // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Referrer-Policy
                var referrer_policy = "no-referrer";
                if (!context.HttpContext.Response.Headers.ContainsKey("Referrer-Policy"))
                {
                    context.HttpContext.Response.Headers.Add("Referrer-Policy", referrer_policy);
                }
            }
        }
    }
}
