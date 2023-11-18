using CnGalWebSite.DataModel.ViewModel.Search;
using System;
using System.Collections.Generic;
using System.Text;

namespace CnGalWebSite.DataModel.ViewModel.Entries
{
    public class PublishGamesTimelineModel: EntryInforTipViewModel
    {
        public string Thumbnail { get; set; }

        public string PublishTimeNote { get; set; }
    }

    public enum PublishGamesDisplayType
    {
        CardList,
        Timeline
    }
}
