using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.DataModel.ViewModel.Stores
{
    public class SteamOfficialDataModel
    {
        public bool Success { get; set; }
        public SteamOfficialDataModelData Data { get; set; }
    }

    public class SteamOfficialDataModelData
    {
        public SteamOfficialDataModelPriceOverview Price_overview { get; set; }
    }

    public class SteamOfficialDataModelPriceOverview
    {
        public string Currency { get; set; }
        public int Initial { get; set; }
        public int Final { get; set; }
        public int Discount_percent { get; set; }
        public string Initial_formatted { get; set; }
        public string Final_formatted { get; set; }
    }
}
