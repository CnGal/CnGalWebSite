using CnGalWebSite.DataModel.Model;
using System.Collections.Generic;
namespace CnGalWebSite.DataModel.ViewModel.Home
{
    public class HomeViewModel
    {
        public List<EntryHomeAloneViewModel> RecentEditEntries { get; set; }
        public List<EntryHomeAloneViewModel> RecentIssuelGame { get; set; }
        public List<EntryHomeAloneViewModel> NewestGame { get; set; }
        public List<EntryHomeAloneViewModel> Notices { get; set; }
        public List<EntryHomeAloneViewModel> Articles { get; set; }

        public List<Carousel> Carousels { get; set; }

        public List<EntryHomeAloneViewModel> FriendLinks { get; set; }
    }
}
