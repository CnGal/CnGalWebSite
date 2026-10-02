
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.APIServer.Configuration;
using CnGalWebSite.APIServer.Models;
using CnGalWebSite.Core.Configuration;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.BackUpArchives;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Threading;
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
        private readonly IOptions<BackupArchiveOptions> _backupArchiveOptions;
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger<BackUpArchiveService> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public BackUpArchiveService(IRepository<BackUpArchive, long> backUpArchiveRepository, IRepository<Entry, long> entryRepository, IRepository<Article, long> articleRepository, IOptions<BackupArchiveOptions> backupArchiveOptions, IRepository<Periphery, long> peripheryRepository,
        IHttpClientFactory clientFactory, IRepository<BackUpArchiveDetail, long> backUpArchiveDetailRepository, IWebHostEnvironment webHostEnvironment, IRepository<Tag, int> tagRepository, IRepository<Video, long> videoRepository, ILogger<BackUpArchiveService> logger)
        {
            _backUpArchiveRepository = backUpArchiveRepository;
            _entryRepository = entryRepository;
            _articleRepository = articleRepository;
            _backupArchiveOptions = backupArchiveOptions;
            _clientFactory = clientFactory;
            _logger = logger;
            _backUpArchiveDetailRepository = backUpArchiveDetailRepository;
            _webHostEnvironment = webHostEnvironment;
            _tagRepository = tagRepository;
            _videoRepository = videoRepository;
            _peripheryRepository = peripheryRepository;
        }


        public async Task BackUpArticle(BackUpArchive backUpArchive)
        {
            await BackUpPage(backUpArchive, "https://www.cngal.org/articles/index/" + backUpArchive.ArticleId);
        }

        public async Task BackUpEntry(BackUpArchive backUpArchive, string entryName)
        {
            await BackUpPage(backUpArchive, "https://www.cngal.org/entries/index/" + backUpArchive.EntryId);
        }

        private async Task BackUpPage(BackUpArchive backUpArchive, string url)
        {
            var configuredBase = new Uri(_backupArchiveOptions.GetOptional(BackupArchiveOptions.SectionName).BaseAddress);
            var query = QueryHelpers.ParseQuery(configuredBase.Query);
            var useRelay = query.ContainsKey("url");
            var upstream = new Uri(BackupArchiveOptions.ArchiveSaveBase + url);
            using var client = _clientFactory.CreateClient("backupArchive");
            using var timeout = new CancellationTokenSource(client.Timeout);
            var timer = System.Diagnostics.Stopwatch.StartNew();
            var success = false;
            try
            {
                for (int hop = 0; hop <= 5; hop++)
                {
                    // 只信任配置的中转入口；上游重定向也必须重新封装，经同一入口访问。
                    query["url"] = upstream.AbsoluteUri;
                    var requestUri = useRelay
                        ? new Uri(QueryHelpers.AddQueryString(configuredBase.GetLeftPart(UriPartial.Path), query))
                        : upstream;
                    using var response = await client.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead, timeout.Token);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        success = true;
                        break;
                    }

                    if (hop == 5 || response.StatusCode is not (HttpStatusCode.MovedPermanently or HttpStatusCode.Found or
                        HttpStatusCode.SeeOther or HttpStatusCode.TemporaryRedirect or HttpStatusCode.PermanentRedirect) ||
                        response.Headers.Location is not Uri location)
                        break;

                    if (!Uri.TryCreate(upstream, location, out var next) ||
                        next.Scheme != Uri.UriSchemeHttps || next.Host != "web.archive.org" || next.Port != 443 || next.UserInfo.Length != 0)
                        break;
                    upstream = next;
                }
                if (!success)
                    _logger.LogWarning("归档请求失败或重定向不符合归档策略");
            }
            catch (Exception ex) when (ex is HttpRequestException or OperationCanceledException)
            {
                _logger.LogError(ex, "归档请求失败");
            }
            await UpdateBackUpInfor(backUpArchive, !success, timer.Elapsed.TotalSeconds);
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
                    Priority = "1.00",
                    LastModified =DateTime.UtcNow.Date.ToString("yyyy-MM-dd"),
                }
            };
            foreach (var item in mainurls)
            {
                data.Add(new SitemapModel
                {
                    Loc = item,
                    ChangeFreq = "weekly",
                    Priority = "0.90",
                    LastModified = DateTime.UtcNow.Date.AddDays(-(int)DateTime.UtcNow.DayOfWeek).ToString("yyyy-MM-dd"),
                });
            }
            foreach (var item in entries)
            {
                data.Add(new SitemapModel
                {
                    Loc = $"{host}entries/index/{item.Id}",
                    ChangeFreq = "monthly",
                    LastModified = item.LastEditTime.ToString("yyyy-MM-dd"),
                    Priority = item.Type == EntryType.Game ? "0.80" : "0.70"
                });
            }
            foreach (var item in articles)
            {
                data.Add(new SitemapModel
                {
                    Loc = $"{host}articles/index/{item.Id}",
                    ChangeFreq = "yearly",
                    LastModified = item.LastEditTime.ToString("yyyy-MM-dd"),
                    Priority = "0.65"
                });
            }
            foreach (var item in tags)
            {
                data.Add(new SitemapModel
                {
                    Loc = $"{host}tags/index/{item.Id}",
                    ChangeFreq = "monthly",
                    LastModified = item.LastEditTime.ToString("yyyy-MM-dd"),
                    Priority = "0.60"
                });
            }
            foreach (var item in peripheries)
            {
                data.Add(new SitemapModel
                {
                    Loc = $"{host}peripheries/index/{item.Id}",
                    ChangeFreq = "monthly",
                    LastModified = item.LastEditTime.ToString("yyyy-MM-dd"),
                    Priority = "0.50"
                });
            }
            foreach (var item in videos)
            {
                data.Add(new SitemapModel
                {
                    Loc = $"{host}videos/index/{item.Id}",
                    ChangeFreq = "monthly",
                    LastModified = item.LastEditTime.ToString("yyyy-MM-dd"),
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
                    xml.WriteElementString("loc", item.Loc);
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
