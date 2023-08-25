using BootstrapBlazor.Components;
using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.Application.Users;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.APIServer.ExamineX;
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
    public class EditRecordService : IEditRecordService
    {
        private readonly IRepository<UserReviewEditRecord, long> _userReviewEditRecordRepository;
        private readonly IAppHelper _appHelper;
        private readonly IExamineService _examineService;
        private readonly IUserService _userService;
        
        private readonly IRepository<UserCertification, long> _userCertificationRepository;
        private readonly IRepository<UserMonitor, long> _userMonitorsRepository;

        public EditRecordService(IRepository<UserReviewEditRecord, long> userReviewEditRecordRepository, IRepository<UserMonitor, long> userMonitorsRepository, IAppHelper appHelper, IUserService userService,
             IRepository<UserCertification, long> userCertificationRepository, IExamineService examineService)
        {
            _userMonitorsRepository = userMonitorsRepository;
            _userReviewEditRecordRepository = userReviewEditRecordRepository;
            _appHelper = appHelper;
            _userService = userService;
            
            _userCertificationRepository = userCertificationRepository;
            _examineService = examineService;
        }

        /// <summary>
        /// 用于保存审核记录时检查权限
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="user"></param>
        /// <param name="allowEditor"></param>
        /// <returns></returns>
        public async Task<bool> CheckUserExaminePermission( object entry,ApplicationUser user,  bool allowEditor = true)
        {
            var adminRole = allowEditor ? "Editor" : "Admin";
            var isAdmin = _userService.CheckCurrentUserRole(adminRole);

            if (isAdmin == false  && allowEditor)
            {
                if (entry is Entry && await _userCertificationRepository.GetAll().AnyAsync(s => s.ApplicationUserId == user.Id && s.EntryId != null && s.EntryId == (entry as Entry).Id))
                {
                    isAdmin = true;
                }
                else if (entry is ApplicationUser && await _userCertificationRepository.GetAll().AnyAsync(s => s.ApplicationUserId == user.Id && s.EntryId != null))
                {
                    isAdmin = true;
                }
            }

            return isAdmin;
        }

        /// <summary>
        /// 用于获取审核记录视图时检查权限
        /// </summary>
        /// <param name="examine"></param>
        /// <param name="user"></param>
        /// <param name="allowEditor"></param>
        /// <returns></returns>
        public async Task<bool> CheckUserExaminePermission( Examine examine, ApplicationUser user, bool allowEditor = true)
        {
            var adminRole = allowEditor ? "Editor" : "Admin";
            var isAdmin = _userService.CheckCurrentUserRole(adminRole);

            if (isAdmin == false && allowEditor)
            {
                //当前词条为对应认证词条
                if (examine.EntryId != null && await _userCertificationRepository.GetAll().AnyAsync(s => s.ApplicationUserId == user.Id && s.EntryId != null && s.EntryId == examine.EntryId))
                {
                    isAdmin = true;
                }//当前用户为认证用户且编辑自身资料
                else if (await _userCertificationRepository.GetAll().AnyAsync(s => s.ApplicationUserId == user.Id && s.EntryId != null) && (examine.Operation == Operation.UserMainPage || examine.Operation == Operation.EditUserMain))
                {
                    isAdmin = true;
                }

            }

            return isAdmin;
        }

        public async Task SaveAndApplyEditRecord(object entry, ApplicationUser user, object examineData, Operation operation,  string note, bool isCreating = false, bool allowEditor = true)
        {
            //判断是否有权限直接编辑
            var isAdmin = await CheckUserExaminePermission(entry,user,allowEditor);

            //添加审核记录
            var examine = await _examineService.AddEditRecordAsync(entry, user, examineData, operation, note, isAdmin, isCreating);


            if (isAdmin)
            {
                //应用审核记录
                await _examineService.ApplyEditRecordToObject(entry, examineData, operation);
                //更新用户积分
                await _userService.UpdateUserIntegral(user);
                //尝试添加审阅
                await AddEditRecordToUserReview(examine);
            }
        }

        public async Task AddEditRecordToUserReview(Examine examine)
        {
            //判断是否在用户监视列表中
            if (examine.EntryId == null || examine.EntryId.Value == 0)
            {
                return;
            }

            //查找监视的用户
            var userIds = await _userMonitorsRepository.GetAll().AsNoTracking().Where(s => s.EntryId == examine.EntryId&&s.ApplicationUserId!=examine.ApplicationUserId && string.IsNullOrWhiteSpace(s.ApplicationUserId) == false).Select(s => s.ApplicationUserId).ToListAsync();
            //去除重复的
            var existUserIds = await _userReviewEditRecordRepository.GetAll().AsNoTracking()
                .Where(s=>s.ExamineId == examine.Id )
                .Where(s => userIds.Contains(s.ApplicationUserId))
                .Select(s => s.ApplicationUserId)
                .ToListAsync();

            foreach (var item in userIds.Where(s=>existUserIds.Contains(s)==false))
            {
                await _userReviewEditRecordRepository.InsertAsync(new UserReviewEditRecord
                {
                    ApplicationUserId = item,
                    ExamineId = examine.Id,
                });
            }
        }

        public async Task<bool> CheckObjectIsInUserMonitor(ApplicationUser user,long id)
        {
            return await _userMonitorsRepository.GetAll().AnyAsync(s => s.ApplicationUserId == user.Id && s.EntryId == id);
        }
    }
}
