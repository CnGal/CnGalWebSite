using CnGalWebSite.DataModel.Model;
using System;
namespace CnGalWebSite.DataModel.ExamineModel
{
    public class ArticleMain_1_0
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }

        public string BriefIntroduction { get; set; }

        public string MainPicture { get; set; }

        public string BackgroundPicture { get; set; }

        public string SmallBackgroundPicture { get; set; }

        public string OriginalAuthor { get; set; }
        public string OriginalLink { get; set; }
        public DateTime PubishTime { get; set; }

        public DateTime? RealNewsTime { get; set; }

        public ArticleType Type { get; set; }

        public string NewsType { get; set; }
    }
}
