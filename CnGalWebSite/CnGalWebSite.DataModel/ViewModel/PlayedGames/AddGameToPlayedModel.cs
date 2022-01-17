using CnGalWebSite.DataModel.Model;
namespace CnGalWebSite.DataModel.ViewModel.PlayedGames
{
    public class AddGameToPlayedModel
    {
        public int GameId { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        public PlayedGameType Type { get; set; }
    }
}
