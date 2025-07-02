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
        /// 今日是否已签到
        /// </summary>
        public bool IsSignIn { get; set; }

        /// <summary>
        /// 签到总天数
        /// </summary>
        public int SignInDays { get; set; }

        /// <summary>
        /// 是否填写问卷
        /// </summary>
        public bool IsSurvey { get; set; }

        /// <summary>
        /// 是否给游戏评分
        /// </summary>
        public bool IsRateGame { get; set; }

        /// <summary>
        /// 是否绑定群聊QQ
        /// </summary>
        public bool IsBindQQ { get; set; }

        /// <summary>
        /// 是否更换默认头像
        /// </summary>
        public bool IsChangeAvatar { get; set; }

        /// <summary>
        /// 是否更换默认签名
        /// </summary>
        public bool IsChangeSignature { get; set; }

        /// <summary>
        /// 是否填写国G世代
        /// </summary>
        public bool IsSaveGGeneration { get; set; }

        /// <summary>
        /// 是否领取抽奖号码
        /// </summary>
        public bool IsLotteryNumber { get; set; }

        /// <summary>
        /// 总点数
        /// </summary>
        public int TotalPoints { get; set; }

        /// <summary>
        /// 抽奖次数（向下兼容，等于 TotalPoints / 100）
        /// </summary>
        public int LotteryCount => TotalPoints / 100;
    }
}
