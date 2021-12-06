using CnGalWebSite.DataModel.Model;


namespace CnGalWebSite.DataModel.ExamineModel
{
    public class EntryMain
    {
        public string Name { get; set; }

        public string DisplayName { get; set; }
        /// <summary>
        /// 别称
        /// </summary>
        public string AnotherName { get; set; }

        public string BriefIntroduction { get; set; }

        public string MainPicture { get; set; }

        public string Thumbnail { get; set; }

        public string BackgroundPicture { get; set; }

        public string SmallBackgroundPicture { get; set; }

        public EntryType Type { get; set; }
    }
}
