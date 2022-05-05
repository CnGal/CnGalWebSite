using CnGalWebSite.DataModel.ViewModel.Search;
using System.Collections.Generic;

namespace CnGalWebSite.DataModel.ViewModel.Tags
{
    public class RandomTagModel
    {
        public string Name { get; set; }

        public int Id { get; set; }

        public List<EntryInforTipViewModel> Entries { get; set; } = new List<EntryInforTipViewModel>();
    }
}
