using System;
using System.ComponentModel.DataAnnotations;
namespace CnGalWebSite.DataModel.Model
{
    public class PlayedGame
    {
        public long Id { get; set; }

        //对游戏的评分也在这里储存

        /// <summary>
        /// 配音
        /// </summary>
        [Obsolete("该分数弃用")]
        public int CVSocre { get; set; }
        /// <summary>
        /// 演出
        /// </summary>
        public int ShowSocre { get; set; }
        /// <summary>
        /// 系统
        /// </summary>
        [Obsolete("该分数弃用")]
        public int SystemSocre { get; set; }
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

        public bool IsScored => MusicSocre != 0 && ShowSocre != 0 && TotalSocre != 0 && PaintSocre != 0 && ScriptSocre != 0;
        /// <summary>
        /// 评分时间 创建时间
        /// </summary>
        public DateTime ScoreTime { get; set; }
        /// <summary>
        /// 最后编辑时间
        /// </summary>
        public DateTime LastEditTime { get; set; }
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
        /// 是否对自己隐藏
        /// </summary>
        public bool IsHidden { get; set; }
        /// <summary>
        /// 是否向他人公开
        /// </summary>
        public bool ShowPublicly { get; set; }
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
        Uninterested,
        [Display(Name = "未玩")]
        UnPlayed
    }
}
