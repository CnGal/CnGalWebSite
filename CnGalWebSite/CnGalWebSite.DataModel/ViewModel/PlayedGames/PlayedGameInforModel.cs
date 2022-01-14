using CnGalWebSite.DataModel.Model;
using System;
 using System.Collections.Generic;
namespace CnGalWebSite.DataModel.ViewModel.PlayedGames
{
    public class PlayedGameInforModel
    {
        public int GameId { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        public PlayedGameType? Type { get; set; }
        /// <summary>
        /// 游玩时长 分钟
        /// </summary>
        public long PlayDuration { get; set; }
        /// <summary>
        /// 是否在Steam库中
        /// </summary>
        public bool IsInSteam { get; set; }
    }
}
