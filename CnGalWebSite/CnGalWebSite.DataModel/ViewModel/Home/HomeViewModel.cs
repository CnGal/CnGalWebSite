using CnGalWebSite.DataModel.Model;
using System.Collections.Generic;
namespace CnGalWebSite.DataModel.ViewModel.Home
{
    public class HomeViewModel
    {
        public List<MainImageCardModel> RecentEditEntries { get; set; }
        public List<MainImageCardModel> RecentIssuelGame { get; set; }
        public List<MainImageCardModel> NewestGame { get; set; }
        public List<MainImageCardModel> Notices { get; set; }
        public List<MainImageCardModel> Articles { get; set; }

        public List<Carousel> Carousels { get; set; }

        public List<MainImageCardModel> FriendLinks { get; set; }
    }
}
