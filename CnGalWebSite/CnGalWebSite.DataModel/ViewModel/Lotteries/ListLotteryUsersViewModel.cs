using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CnGalWebSite.DataModel.ViewModel.Lotteries
{
    public class ListLotteryUsersInforViewModel
    {
        public long Hiddens { get; set; }

        public long All { get; set; }
    }

    public class ListLotteryUsersViewModel
    {
        public List<ListLotteryUserAloneModel> LotteryUsers { get; set; } = new List<ListLotteryUserAloneModel> { };
    }
    public class ListLotteryUserAloneModel
    {
        public long LotteryUserId { get; set; }

        [Display(Name = "号码")]
        public int Number { get; set; }
        [Display(Name = "用户名")]
        public string Name { get; set; }
        [Display(Name = "Id")]
        public string UserId { get; set; }
        [Display(Name = "Cookie")]
        public string Cookie { get; set; }
        [Display(Name = "Ip")]
        public string Ip { get; set; }
        [Display(Name = "是否隐藏")]
        public bool IsHidden { get; set; }
    }

    public class LotteryUsersPagesInfor
    {
        public CnGalWebSite.DataModel.ViewModel.Search.QueryPageOptions Options { get; set; }
        public ListLotteryUserAloneModel SearchModel { get; set; }

        public long LotteryId { get; set; }
    }
}
