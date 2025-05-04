using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.DataModel.ViewModel.Expo
{
    public class ExpoUserTaskModel
    {
        /// <summary>
        /// 是否已预约
        /// </summary>
        public bool IsBooking { get; set; }

        /// <summary>
        /// 是否绑定Steam
        /// </summary>
        public bool IsSharedGames { get; set; }

        /// <summary>
        /// 是否领取了绑定Steam的奖励
        /// </summary>
        public bool IsPickUpSharedGames { get; set; }

        /// <summary>
        /// 是否已签到
        /// </summary>
        public bool IsSignIn { get; set; }

        /// <summary>
        /// 抽奖次数
        /// </summary>
        public int LotteryCount { get; set; }
    }
}
