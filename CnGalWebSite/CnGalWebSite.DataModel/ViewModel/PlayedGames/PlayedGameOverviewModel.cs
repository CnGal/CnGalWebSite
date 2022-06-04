using CnGalWebSite.DataModel.ViewModel.Search;
using CnGalWebSite.DataModel.ViewModel.Space;
using System;
using System.Collections.Generic;
using System.Text;

namespace CnGalWebSite.DataModel.ViewModel.PlayedGames
{
    public class PlayedGameOverviewModel
    {
        public EntryInforTipViewModel Game { get; set; } = new EntryInforTipViewModel();

        public PlayedGameScoreModel GameTotalScores { get; set; } = new PlayedGameScoreModel();

        public PlayedGameScoreModel GameReviewsScores { get; set; } = new PlayedGameScoreModel();

        public List<PlayedGameUserScoreModel> UserScores { get; set; } = new List<PlayedGameUserScoreModel>();

        public bool MyRecordExist { get; set; }

        public bool MyRecordPublic { get; set; }

        public PlayedGameUserScoreModel MyScores { get; set; } = new PlayedGameUserScoreModel();
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

        public bool IsScored => MusicSocre != 0 && ShowSocre != 0 && TotalSocre != 0 && PaintSocre != 0 && ScriptSocre != 0;
    }
}
