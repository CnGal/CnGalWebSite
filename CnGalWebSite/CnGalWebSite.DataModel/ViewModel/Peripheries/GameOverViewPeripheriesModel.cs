using System.Collections.Generic;
namespace CnGalWebSite.DataModel.ViewModel.Peripheries
{
    public class GameOverviewPeripheryListModel
    {
        public string ObjectId { get; set; }

        public bool IsThumbnail { get; set; }

        public string Image { get; set; }

        public string Name { get; set; }

        public string BriefIntroduction { get; set; }

        public PeripheryOverviewType Type { get; set; }


        public List<GameOverviewPeripheryModel> Peripheries { get; set; } = new List<GameOverviewPeripheryModel>();
    }

    public class GameOverviewPeripheryModel
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string Image { get; set; }

        public bool IsCollected { get; set; }

        public int CollectedCount { get; set; }
    }

    public enum PeripheryOverviewType
    {
        Entry,
        User,
        Periphery
    }
}
