using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.DataModel.Model
{
    public class ExpoActivity
    {
        public long Id { get; set; }

        /// <summary>
        /// 活动名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 前景图片
        /// </summary>
        public string ForegroundImage { get; set; }

        /// <summary>
        /// 背景图片
        /// </summary>
        public string BackgroundImage { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 优先级
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// 是否隐藏
        /// </summary>
        public bool Hidden { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 活动关联的票根
        /// </summary>
        public List<ExpoTicket> Tickets { get; set; } = new List<ExpoTicket>();
    }
}
