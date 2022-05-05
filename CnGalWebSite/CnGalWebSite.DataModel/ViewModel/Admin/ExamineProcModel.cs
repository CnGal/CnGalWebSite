using System.ComponentModel.DataAnnotations;

namespace CnGalWebSite.DataModel.ViewModel.Admin
{
    public class ExamineProcModel
    {
        public long Id { get; set; }
        public bool? IsPassed { get; set; }

        [Display(Name = "附加贡献值")]
        public int ContributionValue { get; set; }

        [Display(Name = "批注")]
        public string Comments { get; set; }
    }
}
