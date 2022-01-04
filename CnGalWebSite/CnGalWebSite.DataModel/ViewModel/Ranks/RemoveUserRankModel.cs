using System;
 using System.Collections.Generic;
namespace CnGalWebSite.DataModel.ViewModel.Ranks
{
    public class RemoveUserRankModel
    {
        public string[] UserIds { get; set; }

        public long[] RankIds { get; set; }
    }
}
