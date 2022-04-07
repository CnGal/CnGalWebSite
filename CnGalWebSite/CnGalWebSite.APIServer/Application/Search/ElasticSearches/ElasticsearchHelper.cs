using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.DataModel.Application.Dtos;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Home;
using Microsoft.EntityFrameworkCore;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.Search.ElasticSearches
{
    public class ElasticsearchHelper : ISearchHelper
    {
        private readonly IElasticClient _elasticClient;
        private readonly IRepository<Entry, int> _entryRepository;
        private readonly IRepository<Tag, int> _tagRepository;
        private readonly IRepository<Article, int> _articleRepository;
        private readonly IRepository<Periphery, int> _peripheryRepository;
        private readonly IElasticsearchBaseService<Entry> _entryElasticsearchBaseService;
        private readonly IElasticsearchBaseService<Tag> _tagElasticsearchBaseService;
        private readonly IElasticsearchBaseService<Article> _articleElasticsearchBaseService;
        private readonly IElasticsearchBaseService<Periphery> _peripheryElasticsearchBaseService;
        private readonly IAppHelper _appHelper;

        public ElasticsearchHelper(IElasticsearchProvider esClientProvider, IRepository<Entry, int> entryRepository, IElasticsearchBaseService<Entry> entryElasticsearchBaseService,
            IRepository<Tag, int> tagRepository, IRepository<Article, int> articleRepository, IRepository<Periphery, int> peripheryRepository, IElasticsearchBaseService<Tag> tagElasticsearchBaseService,
            IElasticsearchBaseService<Article> articleElasticsearchBaseService, IElasticsearchBaseService<Periphery> peripheryElasticsearchBaseService, IAppHelper appHelper)
        {
            _elasticClient = esClientProvider.GetClient();
            _entryRepository = entryRepository;
            _entryElasticsearchBaseService = entryElasticsearchBaseService;
            _peripheryElasticsearchBaseService = peripheryElasticsearchBaseService;
            _tagElasticsearchBaseService = tagElasticsearchBaseService;
            _articleElasticsearchBaseService = articleElasticsearchBaseService;
            _tagRepository = tagRepository;
            _peripheryRepository = peripheryRepository;
            _articleRepository = articleRepository;
            _appHelper = appHelper;
        }

        public async Task UpdateDataToSearchService(DateTime LastUpdateTime, bool updateAll = false)
        {
            await UpdateEntryDataToElasticsearch(LastUpdateTime);
            await UpdateArticleDataToElasticsearch(LastUpdateTime);
            await UpdateTagDataToElasticsearch(LastUpdateTime);
            await UpdatePeripheryDataToElasticsearch(LastUpdateTime);
        }

        public async Task UpdateEntryDataToElasticsearch(DateTime LastUpdateTime, bool updateAll = false)
        {
            var entries = await _entryRepository.GetAll().AsNoTracking()
                .Where(s => s.LastEditTime > LastUpdateTime||updateAll).ToListAsync();
            if (entries.Count != 0)
            {
                await _entryElasticsearchBaseService.InsertRangeAsync(entries);
            }

            var deleted = await _entryRepository.GetAll().Where(s => s.IsHidden || string.IsNullOrWhiteSpace(s.Name)).Select(s => s.Id).ToListAsync();
            foreach (var item in deleted)
            {
                await _entryElasticsearchBaseService.DeleteAsync(item.ToString());
            }
        }

        public async Task UpdateArticleDataToElasticsearch(DateTime LastUpdateTime, bool updateAll = false)
        {
            var entries = await _articleRepository.GetAll().AsNoTracking().Where(s => s.LastEditTime > LastUpdateTime || updateAll).ToListAsync();
            if (entries.Count != 0)
            {
                await _articleElasticsearchBaseService.InsertRangeAsync(entries);

            }

            var deleted = await _articleRepository.GetAll().Where(s => s.IsHidden || string.IsNullOrWhiteSpace(s.Name)).Select(s => s.Id).ToListAsync();
            foreach (var item in deleted)
            {
                await _articleElasticsearchBaseService.DeleteAsync(item.ToString());
            }
        }

        public async Task UpdateTagDataToElasticsearch(DateTime LastUpdateTime, bool updateAll = false)
        {
            var entries = await _tagRepository.GetAll().AsNoTracking().Where(s => s.LastEditTime > LastUpdateTime || updateAll).ToListAsync();
            if (entries.Count != 0)
            {
                await _tagElasticsearchBaseService.InsertRangeAsync(entries);
            }

            var deleted = await _tagRepository.GetAll().Where(s => s.IsHidden || string.IsNullOrWhiteSpace(s.Name)).Select(s => s.Id).ToListAsync();
            foreach (var item in deleted)
            {
                await _tagElasticsearchBaseService.DeleteAsync(item.ToString());
            }
        }

        public async Task UpdatePeripheryDataToElasticsearch(DateTime LastUpdateTime, bool updateAll = false)
        {
            var entries = await _peripheryRepository.GetAll().AsNoTracking().Where(s => s.LastEditTime > LastUpdateTime || updateAll).ToListAsync();
            if (entries.Count != 0)
            {
                await _peripheryElasticsearchBaseService.InsertRangeAsync(entries);

            }

            var deleted = await _peripheryRepository.GetAll().Where(s => s.IsHidden || string.IsNullOrWhiteSpace(s.Name)).Select(s => s.Id).ToListAsync();
            foreach (var item in deleted)
            {
                await _peripheryElasticsearchBaseService.DeleteAsync(item.ToString());
            }
        }

        public async Task DeleteDataOfSearchService()
        {
            await _peripheryElasticsearchBaseService.RemoveIndex();
            await _articleElasticsearchBaseService.RemoveIndex();
            await _entryElasticsearchBaseService.RemoveIndex();
            await _tagElasticsearchBaseService.RemoveIndex();
        }


        public async Task<PagedResultDto<SearchAloneModel>> QueryAsync(int page, int limit, string text, string screeningConditions, string sort, QueryType type)
        {
            var sortString = "id";
            var order = SortOrder.Descending;
            if (string.IsNullOrWhiteSpace(sort) != true)
            {
                if (sort == "Default")
                {
                    if (string.IsNullOrWhiteSpace(text))
                    {
                        sortString = "lastEditTime";
                    }
                    else
                    {
                        sortString = "";
                    }
                }
                else
                {
                    var temp = sort.Split(' ');
                    var f = temp[0][0].ToString();
                    sortString = f.ToLower() + temp[0][1..^0];
                    if (temp.Length == 1)
                    {
                        order = SortOrder.Ascending;
                    }
                }

            }

            PagedResultDto<SearchAloneModel> model = null;

            if (screeningConditions == "词条" || screeningConditions == "游戏" || screeningConditions == "角色" || screeningConditions == "STAFF" || screeningConditions == "制作组")
            {
                model = await QueryEntryAsync(page, limit, text, screeningConditions, sortString, order, type);
            }
            else if (screeningConditions == "文章" || screeningConditions == "感想" || screeningConditions == "访谈" || screeningConditions == "攻略" || screeningConditions == "动态" || screeningConditions == "评测" ||
                screeningConditions == "文章周边" || screeningConditions == "公告" || screeningConditions == "杂谈" || screeningConditions == "二创")
            {
                model = await QueryArticleAsync(page, limit, text, screeningConditions, sortString, order, type);
            }
            else if (screeningConditions == "周边" || screeningConditions == "设定集或画册等" || screeningConditions == "原声集" || screeningConditions == "套装" || screeningConditions == "其他")
            {
                model = await QueryPeripheryAsync(page, limit, text, screeningConditions, sortString, order, type);
            }
            else if (screeningConditions == "标签")
            {
                model = await QueryTagAsync(page, limit, text, screeningConditions, sortString, order, type);
            }
            else
            {
                model = await QueryAllAsync(page, limit, text, screeningConditions, sortString, order, type);
            }

            model.Sorting = sort;
            return model;
        }

        public async Task<PagedResultDto<SearchAloneModel>> QueryAllAsync(int page, int limit, string text, string screeningConditions, string sort, SortOrder sortOrder, QueryType type)
        {
            //根据不同分页方案初始化
            var from = type == QueryType.Index ? page : ((page - 1) * limit);
            //添加索引

            Indices indices = new string[]{
                        _entryElasticsearchBaseService.GetIndex(),
                        _articleElasticsearchBaseService.GetIndex(),
                        _tagElasticsearchBaseService.GetIndex(),
                        _peripheryElasticsearchBaseService.GetIndex(),
                    };
            var sortDescriptor = new SortDescriptor<dynamic>();
            if (string.IsNullOrEmpty(sort) == false)
            {
                sortDescriptor.Field(sort, sortOrder);
            }
            //查询
            var query = string.IsNullOrWhiteSpace(text) ?
                            await _elasticClient.SearchAsync<dynamic>(x => x.Index(indices)
                                                 .From(from)
                                                 .Sort(s => sortDescriptor)
                                                 .Size(limit))
                            : await _elasticClient.SearchAsync<dynamic>(x => x.Index(indices)
                                                .From(from)
                                                .Size(limit)
                                                .Sort(s => sortDescriptor)
                                                .Query(q =>
                                                    q.QueryString(qs => qs
                                                     .Query(text).DefaultOperator(Operator.And))));

            //根据查询结果向数据库获取真实信息

            var entryIds = query.Hits.Where(s => s.Index == _entryElasticsearchBaseService.GetIndex()).Select(s => (int)s.Source["id"]).ToList();

            var entries = await _entryRepository.GetAll().AsNoTracking().Include(s => s.Information)
                    .Include(s => s.EntryRelationFromEntryNavigation).ThenInclude(s => s.ToEntryNavigation).ThenInclude(s => s.Information).ThenInclude(s => s.Additional)
                    .Include(s => s.EntryRelationFromEntryNavigation).ThenInclude(s => s.ToEntryNavigation).ThenInclude(s => s.EntryRelationFromEntryNavigation).ThenInclude(s => s.ToEntryNavigation)
                .Where(s => entryIds.Contains(s.Id) && s.IsHidden != true && string.IsNullOrWhiteSpace(s.Name) == false).ToListAsync();

            var articleIds = query.Hits.Where(s => s.Index == _articleElasticsearchBaseService.GetIndex()).Select(s => (long)s.Source["id"]).ToList();

            var articles = await _articleRepository.GetAll().AsNoTracking().Include(s => s.CreateUser).Where(s => articleIds.Contains(s.Id) && s.IsHidden != true && string.IsNullOrWhiteSpace(s.Name) == false).ToListAsync();

            var tagIds = query.Hits.Where(s => s.Index == _tagElasticsearchBaseService.GetIndex()).Select(s => (int)s.Source["id"]).ToList();

            var tags = await _tagRepository.GetAll().AsNoTracking().Where(s => tagIds.Contains(s.Id) && s.IsHidden != true && string.IsNullOrWhiteSpace(s.Name) == false).ToListAsync();

            var peripheryIds = query.Hits.Where(s => s.Index == _peripheryElasticsearchBaseService.GetIndex()).Select(s => (long)s.Source["id"]).ToList();

            var peripheries = await _peripheryRepository.GetAll().AsNoTracking().Include(s => s.RelatedEntries).Where(s => peripheryIds.Contains(s.Id) && s.IsHidden != true && string.IsNullOrWhiteSpace(s.Name) == false).ToListAsync();


            var result = new PagedResultDto<SearchAloneModel>
            {
                TotalCount = (int)query.Total,
                CurrentPage = page,
                MaxResultCount = limit,
                FilterText = text,
                ScreeningConditions = screeningConditions
            };

            //将真实信息处理后按顺序添加到结果中
            foreach (var item in query.Hits)
            {
                if (_entryElasticsearchBaseService.GetIndex() == item.Index)
                {
                    result.Data.Add(new SearchAloneModel
                    {
                        entry = await _appHelper.GetEntryInforTipViewModel(entries.FirstOrDefault(s => s.Id == (long)item.Source["id"]))
                    });
                }
                else if (_articleElasticsearchBaseService.GetIndex() == item.Index)
                {
                    result.Data.Add(new SearchAloneModel
                    {
                        article = _appHelper.GetArticleInforTipViewModel(articles.FirstOrDefault(s => s.Id == (long)item.Source["id"]))
                    });
                }
                else if (_tagElasticsearchBaseService.GetIndex() == item.Index)
                {
                    result.Data.Add(new SearchAloneModel
                    {
                        tag = _appHelper.GetTagInforTipViewModel(tags.FirstOrDefault(s => s.Id == (long)item.Source["id"]))
                    });
                }
                else if (_peripheryElasticsearchBaseService.GetIndex() == item.Index)
                {
                    result.Data.Add(new SearchAloneModel
                    {
                        periphery = _appHelper.GetPeripheryInforTipViewModel(peripheries.FirstOrDefault(s => s.Id == (long)item.Source["id"]))
                    });
                }
            }


            return result;
        }

        public async Task<PagedResultDto<SearchAloneModel>> QueryEntryAsync(int page, int limit, string text, string screeningConditions, string sort, SortOrder sortOrder, QueryType type)
        {
            var entries = new List<Entry>();

            var entryType = screeningConditions switch
            {
                "游戏" => new EntryType[] { EntryType.Game },
                "角色" => new EntryType[] { EntryType.Role },
                "STAFF" => new EntryType[] { EntryType.Staff },
                "制作组" => new EntryType[] { EntryType.ProductionGroup },
                _ => new EntryType[] { EntryType.Game, EntryType.Role, EntryType.Staff, EntryType.ProductionGroup },
            };

            var result = await _entryElasticsearchBaseService.QueryAsync(page, limit, text, sort, sortOrder, s => s.Filter(s => s.Terms(s => s.Field(s => s.Type).Terms(entryType))), type);
            var query = result.Item2;

            var entryIds = query.Select(s => s.Id).ToList();

            entries = await _entryRepository.GetAll().AsNoTracking().Include(s => s.Information)
                    .Include(s => s.EntryRelationFromEntryNavigation).ThenInclude(s => s.ToEntryNavigation).ThenInclude(s => s.Information).ThenInclude(s => s.Additional)
                    .Include(s => s.EntryRelationFromEntryNavigation).ThenInclude(s => s.ToEntryNavigation).ThenInclude(s => s.EntryRelationFromEntryNavigation).ThenInclude(s => s.ToEntryNavigation)
                .Where(s => entryIds.Contains(s.Id) && s.IsHidden != true && string.IsNullOrWhiteSpace(s.Name) == false).ToListAsync();


            var model = new PagedResultDto<SearchAloneModel>
            {
                TotalCount = result.Item1,
                CurrentPage = page,
                MaxResultCount = limit,
                FilterText = text,
                ScreeningConditions = screeningConditions
            };

            //将真实信息处理后按顺序添加到结果中
            foreach (var item in query)
            {

                model.Data.Add(new SearchAloneModel
                {
                    entry = await _appHelper.GetEntryInforTipViewModel(entries.FirstOrDefault(s => s.Id == item.Id))
                });

            }

            return model;
        }

        public async Task<PagedResultDto<SearchAloneModel>> QueryArticleAsync(int page, int limit, string text, string screeningConditions, string sort, SortOrder sortOrder, QueryType type)
        {
            var articles = new List<Article>();

            var articleType = screeningConditions switch
            {
                "感想" => new ArticleType[] { ArticleType.Tought },
                "访谈" => new ArticleType[] { ArticleType.Interview },
                "攻略" => new ArticleType[] { ArticleType.Strategy },
                "动态" => new ArticleType[] { ArticleType.News },
                "评测" => new ArticleType[] { ArticleType.Evaluation },
                "文章周边" => new ArticleType[] { ArticleType.Peripheral },
                "公告" => new ArticleType[] { ArticleType.Notice },
                "杂谈" => new ArticleType[] { ArticleType.None },
                "二创" => new ArticleType[] { ArticleType.Fan },
                _ => new ArticleType[] { ArticleType.Notice, ArticleType.Peripheral, ArticleType.Evaluation, ArticleType.News, ArticleType.Interview, ArticleType.Tought, ArticleType.None },
            };

            var result = await _articleElasticsearchBaseService.QueryAsync(page, limit, text, sort, sortOrder, s => s.Filter(s => s.Terms(s => s.Field(s => s.Type).Terms(articleType))), type);
            var query = result.Item2;

            var entryIds = query.Select(s => s.Id).ToList();

            articles = await _articleRepository.GetAll().AsNoTracking().Include(s => s.CreateUser).Where(s => entryIds.Contains(s.Id) && s.IsHidden != true && string.IsNullOrWhiteSpace(s.Name) == false).ToListAsync();


            var model = new PagedResultDto<SearchAloneModel>
            {
                TotalCount = result.Item1,
                CurrentPage = page,
                MaxResultCount = limit,
                FilterText = text,
                ScreeningConditions = screeningConditions
            };

            //将真实信息处理后按顺序添加到结果中
            foreach (var item in query)
            {

                model.Data.Add(new SearchAloneModel
                {
                    article = _appHelper.GetArticleInforTipViewModel(articles.FirstOrDefault(s => s.Id == item.Id))
                });

            }

            return model;
        }

        public async Task<PagedResultDto<SearchAloneModel>> QueryPeripheryAsync(int page, int limit, string text, string screeningConditions, string sort, SortOrder sortOrder, QueryType type)
        {
            var peripheries = new List<Periphery>();

            var peripheryType = screeningConditions switch
            {
                "其他" => new PeripheryType[] { PeripheryType.None },
                "设定集或画册等" => new PeripheryType[] { PeripheryType.SetorAlbumEtc },
                "原声集" => new PeripheryType[] { PeripheryType.Ost },
                "套装" => new PeripheryType[] { PeripheryType.Set },
                _ => new PeripheryType[] { PeripheryType.Set, PeripheryType.Ost, PeripheryType.SetorAlbumEtc, PeripheryType.None },
            };

            var result = await _peripheryElasticsearchBaseService.QueryAsync(page, limit, text, sort, sortOrder, s => s.Filter(s => s.Terms(s => s.Field(s => s.Type).Terms(peripheryType))), type);
            var query = result.Item2;

            var entryIds = query.Select(s => s.Id).ToList();

            peripheries = await _peripheryRepository.GetAll().AsNoTracking().Include(s => s.Entries).ThenInclude(s => s.Entry).Where(s => entryIds.Contains(s.Id) && s.IsHidden != true && string.IsNullOrWhiteSpace(s.Name) == false).ToListAsync();


            var model = new PagedResultDto<SearchAloneModel>
            {
                TotalCount = result.Item1,
                CurrentPage = page,
                MaxResultCount = limit,
                FilterText = text,
                ScreeningConditions = screeningConditions
            };

            //将真实信息处理后按顺序添加到结果中
            foreach (var item in query)
            {

                model.Data.Add(new SearchAloneModel
                {
                    periphery = _appHelper.GetPeripheryInforTipViewModel(peripheries.FirstOrDefault(s => s.Id == item.Id))
                });

            }

            return model;
        }

        public async Task<PagedResultDto<SearchAloneModel>> QueryTagAsync(int page, int limit, string text, string screeningConditions, string sort, SortOrder sortOrder, QueryType type)
        {
            var tags = new List<Tag>();

            var result = await _tagElasticsearchBaseService.QueryAsync(page, limit, text, sort, sortOrder, null, type);
            var query = result.Item2;

            var entryIds = query.Select(s => s.Id).ToList();

            tags = await _tagRepository.GetAll().AsNoTracking().Include(s => s.Entries).Where(s => entryIds.Contains(s.Id) && s.IsHidden != true && string.IsNullOrWhiteSpace(s.Name) == false).ToListAsync();


            var model = new PagedResultDto<SearchAloneModel>
            {
                TotalCount = result.Item1,
                CurrentPage = page,
                MaxResultCount = limit,
                FilterText = text,
                ScreeningConditions = screeningConditions
            };

            //将真实信息处理后按顺序添加到结果中
            foreach (var item in query)
            {

                model.Data.Add(new SearchAloneModel
                {
                    tag = _appHelper.GetTagInforTipViewModel(tags.FirstOrDefault(s => s.Id == item.Id))
                });

            }

            return model;
        }
    }
}
