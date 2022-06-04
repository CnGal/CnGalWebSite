using CnGalWebSite.DataModel.Application.Dtos;
using CnGalWebSite.DataModel.ExamineModel;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Search;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.PlayedGames
{
    public interface IPlayedGameService
    {
        Task<PagedResultDto<EntryInforTipViewModel>> GetPaginatedResult(PagedSortedAndFilterInput input);

       void  UpdatePlayedGameDataMain(PlayedGame playedGame, PlayedGameMain examine);

        void UpdateArticleData(PlayedGame playedGame, Examine examine);
    }
}
