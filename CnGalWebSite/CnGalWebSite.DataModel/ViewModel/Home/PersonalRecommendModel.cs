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

        public string Name { get; set; }
        public string MainPicture { get; set; }
        public string BriefIntroduction { get; set; }

        public GameReleaseViewModel Release { get; set; }
    }

    public enum PersonalRecommendDisplayType
    {
        PlainText
    }
}
