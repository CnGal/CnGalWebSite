using BootstrapBlazor.Components;
using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.DataModel.Application.Dtos;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.Models;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Files;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using SortOrder = BootstrapBlazor.Components.SortOrder;
using Microsoft.Extensions.Logging;

namespace CnGalWebSite.APIServer.Application.Files
{
    public class FileService : IFileService
    {
        private readonly IRepository<UserFile, int> _userFileRepository;
        private readonly IRepository<Article, long> _articleRepository;
        private readonly IRepository<Entry, int> _entryRepository;
        private readonly IAppHelper _appHelper;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IRepository<FileManager, int> _fileManagerRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<FileService> _logger;

        private static readonly ConcurrentDictionary<Type, Func<IEnumerable<UserFile>, string, SortOrder, IEnumerable<UserFile>>> SortLambdaCache = new();


        public FileService(IAppHelper appHelper, IRepository<UserFile, int> userFileRepository, IHttpClientFactory clientFactory, IRepository<FileManager, int> fileManagerRepository, IConfiguration configuration,
            IRepository<Article, long> articleRepository, IRepository<Entry, int> entryRepository, ILogger<FileService> logger)
        {
            _userFileRepository = userFileRepository;
            _appHelper = appHelper;
            _clientFactory = clientFactory;
            _fileManagerRepository = fileManagerRepository;
            _configuration = configuration;
            _entryRepository = entryRepository;
            _articleRepository = articleRepository;
            _logger = logger;
        }

        public async Task<PagedResultDto<ImageInforTipViewModel>> GetImagePaginatedResult(PagedSortedAndFilterInput input)
        {
            var query = _userFileRepository.GetAll().Where(s=>s.Type== UploadFileType.Image).AsNoTracking();

            //判断输入的查询名称是否为空
            if (!string.IsNullOrWhiteSpace(input.FilterText))
            {
                query = query.Where(s => s.FileName == input.FilterText);
            }
            //统计查询数据的总条数
            var count = query.Count();
            //根据需求进行排序，然后进行分页逻辑的计算
            //这个特殊方法中当前页数解释为起始位
            query = query.OrderBy(input.Sorting).Skip((input.CurrentPage - 1) * input.MaxResultCount).Take(input.MaxResultCount);

            //将结果转换为List集合 加载到内存中
            List<ImageInforTipViewModel> models = null;
            if (count != 0)
            {
                models = await query.AsNoTracking().Include(s => s.FileManager).ThenInclude(s => s.ApplicationUser)
                    .Select(s => new ImageInforTipViewModel
                    {
                        FileName = s.FileName,
                        Id = s.Id,
                        FileSize = s.FileSize,
                        UploadTime = s.UploadTime,
                        UserId = s.FileManager.ApplicationUserId,
                        UserName = s.FileManager.ApplicationUser.UserName
                    }).ToListAsync();
            }
            else
            {
                models = new List<ImageInforTipViewModel>();
            }

            var dtos = models;

            var dtos_ = new PagedResultDto<ImageInforTipViewModel>
            {
                TotalCount = count,
                CurrentPage = input.CurrentPage,
                MaxResultCount = input.MaxResultCount,
                Data = dtos,
                FilterText = input.FilterText,
                Sorting = input.Sorting,
                ScreeningConditions = input.ScreeningConditions
            };
            return dtos_;
        }

        public async Task<PagedResultDto<ListFileAloneModel>> GetAudioPaginatedResult(PagedSortedAndFilterInput input)
        {
            var query = _userFileRepository.GetAll().Where(s => s.Type == UploadFileType.Audio).AsNoTracking();

            //判断输入的查询名称是否为空
            if (!string.IsNullOrWhiteSpace(input.FilterText))
            {
                query = query.Where(s => s.FileName == input.FilterText);
            }
            //统计查询数据的总条数
            var count = query.Count();
            //根据需求进行排序，然后进行分页逻辑的计算
            //这个特殊方法中当前页数解释为起始位
            query = query.OrderBy(input.Sorting).Skip((input.CurrentPage - 1) * input.MaxResultCount).Take(input.MaxResultCount);

            //将结果转换为List集合 加载到内存中
            List<ListFileAloneModel> models = null;
            if (count != 0)
            {
                models = await query.AsNoTracking().Include(s => s.FileManager).ThenInclude(s => s.ApplicationUser)
                    .Select(s => new ListFileAloneModel
                    {
                        FileName = s.FileName,
                        Id = s.Id,
                        FileSize = s.FileSize,
                        UploadTime = s.UploadTime,
                        UserId = s.FileManager.ApplicationUserId,
                        UserName = s.FileManager.ApplicationUser.UserName,
                        Duration=s.Duration,
                        Sha1 = s.Sha1,
                        Type=s.Type
                    }).ToListAsync();
            }
            else
            {
                models = new List<ListFileAloneModel>();
            }

            var dtos = models;

            var dtos_ = new PagedResultDto<ListFileAloneModel>
            {
                TotalCount = count,
                CurrentPage = input.CurrentPage,
                MaxResultCount = input.MaxResultCount,
                Data = dtos,
                FilterText = input.FilterText,
                Sorting = input.Sorting,
                ScreeningConditions = input.ScreeningConditions
            };
            return dtos_;
        }

