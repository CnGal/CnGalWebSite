using CnGalWebSite.DataModel.ViewModel.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.DataModel.ViewModel.Steam
{
    public class SteamGamesOverviewModel
    {
        public int Count { get; set; }

        public List<PossessionRateHighestGameModel> PossessionRateHighestGames { get; set; }

        public List<HasMostGamesUserModel> HasMostGamesUsers { get; set; }
    }

    public class PossessionRateHighestGameModel : EntryInforTipViewModel
    {
        public double Rate { get; set; }
    }

    public class HasMostGamesUserModel
    {
        public string Name { get; set; }

        public string Image { get; set; }

        public int Count { get; set; }

        public string PersonalSignature { get; set; }
    }
}
