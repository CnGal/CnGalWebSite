using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.Application.Search;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.APIServer.Model;

using CnGalWebSite.DataModel.Application.Search.Dtos;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Search;
using CnGalWebSite.Helper.Extensions;
using Meilisearch;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.Search.Meilisearch
{
    public class MeilisearchHelper : ISearchHelper
    {
        private readonly MeilisearchClient _client;
        private readonly IRepository<Entry, int> _entryRepository;
        private readonly IRepository<Tag, int> _tagRepository;
        private readonly IRepository<Article, long> _articleRepository;
        private readonly IRepository<Periphery, long> _peripheryRepository;
        private readonly IRepository<Video, long> _videoRepository;
        private readonly IRepository<SearchCache, long> _searchCacheRepository;
        private readonly ILogger<MeilisearchHelper> _logger;
        private readonly IAppHelper _appHelper;
        private readonly string _indexName = "SearchCache";

        /// <summary>
        /// WaitForTaskAsync 超时时间（毫秒），默认 5 分钟
        /// </summary>
        private const double TaskTimeoutMs = 300_000;

        public MeilisearchHelper(MeilisearchClient client, IRepository<Entry, int> entryRepository, IRepository<SearchCache, long> searchCacheRepository, IRepository<Video, long> videoRepository, ILogger<MeilisearchHelper> logger,
        IRepository<Tag, int> tagRepository, IRepository<Article, long> articleRepository, IRepository<Periphery, long> peripheryRepository, IAppHelper appHelper)
        {
            _client = client;
            _entryRepository = entryRepository;
            _tagRepository = tagRepository;
            _peripheryRepository = peripheryRepository;
            _articleRepository = articleRepository;
            _appHelper = appHelper;
            _searchCacheRepository = searchCacheRepository;
            _videoRepository = videoRepository;
            _logger = logger;
        }

        public async System.Threading.Tasks.Task UpdateDataToSearchService(DateTime LastUpdateTime, bool updateAll = false)
        {
            await EnsureIndexSettings();

            try { await UpdateEntries(LastUpdateTime, updateAll); }
            catch (Exception ex) { _logger.LogError(ex, "更新词条搜索数据失败"); }

            try { await UpdateArticles(LastUpdateTime, updateAll); }
            catch (Exception ex) { _logger.LogError(ex, "更新文章搜索数据失败"); }

            try { await UpdatePeripheries(LastUpdateTime, updateAll); }
            catch (Exception ex) { _logger.LogError(ex, "更新周边搜索数据失败"); }

            try { await UpdateTags(LastUpdateTime, updateAll); }
            catch (Exception ex) { _logger.LogError(ex, "更新标签搜索数据失败"); }

            try { await UpdateVideos(LastUpdateTime, updateAll); }
            catch (Exception ex) { _logger.LogError(ex, "更新视频搜索数据失败"); }

            _logger.LogInformation("更新搜索数据完成");
        }

        private async System.Threading.Tasks.Task EnsureIndexSettings()
        {
            var index = _client.Index(_indexName);

            try
            {
                await index.FetchInfoAsync();
            }
            catch
            {
                // 索引不存在时创建
                var taskInfo = await _client.CreateIndexAsync(_indexName, "id");
                await _client.WaitForTaskAsync(taskInfo.TaskUid, TaskTimeoutMs);
            }

            var settings = new Settings
            {
                SearchableAttributes = new[] { "name", "displayName", "anotherName", "briefIntroduction", "mainPage" },
                FilterableAttributes = new[] { "type", "originalType", "pubulishTime", "lastEditTime" },
                SortableAttributes = new[] { "lastEditTime", "pubulishTime", "createTime", "readerCount", "originalId" }
            };

            var updateTask = await index.UpdateSettingsAsync(settings);
            await _client.WaitForTaskAsync(updateTask.TaskUid, TaskTimeoutMs);
        }

        private async System.Threading.Tasks.Task UpdateEntries(DateTime LastUpdateTime, bool updateAll = false)
        {
            var entries = await _entryRepository.GetAll().AsNoTracking()
                .Where(s => (s.LastEditTime > LastUpdateTime || updateAll) && s.IsHidden == false && string.IsNullOrWhiteSpace(s.Name) == false).ToListAsync();
            var documents = new List<SearchCache>();
            if (entries.Any())
            {
                var entryIds = entries.Select(s => (long)s.Id).ToList();
                documents = await _searchCacheRepository.GetAll().Where(s => s.Type == 0 && entryIds.Contains(s.OriginalId)).ToListAsync();

                foreach (var item in entries)
                {
                    _logger.LogInformation("Entry:{id}", item.Id);
                    var temp = documents.FirstOrDefault(s => s.OriginalId == item.Id);
                    if (temp == null)
                    {
                        temp = new SearchCache();
                        temp.Copy(item);
                        temp = await _searchCacheRepository.InsertAsync(temp);
                        temp.Copy(item);
                        temp = await _searchCacheRepository.UpdateAsync(temp);
                        documents.Add(temp);
                    }
                    else
                    {
                        temp.Copy(item);
                        temp = await _searchCacheRepository.UpdateAsync(temp);
                    }
                }

                // 将数据发送到 Meilisearch
                var index = _client.Index(_indexName);
                var taskInfo = await index.AddDocumentsAsync(documents, "id");
                await _client.WaitForTaskAsync(taskInfo.TaskUid, TaskTimeoutMs);
                _logger.LogInformation("已索引 {count} 个词条", documents.Count);
            }

            var deleted = await _entryRepository.GetAll().Where(s => s.IsHidden || string.IsNullOrWhiteSpace(s.Name)).Select(s => (long)s.Id).ToListAsync();
            documents = await _searchCacheRepository.GetAll().Where(s => s.Type == 0 && deleted.Contains(s.OriginalId)).ToListAsync();
            if (documents.Count != 0)
            {
                var index = _client.Index(_indexName);
                var ids = documents.Select(s => s.Id).ToList();
                var taskInfo = await index.DeleteDocumentsAsync(ids);
                await _client.WaitForTaskAsync(taskInfo.TaskUid, TaskTimeoutMs);

                await _searchCacheRepository.GetAll().Where(s => s.Type == 0 && deleted.Contains(s.OriginalId)).ExecuteDeleteAsync();
            }
        }

        private async System.Threading.Tasks.Task UpdateArticles(DateTime LastUpdateTime, bool updateAll = false)
        {
            var entries = await _articleRepository.GetAll().AsNoTracking()
                .Where(s => (s.LastEditTime > LastUpdateTime || updateAll) && s.IsHidden == false && string.IsNullOrWhiteSpace(s.Name) == false).ToListAsync();
            var documents = new List<SearchCache>();
            if (entries.Any())
            {
                var entryIds = entries.Select(s => s.Id).ToList();
                documents = await _searchCacheRepository.GetAll().Where(s => s.Type == 1 && entryIds.Contains(s.OriginalId)).ToListAsync();

                foreach (var item in entries)
                {
                    _logger.LogInformation("Article:{id}", item.Id);
                    var temp = documents.FirstOrDefault(s => s.OriginalId == item.Id);
                    if (temp == null)
                    {
                        temp = new SearchCache();
                        temp.Copy(item);
                        temp = await _searchCacheRepository.InsertAsync(temp);
                        temp.Copy(item);
                        temp = await _searchCacheRepository.UpdateAsync(temp);
                        documents.Add(temp);
                    }
                    else
                    {
                        temp.Copy(item);
                        temp = await _searchCacheRepository.UpdateAsync(temp);
                    }
                }

                var index = _client.Index(_indexName);
                var taskInfo = await index.AddDocumentsAsync(documents, "id");
                await _client.WaitForTaskAsync(taskInfo.TaskUid, TaskTimeoutMs);
                _logger.LogInformation("已索引 {count} 篇文章", documents.Count);
            }

            var deleted = await _articleRepository.GetAll().Where(s => s.IsHidden || string.IsNullOrWhiteSpace(s.Name)).Select(s => s.Id).ToListAsync();
            documents = await _searchCacheRepository.GetAll().Where(s => s.Type == 1 && deleted.Contains(s.OriginalId)).ToListAsync();
            if (documents.Count != 0)
            {
                var index = _client.Index(_indexName);
                var ids = documents.Select(s => s.Id).ToList();
                var taskInfo = await index.DeleteDocumentsAsync(ids);
                await _client.WaitForTaskAsync(taskInfo.TaskUid, TaskTimeoutMs);

                await _searchCacheRepository.GetAll().Where(s => s.Type == 1 && deleted.Contains(s.OriginalId)).ExecuteDeleteAsync();
            }
        }

        private async System.Threading.Tasks.Task UpdateVideos(DateTime LastUpdateTime, bool updateAll = false)
        {
            var entries = await _videoRepository.GetAll().AsNoTracking()
                .Where(s => (s.LastEditTime > LastUpdateTime || updateAll) && s.IsHidden == false && string.IsNullOrWhiteSpace(s.Name) == false).ToListAsync();
            var documents = new List<SearchCache>();
            if (entries.Any())
            {
                var entryIds = entries.Select(s => s.Id).ToList();
                documents = await _searchCacheRepository.GetAll().Where(s => s.Type == 4 && entryIds.Contains(s.OriginalId)).ToListAsync();

                foreach (var item in entries)
                {
                    _logger.LogInformation("Video:{id}", item.Id);
                    var temp = documents.FirstOrDefault(s => s.OriginalId == item.Id);
                    if (temp == null)
                    {
                        temp = new SearchCache();
                        temp.Copy(item);
                        temp = await _searchCacheRepository.InsertAsync(temp);
                        temp.Copy(item);
                        temp = await _searchCacheRepository.UpdateAsync(temp);
                        documents.Add(temp);
                    }
                    else
                    {
                        temp.Copy(item);
                        temp = await _searchCacheRepository.UpdateAsync(temp);
                    }
                }

                var index = _client.Index(_indexName);
                var taskInfo = await index.AddDocumentsAsync(documents, "id");
                await _client.WaitForTaskAsync(taskInfo.TaskUid, TaskTimeoutMs);
                _logger.LogInformation("已索引 {count} 个视频", documents.Count);
            }

            var deleted = await _videoRepository.GetAll().Where(s => s.IsHidden || string.IsNullOrWhiteSpace(s.Name)).Select(s => s.Id).ToListAsync();
            documents = await _searchCacheRepository.GetAll().Where(s => s.Type == 4 && deleted.Contains(s.OriginalId)).ToListAsync();
            if (documents.Count != 0)
            {
                var index = _client.Index(_indexName);
                var ids = documents.Select(s => s.Id).ToList();
                var taskInfo = await index.DeleteDocumentsAsync(ids);
                await _client.WaitForTaskAsync(taskInfo.TaskUid, TaskTimeoutMs);

                await _searchCacheRepository.GetAll().Where(s => s.Type == 4 && deleted.Contains(s.OriginalId)).ExecuteDeleteAsync();
            }
        }

        private async System.Threading.Tasks.Task UpdatePeripheries(DateTime LastUpdateTime, bool updateAll = false)
        {
            var entries = await _peripheryRepository.GetAll().AsNoTracking()
                .Where(s => (s.LastEditTime > LastUpdateTime || updateAll) && s.IsHidden == false && string.IsNullOrWhiteSpace(s.Name) == false).ToListAsync();
            var documents = new List<SearchCache>();
            if (entries.Any())
            {
                var entryIds = entries.Select(s => s.Id).ToList();
                documents = await _searchCacheRepository.GetAll().Where(s => s.Type == 2 && entryIds.Contains(s.OriginalId)).ToListAsync();

                foreach (var item in entries)
                {
                    _logger.LogInformation("Periphery:{id}", item.Id);
                    var temp = documents.FirstOrDefault(s => s.OriginalId == item.Id);
                    if (temp == null)
                    {
                        temp = new SearchCache();
                        temp.Copy(item);
                        temp = await _searchCacheRepository.InsertAsync(temp);
                        temp.Copy(item);
                        temp = await _searchCacheRepository.UpdateAsync(temp);
                        documents.Add(temp);
                    }
                    else
                    {
                        temp.Copy(item);
                        temp = await _searchCacheRepository.UpdateAsync(temp);
                    }
                }

                var index = _client.Index(_indexName);
                var taskInfo = await index.AddDocumentsAsync(documents, "id");
                await _client.WaitForTaskAsync(taskInfo.TaskUid, TaskTimeoutMs);
                _logger.LogInformation("已索引 {count} 个周边", documents.Count);
            }

            var deleted = await _peripheryRepository.GetAll().Where(s => s.IsHidden || string.IsNullOrWhiteSpace(s.Name)).Select(s => s.Id).ToListAsync();
            documents = await _searchCacheRepository.GetAll().Where(s => s.Type == 2 && deleted.Contains(s.OriginalId)).ToListAsync();
            if (documents.Count != 0)
            {
                var index = _client.Index(_indexName);
                var ids = documents.Select(s => s.Id).ToList();
                var taskInfo = await index.DeleteDocumentsAsync(ids);
                await _client.WaitForTaskAsync(taskInfo.TaskUid, TaskTimeoutMs);

                await _searchCacheRepository.GetAll().Where(s => s.Type == 2 && deleted.Contains(s.OriginalId)).ExecuteDeleteAsync();
            }
        }

        private async System.Threading.Tasks.Task UpdateTags(DateTime LastUpdateTime, bool updateAll = false)
        {
            var entries = await _tagRepository.GetAll().AsNoTracking()
                .Where(s => (s.LastEditTime > LastUpdateTime || updateAll) && s.IsHidden == false && string.IsNullOrWhiteSpace(s.Name) == false).ToListAsync();
            var documents = new List<SearchCache>();
            if (entries.Any())
            {
                var entryIds = entries.Select(s => (long)s.Id).ToList();
                documents = await _searchCacheRepository.GetAll().Where(s => s.Type == 3 && entryIds.Contains(s.OriginalId)).ToListAsync();

                foreach (var item in entries)
                {
                    _logger.LogInformation("Tag:{id}", item.Id);
                    var temp = documents.FirstOrDefault(s => s.OriginalId == item.Id);
                    if (temp == null)
                    {
                        temp = new SearchCache();
                        temp.Copy(item);
                        temp = await _searchCacheRepository.InsertAsync(temp);
                        temp.Copy(item);
                        temp = await _searchCacheRepository.UpdateAsync(temp);
                        documents.Add(temp);
                    }
                    else
                    {
                        temp.Copy(item);
                        temp = await _searchCacheRepository.UpdateAsync(temp);
                    }
                }

                var index = _client.Index(_indexName);
                var taskInfo = await index.AddDocumentsAsync(documents, "id");
                await _client.WaitForTaskAsync(taskInfo.TaskUid, TaskTimeoutMs);
                _logger.LogInformation("已索引 {count} 个标签", documents.Count);
            }

            var deleted = await _tagRepository.GetAll().Where(s => s.IsHidden || string.IsNullOrWhiteSpace(s.Name)).Select(s => (long)s.Id).ToListAsync();
            documents = await _searchCacheRepository.GetAll().Where(s => s.Type == 3 && deleted.Contains(s.OriginalId)).ToListAsync();
            if (documents.Count != 0)
            {
                var index = _client.Index(_indexName);
                var ids = documents.Select(s => s.Id).ToList();
                var taskInfo = await index.DeleteDocumentsAsync(ids);
                await _client.WaitForTaskAsync(taskInfo.TaskUid, TaskTimeoutMs);

                await _searchCacheRepository.GetAll().Where(s => s.Type == 3 && deleted.Contains(s.OriginalId)).ExecuteDeleteAsync();
            }
        }

        public async System.Threading.Tasks.Task DeleteDataOfSearchService()
        {
            try
            {
                var taskInfo = await _client.DeleteIndexAsync(_indexName);
                await _client.WaitForTaskAsync(taskInfo.TaskUid, TaskTimeoutMs);
            }
            catch
            {
            }
        }

        private async Task<PagedResultDto<SearchAloneModel>> ProcSearchResult(ISearchable<SearchCache> model, int page)
        {
            // 根据查询结果向数据库获取真实信息
            var hits = model.Hits.ToList();

            var entryIds = hits.Where(s => s.Type == 0).Select(s => s.OriginalId).ToList();
            var entries = await _entryRepository.GetAll().AsNoTracking().Include(s => s.Information)
                    .Include(s => s.EntryRelationFromEntryNavigation).ThenInclude(s => s.ToEntryNavigation)
                    .Include(s => s.EntryStaffToEntryNavigation).ThenInclude(s => s.FromEntryNavigation)
                    .Include(s => s.EntryStaffFromEntryNavigation).ThenInclude(s => s.ToEntryNavigation)
                    .Where(s => entryIds.Contains(s.Id) && s.IsHidden != true && string.IsNullOrWhiteSpace(s.Name) == false).ToListAsync();

            var articleIds = hits.Where(s => s.Type == 1).Select(s => s.OriginalId).ToList();
            var articles = await _articleRepository.GetAll().AsNoTracking().Include(s => s.CreateUser).Where(s => articleIds.Contains(s.Id) && s.IsHidden != true && string.IsNullOrWhiteSpace(s.Name) == false).ToListAsync();

            var peripheryIds = hits.Where(s => s.Type == 2).Select(s => s.OriginalId).ToList();
            var peripheries = await _peripheryRepository.GetAll().AsNoTracking().Include(s => s.RelatedEntries).Where(s => peripheryIds.Contains(s.Id) && s.IsHidden != true && string.IsNullOrWhiteSpace(s.Name) == false).ToListAsync();

            var tagIds = hits.Where(s => s.Type == 3).Select(s => s.OriginalId).ToList();
            var tags = await _tagRepository.GetAll().AsNoTracking().Where(s => tagIds.Contains(s.Id) && s.IsHidden != true && string.IsNullOrWhiteSpace(s.Name) == false).ToListAsync();

            var videoIds = hits.Where(s => s.Type == 4).Select(s => s.OriginalId).ToList();
            var videos = await _videoRepository.GetAll().AsNoTracking().Include(s => s.CreateUser).Where(s => videoIds.Contains(s.Id) && s.IsHidden != true && string.IsNullOrWhiteSpace(s.Name) == false).ToListAsync();

            // 获取总数
            var totalCount = 0;
            if (model is PaginatedSearchResult<SearchCache> paginatedResult)
            {
                totalCount = paginatedResult.TotalHits;
            }

            var result = new PagedResultDto<SearchAloneModel>
            {
                TotalCount = totalCount,
                CurrentPage = page,
            };

            // 将真实信息处理后按顺序添加到结果中
            foreach (var item in hits)
            {
                if (item.Type == 0)
                {
                    var temp = entries.FirstOrDefault(s => s.Id == item.OriginalId);
                    if (temp != null)
                    {
                        result.Data.Add(new SearchAloneModel
                        {
                            entry = _appHelper.GetEntryInforTipViewModel(temp)
                        });
                    }
                }
                else if (item.Type == 1)
                {
                    var temp = articles.FirstOrDefault(s => s.Id == item.OriginalId);
                    if (temp != null)
                    {
                        result.Data.Add(new SearchAloneModel
                        {
                            article = _appHelper.GetArticleInforTipViewModel(temp)
                        });
                    }
                }
                else if (item.Type == 2)
                {
                    var temp = peripheries.FirstOrDefault(s => s.Id == item.OriginalId);
                    if (temp != null)
                    {
                        result.Data.Add(new SearchAloneModel
                        {
                            periphery = _appHelper.GetPeripheryInforTipViewModel(temp)
                        });
                    }
                }
                else if (item.Type == 3)
                {
                    var temp = tags.FirstOrDefault(s => s.Id == item.OriginalId);
                    if (temp != null)
                    {
                        result.Data.Add(new SearchAloneModel
                        {
                            tag = _appHelper.GetTagInforTipViewModel(temp)
                        });
                    }
                }
                else if (item.Type == 4)
                {
                    var temp = videos.FirstOrDefault(s => s.Id == item.OriginalId);
                    if (temp != null)
                    {
                        result.Data.Add(new SearchAloneModel
                        {
                            video = _appHelper.GetVideoInforTipViewModel(temp)
                        });
                    }
                }
            }

            return result;
        }

        public async Task<PagedResultDto<SearchAloneModel>> QueryAsync(SearchInputModel model)
        {
            var sortList = new List<string>();
            var filterParts = new List<string>();

            // 可排序字段白名单
            var validSortFields = new HashSet<string> { "lastEditTime", "pubulishTime", "createTime", "readerCount", "originalId" };

            // 初始化排序
            if (string.IsNullOrWhiteSpace(model.Sorting) == false)
            {
                var isAscending = !model.Sorting.Contains(" desc");
                var sortField = model.Sorting.Replace(" desc", "");
                var f = sortField[0].ToString();
                var sortString = f.ToLower() + sortField[1..^0];
                sortString = sortString.Replace("id", "originalId");

                // 仅在字段名有效时使用自定义排序，否则回退到默认
                if (validSortFields.Contains(sortString))
                {
                    sortList.Add(isAscending ? $"{sortString}:asc" : $"{sortString}:desc");
                }
                else
                {
                    _logger.LogWarning("无效的排序字段: {sortField}，使用默认排序", sortString);
                    if (string.IsNullOrWhiteSpace(model.FilterText))
                    {
                        sortList.Add("lastEditTime:desc");
                    }
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(model.FilterText))
                {
                    sortList.Add("lastEditTime:desc");
                }
                // 有搜索词时不指定排序，使用 Meilisearch 默认的相关性排序
            }


            // 筛选时间
            if (model.Times.Any())
            {
                var timeParts = new List<string>();
                foreach (var item in model.Times)
                {
                    timeParts.Add($"(pubulishTime >= {item.AfterTime.ToUnixTimeMilliseconds()} AND pubulishTime <= {item.BeforeTime.ToUnixTimeMilliseconds()})");
                }
                filterParts.Add($"({string.Join(" OR ", timeParts)})");
            }

            // 筛选类别
            if (model.Types.Any())
            {
                var types = new List<int>();

                foreach (var item in model.Types)
                {
                    if (item == SearchType.Entry || item == SearchType.Article || item == SearchType.Periphery || item == SearchType.Tag || item == SearchType.Video)
                    {
                        types.AddRange(item.ToTypeList());
                    }
                    else
                    {
                        types.Add((int)item);
                    }
                }

                types = types.Distinct().ToList();

                if (types.Count > 0)
                {
                    filterParts.Add($"originalType IN [{string.Join(", ", types)}]");
                }
            }

            // 构建搜索查询
            var searchQuery = new SearchQuery
            {
                Q = model.FilterText ?? "",
                HitsPerPage = model.MaxResultCount,
                Page = model.CurrentPage,
            };

            if (sortList.Any())
            {
                searchQuery.Sort = sortList;
            }

            if (filterParts.Any())
            {
                searchQuery.Filter = string.Join(" AND ", filterParts);
            }

            // 进行搜索
            var index = _client.Index(_indexName);
            var searchResult = await index.SearchAsync<SearchCache>(searchQuery.Q, searchQuery);

            var result = await ProcSearchResult(searchResult, model.CurrentPage);
            result.MaxResultCount = model.MaxResultCount;

            return result;
        }
    }
}
