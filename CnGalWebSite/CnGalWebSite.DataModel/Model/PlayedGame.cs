using System;
using System.ComponentModel.DataAnnotations;
namespace CnGalWebSite.DataModel.Model
{
    public class PlayedGame
    {
        public long Id { get; set; }

        //对游戏的评分也在这里储存
        [Range(0, 5, ErrorMessage = "请在1-5范围评分")]
        public int CVSocre { get; set; }
        [Range(0, 5, ErrorMessage = "请在1-5范围评分")]
        public int ShowSocre { get; set; }
        [Range(0, 5, ErrorMessage = "请在1-5范围评分")]
        public int SystemSocre { get; set; }
        [Range(0, 5, ErrorMessage = "请在1-5范围评分")]
        public int PaintSocre { get; set; }
        [Range(0, 5, ErrorMessage = "请在1-5范围评分")]
        public int ScriptSocre { get; set; }

        public bool IsScored => CVSocre != 0 && ShowSocre != 0 && SystemSocre != 0 && PaintSocre != 0 && ScriptSocre != 0;
        /// <summary>
        /// 评分时间
        /// </summary>
        public DateTime ScoreTime { get; set; }
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
        /// 游玩感想
        /// </summary>
        public string PlayImpressions { get; set; }

        public Entry Entry { get; set; }
        public int? EntryId { get; set; }

        public ApplicationUser ApplicationUser { get; set; }
        public string ApplicationUserId { get; set; }

    }

    public enum PlayedGameType
    {
        [Display(Name = "想玩")]
        WantToPlay,
        [Display(Name = "正在玩")]
        Playing,
        [Display(Name = "已玩")]
        Played,
        [Display(Name = "不感兴趣")]
        Uninterested
    }
}
