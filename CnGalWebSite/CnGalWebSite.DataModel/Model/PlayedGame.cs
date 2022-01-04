using System.ComponentModel.DataAnnotations;

using System;
 using System.Collections.Generic;
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

        public Entry Entry { get; set; }
        public int? EntryId { get; set; }

        public ApplicationUser ApplicationUser { get; set; }
        public string ApplicationUserId { get; set; }

    }
}
