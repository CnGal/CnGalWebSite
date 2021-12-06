namespace CnGalWebSite.DataModel.Model
{
    public class BackUpArchive
    {
        public long Id { get; set; }

        public DateTime LastBackUpTime { get; set; }

        public bool IsLastFail { get; set; }

        public double LastTimeUsed { get; set; }


        public List<BackUpArchiveDetail>? BackUpArchiveDetails { get; set; }

        public int? EntryId { get; set; }
        public Entry? Entry { get; set; }

        public long? ArticleId { get; set; }
        public Article? Article { get; set; }
    }

    public class BackUpArchiveDetail
    {
        public long Id { get; set; }

        public DateTime BackUpTime { get; set; }

        public bool IsFail { get; set; }
        /// <summary>
        /// 单位 秒
        /// </summary>
        public double TimeUsed { get; set; }

        public long? BackUpArchiveId { get; set; }
        public BackUpArchive? BackUpArchive { get; set; }


    }
}
