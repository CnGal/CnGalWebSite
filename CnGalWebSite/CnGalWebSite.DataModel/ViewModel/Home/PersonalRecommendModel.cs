using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.DataModel.ViewModel.Home
{
    public class PersonalRecommendModel
    {
        public PersonalRecommendDisplayType DisplayType { get; set; }

        public int Id { get; set; }
        public string SteamId { get; set; }

        public string Name { get; set; }
        public string MainPicture { get; set; }
        public string BriefIntroduction { get; set; }

        public GameReleaseViewModel Release { get; set; }

        /// <summary>
        /// 图片墙
        /// </summary>
        public List<PersonalRecommendImageCardModel> ImageCards { get; set; }=new List<PersonalRecommendImageCardModel>();

        /// <summary>
        /// 词条相册
        /// </summary>
        public List<string> Images { get; set; } = new List<string>();
        /// <summary>
        /// 标签
        /// </summary>
        public List<string> Tags { get; set; } = new List<string>();
    }
    public class PersonalRecommendEvaluationCardModel
    {
        public int Id { get; set; }
        public string Image { get; set; }
        public string Name { get; set; }
    }

    public class PersonalRecommendImageCardModel
    {
        public int Id { get; set; }
        public string Image { get; set; }
        public string Name { get; set; }
    }

    public enum PersonalRecommendDisplayType
    {
        PlainText,
        ImageGames,
        Gallery
    }
}
