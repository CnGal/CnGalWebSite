using System.Collections.Generic;

namespace CnGalWebSite.DataModel.ViewModel.Entries
{
    public class GameCGModel
    {
        public string Name { get; set; }

        public int Id { get; set; }

        public List<EditImageAloneModel> Pictures { get; set; } = new List<EditImageAloneModel>();
    }
}
