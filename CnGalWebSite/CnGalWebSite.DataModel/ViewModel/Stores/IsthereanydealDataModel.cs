using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.DataModel.ViewModel.Stores
{
    public class IsthereanydealGetIdModel
    {
        public bool found { get; set; }
        public IsthereanydealGameInfo game { get; set; }
    }

    public class IsthereanydealGameInfo
    {
        public string id { get; set; }
    }


    public class IsthereanydealDataModel
    {
        public List<IsthereanydealPriceInfo> prices { get; set; }
    }

    public class IsthereanydealPriceInfo
    {
        public IsthereanydealCurrentPrice current { get; set; }
        public IsthereanydealLowestPrice lowest { get; set; }
    }

    public class IsthereanydealCurrentPrice
    {
        public IsthereanydealPrice price { get; set; }
        public IsthereanydealPrice regular { get; set; }
        public int cut { get; set; }
    }

    public class IsthereanydealLowestPrice
    {
        public IsthereanydealPrice price { get; set; }
        public IsthereanydealPrice regular { get; set; }
        public int cut { get; set; }
    }

    public class IsthereanydealPrice
    {
        public double amount { get; set; }
        public int amountInt { get; set; }
        public string currency { get; set; }
    }
}
