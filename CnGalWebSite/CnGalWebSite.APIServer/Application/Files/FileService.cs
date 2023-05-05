using BootstrapBlazor.Components;
using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.Core.Services;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.Models;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Files;
using CnGalWebSite.DrawingBed.Helper.Services;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Dynamic.Core;
using SortOrder = BootstrapBlazor.Components.SortOrder;

namespace CnGalWebSite.APIServer.Application.Files
{
    public class FileService : IFileService
    {
        private readonly IRepository<Article, long> _articleRepository;
        private readonly IRepository<Entry, int> _entryRepository;
        private readonly IRepository<Video, long> _videoRepository;
        private readonly IAppHelper _appHelper;
        private readonly IHttpService _httpService;
        private readonly IFileUploadService _fileUploadService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<FileService> _logger;

        public FileService(IAppHelper appHelper,IConfiguration configuration, IHttpService httpService,
        IRepository<Article, long> articleRepository, IRepository<Entry, int> entryRepository, ILogger<FileService> logger, IRepository<Video, long> videoRepository, IFileUploadService fileUploadService)
        {
            _appHelper = appHelper;
            _configuration = configuration;
            _entryRepository = entryRepository;
            _articleRepository = articleRepository;
            _logger = logger;
            _videoRepository = videoRepository;
            _fileUploadService = fileUploadService;
            _httpService = httpService;
        }

        /// <summary>
        /// 转存图片到公共图床
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<string> TransferDepositFile(string url)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(url))
                {
                    return null;
                }

                if (url.Contains("http") == false)
                {
                    url = "https:" + url;
                }

                var client = await _httpService.GetClientAsync();
                using var content = new MultipartFormDataContent();
                using var fileContent = new StreamContent(await client.GetStreamAsync(url));

                content.Add(
                    content: fileContent,
                    name: "file",
                    fileName: "test.png");
                content.Add(new StringContent(_configuration["TucangCCAPIToken"]), "token");

                var response = await client.PostAsync(_configuration["TucangCCAPIUrl"], content);

                var newUploadResults = await response.Content.ReadAsStringAsync();
                var result = JObject.Parse(newUploadResults);

