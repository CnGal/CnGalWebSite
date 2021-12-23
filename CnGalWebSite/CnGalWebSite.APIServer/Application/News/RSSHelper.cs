using Microsoft.Extensions.Configuration;
using System.Net.Http;
using CnGalWebSite.DataModel.Models;
using CnGalWebSite.DataModel.Model;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Xml;
using TencentCloud.Ame.V20190916.Models;
using System;
using Nest;
using CnGalWebSite.APIServer.Application.Files;
using CnGalWebSite.DataModel.Helper;

namespace CnGalWebSite.APIServer.Application.News
{
    public class RSSHelper:IRSSHelper
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IConfiguration _configuration;
        private readonly IFileService _fileService;

        public RSSHelper(IHttpClientFactory clientFactory, IConfiguration configuration, IFileService fileService)
        {
            _clientFactory = clientFactory;
            _configuration = configuration;
            _fileService = fileService;
        }

        public async Task<List<OriginalRSS>> GetOriginalWeibo(long id,DateTime fromTime)
        {
            List<OriginalRSS> model = new List<OriginalRSS>();
            using var client = _clientFactory.CreateClient();

            //获取最新微博数据
            var xmlStr = await client.GetStringAsync(_configuration["RSSUrl"] + "weibo/user/" +id);
            //反序列化数据
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlStr);

            var items= doc.SelectSingleNode("rss").SelectSingleNode("channel").SelectNodes("item");
            foreach (XmlNode item in items)
            {
                var weibo = new OriginalRSS
                {
                    Type = OriginalRSSType.Weibo
                };

                var pubDate = (XmlElement)item.SelectSingleNode("pubDate");
                weibo.PublishTime = GMT2Local(pubDate.InnerText);

                //微博时间是否早于起始时间
                if (weibo.PublishTime <= fromTime)
                {
                    return model;
                }
                var title = (XmlElement)item.SelectSingleNode("title");
                weibo.Title = title.InnerText;

                var description = (XmlElement)item.SelectSingleNode("description");
                weibo.Description = description.InnerText;
                var link = (XmlElement)item.SelectSingleNode("link");
                weibo.Link = link.InnerText;

                model.Add(weibo);
            }

            //将图片上传到图床
            foreach (var item in model)
            {
                var images = ToolHelper.GetImageLinks(item.Description);
                foreach (var temp in images)
                {
                    var infor = await _fileService.SaveImageAsync(temp, _configuration["NewsAdminId"]);

                    //替换图片
                    item.Description= item.Description.Replace(temp, infor);
                }
            }

            return model;
        }

        public async Task<WeiboUserInfor> GetWeiboUserInfor(long id)
        {
            var model = new WeiboUserInfor
            {
                WeiboId = id
            };

            using var client = _clientFactory.CreateClient();

            //获取最新微博数据
            var xmlStr = await client.GetStringAsync(_configuration["RSSUrl"] + "weibo/user/" + id.ToString());
            //反序列化数据
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlStr);

            var item = (XmlElement)doc.SelectSingleNode("rss").SelectSingleNode("channel").SelectSingleNode("title");

            model.WeiboName = item.InnerText.Replace("的微博", "");

            //获取头像
            item = (XmlElement)doc.SelectSingleNode("rss").SelectSingleNode("channel").SelectSingleNode("image").SelectSingleNode("url");
            model.Image = await _fileService.SaveImageAsync(item.InnerText, _configuration["NewsAdminId"]);

            return model;
        }

        /// <summary>
        /// GMT时间转成本地时间
        /// </summary>
        /// <param name="gmt">字符串形式的GMT时间</param>
        /// <returns></returns>
        public static DateTime GMT2Local(string gmt)
        {
            DateTime dt = DateTime.MinValue;
            try
            {
                string pattern = "";
                if (gmt.IndexOf("+0") != -1)
                {
                    gmt = gmt.Replace("GMT", "");
                    pattern = "ddd, dd MMM yyyy HH':'mm':'ss zzz";
                }
                if (gmt.ToUpper().IndexOf("GMT") != -1)
                {
                    pattern = "ddd, dd MMM yyyy HH':'mm':'ss 'GMT'";
                }
                if (pattern != "")
                {
                    dt = DateTime.ParseExact(gmt, pattern, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AdjustToUniversal);
                    dt = dt.ToLocalTime();
                }
                else
                {
                    dt = Convert.ToDateTime(gmt);
                }
            }
            catch
            {
            }
            return dt;
        }
    }
}
