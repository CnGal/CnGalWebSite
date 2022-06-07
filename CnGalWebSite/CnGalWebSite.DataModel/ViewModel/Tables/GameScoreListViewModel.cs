using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CnGalWebSite.DataModel.ViewModel.Tables
{
    public class GameScoreListViewModel
    {
        public List<GameScoreTableModel> GameScores { get; set; }=new List<GameScoreTableModel> { };
    }

    public class GameScoreTableModel
    {
        public long Id { get; set; }
        [Display(Name = "Id")]
        public long GameId { get; set; }
        [Display(Name = "游戏名")]
        public string GameName { get; set; }

        [Display(Name = "配音（所有）")]
        public double AllCVSocre { get; set; }
        [Display(Name = "程序")]
        public double AllSystemSocre { get; set; }
        [Display(Name = "演出")]
        public double AllShowSocre { get; set; }
        [Display(Name = "美术")]
        public double AllPaintSocre { get; set; }
        [Display(Name = "剧本")]
        public double AllScriptSocre { get; set; }
        [Display(Name = "音乐")]
        public double AllMusicSocre { get; set; }
        [Display(Name = "总分")]
        public double AllTotalSocre { get; set; }

        [Display(Name = "配音（过滤后）")]
        public double FilterCVSocre { get; set; }
        [Display(Name = "程序")]
        public double FilterSystemSocre { get; set; }
        [Display(Name = "演出")]
        public double FilterShowSocre { get; set; }
        [Display(Name = "美术")]
        public double FilterPaintSocre { get; set; }
        [Display(Name = "剧本")]
        public double FilterScriptSocre { get; set; }
        [Display(Name = "音乐")]
        public double FilterMusicSocre { get; set; }
        [Display(Name = "总分")]
        public double FilterTotalSocre { get; set; }
    }
}
