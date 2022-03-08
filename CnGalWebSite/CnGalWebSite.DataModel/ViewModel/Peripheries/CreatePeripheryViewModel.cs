using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Base;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace CnGalWebSite.DataModel.ViewModel.Peripheries
{
    public class CreatePeripheryViewModel : BaseEditModel
    {
        public EditPeripheryMainViewModel Main { get; set; } = new EditPeripheryMainViewModel();
        public EditPeripheryImagesViewModel Images { get; set; } = new EditPeripheryImagesViewModel();
        public EditPeripheryRelatedEntriesViewModel Entries { get; set; } = new EditPeripheryRelatedEntriesViewModel();
        public EditPeripheryRelatedPeripheriesViewModel Peripheries { get; set; } = new EditPeripheryRelatedPeripheriesViewModel();

        public override Result Validate()
        {
            var result = Main.Validate();
            if (!result.Successful)
            {
                return result;
            }
            result = Images.Validate();
            if (!result.Successful)
            {
                return result;
            }
            result = Entries.Validate();
            if (!result.Successful)
            {
                return result;
            }
            result = Peripheries.Validate();
            if (!result.Successful)
            {
                return result;
            }

            return new Result { Successful = true };
        }
    }
}
