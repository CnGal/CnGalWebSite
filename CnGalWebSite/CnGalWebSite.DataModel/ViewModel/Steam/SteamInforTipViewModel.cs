using System;
using System.ComponentModel.DataAnnotations;

namespace CnGalWebSite.DataModel.ViewModel.Steam
{
    public class SteamInforTipViewModel
    {
        public string Name { get; set; }

        public string BriefIntroduction { get; set; }

        public string MainImage { get; set; }

        public DateTime? PublishTime { get; set; }

        public int SteamId { get; set; }

        public int OriginalPrice { get; set; }

        public int PriceNow { get; set; }
        public string PriceNowString { get; set; }
        public int CutNow { get; set; }

        public int PriceLowest { get; set; }
        public string PriceLowestString { get; set; }
        public int CutLowest { get; set; }

        public double PlayTime { get; set; }

        /// <summary>
        /// 评测数
        /// </summary>
        public int EvaluationCount { get; set; }
        /// <summary>
        /// 好评率
        /// </summary>
        public int RecommendationRate { get; set; }

        /// <summary>
        /// 词条Id
        /// </summary>
        public int Id { get; set; }

        public DateTime LowestTime { get; set; }
    }

    public enum PurchasedSteamType
    {
        [Display(Name = "全部")]
        All,
        [Display(Name = "已入库")]
        Purchased,
        [Display(Name = "未入库")]
        UnPurchased
    }

    public enum ScreenSteamType
    {
        [Display(Name = "全部")]
        All,
        [Display(Name = "平史低")]
        FlatHistoryLow,
        [Display(Name = "新史低")]
        NewHistoryLow
    }

    public enum SteamDisplayType
    {
        [Display(Name = "条幅")]
        LongCard,
        [Display(Name = "大卡片")]
        LargeCard
    }

    public enum SteamSortType
    {
        [Display(Name = "折扣")]
        Discount,
        [Display(Name = "价格")]
        Price,
        [Display(Name = "发行时间")]
        PublishTime,
        [Display(Name = "评测数")]
        EvaluationCount,
        [Display(Name = "好评率")]
        RecommendationRate,
    }
}
