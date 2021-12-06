using BootstrapBlazor.Components;
using Microsoft.EntityFrameworkCore;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Admin;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.ErrorCounts
{
    public class ErrorCountService : IErrorCountService
    {
        private readonly IRepository<ErrorCount, int> _errorCountRepository;

        private static readonly ConcurrentDictionary<Type, Func<IEnumerable<ErrorCount>, string, SortOrder, IEnumerable<ErrorCount>>> SortLambdaCache = new();


        public ErrorCountService(IRepository<ErrorCount, int> errorCountRepository)
        {
            _errorCountRepository = errorCountRepository;
        }
        public Task<QueryData<ListErrorCountAloneModel>> GetPaginatedResult(QueryPageOptions options, ListErrorCountAloneModel searchModel)
        {
            IEnumerable<ErrorCount> items = _errorCountRepository.GetAll().AsNoTracking();
            // 处理高级搜索
            if (!string.IsNullOrWhiteSpace(searchModel.Text))
            {
                items = items.Where(item => item.Text?.Contains(searchModel.Text, StringComparison.OrdinalIgnoreCase) ?? false);
            }


            // 处理 Searchable=true 列与 SeachText 模糊搜索
            if (options.Searchs.Any())
            {

                // items = items.Where(options.Searchs.GetFilterFunc<Entry>(FilterLogic.Or));
            }
            else
            {
                // 处理 SearchText 模糊搜索
                if (!string.IsNullOrWhiteSpace(options.SearchText))
                {
                    items = items.Where(item => (item.Text?.Contains(options.SearchText) ?? false));
                }
            }
            // 过滤
            /* var isFiltered = false;
             if (options.Filters.Any())
             {
                 items = items.Where(options.Filters.GetFilterFunc<Entry>());
                 isFiltered = true;
             }*/

            // 排序
            var isSorted = false;
            if (!string.IsNullOrWhiteSpace(options.SortName))
            {
                // 外部未进行排序，内部自动进行排序处理
                var invoker = SortLambdaCache.GetOrAdd(typeof(ErrorCount), key => LambdaExtensions.GetSortLambda<ErrorCount>().Compile());
                items = invoker(items, options.SortName, options.SortOrder);
                isSorted = true;
            }

            // 设置记录总数
            var total = items.Count();

            // 内存分页
            items = items.Skip((options.PageIndex - 1) * options.PageItems).Take(options.PageItems).ToList();

            //复制数据
            var resultItems = new List<ListErrorCountAloneModel>();
            foreach (var item in items)
            {
                resultItems.Add(new ListErrorCountAloneModel
                {
                    Id = item.Id,
                    Text = item.Text,
                    LastUpdateTime = item.LastUpdateTime
                });
            }

            return Task.FromResult(new QueryData<ListErrorCountAloneModel>()
            {
                Items = resultItems,
                TotalCount = total,
                IsSorted = isSorted,
                // IsFiltered = isFiltered
            });
        }
    }
}
