using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.DataModel.Model
{
    public class ExpoTicket
    {
        public long Id { get; set; }

        /// <summary>
        /// 活动ID
        /// </summary>
        public long ActivityId { get; set; }

        /// <summary>
        /// 活动
        /// </summary>
        public ExpoActivity Activity { get; set; }

        /// <summary>
        /// 座位信息（x号厅x排x座）
        /// </summary>
        public string SeatInfo { get; set; }

        /// <summary>
        /// 昵称
        /// </summary>
        public string Nickname { get; set; }

        /// <summary>
        /// 编号数字
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 是否隐藏
        /// </summary>
        public bool Hidden { get; set; }
    }
}
