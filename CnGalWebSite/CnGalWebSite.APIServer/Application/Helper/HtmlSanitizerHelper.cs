using Ganss.Xss;
using System;
using System.Linq;

namespace CnGalWebSite.APIServer.Application.Helper
{
    /// <summary>
    /// HTML 净化器：所有 Markdown 渲染结果在返回前端前统一经过白名单净化，防止存储型 XSS。
    /// 允许常见排版标签与属性；iframe 校验受信任平台的初始地址，不限制后续重定向；危险协议一律剥离。
    /// </summary>
    public static class HtmlSanitizerHelper
    {
        private static readonly HtmlSanitizer _sanitizer = CreateSanitizer();

        public static string Sanitize(string html)
        {
            return string.IsNullOrWhiteSpace(html) ? html : _sanitizer.Sanitize(html);
        }

        private static HtmlSanitizer CreateSanitizer()
        {
            var sanitizer = new HtmlSanitizer();

            sanitizer.AllowedTags.Clear();
            foreach (var tag in new[]
            {
                "a","abbr","audio","b","bdi","bdo","blockquote","br","caption","center","cite","code","col","colgroup",
                "dd","del","details","dfn","div","dl","dt","em","figcaption","figure","font","h1","h2","h3","h4","h5","h6",
                "hr","i","iframe","img","input","ins","kbd","li","mark","ol","p","picture","pre","q","rp","rt","ruby","s","samp",
                "small","source","span","strike","strong","sub","summary","sup","table","tbody","td","tfoot","th","thead",
                "tr","u","ul","var","video",
                "article","aside","footer","header","main","nav","section"
            })
            {
                sanitizer.AllowedTags.Add(tag);
            }

            sanitizer.AllowedAttributes.Clear();
            // srcset remains excluded: control-character candidates can be rewritten into extra URLs.
            foreach (var attr in new[]
            {
                "abbr","align","allow","allowfullscreen","alt","autoplay","border","cellpadding","cellspacing","charset",
                "cite","class","colspan","controls","datetime","dir","download","frameborder","framespacing","headers",
                "height","href","hreflang","hspace","id","lang","loading","loop","media","muted","name","playsinline",
                "poster","preload","rel","rowspan","scope","scrolling","size","span","src","start","target","title","type",
                "valign","vspace","width","style","checked","disabled","crossorigin"
            })
            {
                sanitizer.AllowedAttributes.Add(attr);
            }

            sanitizer.AllowedSchemes.Clear();
            foreach (var scheme in new[] { "http", "https", "mailto" })
            {
                sanitizer.AllowedSchemes.Add(scheme);
            }

            sanitizer.AllowDataAttributes = false;

            // Media must not inherit mailto support from ordinary hyperlinks.
            var mediaBaseUri = new Uri("https://sanitizer.invalid/");
            sanitizer.FilterUrl += (_, e) =>
            {
                if (e.Tag.LocalName is "img" or "source" or "audio" or "video"
                    && e.SanitizedUrl != null
                    && (!Uri.TryCreate(mediaBaseUri, e.SanitizedUrl, out var uri)
                        || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)))
                {
                    e.SanitizedUrl = null;
                }
            };

            sanitizer.AllowedCssProperties.Clear();
            sanitizer.AllowedCssProperties.UnionWith(new[]
            {
                "color", "background-color", "text-align", "font-weight", "font-style", "text-decoration",
                "text-decoration-line", "text-decoration-style", "text-decoration-color"
            });

            // Preserve task-list state without allowing interactive form controls.
            sanitizer.PostProcessNode += (_, e) =>
            {
                if (e.Node is AngleSharp.Html.Dom.IHtmlElement media)
                {
                    if (media.HasAttribute("crossorigin"))
                    {
                        var value = media.GetAttribute("crossorigin");
                        if (media.LocalName is "img" or "audio" or "video"
                            && (value == "" || string.Equals(value, "anonymous", StringComparison.OrdinalIgnoreCase)))
                        {
                            media.SetAttribute("crossorigin", "anonymous");
                        }
                        else
                        {
                            media.RemoveAttribute("crossorigin");
                        }
                    }
                }

                if (e.Node is AngleSharp.Html.Dom.IHtmlElement input
                    && string.Equals(input.TagName, "INPUT", StringComparison.OrdinalIgnoreCase))
                {
                    if (!string.Equals(input.GetAttribute("type"), "checkbox", StringComparison.OrdinalIgnoreCase))
                    {
                        input.Remove();
                        return;
                    }
                    foreach (var attribute in input.Attributes.ToArray())
                    {
                        if (attribute.Name != "type" && attribute.Name != "checked")
                        {
                            input.RemoveAttribute(attribute.Name);
                        }
                    }
                    input.SetAttribute("disabled", "");
                }

                if (e.Node is AngleSharp.Html.Dom.IHtmlElement element
                    && string.Equals(element.TagName, "IFRAME", StringComparison.OrdinalIgnoreCase))
                {
                    var ok = false;
                    var src = element.GetAttribute("src");
                    if (src?.StartsWith("//", StringComparison.Ordinal) == true)
                    {
                        src = "https:" + src;
                    }
                    if (Uri.TryCreate(src, UriKind.Absolute, out var uri)
                        && uri.Scheme == Uri.UriSchemeHttps && uri.IsDefaultPort
                        && string.IsNullOrEmpty(uri.UserInfo))
                    {
                        ok = new[] { "bilibili.com", "b23.tv", "weibo.com", "weibo.cn", "youtube.com" }
                            .Any(host => uri.Host.Equals(host, StringComparison.OrdinalIgnoreCase)
                                || uri.Host.EndsWith("." + host, StringComparison.OrdinalIgnoreCase));
                    }
                    if (!ok)
                    {
                        element.Remove();
                        return;
                    }
                    element.SetAttribute("src", src);
                    element.SetAttribute("loading", "lazy");
                    element.SetAttribute("allow", "fullscreen");
                    element.SetAttribute("referrerpolicy", "strict-origin-when-cross-origin");
                }

                if (e.Node is AngleSharp.Html.Dom.IHtmlElement link
                    && string.Equals(link.TagName, "A", StringComparison.OrdinalIgnoreCase)
                    && string.Equals(link.GetAttribute("target"), "_blank", StringComparison.OrdinalIgnoreCase))
                {
                    link.SetAttribute("rel", "noopener noreferrer");
                }
            };

            return sanitizer;
        }
    }
}
