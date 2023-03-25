﻿using BootstrapBlazor.Components;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.APIServer.Models;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.BackUpArchives;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using Tag = CnGalWebSite.DataModel.Model.Tag;

namespace CnGalWebSite.APIServer.Application.BackUpArchives
{
    public class BackUpArchiveService : IBackUpArchiveService
    {
        private readonly IRepository<BackUpArchive, long> _backUpArchiveRepository;
        private readonly IRepository<BackUpArchiveDetail, long> _backUpArchiveDetailRepository;
        private readonly IRepository<Entry, long> _entryRepository;
        private readonly IRepository<Article, long> _articleRepository;
        private readonly IRepository<Video, long> _videoRepository;
        private readonly IRepository<Periphery, long> _peripheryRepository;
        private readonly IRepository<Tag, int> _tagRepository;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IWebHostEnvironment _webHostEnvironment;


        private static readonly ConcurrentDictionary<Type, Func<IEnumerable<BackUpArchive>, string, SortOrder, IEnumerable<BackUpArchive>>> SortLambdaCacheApplicationUser = new();

        public BackUpArchiveService(IRepository<BackUpArchive, long> backUpArchiveRepository, IRepository<Entry, long> entryRepository, IRepository<Article, long> articleRepository, IConfiguration configuration, IRepository<Periphery, long> peripheryRepository,
        IHttpClientFactory clientFactory, IRepository<BackUpArchiveDetail, long> backUpArchiveDetailRepository, IWebHostEnvironment webHostEnvironment, IRepository<Tag, int> tagRepository, IRepository<Video, long> videoRepository)
        {
            _backUpArchiveRepository = backUpArchiveRepository;
            _entryRepository = entryRepository;
            _articleRepository = articleRepository;
            _configuration = configuration;
            _clientFactory = clientFactory;
            _backUpArchiveDetailRepository = backUpArchiveDetailRepository;
            _webHostEnvironment = webHostEnvironment;
            _tagRepository = tagRepository;
            _videoRepository = videoRepository;
            _peripheryRepository = peripheryRepository;
        }

        public Task<QueryData<ListBackUpArchiveAloneModel>> GetPaginatedResult(CnGalWebSite.DataModel.ViewModel.Search.QueryPageOptions options, ListBackUpArchiveAloneModel searchModel)
        {
            var tempDateTimeNow = DateTime.Now.ToCstTime();

            IEnumerable<BackUpArchive> items = _backUpArchiveRepository.GetAll().AsNoTracking().Where(s => s.LastBackUpTime > tempDateTimeNow.AddYears(-10));
            // 处理高级搜索
            if (searchModel.EntryId != null)
            {
                items = items.Where(item => item.EntryId == searchModel.EntryId);
            }
            if (searchModel.ArticleId != null)
            {
                items = items.Where(item => item.ArticleId == searchModel.ArticleId);
            }




            // 处理 SearchText 模糊搜索
            if (!string.IsNullOrWhiteSpace(options.SearchText))
            {
                // items = items.Where(item => (item.Name?.Contains(options.SearchText) ?? false));
            }

            // 排序
            var isSorted = false;
            if (!string.IsNullOrWhiteSpace(options.SortName))
            {
                // 外部未进行排序，内部自动进行排序处理
                var invoker = SortLambdaCacheApplicationUser.GetOrAdd(typeof(BackUpArchive), key => LambdaExtensions.GetSortLambda<BackUpArchive>().Compile());
                items = invoker(items, options.SortName, (BootstrapBlazor.Components.SortOrder)options.SortOrder);
                isSorted = true;
            }

            // 设置记录总数
            var total = items.Count();

            // 内存分页
            items = items.Skip((options.PageIndex - 1) * options.PageItems).Take(options.PageItems).ToList();

            //复制数据
            var resultItems = new List<ListBackUpArchiveAloneModel>();
            foreach (var item in items)
            {
                resultItems.Add(new ListBackUpArchiveAloneModel
                {
                    Id = item.Id,
                    LastBackUpTime = item.LastBackUpTime,
                    EntryId = item.EntryId,
                    ArticleId = item.ArticleId,
                    IsLastFail = item.IsLastFail,
                    LastTimeUsed = item.LastTimeUsed
                });
            }

            return Task.FromResult(new QueryData<ListBackUpArchiveAloneModel>()
            {
                Items = resultItems,
                TotalCount = total,
                IsSorted = isSorted,
                // IsFiltered = isFiltered
            });
        }




