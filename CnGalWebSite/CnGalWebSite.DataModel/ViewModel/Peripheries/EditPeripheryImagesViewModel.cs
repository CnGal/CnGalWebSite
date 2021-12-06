using System.ComponentModel.DataAnnotations;

namespace CnGalWebSite.DataModel.ViewModel.Peripheries
{
    public class EditPeripheryImagesViewModel
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public List<EditImageAloneModel> Images { get; set; }

        [Display(Name = "备注")]
        public string Note { get; set; }
    }
}
