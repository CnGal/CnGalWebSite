
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace CnGalWebSite.DataModel.ViewModel.Ranks
{
    public class ListRankUsersInforViewModel
    {
        public long Hiddens { get; set; }
        public long All { get; set; }
    }

    public class ListRankUsersViewModel
    {
        public List<ListRankUserAloneModel> RankUsers { get; set; }
    }
    public class ListRankUserAloneModel
    {
        public long Id { get; set; }
        [Display(Name = "用户Id")]
        public string UserId { get; set; }
        [Display(Name = "用户名")]
        public string UserName { get; set; }

        [Display(Name = "获得时间")]
        public DateTime Time { get; set; }
        [Display(Name = "是否隐藏")]
        public bool IsHidden { get; set; }


    }

    public class RankUsersPagesInfor
    {
        public CnGalWebSite.DataModel.ViewModel.Search.QueryPageOptions Options { get; set; }
        public ListRankUserAloneModel SearchModel { get; set; }

        public long RankId { get; set; }
    }
}
