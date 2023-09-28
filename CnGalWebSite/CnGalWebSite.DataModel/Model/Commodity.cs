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
        public virtual ICollection<CommodityApplicationUser> CommodityApplicationUsers { get; set; }
    }

    public class CommodityApplicationUser
    {
        public long Id { get; set; }

        public long CommodityId { get; set; }
        public string ApplicationUserId { get; set; }

        public Commodity Commodity { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
    }

    public enum CommodityType
    {
        [Display(Name ="衣服")]
        Clothes,
        [Display(Name = "丝袜")]
        Stockings,
        [Display(Name = "鞋子")]
        Shoes
    }
}
