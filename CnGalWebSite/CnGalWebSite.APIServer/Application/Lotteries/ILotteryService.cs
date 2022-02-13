using BootstrapBlazor.Components;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Lotteries;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.Lotteries
{
    public interface ILotteryService
    {
        Task SendPrizeToWinningUser(LotteryUser user, LotteryAward award);

        Task<QueryData<ListLotteryAloneModel>> GetPaginatedResult(CnGalWebSite.DataModel.ViewModel.Search.QueryPageOptions options, ListLotteryAloneModel searchModel);

        Task<QueryData<ListLotteryUserAloneModel>> GetPaginatedResult(CnGalWebSite.DataModel.ViewModel.Search.QueryPageOptions options, ListLotteryUserAloneModel searchModel, long lotteryId); 

        Task DrawAllLottery();
    }
}
