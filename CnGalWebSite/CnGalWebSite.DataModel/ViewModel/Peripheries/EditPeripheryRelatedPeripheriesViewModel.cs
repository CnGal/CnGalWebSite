using CnGalWebSite.DataModel.ViewModel.Base;
using System.Collections.Generic;
namespace CnGalWebSite.DataModel.ViewModel.Peripheries
{
    public class EditPeripheryRelatedPeripheriesViewModel : BaseEditModel
    {
        public List<RelevancesModel> Peripheries { get; set; } = new List<RelevancesModel>();
    }
}
