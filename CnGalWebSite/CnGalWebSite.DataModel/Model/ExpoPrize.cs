using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.DataModel.Model
{
    public class ExpoAward
    {
        public long Id { get; set; }

        public ExpoAwardType Type { get; set; }

        public int Count { get; set; }

        public List<ExpoPrize> Prizes { get; set; }

        public string Image { get; set; }

        public string Name { get; set; }

        public string Url { get; set; }
    }

    public enum ExpoAwardType
    {
        [Display(Name = "激活码")]
        Key,
        [Display(Name = "无实体")]
        NoEntry
    }

    public class ExpoPrize
    {
        public long Id { get; set; }

        public string Content { get; set; }

        public string? ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        public long AwardId { get; set; }
        public ExpoAward Award { get; set; }

        public DateTime DrawTime { get; set; }
    }
}
