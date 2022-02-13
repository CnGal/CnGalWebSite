using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Xml.Linq;

namespace CnGalWebSite.DataModel.ViewModel.Lotteries
{
    public class UserLotteryStateModel
    {
        public UserLotteryState State { get; set; }

        public int Number { get; set; }

        public LotteryAwardViewModel Award { get; set; }

    }

    public enum UserLotteryState
    {
        [Display(Name = "未登入")]
        NotLogin,
        [Display(Name = "未参与")]
        NotInvolved,
        [Display(Name = "等待开奖")]
        WaitingDraw,
        [Display(Name = "未中奖")]
        NotWin,
        [Display(Name = "中奖")]
        Win,
        [Display(Name = "等待填写收货地址")]
        WaitAddress,
        [Display(Name = "等待发货")]
        WaitShipments,
        [Display(Name = "已发货")]
        Shipped,
    }

}
