using CnGalWebSite.APIServer.Application.Files;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Files;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace CnGalWebSite.APIServer.Application.News
{
    public class RSSHelper : IRSSHelper
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IFileService _fileService;

        public RSSHelper(HttpClient httpClient, IConfiguration configuration, IFileService fileService)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _fileService = fileService;
        }

        public async Task<List<OriginalRSS>> GetOriginalWeibo(long id, DateTime fromTime)
        {
            var model = new List<OriginalRSS>();
          
            //获取最新微博数据
            var xmlStr = await _httpClient.GetStringAsync(_configuration["RSSUrl"] + "weibo/user/" + id);
            //反序列化数据
            var doc = new XmlDocument();
            doc.LoadXml(xmlStr);

            var items = doc.SelectSingleNode("rss").SelectSingleNode("channel").SelectNodes("item");
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

                var author = (XmlElement)item.SelectSingleNode("author");
                weibo.Author = author.InnerText;

                var description = (XmlElement)item.SelectSingleNode("description");
                weibo.Description = description.InnerText;
                var link = (XmlElement)item.SelectSingleNode("link");
                weibo.Link = link.InnerText;

                model.Add(weibo);
            }

            //将图片上传到图床
            foreach (var item in model)
            {
                item.Description= await _fileService.TransformImagesAsync(item.Description, _configuration["NewsAdminId"]);
            }

            return model;
        }



        public async Task<WeiboUserInfor> GetWeiboUserInfor(long id)
        {
            var model = new WeiboUserInfor
            {
                WeiboId = id
            };

     

            //获取最新微博数据
            var xmlStr = await _httpClient.GetStringAsync(_configuration["RSSUrl"] + "weibo/user/" + id.ToString());
            //反序列化数据
            var doc = new XmlDocument();
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
            var dt = DateTime.MinValue;
            try
            {
                var pattern = "";
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
                    dt = dt.AddHours(8);
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

        public async Task<OriginalRSS> CorrectOriginalWeiboInfor(string Text, long id)
        {
            try
            {
                var model = new OriginalRSS();

                //获取最新微博数据
                var xmlStr = await _httpClient.GetStringAsync(_configuration["RSSUrl"] + "weibo/user/" + id);
                //反序列化数据
                var doc = new XmlDocument();
                doc.LoadXml(xmlStr);

                var items = doc.SelectSingleNode("rss").SelectSingleNode("channel").SelectNodes("item");
                foreach (XmlNode item in items)
                {
                    var weibo = new OriginalRSS
                    {
                        Type = OriginalRSSType.Weibo
                    };

                    var pubDate = (XmlElement)item.SelectSingleNode("pubDate");
                    weibo.PublishTime = GMT2Local(pubDate.InnerText);

                    var author = (XmlElement)item.SelectSingleNode("author");
                    weibo.Author = author.InnerText;

                    var title = (XmlElement)item.SelectSingleNode("title");
                    weibo.Title = title.InnerText;

                    var description = (XmlElement)item.SelectSingleNode("description");
                    weibo.Description = description.InnerText;
                    var link = (XmlElement)item.SelectSingleNode("link");
                    weibo.Link = link.InnerText;

                    if (weibo.Title.Contains(Text))
                    {
                        return weibo;
                    }
                }
            }
            catch
            {
                return null;
            }


            return null;
        }

        public async Task<OriginalRSS> GetOriginalWeibo(long id, string keyWord)
        {
            var model = new OriginalRSS();

            //获取最新微博数据
            var xmlStr = await _httpClient.GetStringAsync(_configuration["RSSUrl"] + "weibo/user/" + id);
            //反序列化数据
            var doc = new XmlDocument();
            doc.LoadXml(xmlStr);

            var items = doc.SelectSingleNode("rss").SelectSingleNode("channel").SelectNodes("item");
            foreach (XmlNode item in items)
            {
                var weibo = new OriginalRSS
                {
                    Type = OriginalRSSType.Weibo
                };

                var pubDate = (XmlElement)item.SelectSingleNode("pubDate");
                weibo.PublishTime = GMT2Local(pubDate.InnerText);

                var title = (XmlElement)item.SelectSingleNode("title");
                weibo.Title = title.InnerText;

                var author = (XmlElement)item.SelectSingleNode("author");
                weibo.Author = author.InnerText;

                var description = (XmlElement)item.SelectSingleNode("description");
                weibo.Description = description.InnerText;
                var link = (XmlElement)item.SelectSingleNode("link");
                weibo.Link = link.InnerText;

                if (weibo.Link.Contains(keyWord))
                {
                    //将图片上传到图床

                    weibo.Description= await _fileService.TransformImagesAsync(weibo.Description, _configuration["NewsAdminId"]);
                    

                    return weibo;
                }
            }

            return null;
        }
    }
}
