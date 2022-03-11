
using BootstrapBlazor.Components;
using CnGalWebSite.APIServer.Application.Users.Dtos;
using CnGalWebSite.DataModel.Application.Dtos;
using CnGalWebSite.DataModel.ExamineModel;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Space;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.Users
{
    public interface IUserService
    {
        Task<PagedResultDto<ApplicationUser>> GetPaginatedResult(GetUserInput input);

        Task<QueryData<ListUserAloneModel>> GetPaginatedResult(CnGalWebSite.DataModel.ViewModel.Search.QueryPageOptions options, ListUserAloneModel searchModel);

        Task<bool> AddUserThirdPartyLogin(string id_token, ApplicationUser user, ThirdPartyLoginType type);

        Task<ApplicationUser> GetThirdPartyLoginUser(string id_token, ThirdPartyLoginType type);

        Task<string> GetThirdPartyLoginIdToken(string code, string returnUrl, bool isSSR, ThirdPartyLoginType type);



        Task UpdateUserDataMain(ApplicationUser user, UserMain examine);

        Task UpdateUserDataMainPage(ApplicationUser user, string examine);

        Task UpdateUserData(ApplicationUser user, Examine examine);


        Task<UserEditInforBindModel> GetUserEditInforBindModel(ApplicationUser user);

        Task AddUserIntegral(AddUserIntegralModel model);

        Task<UserInforViewModel> GetUserInforViewModel(ApplicationUser user);

    }
}
