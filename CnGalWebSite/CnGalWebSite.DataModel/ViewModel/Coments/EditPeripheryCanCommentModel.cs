using System;
 using System.Collections.Generic;
namespace CnGalWebSite.DataModel.ViewModel.Coments
{
    public class EditPeripheryCanCommentModel
    {
        public long[] Ids { get; set; } = Array.Empty<long>();

        public bool CanComment { get; set; }
    }
}
