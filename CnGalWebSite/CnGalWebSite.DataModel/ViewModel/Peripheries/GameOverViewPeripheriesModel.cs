using System;
 using System.Collections.Generic;
namespace CnGalWebSite.DataModel.ViewModel.Peripheries
{
    public class GameOverviewPeripheriesModel
    {
        public int EntryId { get; set; }

        public long PeripheryId { get; set; }


        public string UserId { get; set; }

        public string Image { get; set; }

        public string Name { get; set; }

        public PeripheryOverviewHeadType Type { get; set; }


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

    public enum PeripheryOverviewHeadType
    {
        GameOrGroup,
        RoleOrStaff,
        User,
        Periphery
    }
}
