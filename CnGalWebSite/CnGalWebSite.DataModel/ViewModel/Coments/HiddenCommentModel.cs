using System;
 using System.Collections.Generic;
namespace CnGalWebSite.DataModel.ViewModel.Coments
{
    public class HiddenCommentModel
    {
        public long[] Ids { get; set; } = Array.Empty<long>();

        public bool IsHidden { get; set; }
    }
}
