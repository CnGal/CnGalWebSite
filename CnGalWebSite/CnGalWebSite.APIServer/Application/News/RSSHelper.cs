﻿using CnGalWebSite.APIServer.Application.Files;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Files;
using CnGalWebSite.DrawingBed.Helper.Services;
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
        private readonly IFileUploadService _fileUploadService;

        public RSSHelper(HttpClient httpClient, IConfiguration configuration, IFileService fileService, IFileUploadService fileUploadService)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _fileService = fileService;
            _fileUploadService = fileUploadService;
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
                weibo.PublishTime = DateTime.Parse(pubDate.InnerText).ToUniversalTime();

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
                item.Description = (await _fileUploadService.TransformImagesAsync(item.Description)).Text;
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
            model.Image = await _fileUploadService.TransformImageAsync(item.InnerText);

            return model;
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
                    weibo.PublishTime = DateTime.Parse(pubDate.InnerText).ToUniversalTime();

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
                weibo.PublishTime = DateTime.Parse(pubDate.InnerText).ToUniversalTime();

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

                    weibo.Description = (await _fileUploadService.TransformImagesAsync(weibo.Description)).Text;
                    

                    return weibo;
                }
            }

            return null;
        }

        public async Task<List<OriginalRSS>> GetOriginalBilibili(long id, DateTime fromTime)
        {
            var model = new List<OriginalRSS>();

            //获取最新B站数据
            var xmlStr = await _httpClient.GetStringAsync(_configuration["BilibiliRSSUrl"]);
            //反序列化数据
            var doc = new XmlDocument();
            doc.LoadXml(xmlStr);

            var items = doc.SelectSingleNode("rss").SelectSingleNode("channel").SelectNodes("item");
            foreach (XmlNode item in items)
            {
                var weibo = new OriginalRSS
                {
                    Type = OriginalRSSType.Bilibili
                };

                var pubDate = (XmlElement)item.SelectSingleNode("pubDate");
                weibo.PublishTime = DateTime.Parse(pubDate.InnerText).ToUniversalTime();

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
                item.Description = (await _fileUploadService.TransformImagesAsync(item.Description)).Text;
            }

            return model;
        }

    }
}
