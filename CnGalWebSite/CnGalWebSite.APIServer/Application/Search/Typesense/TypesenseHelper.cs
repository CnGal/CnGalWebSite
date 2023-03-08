using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.Application.Search;
using CnGalWebSite.APIServer.Controllers;
using CnGalWebSite.APIServer.CustomMiddlewares;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.APIServer.Model;

using CnGalWebSite.DataModel.Application.Search.Dtos;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Search;
using CnGalWebSite.Helper.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;
using Typesense;

namespace CnGalWebSite.APIServer.Application.Typesense
{
    public class TypesenseHelper : ISearchHelper
    {
        private readonly ITypesenseClient _typesenseClient;
        private readonly IRepository<Entry, int> _entryRepository;
        private readonly IRepository<Tag, int> _tagRepository;
        private readonly IRepository<Article, long> _articleRepository;
        private readonly IRepository<Periphery, long> _peripheryRepository;
        private readonly IRepository<Video, long> _videoRepository;
        private readonly IRepository<SearchCache, long> _searchCacheRepository;
        private readonly ILogger<TypesenseHelper> _logger;
        private readonly IAppHelper _appHelper;
        private readonly string _collectionName = "SearchCache";
        private readonly int _preMaxCount = 100;

        public TypesenseHelper(ITypesenseClient typesenseClient, IRepository<Entry, int> entryRepository, IRepository<SearchCache, long> searchCacheRepository, IRepository<Video, long> videoRepository, ILogger<TypesenseHelper> logger,
        IRepository<Tag, int> tagRepository, IRepository<Article, long> articleRepository, IRepository<Periphery, long> peripheryRepository, IAppHelper appHelper)
        {
            _typesenseClient = typesenseClient;
            _entryRepository = entryRepository;

            _tagRepository = tagRepository;
            _peripheryRepository = peripheryRepository;
            _articleRepository = articleRepository;
            _appHelper = appHelper;
            _searchCacheRepository = searchCacheRepository;
            _videoRepository = videoRepository;

            _logger = logger;
        }

        public async Task UpdateDataToSearchService(DateTime LastUpdateTime, bool updateAll = false)
        {
            await CreateCollection();
            await UpdateEntries(LastUpdateTime, updateAll);
            await UpdateArticles(LastUpdateTime, updateAll);
            await UpdatePeripheries(LastUpdateTime, updateAll);
            await UpdateTags(LastUpdateTime, updateAll);
            await UpdateVideos(LastUpdateTime, updateAll);

            _logger.LogInformation("更新搜索数据完成");
        }

        private async Task CreateCollection()
        {
            var schema = new Schema(
              _collectionName,
              new List<Field>
              {
                    new Field("id", FieldType.String, false),
                    new Field("originalId", FieldType.Int64, false),
                    new Field("name", FieldType.String, false, true,locale:"zh"),
                    new Field("displayName", FieldType.String, false, true,locale:"zh"),
                    new Field("anotherName", FieldType.String, false, true,locale:"zh"),
                    new Field("briefIntroduction", FieldType.String, false,locale:"zh"),
                    new Field("mainPage", FieldType.String, false,locale:"zh"),
                    new Field("lastEditTime", FieldType.Int64, false),
                    new Field("pubulishTime", FieldType.Int64, false),
                    new Field("createTime", FieldType.Int64, false),
                    new Field("readerCount", FieldType.Int32, false),
                    new Field("type", FieldType.Int32, false),
                    new Field("originalType", FieldType.Int32, false),
              });
            try
            {
                var createCollectionResponse = await _typesenseClient.CreateCollection(schema);
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "创建集合失败");
            }
        }

        private async Task UpdateEntries(DateTime LastUpdateTime, bool updateAll = false)
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

                //将数据发送到Typesence
                var result = await _typesenseClient.ImportDocuments(_collectionName, documents, documents.Count, ImportType.Upsert);

