

using CnGalWebSite.DataModel.ExamineModel;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Space;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tag = CnGalWebSite.DataModel.Model.Tag;

namespace CnGalWebSite.APIServer.ExamineX
{
    public interface IExamineService
    {
        Task<PagedResultDto<ExaminedNormalListModel>> GetPaginatedResult(GetExamineInput input, int entryId = 0, string userId = "");
        /// <summary>
        /// 将审核列表优化成精简模式以减少流量消耗
        /// </summary>
        /// <param name="examines">数据库审核列表模型</param>
        /// <param name="isShowRanks">是否展示头衔</param>
        /// <returns></returns>
        Task<List<ExaminedNormalListModel>> GetExaminesToNormalListAsync(IQueryable<Examine> examines, bool isShowRanks);
        /// <summary>
        /// 获取用户待审核编辑 序列化后的对象列表
        /// </summary>
        /// <param name="Id">用户Id</param>
        /// <returns></returns>
        Task<List<UserPendingDataModel>> GetUserPendingData(string Id);
        /// <summary>
        /// 获取审核预览视图
        /// </summary>
        /// <param name="model"></param>
        /// <param name="examine"></param>
        /// <returns></returns>
        Task<bool> GetExamineView(ExamineViewModel model, Examine examine);
      
        /// <summary>
        /// 获取当前用户对该词条部分的待审核记录
        /// </summary>
        /// <param name="entryId">词条Id</param>
        /// <param name="userId">用户Id</param>
        /// <param name="operation">用操作表示部分</param>
        /// <returns>审核记录</returns>
        Task<Examine> GetUserEntryActiveExamineAsync(int entryId, string userId, Operation operation);
      
        /// <summary>
        /// 为批量导入的词条建立审核记录
        /// </summary>
        /// <param name="model">词条</param>
        /// <param name="user">管理员用户</param>
        /// <param name="note">备注</param>
        /// <returns></returns>
        Task<Entry> AddNewEntryExaminesAsync(Entry model, ApplicationUser user, string note);

        /// <summary>
        /// 获取当前用户对该 文章 部分的待审核记录
        /// </summary>
        /// <param name="articleId">文章Id</param>
        /// <param name="userId">用户Id</param>
        /// <param name="operation">用操作表示部分</param>
        /// <returns>审核记录</returns>
        Task<Examine> GetUserArticleActiveExamineAsync(long articleId, string userId, Operation operation);
      

        /// <summary>
        /// 为批量导入的文章建立审核记录
        /// </summary>
        /// <param name="model">文章</param>
        /// <param name="user">用户</param>
        /// <param name="note">备注</param>
        /// <returns></returns>
        Task<Article> AddNewArticleExaminesAsync(Article model, ApplicationUser user, string note);
        /// <summary>
        /// 为批量导入的周边建立审核记录
        /// </summary>
        /// <param name="model">周边</param>
        /// <param name="user">用户</param>
        /// <param name="note">备注</param>
        /// <returns></returns>
        Task<Periphery> AddNewPeripheryExaminesAsync(Periphery model, ApplicationUser user, string note);
        /// <summary>
        /// 为批量导入的标签建立审核记录
        /// </summary>
        /// <param name="model">标签</param>
        /// <param name="user">用户</param>
        /// <param name="note">备注</param>
        /// <returns></returns>
        Task<Tag> AddNewTagExaminesAsync(Tag model, ApplicationUser user, string note);
        /// <summary>
        /// 获取当前用户 标签 等待审核
        /// </summary>
        /// <param name="tagId"></param>
        /// <param name="userId"></param>
        /// <param name="operation"></param>
        /// <returns></returns>
        Task<Examine> GetUserTagActiveExamineAsync(int tagId, string userId, Operation operation);
      
        /// <summary>
        /// 获取当前用户对该 用户信息 部分的待审核记录
        /// </summary>
        /// <param name="userId">用户Id</param>
        /// <param name="operation">用操作表示部分</param>
        /// <returns>审核记录</returns>
        Task<Examine> GetUserInforActiveExamineAsync(string userId, Operation operation);

    
        /// <summary>
        /// 获取当前用户 周边 等待审核
        /// </summary>
        /// <param name="peripheryId"></param>
        /// <param name="userId"></param>
        /// <param name="operation"></param>
        /// <returns></returns>
        Task<Examine> GetUserPeripheryActiveExamineAsync(long peripheryId, string userId, Operation operation);
     
        /// <summary>
        /// 补全审核记录
        /// </summary>
        /// <returns></returns>
        Task ExaminesCompletion();



        Task<object> GenerateModelFromExamines(List<Examine> examines);

        Task ReplaceEditEntryStaffExamineContext();

        Task ReplaceEntryStaff();

        Task RefreshAllEntryStaffRelevances(bool autoCreate, PositionGeneralType type);

        Task RefreshEntryStaffRelevances(int id, bool autoCreate, PositionGeneralType type);

        /// <summary>
        /// 添加编辑记录到数据库中
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="user"></param>
        /// <param name="examineData"></param>
        /// <param name="operation"></param>
        /// <param name="note"></param>
        /// <param name="isAdmin"></param>
        /// <param name="isCreating">是否为创建相关</param>
        /// <returns></returns>
        Task<Examine> AddEditRecordAsync(object entry, ApplicationUser user, object examineData, Operation operation, string note, bool isAdmin, bool isCreating = false);

        /// <summary>
        /// 使审核记录真实作用在目标上 仅管理员
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="examine"></param>
        /// <param name="operation"></param>
        /// <returns></returns>
        Task ApplyEditRecordToObject(object entry, object examine, Operation operation);

        Task<Video> AddNewVideoExaminesAsync(Video model, ApplicationUser user, string note);

        Task<Examine> GetUserVideoActiveExamineAsync(long videoId, string userId, Operation operation);

        Task<object> GenerateModelFromExamines(object model);
    }
}
