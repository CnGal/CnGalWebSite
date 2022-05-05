using CnGalWebSite.DataModel.ViewModel.Base;
using System.Collections.Generic;
namespace CnGalWebSite.DataModel.ViewModel.Tags
{
    public class EditTagChildTagsViewModel : BaseEditModel
    {
        public List<RelevancesModel> Tags { get; set; } = new List<RelevancesModel>();
    }
}
