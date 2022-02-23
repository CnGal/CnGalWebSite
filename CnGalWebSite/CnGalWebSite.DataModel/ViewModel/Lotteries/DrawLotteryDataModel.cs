using System.Collections.Generic;

namespace CnGalWebSite.DataModel.ViewModel.Lotteries
{
    public class DrawLotteryDataModel
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public List<LotteryUserDataModel> NotWinningUsers { get; set; } = new List<LotteryUserDataModel>();

        public List<LotteryAwardDataModel> Awards { get; set; } = new List<LotteryAwardDataModel>();
    }

    public class LotteryAwardDataModel
    {
        public long Id { get; set; }

        public int Priority { get; set; }

        public string Name { get; set; }

        public int TotalCount { get; set; }

        public List<LotteryUserDataModel> WinningUsers { get; set; } = new List<LotteryUserDataModel>();
    }

    public class LotteryUserDataModel
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public int Number { get; set; }
    }
}
