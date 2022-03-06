using System;
using System.Collections.Generic;
using System.Text;

namespace CnGalWebSite.DataModel.ViewModel.Entries
{
    public class GameCGModel
    {
        public string Name { get; set; }

        public int Id { get; set; }

        public List<EditImageAloneModel> Pictures { get; set; } = new List<EditImageAloneModel>();
    }
}
