using CnGalWebSite.DataModel.Model;
using System.ComponentModel.DataAnnotations;

namespace CnGalWebSite.DataModel.ViewModel
{
    public class EditEntryTagViewModel
    {
        public EntryType Type { get; set; }
        public string Name { get; set; }
        public int Id { get; set; }
        public List<RelevancesModel> Tags { get; set; } = new List<RelevancesModel>();

        [Display(Name = "备注")]
        public string Note { get; set; }

    }
}
