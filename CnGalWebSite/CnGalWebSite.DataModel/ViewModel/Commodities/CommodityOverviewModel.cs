using CnGalWebSite.DataModel.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.DataModel.ViewModel.Commodities
{
    public class CommodityBaseModel
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string BriefIntroduction { get; set; }

        public int Price { get; set; }

        public string Image { get; set; }

        public CommodityType Type { get; set; }

        public string Value { get; set; }

        /// <summary>
        /// 是否隐藏
        /// </summary>
        public bool IsHidden { get; set; }

        /// <summary>
        /// 优先级
        /// </summary>
        public int Priority { get; set; }
    }

    public class CommodityOverviewModel: CommodityBaseModel
    {
        public int UserCount { get; set; }

        public DateTime LastEditTime { get; set; }

    }

    public class CommodityEditModel: CommodityBaseModel
    {

    }

    public class CommodityUserModel : CommodityBaseModel
    {
        public bool IsOwned { get; set; }
    }
}
