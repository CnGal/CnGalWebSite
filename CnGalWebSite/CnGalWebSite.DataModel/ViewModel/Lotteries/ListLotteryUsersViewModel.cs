using CnGalWebSite.DataModel.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Xml.Linq;

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
        [Display(Name = "号码")]
        public int Number { get; set; }
        [Display(Name = "用户名")]
        public string Name { get; set; }
        [Display(Name = "Id")]
        public string Id { get; set; }
    }

    public class LotteryUsersPagesInfor
    {
        public CnGalWebSite.DataModel.ViewModel.Search.QueryPageOptions Options { get; set; }
        public ListLotteryUserAloneModel SearchModel { get; set; }

        public long LotteryId { get; set; }
    }
}
