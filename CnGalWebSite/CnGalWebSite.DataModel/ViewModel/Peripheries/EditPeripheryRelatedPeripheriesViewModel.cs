using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace CnGalWebSite.DataModel.ViewModel.Peripheries
{
    public class EditPeripheryRelatedPeripheriesViewModel
    {
        public string Name { get; set; }
        public long Id { get; set; }

        public List<RelevancesModel> Peripheries { get; set; } = new List<RelevancesModel>();

        [Display(Name = "备注")]
        public string Note { get; set; }
    }
}
