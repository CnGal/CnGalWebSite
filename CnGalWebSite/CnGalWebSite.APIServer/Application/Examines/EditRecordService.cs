using BootstrapBlazor.Components;
using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.Application.Users;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.EditRecords;
using CnGalWebSite.DataModel.ViewModel.Lotteries;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;


namespace CnGalWebSite.APIServer.Application.Examines
{
    public class EditRecordService:IEditRecordService
    {
        private readonly IRepository<UserReviewEditRecord, long> _UserReviewEditRecordRepository;
        private readonly IRepository<UserMonitorEntry, long> _UserMonitorEntryRepository;
        private readonly IAppHelper _appHelper;

        public EditRecordService(IRepository<UserReviewEditRecord, long> userReviewEditRecordRepository, IRepository<UserMonitorEntry, long> userMonitorEntryRepository, IAppHelper appHelper)
        {
            _UserMonitorEntryRepository= userMonitorEntryRepository;
            _UserReviewEditRecordRepository= userReviewEditRecordRepository;
            _appHelper = appHelper;
        }

        public Task<QueryData<ListUserMonitorEntryAloneModel>> GetPaginatedResult(CnGalWebSite.DataModel.ViewModel.Search.QueryPageOptions options, ListUserMonitorEntryAloneModel searchModel, string userId)
        {
            var items = _UserMonitorEntryRepository.GetAll().Include(s=>s.Entry).Where(s => s.EntryId!=null&&s.ApplicationUserId==userId).AsNoTracking();

            // 处理 SearchText 模糊搜索
            if (!string.IsNullOrWhiteSpace(options.SearchText))
            {
                items = items.Where(item => item.Id.ToString().Contains(options.SearchText)
                             || item.EntryId.ToString().Contains(options.SearchText));
            }


            // 排序
            var isSorted = false;
            if (!string.IsNullOrWhiteSpace(options.SortName))
            {
                items = items.OrderBy(s => s.Id).Sort(options.SortName, (BootstrapBlazor.Components.SortOrder)options.SortOrder);
                isSorted = true;
            }

            // 设置记录总数
            var total = items.Count();

            // 内存分页
            var list = items.Skip((options.PageIndex - 1) * options.PageItems).Take(options.PageItems).ToList();

            //复制数据
            var resultItems = new List<ListUserMonitorEntryAloneModel>();
            foreach (var item in list)
            {
                resultItems.Add(new ListUserMonitorEntryAloneModel
                {
                    Id = item.Id,
                    CreateTime = item.CreateTime,
                    EntryId=item.Entry.Id,
                    EntryName=item.Entry.DisplayName,
                    Type=item.Entry.Type
                });
            }

            return Task.FromResult(new QueryData<ListUserMonitorEntryAloneModel>()
            {
                Items = resultItems,
                TotalCount = total,
                IsSorted = isSorted,
                // IsFiltered = isFiltered
            });
        }

        public Task<QueryData<ListUserReviewEditRecordAloneModel>> GetPaginatedResult(CnGalWebSite.DataModel.ViewModel.Search.QueryPageOptions options, ListUserReviewEditRecordAloneModel searchModel,string userId)
        {
            var items = _UserReviewEditRecordRepository.GetAll().AsNoTracking()
                .Include(s => s.Examine).ThenInclude(s => s.Entry)
                .Include(s => s.Examine).ThenInclude(s => s.ApplicationUser)
                .Where(s => s.ExamineId != null && s.ApplicationUserId == userId);

            // 处理 SearchText 模糊搜索
            if (!string.IsNullOrWhiteSpace(options.SearchText))
            {
                items = items.Where(item => item.Id.ToString().Contains(options.SearchText)
                             || item.ExamineId.ToString().Contains(options.SearchText));
            }


            // 排序
            var isSorted = false;
            if (!string.IsNullOrWhiteSpace(options.SortName))
            {
                items = items.OrderBy(s => s.Id).Sort(options.SortName, (BootstrapBlazor.Components.SortOrder)options.SortOrder);
                isSorted = true;
            }

            // 设置记录总数
            var total = items.Count();

            // 内存分页
            var list = items.Skip((options.PageIndex - 1) * options.PageItems).Take(options.PageItems).ToList();

            //复制数据
            var resultItems = new List<ListUserReviewEditRecordAloneModel>();
            foreach (var item in list)
            {
                resultItems.Add(new ListUserReviewEditRecordAloneModel
                {
                    Id = item.Id,
                    State = item.State,
                    EntryId=item.Examine.Entry.Id,
                    EntryName= item.Examine.Entry.Name,
                    Operation=item.Examine.Operation,
                    UserId=item.Examine.ApplicationUserId,
                    UserName=item.Examine.ApplicationUser.UserName
                });
            }

            return Task.FromResult(new QueryData<ListUserReviewEditRecordAloneModel>()
            {
                Items = resultItems,
                TotalCount = total,
                IsSorted = isSorted,
                // IsFiltered = isFiltered
            });
        }
    }
}
