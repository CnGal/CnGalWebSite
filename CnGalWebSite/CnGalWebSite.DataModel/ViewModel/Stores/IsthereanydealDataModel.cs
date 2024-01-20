using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.DataModel.ViewModel.Stores
{
    public class IsthereanydealDataModel
    {
        public IsthereanydealDataModelPrice Price { get; set; }
        public IsthereanydealDataModelPriceLowest Lowest { get; set; }
    }


    public class IsthereanydealDataModelPrice
    {
        public double Price { get; set; }
        public int Cut { get; set; }
        public string Price_formatted { get; set; }
    }

    public class IsthereanydealDataModelPriceLowest
    {
        public double Price { get; set; }
        public int Cut { get; set; }
        public string Price_formatted { get; set; }
        public long Recorded { get; set; }
    }
}
