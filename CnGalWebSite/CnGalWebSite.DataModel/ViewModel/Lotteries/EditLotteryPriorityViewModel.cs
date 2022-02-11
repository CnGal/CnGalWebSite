using System;
using System.Collections.Generic;
using System.Text;

namespace CnGalWebSite.DataModel.ViewModel.Lotteries
{
    public class EditLotteryPriorityViewModel
    {
        public long[] Ids { get; set; } = Array.Empty<long>();

        public int PlusPriority { get; set; }
    }
}
