using System;
using System.Collections.Generic;
using System.Text;

namespace CnGalWebSite.DataModel.ExamineModel.PlayedGames
{
    public class PlayedGameMain
    {
        /// <summary>
        /// 游玩感想
        /// </summary>
        public string PlayImpressions { get; set; }
        /// <summary>
        /// 是否向他人公开
        /// </summary>
        public bool ShowPublicly { get; set; }
    }
}