        public Task<QueryData<ListFileAloneModel>> GetPaginatedResult(CnGalWebSite.DataModel.ViewModel.Search.QueryPageOptions options, ListFileAloneModel searchModel)
        {
            IEnumerable<UserFile> items = _userFileRepository.GetAll();
            // 处理高级搜索
            if (!string.IsNullOrWhiteSpace(searchModel.FileName))
            {
                items = items.Where(item => item.FileName?.Contains(searchModel.FileName, StringComparison.OrdinalIgnoreCase) ?? false);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.UserId))
            {
                items = items.Where(item => item.UserId?.Contains(searchModel.UserId, StringComparison.OrdinalIgnoreCase) ?? false);
            }


            // 处理 SearchText 模糊搜索
            if (!string.IsNullOrWhiteSpace(options.SearchText))
            {
                items = items.Where(item => (item.FileName?.Contains(options.SearchText) ?? false)
                             || (item.UserId?.Contains(options.SearchText) ?? false));
            }

            // 排序
            var isSorted = false;
            if (!string.IsNullOrWhiteSpace(options.SortName))
            {
                // 外部未进行排序，内部自动进行排序处理
                var invoker = SortLambdaCache.GetOrAdd(typeof(UserFile), key => LambdaExtensions.GetSortLambda<UserFile>().Compile());
                items = invoker(items, options.SortName, (BootstrapBlazor.Components.SortOrder)options.SortOrder);
                isSorted = true;
            }

            // 设置记录总数
            var total = items.Count();

            // 内存分页
            items = items.Skip((options.PageIndex - 1) * options.PageItems).Take(options.PageItems).ToList();

            //复制数据
            var resultItems = new List<ListFileAloneModel>();
            foreach (var item in items)
            {
                resultItems.Add(new ListFileAloneModel
                {
                    Id = item.Id,
                    FileName = item.FileName,
                    FileSize = item.FileSize,
                    UploadTime = item.UploadTime,
                    UserId = item.UserId,
                    Duration= item.Duration,
                    Type= item.Type,
                    Sha1= item.Sha1,
                });
            }

            return Task.FromResult(new QueryData<ListFileAloneModel>()
            {
                Items = resultItems,
                TotalCount = total,
                IsSorted = isSorted,
                // IsFiltered = isFiltered
            });
        }

        public async Task<string> SaveImageAsync(string url, string userId, double x = 0, double y = 0)
        {
            try
            {
                using var client = _clientFactory.CreateClient();
                //client.Timeout = TimeSpan.FromSeconds(30);

                var result = await client.PostAsJsonAsync($"{_configuration["TransferDepositFileAPI"]}api/files/linkToImgUrl?url={url}&x={x}&y={y}", new TransferDepositFileModel());
                var jsonContent = result.Content.ReadAsStringAsync().Result;
                var obj = System.Text.Json.JsonSerializer.Deserialize<LinkToImgResult>(jsonContent, ToolHelper.options);

                if (obj.Successful)
                {
                    _logger.LogInformation("转存图片成功：{url}", obj.Url);
                    return obj.Url;
                }
                else
                {
                    _logger.LogError("转存图片失败：{url}", url);
                    return url;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"转存图片失败：{url}", url);
                return url;
            }

        }

        public async Task<string> TransferDepositFile(string url)
        {
            try
            {
                if(url.Contains("http")==false)
                {
                    url = "https:" + url;
                }
                using var client = _clientFactory.CreateClient();

                using var content = new MultipartFormDataContent();
                using var fileContent = new StreamContent(await client.GetStreamAsync(url));

                content.Add(
                    content: fileContent,
                    name: "file",
                    fileName: "test.png");
                content.Add(new StringContent(_configuration["TucangCCAPIToken"]), "token");

                var response = await client.PostAsync("https://tucang.cc/api/v1/upload", content);

                var newUploadResults = await response.Content.ReadAsStringAsync();
                var result = JObject.Parse(newUploadResults);

                if (result["code"].ToObject<int>() == 200)
                {
                    return result["data"]["url"].ToObject<string>().Replace("tucang.cc", _configuration["CustomTucangCCUrl"]) + "?" + url;
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

        public async Task TransferAllMainImages(int maxCount)
        {
            var entries = await _entryRepository.GetAll().Where(s => string.IsNullOrWhiteSpace(s.MainPicture) == false && s.MainPicture.Contains("?") == false && s.MainPicture.Contains("cngal"))
                .OrderBy(s => s.Id)
                .Take(maxCount).ToListAsync();

            foreach (var item in entries)
            {
                var temp = await TransferDepositFile(item.MainPicture);
                if (string.IsNullOrWhiteSpace(temp) == false)
                {
                    item.MainPicture = temp;
                    await _entryRepository.UpdateAsync(item);
                    _logger.LogInformation("转存 词条 - {name}({id}) 主图到 tucang.cc 图床，链接替换为：{url}", item.Name, item.Id, item.MainPicture);
                }
            }

            var articles = await _articleRepository.GetAll().Where(s => string.IsNullOrWhiteSpace(s.MainPicture) == false && s.MainPicture.Contains("?") == false && s.MainPicture.Contains("cngal"))
                .OrderBy(s => s.Id)
               .Take(maxCount).ToListAsync();

            foreach (var item in articles)
            {
                var temp = await TransferDepositFile(item.MainPicture);
                if (string.IsNullOrWhiteSpace(temp) == false)
                {
                    item.MainPicture = temp;
                    await _articleRepository.UpdateAsync(item);
                    _logger.LogInformation("转存 文章 - {name}({id}) 主图到 tucang.cc 图床，链接替换为：{url}", item.Name, item.Id, item.MainPicture);
                }
            }
        }
    }
}


