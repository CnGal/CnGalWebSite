using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.DataModel.ViewModel.Stores
{
    public class GameRevenueInfoCacheModel
    {
        public int Max { get; set; }
        public int Page { get; set; }
        public int Year { get; set; }
    }

    public class GameRevenueInfoViewModel
    {
        public List<GameRevenueInfoCardModel> Items { get; set; } = new List<GameRevenueInfoCardModel>();

        public int TotalPages { get; set; }
    }

    public class GameRevenueInfoCardModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Publisher { get; set; }

        public int Index { get; set; }

        /// <summary>
        /// 评测数
        /// </summary>
        public int EvaluationCount { get; set; }

        /// <summary>
        /// 好评率
        /// </summary>
        public double RecommendationRate { get; set; }

        /// <summary>
        /// 平均游玩时长 分钟
        /// </summary>
        public int PlayTime { get; set; }

        /// <summary>
        /// 销售额 单位 人民币
        /// </summary>
        public int Revenue { get; set; }

        /// <summary>
        /// 估计拥有人数
        /// </summary>
        public int Owner { get; set; }

        public double Price { get; set; }

        public string MainImage { get; set; }

        public DateTime? PublishTime { get; set; }
    }
}
