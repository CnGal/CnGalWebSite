using BootstrapBlazor.Components;
using Microsoft.EntityFrameworkCore;
using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.DataModel.Application.Dtos;
using CnGalWebSite.DataModel.Models;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Files;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.Files
{
    public class FileService : IFileService
    {
        private readonly IRepository<UserFile, int> _userFileRepository;
        private readonly IAppHelper _appHelper;

        private static readonly ConcurrentDictionary<Type, Func<IEnumerable<UserFile>, string, SortOrder, IEnumerable<UserFile>>> SortLambdaCache = new();


        public FileService(IAppHelper appHelper, IRepository<UserFile, int> userFileRepository)
        {
            _userFileRepository = userFileRepository;
            _appHelper = appHelper;
        }

        public async Task<PagedResultDto<ImageInforTipViewModel>> GetPaginatedResult(PagedSortedAndFilterInput input)
        {
            var query = _userFileRepository.GetAll().AsNoTracking();

            //判断输入的查询名称是否为空
            if (!string.IsNullOrWhiteSpace(input.FilterText))
            {
                query = query.Where(s => s.FileName == input.FilterText);
            }
            //统计查询数据的总条数
            var count = query.Count();
            //根据需求进行排序，然后进行分页逻辑的计算
            //这个特殊方法中当前页数解释为起始位
            query = query.OrderBy(input.Sorting).Skip(input.CurrentPage).Take(input.MaxResultCount);

            //将结果转换为List集合 加载到内存中
            List<ImageInforTipViewModel> models = null;
            if (count != 0)
            {
                models = await query.AsNoTracking().Include(s => s.FileManager).ThenInclude(s => s.ApplicationUser)
                    .Select(s => new ImageInforTipViewModel
                    {
                        FileName = _appHelper.GetImagePath(s.FileName, ""),
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

        public Task<QueryData<ListFileAloneModel>> GetPaginatedResult(QueryPageOptions options, ListFileAloneModel searchModel)
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
                    items = items.Where(item => (item.FileName?.Contains(options.SearchText) ?? false)
                                 || (item.UserId?.Contains(options.SearchText) ?? false));
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
                var invoker = SortLambdaCache.GetOrAdd(typeof(UserFile), key => LambdaExtensions.GetSortLambda<UserFile>().Compile());
                items = invoker(items, options.SortName, options.SortOrder);
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
                    UserId = item.UserId
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


    }
}
