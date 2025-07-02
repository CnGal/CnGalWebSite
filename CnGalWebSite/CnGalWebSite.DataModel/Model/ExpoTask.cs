using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.DataModel.Model
{
    public class ExpoTask
    {
        public long Id { get; set; }

        public ExpoTaskType Type { get; set; }

        public DateTime Time { get; set; }

        /// <summary>
        /// 抽奖次数
        /// </summary>
        public int LotteryCount { get; set; }

        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
    }

    public enum ExpoTaskType
    {
        [Display(Name = "签到")]
        SignIn,
        [Display(Name = "预约直播")]
        Booking,
        [Display(Name = "分享游戏库")]
        ShareGames,
        [Display(Name = "抽奖")]
        Lottery,
        [Display(Name = "填写问卷")]
        Survey,
        [Display(Name = "给游戏评分")]
        RateGame,
        [Display(Name = "绑定群聊QQ")]
        BindQQ,
        [Display(Name = "更换默认头像")]
        ChangeAvatar,
        [Display(Name = "更换默认签名")]
        ChangeSignature,
        [Display(Name = "填写国G世代")]
        SaveGGeneration,
        [Display(Name = "领取抽奖号码")]
        LotteryNumber
    }
}
