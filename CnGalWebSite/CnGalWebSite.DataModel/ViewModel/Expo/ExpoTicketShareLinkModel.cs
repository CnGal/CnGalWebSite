using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.DataModel.ViewModel.Expo
{
    public class ExpoTicketShareLinkModel
    {
        /// <summary>
        /// 加密的票根ID
        /// </summary>
        public string EncryptedId { get; set; }

        /// <summary>
        /// 访问令牌
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// 分享链接
        /// </summary>
        public string ShareUrl { get; set; }

        /// <summary>
        /// 票根基本信息
        /// </summary>
        public ExpoTicketShareInfo TicketInfo { get; set; }
    }

    public class ExpoTicketShareInfo
    {
        /// <summary>
        /// 票根ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 活动名称
        /// </summary>
        public string ActivityName { get; set; }

        /// <summary>
        /// 座位信息
        /// </summary>
        public string SeatInfo { get; set; }

        /// <summary>
        /// 昵称
        /// </summary>
        public string Nickname { get; set; }

        /// <summary>
        /// 编号
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// 票根图片
        /// </summary>
        public string Image { get; set; }
    }
}
