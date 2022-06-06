using BootstrapBlazor.Components;
using CnGalWebSite.DataModel.Application.Dtos;
using CnGalWebSite.DataModel.ExamineModel;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Search;
using CnGalWebSite.DataModel.ViewModel.Tables;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.PlayedGames
{
    public interface IPlayedGameService
    {
        Task<QueryData<ListPlayedGameAloneModel>> GetPaginatedResult(DataModel.ViewModel.Search.QueryPageOptions options, ListPlayedGameAloneModel searchModel);

       void  UpdatePlayedGameDataMain(PlayedGame playedGame, PlayedGameMain examine);

        void UpdatePlayedGameData(PlayedGame playedGame, Examine examine);

        Task<GameScoreTableModel> GetGameScores(int id);

        Task UpdateAllGameScore();
    }
}
