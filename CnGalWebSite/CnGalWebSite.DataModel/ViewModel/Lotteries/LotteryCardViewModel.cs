using System;

namespace CnGalWebSite.DataModel.ViewModel.Lotteries
{
    public class LotteryCardViewModel
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string BriefIntroduction { get; set; }

        public string MainPicture { get; set; }

        public DateTime BeginTime { get; set; }

        public DateTime EndTime { get; set; }

        public long Count { get; set; }

    }
}
