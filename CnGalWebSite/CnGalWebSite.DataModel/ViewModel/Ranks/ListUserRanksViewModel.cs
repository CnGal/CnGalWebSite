
using System.ComponentModel.DataAnnotations;

using System;
 using System.Collections.Generic;
namespace CnGalWebSite.DataModel.ViewModel.Ranks
{
    public class ListUserRanksInforViewModel
    {
        public long Hiddens { get; set; }
        public long All { get; set; }
    }

    public class ListUserRanksViewModel
    {
        public List<ListUserRankAloneModel> UserRanks { get; set; }
    }
    public class ListUserRankAloneModel
    {
        public long Id { get; set; }
        [Display(Name = "Id")]
        public long RankId { get; set; }
        [Display(Name = "名称")]
        public string Name { get; set; }

        [Display(Name = "获得时间")]
        public DateTime Time { get; set; }

        [Display(Name = "是否隐藏")]
        public bool IsHidden { get; set; }

    }

    public class UserRanksPagesInfor
    {
        public CnGalWebSite.DataModel.ViewModel.Search.QueryPageOptions Options { get; set; }
        public ListUserRankAloneModel SearchModel { get; set; }

        public string UserId { get; set; }
    }
}
