using System;
namespace CnGalWebSite.DataModel.ViewModel.Entries
{
    public class EditEntryCanCommentModel
    {
        public int[] Ids { get; set; } = Array.Empty<int>();

        public bool CanComment { get; set; }
    }
}
