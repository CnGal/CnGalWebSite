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

        public int ObjectId { get; set; }
    }

    public enum PersonalRecommendDisplayType
    {
        PlainText
    }
}
