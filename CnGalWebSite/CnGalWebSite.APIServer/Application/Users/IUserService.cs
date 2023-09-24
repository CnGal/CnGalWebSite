

using CnGalWebSite.APIServer.Application.Users.Dtos;

using CnGalWebSite.DataModel.ExamineModel.Users;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Others;
using CnGalWebSite.DataModel.ViewModel.Space;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.Users
{
    public interface IUserService
    {
        Task<PagedResultDto<ApplicationUser>> GetPaginatedResult(GetUserInput input);

        Task UpdateUserDataMain(ApplicationUser user, UserMain examine);

        Task UpdateUserDataMainPage(ApplicationUser user, string examine);

        Task UpdateUserData(ApplicationUser user, Examine examine);


        Task<UserEditInforBindModel> GetUserEditInforBindModel(ApplicationUser user);

        Task AddUserIntegral(AddUserIntegralModel model);

        Task<UserInforViewModel> GetUserInforViewModel(ApplicationUser user, bool ignoreAddinfor = false);

        Task<string> GenerateBindGroupQQCode(ApplicationUser user);

        Task<Result> BindGroupQQ(string code, long groupQQ);

        Task UpdateUserCertificationDataMain(UserCertification userCertification, UserCertificationMain examine);

        Task UpdateUserCertificationData(UserCertification userCertification, Examine examine);

        Task UpdateUserIntegral(ApplicationUser user);

        Task<List<string>> GetAllNotCertificatedEntriesAsync(EntryType? type = null);

        bool CheckCurrentUserRole( string role);

        Task<EChartsHeatMapOptionModel> GetUserEditRecordHeatMap(string id, DateTime afterTime, DateTime beforeTime);

        Task<EChartsHeatMapOptionModel> GetUserSignInDaysHeatMap(string id, DateTime afterTime, DateTime beforeTime);

    }
}
