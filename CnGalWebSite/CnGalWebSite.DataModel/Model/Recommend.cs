using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.DataModel.Model
{
    public class Recommend
    {
        public long Id { get; set; }

        public int EntryId { get;set; }
        public Entry Entry { get; set; }

        public RecommendReason Reason { get; set; }

        /// <summary>
        /// 是否隐藏
        /// </summary>
        public bool IsHidden { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdateTime { get; set; }
    }

    public enum RecommendReason
    {
        [Display(Name = "经典作品")]
        Classics,
        [Display(Name = "新史低")]
        NewHistoryLow,
        [Display(Name = "冷门佳作")]
        UnpopularMasterpiece,
        [Display(Name = "好评率高")]
        HighPraiseRate,
        [Display(Name = "本站热门")]
        Popular,
        [Display(Name = "缺省")]
        None,
    }
}
