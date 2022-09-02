using System.Collections.Generic;

namespace CnGalWebSite.DataModel.ViewModel.Tags
{
    public class TagTreeModel
    {
        public string Icon { get; set; }

        public string Title { get; set; }

        public long Id { get; set; }

        public int EntryCount { get; set; }

        public List<TagTreeModel> Children { get; set; } = new List<TagTreeModel>();
    }
}
