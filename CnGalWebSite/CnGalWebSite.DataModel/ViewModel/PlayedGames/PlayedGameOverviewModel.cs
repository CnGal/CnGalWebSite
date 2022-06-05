using CnGalWebSite.DataModel.ViewModel.Search;
using CnGalWebSite.DataModel.ViewModel.Space;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CnGalWebSite.DataModel.ViewModel.PlayedGames
{
    public class PlayedGameOverviewModel
    {
        public EntryInforTipViewModel Game { get; set; } = new EntryInforTipViewModel();

        public PlayedGameScoreModel GameTotalScores { get; set; } = new PlayedGameScoreModel();

        public PlayedGameScoreModel GameReviewsScores { get; set; } = new PlayedGameScoreModel();

        public List<PlayedGameUserScoreModel> UserScores { get; set; } = new List<PlayedGameUserScoreModel>();

        /// <summary>
        /// 当前用户的评分是否公开
        /// </summary>
        public bool IsCurrentUserScorePublic { get; set; }
        public bool IsCurrentUserScoreExist { get; set; }
        public string CurrentUserId { get; set; }
    }

    public class PlayedGameUserScoreModel
    {
        public UserInforViewModel User { get; set; } = new UserInforViewModel();
        public PlayedGameScoreModel Socres { get; set; } = new PlayedGameScoreModel();

        public string PlayImpressions { get; set; }

        public DateTime LastEditTime { get; set; }



    }

    public class PlayedGameScoreModel
    {
        /// <summary>
        /// 配音
        /// </summary>
        public double CVSocre { get; set; }
        /// <summary>
        /// 程序
        /// </summary>
        public double SystemSocre { get; set; }
        /// <summary>
        /// 演出
        /// </summary>
        public double ShowSocre { get; set; }
        /// <summary>
        /// 美术
        /// </summary>
        public double PaintSocre { get; set; }
        /// <summary>
        /// 剧本
        /// </summary>
        public double ScriptSocre { get; set; }
        /// <summary>
        /// 音乐
        /// </summary>
        public double MusicSocre { get; set; }
        /// <summary>
        /// 总分
        /// </summary>
        public double TotalSocre { get; set; }

        public bool IsScored => MusicSocre != 0 && ShowSocre != 0 && TotalSocre != 0 && PaintSocre != 0 && ScriptSocre != 0 && CVSocre != 0 && SystemSocre != 0;
    }

    public enum PlayedGameScoreType
    {
        [Display(Name ="总评")]
        Total,
        [Display(Name = "配音")]
        CV,
        [Display(Name = "程序")]
        System,
        [Display(Name = "演出")]
        Show,
        [Display(Name = "美术")]
        Paint,
        [Display(Name = "剧本")]
        Script,
        [Display(Name = "音乐")]
        Music,

    }

    public enum PlayedGameCountScope
    {
        [Display(Name = "所有")]
        All,
        [Display(Name = "过滤后")]
        Review,
    }
}
