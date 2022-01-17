using System;
namespace CnGalWebSite.DataModel.Model
{
    public class PerfectionOverview
    {
        public long Id { get; set; }

        public DateTime LastUpdateTime { get; set; }

        public int ToBeImprovedCount { get; set; }
        public int GoodCount { get; set; }
        public int ExcellentCount { get; set; }

        /// <summary>
        /// 平均值
        /// </summary>
        public double AverageValue { get; set; }
        /// <summary>
        /// 中位数
        /// </summary>
        public double Median { get; set; }
        /// <summary>
        /// 众数
        /// </summary>
        public int Mode { get; set; }
        /// <summary>
        /// 标准差
        /// </summary>
        public double StandardDeviation { get; set; }
    }
}