                var errors = result.Where(s => s.Success == false);
                foreach (var item in errors)
                {
                    Console.WriteLine(item.Error);
                }
            }


            var deleted = await _entryRepository.GetAll().Where(s => s.IsHidden || string.IsNullOrWhiteSpace(s.Name)).Select(s => (long)s.Id).ToListAsync();
            documents = await _searchCacheRepository.GetAll().Where(s => s.Type == 0 && deleted.Contains(s.OriginalId)).ToListAsync();
            foreach (var item in documents)
            {
                try
                {
                    await _typesenseClient.DeleteDocument<SearchCache>(_collectionName, item.Id);
                }
                catch
                {

                }
            }
            if (documents.Count != 0)
            {
                await _searchCacheRepository.GetAll().Where(s => s.Type == 0 && deleted.Contains(s.OriginalId)).ExecuteDeleteAsync();

            }
        }

        private async Task UpdateArticles(DateTime LastUpdateTime, bool updateAll = false)
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
                //将数据发送到Typesence
                var result = await _typesenseClient.ImportDocuments(_collectionName, documents, documents.Count, ImportType.Upsert);

                var errors = result.Where(s => s.Success == false);
                foreach (var item in errors)
                {
                    Console.WriteLine(item.Error);
                }
            }

            var deleted = await _articleRepository.GetAll().Where(s => s.IsHidden || string.IsNullOrWhiteSpace(s.Name)).Select(s => s.Id).ToListAsync();
            documents = await _searchCacheRepository.GetAll().Where(s => s.Type == 1 && deleted.Contains(s.OriginalId)).ToListAsync();
            foreach (var item in documents)
            {
                try
                {
                    await _typesenseClient.DeleteDocument<SearchCache>(_collectionName, item.Id);
                }
                catch
                {

                }
            }
            if (documents.Count != 0)
            {
                await _searchCacheRepository.GetAll().Where(s => s.Type == 1 && deleted.Contains(s.OriginalId)).ExecuteDeleteAsync();

            }
        }

        private async Task UpdateVideos(DateTime LastUpdateTime, bool updateAll = false)
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
                //将数据发送到Typesence
                var result = await _typesenseClient.ImportDocuments(_collectionName, documents, documents.Count, ImportType.Upsert);

                var errors = result.Where(s => s.Success == false);
                foreach (var item in errors)
                {
                    Console.WriteLine(item.Error);
                }
            }

            var deleted = await _videoRepository.GetAll().Where(s => s.IsHidden || string.IsNullOrWhiteSpace(s.Name)).Select(s => s.Id).ToListAsync();
            documents = await _searchCacheRepository.GetAll().Where(s => s.Type == 4 && deleted.Contains(s.OriginalId)).ToListAsync();
            foreach (var item in documents)
            {
                try
                {
                    await _typesenseClient.DeleteDocument<SearchCache>(_collectionName, item.Id);
                }
                catch
                {

                }
            }
            if (documents.Count != 0)
            {
                await _searchCacheRepository.GetAll().Where(s => s.Type == 1 && deleted.Contains(s.OriginalId)).ExecuteDeleteAsync();

            }
        }

        private async Task UpdatePeripheries(DateTime LastUpdateTime, bool updateAll = false)
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
                //将数据发送到Typesence
                var result = await _typesenseClient.ImportDocuments(_collectionName, documents, documents.Count, ImportType.Upsert);

                var errors = result.Where(s => s.Success == false);
                foreach (var item in errors)
                {
                    Console.WriteLine(item.Error);
                }
            }

            var deleted = await _peripheryRepository.GetAll().Where(s => s.IsHidden || string.IsNullOrWhiteSpace(s.Name)).Select(s => s.Id).ToListAsync();
            documents = await _searchCacheRepository.GetAll().Where(s => s.Type == 2 && deleted.Contains(s.OriginalId)).ToListAsync();
            foreach (var item in documents)
            {
                try
                {
                    await _typesenseClient.DeleteDocument<SearchCache>(_collectionName, item.Id);

                }
                catch
                {

                }
            }
            if (documents.Count != 0)
            {
                await _searchCacheRepository.GetAll().Where(s => s.Type == 2 && deleted.Contains(s.OriginalId)).ExecuteDeleteAsync();

            }
        }

        private async Task UpdateTags(DateTime LastUpdateTime, bool updateAll = false)
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
                //将数据发送到Typesence
                var result = await _typesenseClient.ImportDocuments(_collectionName, documents, documents.Count, ImportType.Upsert);

                var errors = result.Where(s => s.Success == false);
                foreach (var item in errors)
                {
                    Console.WriteLine(item.Error);
                }
            }


            var deleted = await _tagRepository.GetAll().Where(s => s.IsHidden || string.IsNullOrWhiteSpace(s.Name)).Select(s => (long)s.Id).ToListAsync();
            documents = await _searchCacheRepository.GetAll().Where(s => s.Type == 3 && deleted.Contains(s.OriginalId)).ToListAsync();
            foreach (var item in documents)
            {
                try
                {
                    await _typesenseClient.DeleteDocument<SearchCache>(_collectionName, item.Id);

                }
                catch
                {

                }
            }
            if (documents.Count != 0)
            {
                await _searchCacheRepository.GetAll().Where(s => s.Type == 3 && deleted.Contains(s.OriginalId)).ExecuteDeleteAsync();

            }
        }

        public async Task DeleteDataOfSearchService()
        {
            try
            {
                await _typesenseClient.DeleteCollection(_collectionName);
                //await _searchCacheRepository.GetAll().Where(s => true).ExecuteDeleteAsync();
            }
            catch
            {

            }
        }

        private async Task<PagedResultDto<SearchAloneModel>> ProcSearchResult(SearchResult<SearchCache> model)
        {
            //根据查询结果向数据库获取真实信息

            var entryIds = model.Hits.Where(s => s.Document.Type == 0).Select(s => s.Document.OriginalId).ToList();

            var entries = await _entryRepository.GetAll().AsNoTracking().Include(s => s.Information)
                    .Include(s => s.EntryRelationFromEntryNavigation).ThenInclude(s => s.ToEntryNavigation)
                    .Include(s => s.EntryStaffToEntryNavigation).ThenInclude(s => s.FromEntryNavigation)
                    .Include(s => s.EntryStaffFromEntryNavigation).ThenInclude(s => s.ToEntryNavigation)
                    .Where(s => entryIds.Contains(s.Id) && s.IsHidden != true && string.IsNullOrWhiteSpace(s.Name) == false).ToListAsync();

            var articleIds = model.Hits.Where(s => s.Document.Type == 1).Select(s => s.Document.OriginalId).ToList();

            var articles = await _articleRepository.GetAll().AsNoTracking().Include(s => s.CreateUser).Where(s => articleIds.Contains(s.Id) && s.IsHidden != true && string.IsNullOrWhiteSpace(s.Name) == false).ToListAsync();

            var peripheryIds = model.Hits.Where(s => s.Document.Type == 2).Select(s => s.Document.OriginalId).ToList();

            var peripheries = await _peripheryRepository.GetAll().AsNoTracking().Include(s => s.RelatedEntries).Where(s => peripheryIds.Contains(s.Id) && s.IsHidden != true && string.IsNullOrWhiteSpace(s.Name) == false).ToListAsync();

            var tagIds = model.Hits.Where(s => s.Document.Type == 3).Select(s => s.Document.OriginalId).ToList();

            var tags = await _tagRepository.GetAll().AsNoTracking().Where(s => tagIds.Contains(s.Id) && s.IsHidden != true && string.IsNullOrWhiteSpace(s.Name) == false).ToListAsync();

            var videoIds = model.Hits.Where(s => s.Document.Type == 4).Select(s => s.Document.OriginalId).ToList();

            var videos = await _videoRepository.GetAll().AsNoTracking().Include(s => s.CreateUser).Where(s => videoIds.Contains(s.Id) && s.IsHidden != true && string.IsNullOrWhiteSpace(s.Name) == false).ToListAsync();



            var result = new PagedResultDto<SearchAloneModel>
            {
                TotalCount = model.Found,
                CurrentPage = model.Page,
            };

            //将真实信息处理后按顺序添加到结果中
            foreach (var item in model.Hits)
            {
                if (item.Document.Type == 0)
                {
                    var temp = entries.FirstOrDefault(s => s.Id == item.Document.OriginalId);
                    if (temp != null)
                    {
                        result.Data.Add(new SearchAloneModel
                        {
                            entry = _appHelper.GetEntryInforTipViewModel(temp)
                        });
                    }

                }
                else if (item.Document.Type == 1)
                {
                    var temp = articles.FirstOrDefault(s => s.Id == item.Document.OriginalId);
                    if (temp != null)
                    {
                        result.Data.Add(new SearchAloneModel
                        {
                            article = _appHelper.GetArticleInforTipViewModel(temp)
                        });
                    }

                }

                else if (item.Document.Type == 2)
                {
                    var temp = peripheries.FirstOrDefault(s => s.Id == item.Document.OriginalId);
                    if (temp != null)
                    {
                        result.Data.Add(new SearchAloneModel
                        {
                            periphery = _appHelper.GetPeripheryInforTipViewModel(temp)
                        });
                    }

                }
                else if (item.Document.Type == 3)
                {
                    var temp = tags.FirstOrDefault(s => s.Id == item.Document.OriginalId);
                    if (temp != null)
                    {
                        result.Data.Add(new SearchAloneModel
                        {
                            tag = _appHelper.GetTagInforTipViewModel(temp)
                        });
                    }

                }
                else if (item.Document.Type == 4)
                {
                    var temp = videos.FirstOrDefault(s => s.Id == item.Document.OriginalId);
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
            var sortString = "";
            var filterString = new StringBuilder();
            var isAscending = !model.Sorting.Contains(" desc");
            model.Sorting = model.Sorting.Replace(" desc", "");

            //初始化排序
            if (string.IsNullOrWhiteSpace(model.Sorting) == false)
            {
                var f = model.Sorting[0].ToString();
                sortString = f.ToLower() + model.Sorting[1..^0];
            }
            else
            {
                if (string.IsNullOrWhiteSpace(model.FilterText))
                {
                    sortString = "lastEditTime";
                    isAscending = false;
                }
                else
                {
                    sortString = "_text_match";
                }
            }

            sortString = sortString.Replace("id", "originalId");
            if (isAscending == false)
            {
                sortString += ":desc";
            }
            else
            {
                sortString += ":asc";
            }

            //设置搜索字段
            var query = new SearchParameters(model.FilterText, "name,displayName,anotherName,briefIntroduction,mainPage");
            //设置排序
            if (string.IsNullOrWhiteSpace(sortString) == false)
            {
                query.SortBy = sortString;
            }

            //页数
            query.PerPage = model.MaxResultCount;
            query.Page = model.CurrentPage;

            //筛选时间
            if (model.Times.Any())
            {
                filterString.Append("(");
                foreach (var item in model.Times)
                {
                    if (model.Times.IndexOf(item)!=0)
                    {
                        filterString.Append(" || ");
                    }
                    filterString.Append($"pubulishTime: [{item.AfterTime.ToUnixTimeMilliseconds()}..{item.BeforeTime.ToUnixTimeMilliseconds()}]");
                }
                filterString.Append(")");
            }

            //筛选类别
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
                    if (filterString.Length != 0)
                    {
                        filterString.Append(" && ");
                    }
                    filterString.Append($"originalType: [");
                    foreach (var item in types)
                    {
                        filterString.Append(item);

                        if (types.IndexOf(item) != types.Count - 1)
                        {
                            filterString.Append(", ");
                        }
                    }
                    filterString.Append(']');
                }
            }


            if (filterString.Length != 0)
            {
                query.FilterBy = filterString.ToString();
            }

            //进行搜索
            var searchResult = await _typesenseClient.Search<SearchCache>(_collectionName, query);

            var result = await ProcSearchResult(searchResult);

            result.MaxResultCount = model.MaxResultCount;

            return result;
        }
    }
}
