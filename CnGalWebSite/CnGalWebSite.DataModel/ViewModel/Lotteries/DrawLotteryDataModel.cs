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


        /// <summary>
        /// 赞助商
        /// </summary>
        public string Sponsor { get; set; }
        /// <summary>
        /// 图片
        /// </summary>
        public string Image { get; set; }
        /// <summary>
        /// 链接 用于展示对于的游戏或贩售地址
        /// </summary>
        public string Link { get; set; }

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
