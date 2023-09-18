using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CnGalWebSite.DataModel.ViewModel.Lotteries
{
    public class LotteryUserOverviewModel
    {
        public long LotteryUserId { get; set; }

        /// <summary>
        /// 号码
        /// </summary>
        public int Number { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Id
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        /// Cookie
        /// </summary>
        public string Cookie { get; set; }
        /// <summary>
        /// Ip
        /// </summary>
        public string Ip { get; set; }
        /// <summary>
        /// 是否隐藏
        /// </summary>
        public bool IsHidden { get; set; }
    }
}
