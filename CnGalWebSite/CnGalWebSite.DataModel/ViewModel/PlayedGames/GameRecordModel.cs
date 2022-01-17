using CnGalWebSite.DataModel.Model;

namespace CnGalWebSite.DataModel.ViewModel.PlayedGames
{
    public class GameRecordModel
    {
        /// <summary>
        /// 类型
        /// </summary>
        public PlayedGameType Type { get; set; }
        /// <summary>
        /// 游玩时长 分钟
        /// </summary>
        public long PlayDuration { get; set; }
        /// <summary>
        /// 是否在Steam库中
        /// </summary>
        public bool IsInSteam { get; set; }

        /// <summary>
        /// 游戏名称
        /// </summary>
        public int GameId { get; set; }
        /// <summary>
        /// 游戏名称
        /// </summary>
        public string GameName { get; set; }
        /// <summary>
        /// 游戏主图
        /// </summary>
        public string GameImage { get; set; }
        /// <summary>
        /// 游戏简介
        /// </summary>
        public string BriefIntroduction { get; set; }

    }
}
