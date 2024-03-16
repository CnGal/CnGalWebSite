using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.DataModel.ViewModel.Stores
{
    public class TapTapDataModel
    {
        public TapTapRedirectModel Redirect { get; set; }

        public TapTapDataDataModel Data { get; set; }
    }

    public class TapTapRedirectModel
    {
        public string web_url { get; set; }
    }

    public class TapTapDataDataModel
    {
        public int Code { get; set; }

        public TapTapDataStaModel Stat { get; set; }

        public TapTapDataPriceModel Price { get; set; }
    }

    public class TapTapDataStaModel
    {
        public int bought_count { get; set; }
        public int hits_total { get; set; }
        public int review_count { get; set; }

        public TapTapStatRatingMode Rating { get; set; }
    }

    public class TapTapDataPriceModel
    {
        public string taptap_current { get; set; }
        public string taptap_original { get; set; }
        public int discount_rate { get; set; }
    }

    public class TapTapStatRatingMode
    {
        public int Max { get; set; }
        public string Score { get; set; }
    }
}
