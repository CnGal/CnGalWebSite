namespace CnGalWebSite.DataModel.ViewModel.Admin
{
    public class OverviewDataViewModel
    {
        public int TotalRegisterCount { get; set; }
        public int YesterdayRegisterCount { get; set; }
        public int TodayRegisterCount { get; set; }
        public int TodayOnlineCount { get; set; }

        public int TotalEntryCount { get; set; }
        public int YesterdayEditEntryCount { get; set; }
        public int TodayEditEntryCount { get; set; }

        public long TotalArticleCount { get; set; }
        public long YesterdayCreateArticleCount { get; set; }
        public long YesterdayEditArticleCount { get; set; }
        public long TodayCreateArticleCount { get; set; }
        public long TodayEditArticleCount { get; set; }

        public int TotalTagCount { get; set; }
        public int YesterdayEditTagCount { get; set; }
        public int TodayEditTagCount { get; set; }


        public long TotalExamineCount { get; set; }
        public long YesterdayTotalExamineCount { get; set; }
        public long TodayTotalExamineCount { get; set; }
        public long TotalExaminingCount { get; set; }


        public long TotalCommentCount { get; set; }
        public long YesterdayCommentCount { get; set; }
        public long TodayCommentCount { get; set; }

        public long TotalMessageCount { get; set; }
        public long YesterdayMessageCount { get; set; }
        public long TodayMessageCount { get; set; }

        public int TotalFileCount { get; set; }
        public long TotalFileSpace { get; set; }
        public int YesterdayFileCount { get; set; }
        public long YesterdayFileSpace { get; set; }
        public int TodayFileCount { get; set; }
        public long TodayFileSpace { get; set; }
    }
}
