using CnGalWebSite.DataModel.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.DataModel.ViewModel.Commodities
{
    public class GCoinsRecordBaseModel
    {
        public DateTime Time { get; set; }

        public int Count { get; set; }

        public string Note { get; set; }

        public UserIntegralSourceType SourceType { get; set; }
    }

    public class GCoinsRecordOverviewModel : GCoinsRecordBaseModel
    {

    }
}
