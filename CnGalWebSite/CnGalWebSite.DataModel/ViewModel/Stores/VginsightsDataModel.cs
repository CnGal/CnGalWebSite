using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.DataModel.ViewModel.Stores
{
    public class VginsightsDataModel
    {
        public double price { get; set; }
        public double price_final { get; set; }
        public double rating { get; set; }
        public int reviews { get; set; }
        public int reviews_positive { get; set; }
        public int reviews_negative { get; set; }
        public int units_sold_vgi { get; set; }
        public string revenue_vgi { get; set; }
        public bool isReleased { get; set; }
    }
}
