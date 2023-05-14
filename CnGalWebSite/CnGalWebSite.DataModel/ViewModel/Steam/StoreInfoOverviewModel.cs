using CnGalWebSite.DataModel.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.DataModel.ViewModel.Steam
{
    public class StoreInfoOverviewModel
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
        /// 信息更新方式
        /// </summary>
        public StoreUpdateType UpdateType { get; set; }

        /// <summary>
        /// 现价
        /// </summary>
        public double? PriceNow { get; set; }

        /// <summary>
        /// 折扣
        /// </summary>
        public double? CutNow { get; set; }

        /// <summary>
        /// 关联的游戏
        /// </summary>
        public int? EntryId { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdateTime { get; set; }

    }
}