        public async Task BackUpArticle(BackUpArchive backUpArchive)
        {
            var BeginTime = DateTime.Now.ToCstTime();
            var client = _clientFactory.CreateClient();
            var url = "https://www.cngal.org/articles/index/" + backUpArchive.ArticleId;

            var response = await client.GetAsync(_configuration["BackUpArchiveUrl"] + url);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                //如果成功则写入数据 不成功也要写
                await UpdateBackUpInfor(backUpArchive, false, (DateTime.Now.ToCstTime() - BeginTime).TotalSeconds);
            }
            else
            {
                //如果成功则写入数据 不成功也要写
                await UpdateBackUpInfor(backUpArchive, true, (DateTime.Now.ToCstTime() - BeginTime).TotalSeconds);
            }
        }

        public async Task BackUpEntry(BackUpArchive backUpArchive, string entryName)
        {
            var BeginTime = DateTime.Now.ToCstTime();
            var client = _clientFactory.CreateClient();
            var url1 = "https://www.cngal.org/entries/index/" + backUpArchive.EntryId;
            var response1 = await client.GetAsync(_configuration["BackUpArchiveUrl"] + url1);
            var url2 = "https://www.cngal.org/entries/index/" + ToolHelper.Base64EncodeName(entryName);
            var response2 = await client.GetAsync(_configuration["BackUpArchiveUrl"] + url2);

            if (response1.StatusCode == System.Net.HttpStatusCode.OK || response2.StatusCode == System.Net.HttpStatusCode.OK)
            {
                //如果成功则写入数据 不成功也要写
                await UpdateBackUpInfor(backUpArchive, false, (DateTime.Now.ToCstTime() - BeginTime).TotalSeconds / 2);
            }
            else
            {
                //如果成功则写入数据 不成功也要写
                await UpdateBackUpInfor(backUpArchive, true, (DateTime.Now.ToCstTime() - BeginTime).TotalSeconds / 2);
            }
        }

        public async Task BackUpAllArticles(int maxNum)
        {
            //获取数据
            var articles = new List<BackUpArchive>();
            var tempIndex = await _articleRepository.GetAll().Include(s => s.BackUpArchive).Where(s => s.BackUpArchive == null && string.IsNullOrWhiteSpace(s.Name) == false)
                .OrderBy(s => s.Id)
                .Select(s => s.Id)
                .Take(maxNum).ToListAsync();
            //创建备份记录
            foreach (var item in tempIndex)
            {
                articles.Add(await _backUpArchiveRepository.InsertAsync(new BackUpArchive
                {
                    ArticleId = item
                }));
            }

            //如果没有数据 则查找已经备份过的
            if (articles.Count == 0)
            {
                articles = await _articleRepository.GetAll()
                   .Include(s => s.BackUpArchive).Where(s => s.BackUpArchive != null && (s.BackUpArchive.LastBackUpTime < s.LastEditTime || s.BackUpArchive.IsLastFail == true) && string.IsNullOrWhiteSpace(s.Name) == false)
                    .OrderBy(s => s.Id)
                   .Select(s => s.BackUpArchive)
                   .Take(maxNum).ToListAsync();
            }

            foreach (var item in articles)
            {
                await BackUpArticle(item);
            }

        }

        public async Task BackUpAllEntries(int maxNum)
        {
            //获取数据
            var entries = new List<BackUpArchive>();
            var tempIndex = await _entryRepository.GetAll().Include(s => s.BackUpArchive).Where(s => s.BackUpArchive == null && string.IsNullOrWhiteSpace(s.Name) == false)
                 .OrderBy(s => s.Id)
                .Select(s => s.Id)
                .Take(maxNum).ToListAsync();
            //创建备份记录
            foreach (var item in tempIndex)
            {
                entries.Add(await _backUpArchiveRepository.InsertAsync(new BackUpArchive
                {
                    EntryId = item
                }));
            }

            //如果没有数据 则查找已经备份过的
            if (entries.Count == 0)
            {
                entries = await _entryRepository.GetAll()
                   .Include(s => s.BackUpArchive).Where(s => s.BackUpArchive != null && (s.BackUpArchive.LastBackUpTime < s.LastEditTime || s.BackUpArchive.IsLastFail == true) && string.IsNullOrWhiteSpace(s.Name) == false)
                    .OrderBy(s => s.Id)
                   .Select(s => s.BackUpArchive)
                   .Take(maxNum).ToListAsync();
            }

            foreach (var item in entries)
            {
                await BackUpEntry(item, await _entryRepository.GetAll().Where(s => s.Id == item.EntryId).Select(s => s.Name).FirstOrDefaultAsync());
            }
        }

        private async Task UpdateBackUpInfor(BackUpArchive backUpArchive, bool isFail, double timeUsed)
        {
            //如果成功则写入数据 不成功也要写
            backUpArchive.LastBackUpTime = DateTime.Now.ToCstTime();
            backUpArchive.IsLastFail = isFail;
            backUpArchive.LastTimeUsed = timeUsed;

            backUpArchive = await _backUpArchiveRepository.UpdateAsync(backUpArchive);

            await _backUpArchiveDetailRepository.InsertAsync(new BackUpArchiveDetail
            {
                IsFail = backUpArchive.IsLastFail,
                BackUpTime = backUpArchive.LastBackUpTime,
                TimeUsed = backUpArchive.LastTimeUsed,
                BackUpArchive = backUpArchive,
                BackUpArchiveId = backUpArchive.Id
            });
        }

