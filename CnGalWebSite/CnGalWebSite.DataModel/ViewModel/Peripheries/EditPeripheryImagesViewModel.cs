using CnGalWebSite.DataModel.ViewModel.Base;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace CnGalWebSite.DataModel.ViewModel.Peripheries
{
    public class EditPeripheryImagesViewModel: BaseEditModel
    {
        public List<EditImageAloneModel> Images { get; set; } = new List<EditImageAloneModel>();
    }
}
