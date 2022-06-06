using CnGalWebSite.DataModel.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CnGalWebSite.DataModel.ViewModel.Admin
{ 
    public class ListPlayedGamesViewModel
    {
        public List<ListPlayedGameAloneModel> PlayedGames { get; set; } = new List<ListPlayedGameAloneModel> { };
    }
    public class ListPlayedGameAloneModel
    {
        [Display(Name = "Id")]
        public long Id { get; set; }
        [Display(Name = "游戏名称")]
        public string GameName { get; set; }
        [Display(Name = "游戏Id")]
        public int GameId { get; set; }
        [Display(Name = "用户名")]
        public string UserName { get; set; }
        [Display(Name = "用户Id")]
        public string UserId { get; set; }
        [Display(Name = "评语")]
        public string PlayImpressions { get; set; }
        [Display(Name = "类型")]
        public PlayedGameType? Type { get; set; }
        [Display(Name = "游玩时长")]
        public long PlayDuration { get; set; }
        [Display(Name = "是否在Steam库中")]
        public bool IsInSteam { get; set; }

        [Display(Name = "配音")]
        public int CVSocre { get; set; }
        [Display(Name = "程序")]
        public int SystemSocre { get; set; }
        [Display(Name = "演出")]
        public int ShowSocre { get; set; }
        [Display(Name = "美术")]
        public int PaintSocre { get; set; }
        [Display(Name = "剧本")]
        public int ScriptSocre { get; set; }
        [Display(Name = "音乐")]
        public int MusicSocre { get; set; }
        [Display(Name = "总分")]
        public int TotalSocre { get; set; }

        [Display(Name = "最后编辑时间")]
        public DateTime LastEditTime { get; set; }

        [Display(Name = "是否隐藏")]
        public bool IsHidden { get; set; }
        [Display(Name = "是否公开")]
        public bool ShowPublicly { get; set; }
    }

    public class PlayedGamesPagesInfor
    {
        public CnGalWebSite.DataModel.ViewModel.Search.QueryPageOptions Options { get; set; }
        public ListPlayedGameAloneModel SearchModel { get; set; }
    }
}
