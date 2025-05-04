using CnGalWebSite.DataModel.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.DataModel.ViewModel.Expo
{
    public class ExpoTaskOverviewModel
    {
        public long Id { get; set; }

        public ExpoTaskType Type { get; set; }

        public DateTime Time { get; set; }

        /// <summary>
        /// 抽奖次数
        /// </summary>
        public int LotteryCount { get; set; }

        public string UserId { get; set; }
        public string UserName { get; set; }
    }
}
