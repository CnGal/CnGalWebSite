using CnGalWebSite.DataModel.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CnGalWebSite.DataModel.ViewModel.Lotteries
{
    public class WinnerOverviewModel
    {
        /// <summary>
        /// 用户Id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 号码
        /// </summary>
        public int Number { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 电子邮箱
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// 奖品
        /// </summary>
        public string AwardName { get; set; }
        /// <summary>
        /// 类型
        /// </summary>
        public LotteryAwardType AwardType { get; set; }

        public long AwardId { get; set; }
        /// <summary>
        /// 激活码
        /// </summary>
        public string ActivationCode { get; set; }
        /// <summary>
        /// 快递单号
        /// </summary>
        public string TrackingNumber { get; set; }
        /// <summary>
        /// 真实姓名
        /// </summary>
        public string RealName { get; set; }
        /// <summary>
        /// 地址
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// 电话号码
        /// </summary>
        public string Phone { get; set; }

        public long PrizeId { get; set; }
    }
}
