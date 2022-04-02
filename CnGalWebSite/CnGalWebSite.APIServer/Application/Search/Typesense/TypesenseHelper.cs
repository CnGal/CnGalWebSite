using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.Application.Search;
using CnGalWebSite.APIServer.Application.Search.ElasticSearches;
using CnGalWebSite.APIServer.Application.Search.Typesense;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.DataModel.Application.Dtos;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Home;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
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
        private readonly IRepository<SearchCache, long> _searchCacheRepository;

        private readonly IAppHelper _appHelper;
        private readonly string _collectionName = "SearchCache";

        public TypesenseHelper(ITypesenseProvider typesenseProvider, IRepository<Entry, int> entryRepository, IRepository<SearchCache, long> searchCacheRepository,
        IRepository<Tag, int> tagRepository, IRepository<Article, long> articleRepository, IRepository<Periphery, long> peripheryRepository, IAppHelper appHelper)
        {
            _typesenseClient = typesenseProvider.GetClient();
            _entryRepository = entryRepository;

            _tagRepository = tagRepository;
            _peripheryRepository = peripheryRepository;
            _articleRepository = articleRepository;
            _appHelper = appHelper;
            _searchCacheRepository = searchCacheRepository;
        }

        public async Task UpdateDataToSearchService(DateTime LastUpdateTime)
        {
            await CreateCollection();
            await UpdateEntries(LastUpdateTime);
            await UpdateArticles(LastUpdateTime);
            await UpdatePeripheries(LastUpdateTime);
            await UpdateTags(LastUpdateTime);
        }

        private async Task CreateCollection()
        {
            var schema = new Schema(
              _collectionName,
              new List<Field>
              {
                    new Field("id", FieldType.String, false),
                    new Field("originalId", FieldType.Int64, false),
                    new Field("name", FieldType.String, false, true),
                    new Field("displayName", FieldType.String, false, true),
                    new Field("anotherName", FieldType.String, false, true),
                    new Field("briefIntroduction", FieldType.String, false),
                    new Field("mainPage", FieldType.String, false),
                    new Field("lastEditTime", FieldType.Int64, false),
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

            }
        }

        private async Task UpdateEntries(DateTime LastUpdateTime)
        {
            var entries = await _entryRepository.GetAll().AsNoTracking()
                .Where(s => s.LastEditTime > LastUpdateTime).ToListAsync();

            if(entries.Any()==false)
            {
                return;
            }

            var entryIds = entries.Select(s => (long)s.Id).ToList();

            var documents = await _searchCacheRepository.GetAll().Where(s => s.Type == 0 && entryIds.Contains(s.OriginalId)).ToListAsync();

            foreach (var item in entries)
            {
                var temp = documents.FirstOrDefault(s => s.OriginalId == item.Id);
                if (temp == null)
                {
                    temp = new SearchCache();
                    temp.Copy(item);

                    temp = await _searchCacheRepository.InsertAsync(temp);
                    documents.Add(temp);
                }
                else
                {
                    temp.Copy(item);
                }

            }
            await _typesenseClient.ImportDocuments(_collectionName, documents, documents.Count, ImportType.Upsert);

            var deleted = await _entryRepository.GetAll().Where(s => s.IsHidden || string.IsNullOrWhiteSpace(s.Name)).Select(s => (long)s.Id).ToListAsync();
            documents = await _searchCacheRepository.GetAll().Where(s => s.Type == 0 && deleted.Contains(s.OriginalId)).ToListAsync();
            foreach (var item in documents)
            {
                try
                {
                await _typesenseClient.DeleteDocument<SearchCache>(_collectionName, item.Id.ToString());

                }
                catch
                {

                }
            }
            await _searchCacheRepository.DeleteRangeAsync(s => deleted.Contains(s.OriginalId));
        }

        private async Task UpdateArticles(DateTime LastUpdateTime)
        {
            var entries = await _articleRepository.GetAll().AsNoTracking()
                .Where(s => s.LastEditTime > LastUpdateTime).ToListAsync();

            if (entries.Any() == false)
            {
                return;
            }
            var entryIds = entries.Select(s => (long)s.Id).ToList();

            var documents = await _searchCacheRepository.GetAll().Where(s => s.Type == 1 && entryIds.Contains(s.OriginalId)).ToListAsync();

            foreach (var item in entries)
            {
                var temp = documents.FirstOrDefault(s => s.OriginalId == item.Id);
                if (temp == null)
                {
                    temp = new SearchCache();
                    temp.Copy(item);

                    temp = await _searchCacheRepository.InsertAsync(temp);
                    documents.Add(temp);
                }
                else
                {
                    temp.Copy(item);
                }

            }
            var result = await _typesenseClient.ImportDocuments(_collectionName, documents, documents.Count, ImportType.Upsert);

            var deleted = await _articleRepository.GetAll().Where(s => s.IsHidden || string.IsNullOrWhiteSpace(s.Name)).Select(s => (long)s.Id).ToListAsync();
            documents = await _searchCacheRepository.GetAll().Where(s => s.Type == 1 && deleted.Contains(s.OriginalId)).ToListAsync();
            foreach (var item in documents)
            {
                try
                {
                    await _typesenseClient.DeleteDocument<SearchCache>(_collectionName, item.Id.ToString());
                }
                catch
                {

                }
            }
            await _searchCacheRepository.DeleteRangeAsync(s => deleted.Contains(s.OriginalId));
        }

        private async Task UpdatePeripheries(DateTime LastUpdateTime)
        {
            var entries = await _peripheryRepository.GetAll().AsNoTracking()
                .Where(s => s.LastEditTime > LastUpdateTime).ToListAsync();
            if (entries.Any() == false)
            {
                return;
            }
            var entryIds = entries.Select(s => (long)s.Id).ToList();

            var documents = await _searchCacheRepository.GetAll().Where(s => s.Type == 2 && entryIds.Contains(s.OriginalId)).ToListAsync();

            foreach (var item in entries)
            {
                var temp = documents.FirstOrDefault(s => s.OriginalId == item.Id);
                if (temp == null)
                {
                    temp = new SearchCache();
                    temp.Copy(item);

                    temp = await _searchCacheRepository.InsertAsync(temp);
                    documents.Add(temp);
                }
                else
                {
                    temp.Copy(item);
                }

            }
            var result = await _typesenseClient.ImportDocuments(_collectionName, documents, documents.Count, ImportType.Upsert);

            var deleted = await _peripheryRepository.GetAll().Where(s => s.IsHidden || string.IsNullOrWhiteSpace(s.Name)).Select(s => (long)s.Id).ToListAsync();
            documents = await _searchCacheRepository.GetAll().Where(s => s.Type == 2 && deleted.Contains(s.OriginalId)).ToListAsync();
            foreach (var item in documents)
            {
                try
                {
                    await _typesenseClient.DeleteDocument<SearchCache>(_collectionName, item.Id.ToString());

                }
                catch
                {

                }
            }
            await _searchCacheRepository.DeleteRangeAsync(s => deleted.Contains(s.OriginalId));
        }

        private async Task UpdateTags(DateTime LastUpdateTime)
        {
            var entries = await _tagRepository.GetAll().AsNoTracking()
                .Where(s => s.LastEditTime > LastUpdateTime).ToListAsync();
            if (entries.Any() == false)
            {
                return;
            }
            var entryIds = entries.Select(s => (long)s.Id).ToList();

            var documents = await _searchCacheRepository.GetAll().Where(s => s.Type == 3 && entryIds.Contains(s.OriginalId)).ToListAsync();

            foreach (var item in entries)
            {
                var temp = documents.FirstOrDefault(s => s.OriginalId == item.Id);
                if (temp == null)
                {
                    temp = new SearchCache();
                    temp.Copy(item);

                    temp = await _searchCacheRepository.InsertAsync(temp);
                    documents.Add(temp);
                }
                else
                {
                    temp.Copy(item);
                }

            }
            await _typesenseClient.ImportDocuments(_collectionName, documents, documents.Count, ImportType.Upsert);

            var deleted = await _tagRepository.GetAll().Where(s => s.IsHidden || string.IsNullOrWhiteSpace(s.Name)).Select(s => (long)s.Id).ToListAsync();
            documents = await _searchCacheRepository.GetAll().Where(s => s.Type == 3 && deleted.Contains(s.OriginalId)).ToListAsync();
            foreach (var item in documents)
            {
                try
                {
                    await _typesenseClient.DeleteDocument<SearchCache>(_collectionName, item.Id.ToString());

                }
                catch
                {

                }
            }
            await _searchCacheRepository.DeleteRangeAsync(s => deleted.Contains(s.OriginalId));
        }

        public async Task DeleteDataOfSearchService()
        {
            try
            {
                await _typesenseClient.DeleteCollection(_collectionName);
                //await _searchCacheRepository.DeleteRangeAsync(s => true);
            }
            catch
            {

            }
        }


        public async Task<PagedResultDto<SearchAloneModel>> QueryAsync(int page, int limit, string text, string screeningConditions, string sort, QueryType type)
        {
            string sortString = "";
            string filterString = "";
            bool isAscending = false;

           

            if (string.IsNullOrWhiteSpace(sort) == false)
            {
                if (sort == "Default")
                {
                    if (string.IsNullOrWhiteSpace(text))
                    {
                        sortString = "lastEditTime";
                    }
                    else
                    {
                        sortString = "_text_match";
                    }
                }
                else
                {
                    var temp = sort.Split(' ');
                    var f = temp[0][0].ToString();
                    sortString = f.ToLower() + temp[0][1..^0];
                    if (temp.Length == 1)
                    {
                        isAscending = true;
                    }
                }
            }

            sortString=sortString.Replace("id", "originalId");
            if (isAscending == false)
            {
                sortString += ":desc";
            }
            else
            {
                sortString += ":asc";
            }

            var query = new SearchParameters(text, "name,displayName,anotherName,briefIntroduction,mainPage");
            if (string.IsNullOrWhiteSpace(sortString) == false)
            {
                query.SortBy = sortString;
            }
            query.PerPage = limit.ToString();
            query.Page = page.ToString();

            filterString = screeningConditions switch
            {
                "词条" => "type:=0",
                "文章" => "type:=1",
                "周边" => "type:=2",
                "标签" => "type:=3",
                "游戏" => "type:=0 && originalType:=0",
                "角色" => "type:=0 && originalType:=1",
                "STAFF" => "type:=0 && originalType:=3",
                "制作组" => "type:=0 && originalType:=2",
                "感想" => "type:=1 && originalType:=0",
                "访谈" => "type:=1 && originalType:=2",
                "攻略" => "type:=1 && originalType:=1",
                "动态" => "type:=1 && originalType:=3",
                "评测" => "type:=1 && originalType:=4",
                "周边文章" => "type:=1 && originalType:=5",
                "公告" => "type:=1 && originalType:=6",
                "杂谈" => "type:=1 && originalType:=7",
                "二创" => "type:=1 && originalType:=8",
                "设定集或画册等" => "type:=2 && originalType:=0",
                "原声集" => "type:=2 && originalType:=1",
                "套装" => "type:=2 && originalType:=2",
                "其他周边" => "type:=2 && originalType:=3",
                _ => ""
            };

            if (string.IsNullOrWhiteSpace(filterString) == false)
            {
                query.FilterBy = filterString;
            }

            var searchResult = await _typesenseClient.Search<SearchCache>(_collectionName, query);

            var model = await ProcSearchResult(searchResult);
            model.FilterText = text;
            model.MaxResultCount = limit;
            model.FilterText = text;
            model.ScreeningConditions = screeningConditions;

            model.Sorting = sort;
            return model;
        }

        private async Task<PagedResultDto<SearchAloneModel>> ProcSearchResult(SearchResult<SearchCache> model)
        {
            //根据查询结果向数据库获取真实信息

            var entryIds = model.Hits.Where(s => s.Document.Type == 0).Select(s => s.Document.OriginalId).ToList();

            var entries = await _entryRepository.GetAll().AsNoTracking().Include(s => s.Information)
                    .Include(s => s.EntryRelationFromEntryNavigation).ThenInclude(s => s.ToEntryNavigation).ThenInclude(s => s.Information).ThenInclude(s => s.Additional)
                    .Include(s => s.EntryRelationFromEntryNavigation).ThenInclude(s => s.ToEntryNavigation).ThenInclude(s => s.EntryRelationFromEntryNavigation).ThenInclude(s => s.ToEntryNavigation)
                .Where(s => entryIds.Contains(s.Id) && s.IsHidden != true && string.IsNullOrWhiteSpace(s.Name) == false).ToListAsync();

            var articleIds = model.Hits.Where(s => s.Document.Type == 1).Select(s => s.Document.OriginalId).ToList();

            var articles = await _articleRepository.GetAll().AsNoTracking().Include(s => s.CreateUser).Where(s => articleIds.Contains(s.Id) && s.IsHidden != true && string.IsNullOrWhiteSpace(s.Name) == false).ToListAsync();

            var peripheryIds = model.Hits.Where(s => s.Document.Type == 2).Select(s => s.Document.OriginalId).ToList();

            var peripheries = await _peripheryRepository.GetAll().AsNoTracking().Include(s => s.RelatedEntries).Where(s => peripheryIds.Contains(s.Id) && s.IsHidden != true && string.IsNullOrWhiteSpace(s.Name) == false).ToListAsync();

            var tagIds = model.Hits.Where(s => s.Document.Type == 3).Select(s => s.Document.OriginalId).ToList();

            var tags = await _tagRepository.GetAll().AsNoTracking().Where(s => tagIds.Contains(s.Id) && s.IsHidden != true && string.IsNullOrWhiteSpace(s.Name) == false).ToListAsync();



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
                    result.Data.Add(new SearchAloneModel
                    {
                        entry = await _appHelper.GetEntryInforTipViewModel(entries.FirstOrDefault(s => s.Id == item.Document.OriginalId))
                    });
                }
                else if (item.Document.Type == 1)
                {
                    result.Data.Add(new SearchAloneModel
                    {
                        article = _appHelper.GetArticleInforTipViewModel(articles.FirstOrDefault(s => s.Id == item.Document.OriginalId))
                    });
                }
               
                else if (item.Document.Type == 2)
                {
                    result.Data.Add(new SearchAloneModel
                    {
                        periphery = _appHelper.GetPeripheryInforTipViewModel(peripheries.FirstOrDefault(s => s.Id == item.Document.OriginalId))
                    });
                }
                else if (item.Document.Type == 3)
                {
                    result.Data.Add(new SearchAloneModel
                    {
                        tag = _appHelper.GetTagInforTipViewModel(tags.FirstOrDefault(s => s.Id == item.Document.OriginalId))
                    });
                }
            }


            return result;
        }
    }
}
