using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Entries;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace CnGalWebSite.DataModel.ViewModel
{
    public class EditEntryTagViewModel : BaseEntryEditModel
    {
        public List<RelevancesModel> Tags { get; set; } = new List<RelevancesModel>();

    }
}
