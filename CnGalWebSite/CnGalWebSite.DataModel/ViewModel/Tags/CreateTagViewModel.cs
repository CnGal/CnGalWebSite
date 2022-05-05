using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Base;

namespace CnGalWebSite.DataModel.ViewModel.Tags
{
    public class CreateTagViewModel : BaseEditModel
    {
        public EditTagMainViewModel Main { get; set; } = new EditTagMainViewModel();
        public EditTagChildEntriesViewModel Entries { get; set; } = new EditTagChildEntriesViewModel();
        public EditTagChildTagsViewModel Tags { get; set; } = new EditTagChildTagsViewModel();

        public override Result Validate()
        {
            var result = Main.Validate();
            if (!result.Successful)
            {
                return result;
            }
            result = Entries.Validate();
            if (!result.Successful)
            {
                return result;
            }
            result = Tags.Validate();
            if (!result.Successful)
            {
                return result;
            }


            return new Result { Successful = true };
        }
    }
}
