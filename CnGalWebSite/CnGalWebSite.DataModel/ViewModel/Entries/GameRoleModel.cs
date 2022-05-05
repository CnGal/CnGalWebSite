using CnGalWebSite.DataModel.ViewModel.Search;
using System.Collections.Generic;

namespace CnGalWebSite.DataModel.ViewModel.Entries
{
    public class GameRoleModel
    {
        public string Name { get; set; }

        public string Image { get; set; }

        public int Id { get; set; }

        public List<EntryInforTipViewModel> Roles { get; set; } = new List<EntryInforTipViewModel>();
    }
}
