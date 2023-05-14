using CnGalWebSite.DataModel.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.DataModel.ViewModel.Steam
{
    public class StoreInfoEditModel:StoreInfoOverviewModel
    {
        /// <summary>
             /// 币种
             /// </summary>
        public CurrencyCode CurrencyCode { get; set; }
        /// <summary>
        /// 原价
        /// </summary>
        public double? OriginalPrice { get; set; }

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
    }
}
