
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Ranks;
using CnGalWebSite.DataModel.ViewModel.Space;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.Ranks
{
    public interface IRankService
    {
        Task UpdateUserRanks(ApplicationUser user);

        Task<List<RankViewModel>> GetUserRanks(ApplicationUser user);

        Task<ILookup<string, RankViewModel>> GetVisibleUserRanksAsync(string[] userIds, CancellationToken cancellationToken);

        Task<List<UserEditRankIsShow>> GetUserRankListForEdit(ApplicationUser user);

        Task UpdateUserRanksIsHidden(ApplicationUser user, List<UserEditRankIsShow> model);

        Task<List<RankViewModel>> GetUserRanks(string userId, ApplicationUser user = null);
    }
}
