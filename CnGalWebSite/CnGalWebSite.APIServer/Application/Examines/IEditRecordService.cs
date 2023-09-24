
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.EditRecords;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.Examines
{
    public interface IEditRecordService
    {
        /// <summary>
        /// 保存并应用审核记录
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="user"></param>
        /// <param name="examineData"></param>
        /// <param name="operation"></param>
        /// <param name="note"></param>
        /// <param name="isCreating"></param>
        /// <param name="allowEditor"></param>
        /// <returns></returns>
        Task SaveAndApplyEditRecord(object entry, ApplicationUser user, object examineData, Operation operation, string note, bool isCreating = false, bool allowEditor = true);

        /// <summary>
        /// 尝试添加编辑记录到用户审阅列表 内部判断是否监视
        /// </summary>
        /// <param name="examine"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        Task AddEditRecordToUserReview(Examine examine);

        /// <summary>
        /// 判断用户是否具有编辑权限
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="user"></param>
        /// <param name="type"></param>
        /// <param name="allowEditor"></param>
        /// <returns></returns>
        Task<bool> CheckUserExaminePermission(object entry, ApplicationUser user, bool allowEditor = true);
        Task<bool> CheckUserExaminePermission( Examine examine, ApplicationUser user, bool allowEditor = true);

        /// <summary>
        /// 检测目标是否在指定用户监视下
        /// </summary>
        /// <param name="user"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<bool> CheckObjectIsInUserMonitor(ApplicationUser user, long id);
    }
}
