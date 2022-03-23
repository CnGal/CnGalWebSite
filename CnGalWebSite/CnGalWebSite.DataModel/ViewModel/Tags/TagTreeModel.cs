using System;
using System.Collections.Generic;
using System.Text;

namespace CnGalWebSite.DataModel.ViewModel.Tags
{
    public class TagTreeModel
    {
        public string Icon { get; set; }

        public string Title { get; set; }

        public long Id { get; set; }

        public List<TagTreeModel> Children { get; set; } = new List<TagTreeModel>();
    }
}
