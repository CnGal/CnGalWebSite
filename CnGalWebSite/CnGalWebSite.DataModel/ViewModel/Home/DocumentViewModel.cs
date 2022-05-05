using System.Collections.Generic;

namespace CnGalWebSite.DataModel.ViewModel.Home
{
    public class DocumentViewModel
    {
        public string Icon { get; set; }

        public string Title { get; set; }

        public long Id { get; set; }

        public List<DocumentViewModel> Children { get; set; } = new List<DocumentViewModel>();
    }
}
