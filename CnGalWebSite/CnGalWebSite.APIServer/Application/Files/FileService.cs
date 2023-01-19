using BootstrapBlazor.Components;
using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.DataReositories;

using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.Models;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Files;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Linq.Dynamic.Core;
using SortOrder = BootstrapBlazor.Components.SortOrder;

namespace CnGalWebSite.APIServer.Application.Files
{
    public class FileService : IFileService
    {
        private readonly IRepository<UserFile, int> _userFileRepository;
        private readonly IRepository<Article, long> _articleRepository;
        private readonly IRepository<Entry, int> _entryRepository;
        private readonly IRepository<Video, long> _videoRepository;
        private readonly IAppHelper _appHelper;
        private readonly HttpClient _httpClient;
        private readonly IRepository<FileManager, int> _fileManagerRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<FileService> _logger;

        private static readonly ConcurrentDictionary<Type, Func<IEnumerable<UserFile>, string, SortOrder, IEnumerable<UserFile>>> SortLambdaCache = new();


        public FileService(IAppHelper appHelper, IRepository<UserFile, int> userFileRepository, HttpClient httpClient, IRepository<FileManager, int> fileManagerRepository, IConfiguration configuration,
            IRepository<Article, long> articleRepository, IRepository<Entry, int> entryRepository, ILogger<FileService> logger, IRepository<Video, long> videoRepository)
        {
            _userFileRepository = userFileRepository;
            _appHelper = appHelper;
            _httpClient = httpClient;
            _fileManagerRepository = fileManagerRepository;
            _configuration = configuration;
            _entryRepository = entryRepository;
            _articleRepository = articleRepository;
            _logger = logger;
            _videoRepository = videoRepository;
        }

        public async Task<PagedResultDto<ImageInforTipViewModel>> GetImagePaginatedResult(PagedSortedAndFilterInput input)
        {
            var query = _userFileRepository.GetAll().Where(s => s.Type == UploadFileType.Image).AsNoTracking();

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
                        Duration = s.Duration,
                        Sha1 = s.Sha1,
                        Type = s.Type
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
                    Duration = item.Duration,
                    Type = item.Type,
                    Sha1 = item.Sha1,
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

                using var content = new MultipartFormDataContent();
                using var fileContent = new StreamContent(await _httpClient.GetStreamAsync(url));

                content.Add(
                    content: fileContent,
                    name: "file",
                    fileName: "test.png");
                content.Add(new StringContent(_configuration["TucangCCAPIToken"]), "token");

                var response = await _httpClient.PostAsync(_configuration["TucangCCAPIUrl"], content);

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
        /// 保存图片到本地图床
        /// </summary>
        /// <param name="url"></param>
        /// <param name="userId"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public async Task<string> SaveImageAsync(string url, string userId, double x = 0, double y = 0)
        {
            try
            {
                var result = await _httpClient.PostAsJsonAsync($"{ToolHelper.ImageApiPath}api/files/linkToImgUrl?url={url}&x={x}&y={y}", new TransferDepositFileModel());
                var jsonContent = result.Content.ReadAsStringAsync().Result;
                var obj = System.Text.Json.JsonSerializer.Deserialize<UploadResult>(jsonContent, ToolHelper.options);

                if (obj.Uploaded)
                {
                    await AddUserUploadFileInfor(userId, new AddUserUploadFileInforModel
                    {
                        Sha1 = obj.Sha1,
                        FileSize = obj.FileSize,
                        Duration = obj.Duration,
                        FileName = obj.Url,
                        Type = UploadFileType.Image
                    });

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
                _logger.LogError(ex, "转存图片失败：{url}", url);
                return url;
            }

        }

        /// <summary>
        /// 转存指定数量的主图到公共图仓
        /// </summary>
        /// <param name="maxCount"></param>
        /// <returns></returns>
        public async Task TransferAllMainImages(int maxCount)
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
                    throw;
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
                    throw;
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
                    throw;
                }
            }
        }

        public async Task AddUserUploadFileInfor(string userId, AddUserUploadFileInforModel model)
        {
            //加载文件管理
            var fileManager = await _fileManagerRepository.GetAll().Include(s => s.UserFiles).FirstOrDefaultAsync(s => s.ApplicationUserId == userId);
            if (fileManager == null)
            {
                fileManager = new FileManager
                {
                    ApplicationUserId = userId,
                    TotalSize = 500 * 1024 * 1024,
                    UsedSize = 0,
                    UserFiles = new List<UserFile>()
                };
                fileManager = await _fileManagerRepository.InsertAsync(fileManager);
            }

            //检测是否已添加相同文件
            if (string.IsNullOrWhiteSpace(model.Sha1) == false && await _userFileRepository.GetAll().AnyAsync(s => s.Sha1 == model.Sha1))
            {
                return;
            }


            fileManager.UserFiles.Add(new UserFile
            {
                FileName = model.FileName,
                UploadTime = DateTime.Now.ToCstTime(),
                FileSize = model.FileSize,
                Sha1 = model.Sha1,
                Duration = model.Duration,
                Type = model.Type,
                UserId = fileManager.ApplicationUserId
            });
            fileManager.UsedSize += (model.FileSize ?? 0);
            //更新用户文件列表
            await _fileManagerRepository.UpdateAsync(fileManager);
        }

        /// <summary>
        /// 转存字符串中的图片 并添加到用户文件
        /// </summary>
        /// <param name="text"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<string> TransformImagesAsync(string text, string userId)
        {
            var temp = await ToolHelper.TransformImagesAsync(text, _httpClient);
            text = temp.Text;

            foreach (var infor in temp.UploadResults)
            {
                await AddUserUploadFileInfor(userId, new AddUserUploadFileInforModel
                {
                    Sha1 = infor.Sha1,
                    FileSize = infor.FileSize,
                    Duration = infor.Duration,
                    FileName = infor.Url,
                    Type = UploadFileType.Image
                });
            }

            return text;
        }
    }
}


