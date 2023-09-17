using BootstrapBlazor.Components;
using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.DataReositories;

using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Favorites;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.Favorites
{
    public class FavoriteObjectService : IFavoriteObjectService
    {
        private readonly IRepository<FavoriteObject, long> _favoriteObjectRepository;
        private readonly IRepository<Entry, long> _entryRepository;
        private readonly IRepository<Article, long> _articleRepository;
        private static readonly ConcurrentDictionary<Type, Func<IEnumerable<FavoriteObject>, string, SortOrder, IEnumerable<FavoriteObject>>> SortLambdaCache = new();

        private readonly IAppHelper _appHelper;

        public FavoriteObjectService(IAppHelper appHelper, IRepository<FavoriteObject, long> favoriteObjectRepository, IRepository<Entry, long> entryRepository, IRepository<Article, long> articleRepository)
        {
            _favoriteObjectRepository = favoriteObjectRepository;
            _appHelper = appHelper;
            _articleRepository = articleRepository;
            _entryRepository = entryRepository;
        }

        public async Task<PagedResultDto<FavoriteObjectAloneViewModel>> GetPaginatedResult(PagedSortedAndFilterInput input, long favoriteFolderId)
        {
            IQueryable<FavoriteObject> query;
            //筛选
            if (favoriteFolderId != 0)
            {
                query = _favoriteObjectRepository.GetAll().Where(s => s.FavoriteFolderId == favoriteFolderId).AsNoTracking();

            }
            else
            {
                query = _favoriteObjectRepository.GetAll().AsNoTracking();
            }

            //统计查询数据的总条数
            var count = query.Count();
            //根据需求进行排序，然后进行分页逻辑的计算
            //这个特殊方法中当前页数解释为起始位
            query = query.OrderBy(input.Sorting).Skip((input.CurrentPage-1)* input.MaxResultCount).Take(input.MaxResultCount);

            //将结果转换为List集合 加载到内存中
            List<FavoriteObject> models = null;
            if (count != 0)
            {
                models = await query.AsNoTracking()
                    .Include(s => s.Entry).ThenInclude(s => s.Information)
                    .Include(s => s.Video).ThenInclude(s => s.CreateUser)
                    .Include(s => s.Tag)
                    .Include(s=>s.Entry).ThenInclude(s => s.EntryRelationFromEntryNavigation).ThenInclude(s => s.ToEntryNavigation)
                    .Include(s => s.Article).ThenInclude(s => s.CreateUser)
                    .Include(s => s.Periphery).ToListAsync();
            }
            else
            {
                models = new List<FavoriteObject>();
            }

            var dtos = new List<FavoriteObjectAloneViewModel>();
            foreach (var item in models)
            {
                if (item.Type == FavoriteObjectType.Article)
                {
                    dtos.Add(new FavoriteObjectAloneViewModel
                    {
                        article = _appHelper.GetArticleInforTipViewModel(item.Article)
                    });
                }
                else if (item.Type == FavoriteObjectType.Entry)
                {
                    dtos.Add(new FavoriteObjectAloneViewModel
                    {
                        entry =  _appHelper.GetEntryInforTipViewModel(item.Entry)
                    });
                }
                else if (item.Type == FavoriteObjectType.Periphery)
                {
                    dtos.Add(new FavoriteObjectAloneViewModel
                    {
                        periphery = _appHelper.GetPeripheryInforTipViewModel(item.Periphery)
                    });
                }
                else if (item.Type == FavoriteObjectType.Video)
                {
                    dtos.Add(new FavoriteObjectAloneViewModel
                    {
                        Video = _appHelper.GetVideoInforTipViewModel(item.Video)
                    });
                }
                else if (item.Type == FavoriteObjectType.Tag)
                {
                    dtos.Add(new FavoriteObjectAloneViewModel
                    {
                        Tag = _appHelper.GetTagInforTipViewModel(item.Tag)
                    });
                }
            }

            var dtos_ = new PagedResultDto<FavoriteObjectAloneViewModel>
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

    }
}
