namespace CnGalWebSite.DataModel.ViewModel.PlayedGames
{
    public class PlayedGameInforModel
    {
        public int Id { get; set; }

        public int PlayedCount { get; set; }
        /// <summary>
        /// 当前用户是否已经将此游戏添加到已玩列表中
        /// </summary>
        public bool IsCurrentUserPlayed { get; set; }
        /// <summary>
        /// 当前用户是否已经为其评分
        /// </summary>
        public bool IsCurrentUserScored { get; set; }
        //当前用户评分
        public int CVSocreCurrent { get; set; }

        public int ShowSocreCurrent { get; set; }

        public int SystemSocreCurrent { get; set; }

        public int PaintSocreCurrent { get; set; }

        public int ScriptSocreCurrent { get; set; }

        //平均分
        public double CVSocreAverage { get; set; }

        public double ShowSocreAverage { get; set; }

        public double SystemSocreAverage { get; set; }

        public double PaintSocreAverage { get; set; }

        public double ScriptSocreAverage { get; set; }

        //比例分
        public double CVSocreProportion { get; set; }

        public double ShowSocreProportion { get; set; }

        public double SystemSocreProportion { get; set; }

        public double PaintSocreProportion { get; set; }

        public double ScriptSocreProportion { get; set; }
        /// <summary>
        /// 是否存在有效评分
        /// </summary>
        public bool IsScoreEffective { get; set; }
    }
}
