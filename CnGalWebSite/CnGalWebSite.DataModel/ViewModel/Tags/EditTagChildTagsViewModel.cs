using CnGalWebSite.DataModel.ViewModel.Base;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace CnGalWebSite.DataModel.ViewModel.Tags
{
    public class EditTagChildTagsViewModel: BaseEditModel
    {
        public List<RelevancesModel> Tags { get; set; } = new List<RelevancesModel>();
    }
}
