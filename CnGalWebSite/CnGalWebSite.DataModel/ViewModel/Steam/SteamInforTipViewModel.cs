using System;
using System.ComponentModel.DataAnnotations;

namespace CnGalWebSite.DataModel.ViewModel.Steam
{
    public class StoreInfoCardModel:StoreInfoViewModel
    {
        public int Id { get; set; }

        public string BriefIntroduction { get; set; }

        public string MainImage { get; set; }

        public DateTime? PublishTime { get; set; }
    }

    public enum PurchasedSteamType
    {
        [Display(Name = "全部")]
        All,
        [Display(Name = "已拥有")]
        Purchased,
        [Display(Name = "未拥有")]
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
