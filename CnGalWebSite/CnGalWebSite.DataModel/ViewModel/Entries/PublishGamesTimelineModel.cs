using CnGalWebSite.DataModel.ViewModel.Search;
using System;
using System.Collections.Generic;
using System.Text;

namespace CnGalWebSite.DataModel.ViewModel.Entries
{
    public class PublishGamesTimelineModel
    {
        public string PublishTimeNote { get; set; }

        public EntryInforTipViewModel Infor { get; set; }
    }

    public enum PublishGamesDisplayType
    {
        CardList,
        Timeline
    }
}
