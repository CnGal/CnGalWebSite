using BootstrapBlazor.Components;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Lotteries;
using CnGalWebSite.DataModel.ViewModel.OperationRecords;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.Lotteries
{
    public interface ILotteryService
    {
        Task SendPrizeToWinningUser(LotteryUser user, LotteryAward award);

        Task<QueryData<ListLotteryUserAloneModel>> GetPaginatedResult(CnGalWebSite.DataModel.ViewModel.Search.QueryPageOptions options, ListLotteryUserAloneModel searchModel, long lotteryId);

        Task DrawAllLottery();

        Task ClearLottery(long id);

        Task AddUserToLottery(Lottery lottery, ApplicationUser user, HttpContext httpContext, DeviceIdentificationModel identification);

        Task CopyUserFromBookingToLottery(Booking booking, Lottery lottery);

        Task<string> CheckCondition(ApplicationUser user, Lottery lottery);

    }
}
