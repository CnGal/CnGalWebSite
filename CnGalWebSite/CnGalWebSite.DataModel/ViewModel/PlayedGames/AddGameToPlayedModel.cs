using CnGalWebSite.DataModel.Model;
using System;
 using System.Collections.Generic;
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
