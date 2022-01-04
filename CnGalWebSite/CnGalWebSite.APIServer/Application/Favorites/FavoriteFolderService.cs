using BootstrapBlazor.Components;
using Microsoft.EntityFrameworkCore;
using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Admin;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.Favorites
{
    public class FavoriteFolderService : IFavoriteFolderService
    {
        private readonly IRepository<FavoriteFolder, long> _favoriteFolderRepository;
        private static readonly ConcurrentDictionary<Type, Func<IEnumerable<FavoriteFolder>, string, SortOrder, IEnumerable<FavoriteFolder>>> SortLambdaCache = new();


        public FavoriteFolderService( IRepository<FavoriteFolder, long> favoriteFolderRepository)
        {
            _favoriteFolderRepository = favoriteFolderRepository;
        }

        public async Task<QueryData<ListFavoriteFolderAloneModel>> GetPaginatedResult(CnGalWebSite.DataModel.ViewModel.Search.QueryPageOptions options, ListFavoriteFolderAloneModel searchModel, string userId = "")
        {
            IEnumerable<FavoriteFolder> items;

            //是否限定用户
            if (string.IsNullOrWhiteSpace(userId) == false)
            {
                items =await _favoriteFolderRepository.GetAll().AsNoTracking().Where(s => s.ApplicationUserId == userId).ToListAsync();
            }
            else
            {
                items =await _favoriteFolderRepository.GetAll().AsNoTracking().ToListAsync();
            }

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
                                 || (item.BriefIntroduction?.Contains(options.SearchText) ?? false)
                                 || (item.ApplicationUserId.ToString()?.Contains(options.SearchText) ?? false));
                }
         
            // 排序
            var isSorted = false;
            if (!string.IsNullOrWhiteSpace(options.SortName))
            {
                // 外部未进行排序，内部自动进行排序处理
                var invoker = SortLambdaCache.GetOrAdd(typeof(FavoriteFolder), key => LambdaExtensions.GetSortLambda<FavoriteFolder>().Compile());
                items = invoker(items, options.SortName, (BootstrapBlazor.Components.SortOrder) options.SortOrder);
                isSorted = true;
            }

            // 设置记录总数
            var total = items.Count();

            // 内存分页
            items = items.Skip((options.PageIndex - 1) * options.PageItems).Take(options.PageItems).ToList();

            //复制数据
            var resultItems = new List<ListFavoriteFolderAloneModel>();
            foreach (var item in items)
            {
                resultItems.Add(new ListFavoriteFolderAloneModel
                {
                    Id = item.Id,
                    Name = item.Name,
                    IsDefault = item.IsDefault,
                    BriefIntroduction = item.BriefIntroduction,
                    Count = item.Count,
                    CreateTime = item.CreateTime,
                    UserId = item.ApplicationUserId
                });
            }

            return new QueryData<ListFavoriteFolderAloneModel>()
            {
                Items = resultItems,
                TotalCount = total,
                IsSorted = isSorted,
                // IsFiltered = isFiltered
            };
        }
    }
}
