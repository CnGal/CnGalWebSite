using System.ComponentModel.DataAnnotations;

using System;
 using System.Collections.Generic;
namespace CnGalWebSite.DataModel.ViewModel.Tags
{
    public class EditTagChildEntriesViewModel
    {

        public string Name { get; set; }
        public long Id { get; set; }

        public List<RelevancesModel> Entries { get; set; } = new List<RelevancesModel>();

        [Display(Name = "备注")]
        public string Note { get; set; }
    }
}
