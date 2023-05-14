using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.DataModel.Model
{
    public class StoreInfo
    {
        public long Id { get; set; }

        /// <summary>
        /// 平台类型
        /// </summary>
        public PublishPlatformType PlatformType { get; set; }

        /// <summary>
        /// 平台名称
        /// </summary>
        public string PlatformName { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 链接
        /// </summary>
        public string Link { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public StoreState State { get; set; }

        /// <summary>
        /// 币种
        /// </summary>
        public CurrencyCode CurrencyCode { get; set; }

        /// <summary>
        /// 信息更新方式
        /// </summary>
        public StoreUpdateType UpdateType { get; set; }

        /// <summary>
        /// 原价
        /// </summary>
        public double? OriginalPrice { get; set; }

        /// <summary>
        /// 现价
        /// </summary>
        public double? PriceNow { get; set; }

        /// <summary>
        /// 折扣
        /// </summary>
        public double? CutNow { get; set; }

        /// <summary>
        /// 史低
        /// </summary>
        public double? PriceLowest { get; set; }

        /// <summary>
        /// 最高折扣
        /// </summary>
        public double? CutLowest { get; set; }

        /// <summary>
        /// 平均游玩时长 分钟
        /// </summary>
        public int? PlayTime { get; set; }

        /// <summary>
        /// 评测数
        /// </summary>
        public int? EvaluationCount { get; set; }
        /// <summary>
        /// 好评率
        /// </summary>
        public double? RecommendationRate { get; set; }

        /// <summary>
        /// 估计拥有人数 上限
        /// </summary>
        public int? EstimationOwnersMax { get; set; }
        /// <summary>
        /// 估计拥有人数 下限
        /// </summary>
        public int? EstimationOwnersMin { get; set; }

        /// <summary>
        /// 关联的游戏
        /// </summary>
        public Entry Entry { get; set; }
        public int? EntryId { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdateTime { get; set; }
    }

    public enum CurrencyCode {
        [Display(Name = "人民币")]
        CNY,
        [Display(Name = "日元")]
        JPY,
        [Display(Name = "美元")]
        USD,
        [Display(Name = "港元")]
        HKD,
        [Display(Name = "新台币")]
        TWD,
    }

    public enum StoreState
    {
        [Display(Name = "缺省")]
        None,
        [Display(Name = "未发布")]
        NotPublished,
        [Display(Name = "已发布")]
        OnSale,
        [Display(Name = "已下架")]
        Takedown
    }

    public enum StoreUpdateType
    {
        [Display(Name = "自动")]
        Automatic,
        [Display(Name = "手动")]
        Manual,
    }

}
