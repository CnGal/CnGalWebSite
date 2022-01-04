using System;
 using System.Collections.Generic;
namespace CnGalWebSite.DataModel.ViewModel.Entries
{
    public class EditEntryPriorityViewModel
    {
        public int[] Ids { get; set; }

        public int PlusPriority { get; set; }

        public EditEntryPriorityOperation Operation { get; set; }
    }

    public enum EditEntryPriorityOperation
    {
        None,
        ClearAllGame
    }
}
