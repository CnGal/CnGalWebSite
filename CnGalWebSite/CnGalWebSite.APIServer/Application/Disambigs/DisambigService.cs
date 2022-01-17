using BootstrapBlazor.Components;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.DataModel.ExamineModel;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Admin;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.Disambigs
{
    public class DisambigService : IDisambigService
    {

        private readonly IRepository<Disambig, long> _disambigRepository;
        private readonly IRepository<Entry, int> _entryRepository;
        private readonly IRepository<Article, long> _articleRepository;
        private static readonly ConcurrentDictionary<Type, Func<IEnumerable<Disambig>, string, SortOrder, IEnumerable<Disambig>>> SortLambdaCache = new();

        public DisambigService(IRepository<Disambig, long> disambigRepository, IRepository<Entry, int> entryRepository,
            IRepository<Article, long> articleRepository)
        {
            _disambigRepository = disambigRepository;
            _entryRepository = entryRepository;
            _articleRepository = articleRepository;
        }


        public Task<QueryData<ListDisambigAloneModel>> GetPaginatedResult(CnGalWebSite.DataModel.ViewModel.Search.QueryPageOptions options, ListDisambigAloneModel searchModel)
        {
            IEnumerable<Disambig> items = _disambigRepository.GetAll().AsNoTracking();

            // 处理高级搜索
            if (!string.IsNullOrWhiteSpace(searchModel.Name))
            {
                items = items.Where(item => item.Name?.Contains(searchModel.Name, StringComparison.OrdinalIgnoreCase) ?? false);
            }

            if (!string.IsNullOrWhiteSpace(searchModel.BriefIntroduction))
            {
                items = items.Where(item => item.BriefIntroduction?.Contains(searchModel.BriefIntroduction, StringComparison.OrdinalIgnoreCase) ?? false);
            }


            // 处理 SearchText 模糊搜索
            if (!string.IsNullOrWhiteSpace(options.SearchText))
            {
                items = items.Where(item => (item.Name?.Contains(options.SearchText) ?? false)
                             || (item.BriefIntroduction?.Contains(options.SearchText) ?? false));
            }


            // 排序
            var isSorted = false;
            if (!string.IsNullOrWhiteSpace(options.SortName))
            {
                // 外部未进行排序，内部自动进行排序处理
                var invoker = SortLambdaCache.GetOrAdd(typeof(Disambig), key => LambdaExtensions.GetSortLambda<Disambig>().Compile());
                items = invoker(items, options.SortName, (BootstrapBlazor.Components.SortOrder)options.SortOrder);
                isSorted = true;
            }

            // 设置记录总数
            var total = items.Count();

            // 内存分页
            items = items.Skip((options.PageIndex - 1) * options.PageItems).Take(options.PageItems).ToList();

            //复制数据
            var resultItems = new List<ListDisambigAloneModel>();
            foreach (var item in items)
            {
                resultItems.Add(new ListDisambigAloneModel
                {
                    Id = item.Id,
                    Name = item.Name,
                    BriefIntroduction = item.BriefIntroduction,
                    IsHidden = item.IsHidden
                });
            }

            return Task.FromResult(new QueryData<ListDisambigAloneModel>()
            {
                Items = resultItems,
                TotalCount = total,
                IsSorted = isSorted,
                // IsFiltered = isFiltered
            });
        }

        public void UpdateDisambigDataMain(Disambig disambig, DisambigMain examine)
        {
            disambig.Name = examine.Name;
            disambig.BriefIntroduction = examine.BriefIntroduction;
            disambig.MainPicture = examine.MainPicture;
            disambig.BackgroundPicture = examine.BackgroundPicture;
            disambig.SmallBackgroundPicture = examine.SmallBackgroundPicture;

        }

        public async Task UpdateDisambigDataRelevancesAsync(Disambig disambig, DisambigRelevances examine)
        {
            foreach (var item in examine.Relevances)
            {
                var isAdd = false;

                //遍历信息列表寻找关键词
                if (item.Type == DisambigRelevanceType.Entry)
                {
                    foreach (var infor in disambig.Entries)
                    {

                        if (infor.Id == item.EntryId)
                        {
                            //查看是否为删除操作
                            if (item.IsDelete == true)
                            {
                                disambig.Entries.Remove(infor);
                            }
                            else
                            {
                                //查找词条
                                var temp = await _entryRepository.FirstOrDefaultAsync(s => s.Id == item.EntryId);
                                if (temp != null)
                                {
                                    disambig.Entries.Add(temp);
                                }
                            }
                            isAdd = true;
                            break;
                        }
                    }
                }
                else if (item.Type == DisambigRelevanceType.Article)
                {
                    foreach (var infor in disambig.Articles)
                    {

                        if (infor.Id == item.EntryId)
                        {
                            //查看是否为删除操作
                            if (item.IsDelete == true)
                            {
                                disambig.Articles.Remove(infor);
                            }
                            else
                            {
                                //查找词条
                                var temp = await _articleRepository.FirstOrDefaultAsync(s => s.Id == item.EntryId);
                                if (temp != null)
                                {
                                    disambig.Articles.Add(temp);
                                }
                            }
                            isAdd = true;
                            break;
                        }
                    }
                }
                if (isAdd == false && item.IsDelete == false)
                {
                    if (item.Type == DisambigRelevanceType.Entry)
                    {
                        var temp = await _entryRepository.FirstOrDefaultAsync(s => s.Id == item.EntryId);
                        if (temp != null)
                        {
                            disambig.Entries.Add(temp);
                        }
                    }
                    else if (item.Type == DisambigRelevanceType.Article)
                    {
                        var temp = await _articleRepository.FirstOrDefaultAsync(s => s.Id == item.EntryId);
                        if (temp != null)
                        {
                            disambig.Articles.Add(temp);
                        }
                    }
                }
            }
        }

        public async Task UpdateDisambigDataAsync(Disambig disambig, Examine examine)
        {
            switch (examine.Operation)
            {
                case Operation.DisambigMain:
                    DisambigMain disambigMain = null;
                    using (TextReader str = new StringReader(examine.Context))
                    {
                        var serializer = new JsonSerializer();
                        disambigMain = (DisambigMain)serializer.Deserialize(str, typeof(DisambigMain));
                    }

                    UpdateDisambigDataMain(disambig, disambigMain);
                    break;
                case Operation.DisambigRelevances:
                    DisambigRelevances disambigRelevances = null;
                    using (TextReader str = new StringReader(examine.Context))
                    {
                        var serializer = new JsonSerializer();
                        disambigRelevances = (DisambigRelevances)serializer.Deserialize(str, typeof(DisambigRelevances));
                    }

                    await UpdateDisambigDataRelevancesAsync(disambig, disambigRelevances);
                    break;

            }
        }
    }
}
