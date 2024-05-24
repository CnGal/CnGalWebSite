using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.DataModel.ViewModel.Stores
{
    public class XiaoHeiHeDataModel
    {
        public string Status { get; set; }
        public XiaoHeiHeDataModelResult Result { get; set; }
    }

    public class XiaoHeiHeDataModelResult
    {
        public bool Is_free { get; set; }
        public string Positive_desc { get; set; }
        public XiaoHeiHeDataModelPrice Price { get; set; }
    }
    public class XiaoHeiHeDataModelPrice
    {
        public double Current { get; set; }
        public double Initial { get; set; }
        public double Lowest_price_raw { get; set; }
        public int Lowest_discount { get; set; }
        public int Discount { get; set; }
    }

}
