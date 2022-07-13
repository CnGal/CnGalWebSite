using CnGalWebSite.DataModel.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CnGalWebSite.DataModel.ViewModel.Lotteries
{
    public class WinnerDataModel
    {
        public string UserId { get; set; }
        [Display(Name = "号码")]
        public int Number { get; set; }
        [Display(Name = "用户名")]
        public string UserName { get; set; }
        [Display(Name = "电子邮箱")]
        public string Email { get; set; }
        [Display(Name = "奖品")]
        public string AwardName { get; set; }
        [Display(Name = "类型")]
        public LotteryAwardType AwardType { get; set; }

        public long AwardId { get; set; }
        [Display(Name = "激活码")]
        public string ActivationCode { get; set; }
        [Display(Name = "快递单号")]
        public string TrackingNumber { get; set; }
        [Display(Name = "真实姓名")]
        public string RealName { get; set; }
        [Display(Name = "地址")]
        public string Address { get; set; }
        [Display(Name = "电话号码")]
        public string Phone { get; set; }

        public long PrizeId { get; set; }
    }
}
