using System.Collections.Generic;
namespace CnGalWebSite.DataModel.ViewModel.Peripheries
{
    public class GameOverviewPeripheriesModel
    {
        public string ObjectId { get; set; }

        public bool IsThumbnail { get; set; }

        public string Image { get; set; }

        public string Name { get; set; }

        public string BriefIntroduction { get; set; }

        public PeripheryOverviewType Type { get; set; }


        public List<PeripheryOverviewModel> Peripheries { get; set; } = new List<PeripheryOverviewModel>();
    }

    public class PeripheryOverviewModel
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
