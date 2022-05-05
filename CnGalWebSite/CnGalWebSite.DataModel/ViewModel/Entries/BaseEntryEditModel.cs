using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Base;
using System.ComponentModel.DataAnnotations;

namespace CnGalWebSite.DataModel.ViewModel.Entries
{
    public class BaseEntryEditModel : BaseEditModel
    {
        [Display(Name = "类别")]
        [Required(ErrorMessage = "请选择类别")]
        public EntryType Type { get; set; }
    }
}
