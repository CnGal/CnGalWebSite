using CnGalWebSite.DataModel.Model;

namespace CnGalWebSite.DataModel.ViewModel.PlayedGames
{
    public class GameRecordViewModel
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
        public string GameBriefIntroduction { get; set; }
        /// <summary>
        /// 评语
        /// </summary>
        public string PlayImpressions { get; set; }

        /// <summary>
        /// 配音
        /// </summary>
        public int CVSocre { get; set; }
        /// <summary>
        /// 程序
        /// </summary>
        public int SystemSocre { get; set; }
        /// <summary>
        /// 演出
        /// </summary>
        public int ShowSocre { get; set; }
        /// <summary>
        /// 美术
        /// </summary>
        public int PaintSocre { get; set; }
        /// <summary>
        /// 剧本
        /// </summary>
        public int ScriptSocre { get; set; }
        /// <summary>
        /// 音乐
        /// </summary>
        public int MusicSocre { get; set; }
        /// <summary>
        /// 总分
        /// </summary>
        public int TotalSocre { get; set; }

        /// <summary>
        /// 是否向他人公开
        /// </summary>
        public bool ShowPublicly { get; set; } = true;

        public bool IsScored => MusicSocre != 0 && ShowSocre != 0 && TotalSocre != 0 && PaintSocre != 0 && ScriptSocre != 0 && CVSocre != 0 && SystemSocre != 0;

        public bool IsHidden { get; set; }
    }
}
