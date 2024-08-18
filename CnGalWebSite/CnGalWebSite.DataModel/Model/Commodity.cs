using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.DataModel.Model
{
    public class Commodity
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string BriefIntroduction { get; set; }

        public int Price { get; set; }

        public string Image { get; set; }

        public CommodityType Type { get; set; }

        public string Value { get; set; }

        public DateTime LastEditTime { get; set; }

        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 是否隐藏
        /// </summary>
        public bool IsHidden { get; set; }

        /// <summary>
        /// 优先级
        /// </summary>
        public int Priority { get; set; }

        public virtual ICollection<ApplicationUser> Users { get; set; }
        /// <summary>
        /// 多对多关系映射虚拟字段
        /// </summary>
        public virtual ICollection<ApplicationUserCommodity> ApplicationUserCommodities { get; set; }
    }

    public class ApplicationUserCommodity
    {
        public long Id { get; set; }

        public long CommodityId { get; set; }
        public string ApplicationUserId { get; set; }

        public Commodity Commodity { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
    }

    public enum CommodityType
    {
        [Display(Name = "衣服")]
        Clothes,
        [Display(Name = "丝袜")]
        Stockings,
        [Display(Name = "鞋子")]
        Shoes
    }


    public class CommodityCode
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
        /// 是否已经兑换过了
        /// </summary>
        public bool Redeemed { get; set; }

        /// <summary>
        /// 是否可以兑换
        /// </summary>
        public bool CanRedeemed { get; set; }

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


        /// <summary>
        /// 是否隐藏
        /// </summary>
        public bool Hide { get; set; }

        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
    }

    public enum CommodityCodeType
    {
        [Display(Name = "G币")]
        GCoins
    }
}
