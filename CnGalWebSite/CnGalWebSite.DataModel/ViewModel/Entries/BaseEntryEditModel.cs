using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Xml.Linq;

namespace CnGalWebSite.DataModel.ViewModel.Entries
{
    public class BaseEntryEditModel: BaseEditModel
    {
        [Display(Name = "类别")]
        [Required(ErrorMessage = "请选择类别")]
        public EntryType Type { get; set; }
    }
}
