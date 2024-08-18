using CnGalWebSite.DataModel.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.DataModel.ViewModel.Commodities
{
    public class CommodityCodeBaseModel
    {
        public long Id { get; set; }

        /// <summary>
        /// 兑换码
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 兑换的物品类型
        /// </summary>
        public CommodityCodeType Type { get; set; }

        /// <summary>
        /// 兑换的物品数量
        /// </summary>
        public int Count { get; set; }


        /// <summary>
        /// 是否可以兑换
        /// </summary>
        public bool CanRedeemed { get; set; } = true;

        /// <summary>
        /// 是否隐藏
        /// </summary>
        public bool Hide { get; set; }
    }

    public class CommodityCodeOverviewModel : CommodityCodeBaseModel
    {
        public string UserName { get; set; }

        public string UserId { get; set; }

        /// <summary>
        /// 是否已经兑换过了
        /// </summary>
        public bool Redeemed { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdateTime { get; set; }

        /// <summary>
        /// 兑换时间
        /// </summary>
        public DateTime? RedeemedTime { get; set; }
    }

    public class CommodityCodeEditModel : CommodityCodeBaseModel
    {

    }
}