                if (result["code"].ToObject<int>() == 200)
                {
                    return $"{_configuration["CustomTucangCCUrl"]}{result["data"]["url"].ToObject<string>().Split('/').LastOrDefault()}?{url}";
                }
                else
                {
                    return null;
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "转存图片失败：{url}", url);
                throw;
            }
        }

        /// <summary>
        /// 转存指定数量的主图到公共图仓
        /// </summary>
        /// <param name="maxCount"></param>
        /// <returns></returns>
        public async Task TransferMainImagesToPublic(int maxCount)
        {
            var entries = await _entryRepository.GetAll().Where(s => string.IsNullOrWhiteSpace(s.MainPicture) == false && s.IsHidden == false && string.IsNullOrWhiteSpace(s.Name) == false && s.MainPicture.Contains("?") == false && s.MainPicture.Contains("default") == false)
                .OrderBy(s => s.Id)
                .Take(maxCount).ToListAsync();

            foreach (var item in entries)
            {
                try
                {
                    var temp = await TransferDepositFile(item.MainPicture);
                    if (string.IsNullOrWhiteSpace(temp) == false)
                    {
                        item.MainPicture = temp;
                        await _entryRepository.UpdateAsync(item);
                        _logger.LogInformation("转存 词条 - {name}({id}) 主图到 tucang.cc 图床，链接替换为：{url}", item.Name, item.Id, item.MainPicture);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "转存词条 - {Name}({Id}) 主图失败", item.Name, item.Id);
                }

            }

            entries = await _entryRepository.GetAll().Where(s => string.IsNullOrWhiteSpace(s.Thumbnail) == false && s.IsHidden == false && string.IsNullOrWhiteSpace(s.Name) == false && s.MainPicture.Contains("?") == false && s.MainPicture.Contains("default") == false)
               .OrderBy(s => s.Id)
               .Take(maxCount).ToListAsync();

            foreach (var item in entries)
            {
                try
                {
                    var temp = await TransferDepositFile(item.Thumbnail);
                    if (string.IsNullOrWhiteSpace(temp) == false)
                    {
                        item.Thumbnail = temp;
                        await _entryRepository.UpdateAsync(item);
                        _logger.LogInformation("转存 词条 - {name}({id}) 缩略图到 tucang.cc 图床，链接替换为：{url}", item.Name, item.Id, item.Thumbnail);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "转存词条 - {Name}({Id}) 缩略图失败", item.Name, item.Id);
                }

            }


            var articles = await _articleRepository.GetAll().Where(s => string.IsNullOrWhiteSpace(s.MainPicture) == false && s.IsHidden == false && string.IsNullOrWhiteSpace(s.Name) == false && s.MainPicture.Contains("?") == false && s.MainPicture.Contains("default") == false)
                .OrderBy(s => s.Id)
               .Take(maxCount).ToListAsync();

            foreach (var item in articles)
            {
                try
                {
                    var temp = await TransferDepositFile(item.MainPicture);
                    if (string.IsNullOrWhiteSpace(temp) == false)
                    {
                        item.MainPicture = temp;
                        await _articleRepository.UpdateAsync(item);
                        _logger.LogInformation("转存 文章 - {name}({id}) 主图到 tucang.cc 图床，链接替换为：{url}", item.Name, item.Id, item.MainPicture);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "转存文章 - {Name}({Id}) 主图失败", item.Name, item.Id);
                }
            }

            var videos = await _videoRepository.GetAll().Where(s => string.IsNullOrWhiteSpace(s.MainPicture) == false && s.IsHidden == false && string.IsNullOrWhiteSpace(s.Name) == false && s.MainPicture.Contains("?") == false && s.MainPicture.Contains("default") == false)
              .OrderBy(s => s.Id)
              .Take(maxCount).ToListAsync();

            foreach (var item in videos)
            {
                try
                {
                    var temp = await TransferDepositFile(item.MainPicture);
                    if (string.IsNullOrWhiteSpace(temp) == false)
                    {
                        item.MainPicture = temp;
                        await _videoRepository.UpdateAsync(item);
                        _logger.LogInformation("转存 视频 - {name}({id}) 主图到 tucang.cc 图床，链接替换为：{url}", item.Name, item.Id, item.MainPicture);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "转存视频 - {Name}({Id}) 主图失败", item.Name, item.Id);
                }
            }

            var entryPictures = await _entryRepository.GetAll()
                .Include(s => s.Pictures)
                .Where(s => s.IsHidden == false && string.IsNullOrWhiteSpace(s.Name) == false)
                .Where(s => s.Pictures.Any(s => s.Url.Contains("?") == false && s.Url.Contains("default") == false))
                .OrderBy(s => s.Id)
                .Take(maxCount)
                .ToListAsync();

            foreach (var item in entryPictures)
            {
                try
                {
                    var count = item.Pictures.Count(s => s.Url.Contains('?') == false && s.Url.Contains("default") == false);
                    foreach (var infor in item.Pictures.Where(s => s.Url.Contains('?') == false && s.Url.Contains("default") == false))
                    {
                        var temp = await TransferDepositFile(infor.Url);
                        if (string.IsNullOrWhiteSpace(temp) == false)
                        {
                            infor.Url = temp;
                        }
                        else
                        {
                            throw new Exception("转存图片失败");
                        }
                    }

                    await _entryRepository.UpdateAsync(item);
                    _logger.LogInformation("转存 词条 - {name}({id}) 相册到 tucang.cc 图床，总计 {count} 张图片", item.Name, item.Id, count);

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "转存 词条 - {name}({id}) 相册失败", item.Name, item.Id);
                }
            }
        }
    }
}


