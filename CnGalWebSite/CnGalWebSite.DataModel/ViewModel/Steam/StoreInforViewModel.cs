using CnGalWebSite.DataModel.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace CnGalWebSite.DataModel.ViewModel.Steam
{
    public class StoreInforViewModel
    {
        /// <summary>
        /// 发行平台类型
        /// </summary>
        public PublishPlatformType PublishPlatformType { get; set; }

        /// <summary>
        /// 平台Id
        /// </summary>
        public string StoreId { get; set; }

        /// <summary>
        /// 关联的游戏
        /// </summary>
        public int EntryId { get; set; }

        public int OriginalPrice { get; set; }

        public int PriceNow { get; set; }
        public string PriceNowString { get; set; }
        public int CutNow { get; set; }

        /// <summary>
        /// 平均游玩时长 小时 不用分钟是因为之前就是double
        /// </summary>
        public double PlayTime { get; set; }

        public int PriceLowest { get; set; }
        public string PriceLowestString { get; set; }
        public int CutLowest { get; set; }

        /// <summary>
        /// 评测数
        /// </summary>
        public int EvaluationCount { get; set; }
        /// <summary>
        /// 好评率
        /// </summary>
        public int RecommendationRate { get; set; }

        /// <summary>
        /// 估计拥有人数 上限
        /// </summary>
        public int EstimationOwnersMax { get; set; }
        /// <summary>
        /// 估计拥有人数 下限
        /// </summary>
        public int EstimationOwnersMin { get; set; }

        public DateTime LowestTime { get; set; }
        public DateTime UpdateTime { get; set; }
    }
}
