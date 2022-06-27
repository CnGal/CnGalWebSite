using CnGalWebSite.Helper.Extensions;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.Shared.Service
{
    public class StructuredDataService:IStructuredDataService
    {
        public EventCallback Refresh { get; set; }

        public string Data { get; set; }

        public async Task SetStructuredData(string data)
        {
            Data = data;
            await Refresh.InvokeAsync();
        }

        public async Task SetStructuredData(List<string> urls)
        {
            urls.RemoveAll(s => string.IsNullOrWhiteSpace(s));
            urls.Purge();
            Data = GenerateCarouselStructuredDataJson(urls);
            await Refresh.InvokeAsync();
        }


        /// <summary>
        /// 生成轮播数据结构
        /// </summary>
        /// <param name="urls"></param>
        /// <returns></returns>
        public static string GenerateCarouselStructuredDataJson(List<string> urls)
        {
            var result = new StringBuilder("<script type=\"application/ld+json\">{\"@context\":\"https://schema.org\",\"@type\":\"ItemList\",\"itemListElement\":[");
            foreach (var item in urls)
            {
                if (urls.IndexOf(item) != 0)
                {
                    result.Append(',');
                }

                result.Append($"{{\"@type\":\"ListItem\",\"position\":{urls.IndexOf(item) + 1},\"url\":\"{item}\"}}");


            }

            result.Append("]}</script>");

            return result.ToString();
        }

    }
}
