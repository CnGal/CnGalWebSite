using CnGalWebSite.DataModel.Model;
using System;
using System.ComponentModel.DataAnnotations;

namespace CnGalWebSite.DataModel.ViewModel.PlayedGames
{
    public class EditGameRecordModel
    {

        public int GameId { get; set; }

        public bool IsHidden { get; set; }

        [Display(Name = "类型")]
        public PlayedGameType Type { get; set; }

        [Display(Name = "感想")]
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

    }
}
