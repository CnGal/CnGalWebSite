using System;

namespace CnGalWebSite.DataModel.ViewModel.Coments
{
    public class EditLotteryCanCommentModel
    {
        public long[] Ids { get; set; } = Array.Empty<long>();

        public bool CanComment { get; set; }
    }
}
