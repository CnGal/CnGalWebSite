

using CnGalWebSite.DataModel.ExamineModel.PlayedGames;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.PlayedGames;
using CnGalWebSite.DataModel.ViewModel.Search;
using CnGalWebSite.DataModel.ViewModel.Tables;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.PlayedGames
{
    public interface IPlayedGameService
    {
       void  UpdatePlayedGameDataMain(PlayedGame playedGame, PlayedGameMain examine);

        void UpdatePlayedGameData(PlayedGame playedGame, Examine examine);

        Task<GameScoreTableModel> GetGameScores(int id);

        Task UpdateAllGameScore();
    }
}