        public async Task UpdateSitemap()
        {
            var path = Path.Combine(_webHostEnvironment.WebRootPath, "sitemap.xml");
            try
            {
                File.Delete(path);
            }
            catch { }

            //获取词条名称和id 文章id
            var entries = await _entryRepository.GetAll().Where(s => s.IsHidden != true && string.IsNullOrWhiteSpace(s.Name) == false)
                .Select(s => new
                {
                    s.Type,
                    s.LastEditTime,
                    s.Id
                }).ToListAsync();

            var articles = await _articleRepository.GetAll().Where(s => s.IsHidden != true && string.IsNullOrWhiteSpace(s.Name) == false)
                .Select(s => new
                {
                    s.LastEditTime,
                    s.Id
                }).ToListAsync();
            var tags = await _tagRepository.GetAll().Where(s => s.IsHidden != true && string.IsNullOrWhiteSpace(s.Name) == false)
                .Select(s => new
                {
                    s.LastEditTime,
                    s.Id
                }).ToListAsync();
            var peripheries = await _peripheryRepository.GetAll().Where(s => s.IsHidden != true && string.IsNullOrWhiteSpace(s.Name) == false)
                .Select(s => new
                {
                    s.LastEditTime,
                    s.Id
                }).ToListAsync();
            var videos = await _videoRepository.GetAll().Where(s => s.IsHidden != true && string.IsNullOrWhiteSpace(s.Name) == false)
                .Select(s => new
                {
                    s.LastEditTime,
                    s.Id
                }).ToListAsync();
            var host = "https://www.cngal.org/";

            var mainurls = new List<string>
            {
                $"{host}data",
                $"{host}about",
                $"{host}structure",
                $"{host}privacy",
                $"{host}updatelog",
                $"{host}entries",
                $"{host}cv",
                $"{host}articles",
                $"{host}square",
            };
            var data = new List<SitemapModel>
            {
                new SitemapModel
                {
                    Loc =host,
                    ChangeFreq = "daily",
                    Priority = "1.00"
                }
            };
            foreach (var item in mainurls)
            {
                data.Add(new SitemapModel
                {
                    Loc = item,
                    ChangeFreq = "daily",
                    Priority = "0.90"
                });
            }
            foreach (var item in entries)
            {
                data.Add(new SitemapModel
                {
                    Loc = $"{host}entries/index/{item.Id}",
                    ChangeFreq = "monthly",
                    LastModified = item.LastEditTime.ToString("s"),
                    Priority = item.Type == EntryType.Game ? "0.80" : "0.70"
                });
            }
            foreach (var item in articles)
            {
                data.Add(new SitemapModel
                {
                    Loc = $"{host}articles/index/{item.Id}",
                    ChangeFreq = "yearly",
                    LastModified = item.LastEditTime.ToString("s"),
                    Priority = "0.65"
                });
            }
            foreach (var item in tags)
            {
                data.Add(new SitemapModel
                {
                    Loc = $"{host}tags/index/{item.Id}",
                    ChangeFreq = "monthly",
                    LastModified = item.LastEditTime.ToString("s"),
                    Priority = "0.60"
                });
            }
            foreach (var item in peripheries)
            {
                data.Add(new SitemapModel
                {
                    Loc = $"{host}peripheries/index/{item.Id}",
                    ChangeFreq = "monthly",
                    LastModified = item.LastEditTime.ToString("s"),
                    Priority = "0.50"
                });
            }
            foreach (var item in videos)
            {
                data.Add(new SitemapModel
                {
                    Loc = $"{host}videos/index/{item.Id}",
                    ChangeFreq = "monthly",
                    LastModified = item.LastEditTime.ToString("s"),
                    Priority = "0.40"
                });
            }

            using (var xml = XmlWriter.Create(path, new XmlWriterSettings { Indent = true }))
            {
                xml.WriteStartDocument();
                xml.WriteStartElement("urlset", "http://www.sitemaps.org/schemas/sitemap/0.9");
                foreach (var item in data)
                {
                    xml.WriteStartElement("url");
                    xml.WriteElementString("loc",  item.Loc);
                    xml.WriteElementString("priority", item.Priority);
                    xml.WriteElementString("changefreq", item.ChangeFreq);
                    xml.WriteElementString("lastmod", item.LastModified);
                    xml.WriteEndElement();
                }
                xml.WriteEndElement();
            }
        }
    }
}
