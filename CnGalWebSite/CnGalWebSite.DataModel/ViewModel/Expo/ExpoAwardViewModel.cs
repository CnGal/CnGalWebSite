using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CnGalWebSite.DataModel.Model;

namespace CnGalWebSite.DataModel.ViewModel.Expo
{
    public class ExpoAwardViewModel
    {
        public long Id { get; set; }

        public ExpoAwardType Type { get; set; }

        public int Count { get; set; }

        public string Image { get; set; }

        public string Name { get; set; }

        public string Url { get; set; }

        /// <summary>
        /// 是否启用，未启用的奖项不参与抽奖
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        public List<ExpoPrizeOverviewModel> Prizes { get; set; } = new List<ExpoPrizeOverviewModel>();
    }

    public class ExpoPrizeOverviewModel
    {
        public long Id { get; set; }

        public string Content { get; set; }

        public string UserId { get; set; }

        public string UserName { get; set; }

        public DateTime DrawTime { get; set; }

        public long AwardId { get; set; }

        public string AwardName { get; set; }

        public ExpoAwardType AwardType { get; set; }
    }
}
