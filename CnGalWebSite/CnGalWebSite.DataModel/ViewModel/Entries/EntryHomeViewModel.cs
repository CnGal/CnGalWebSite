using System;
 using System.Collections.Generic;
namespace CnGalWebSite.DataModel.ViewModel
{
    public class EntryHomeViewModel
    {
        public List<EntryHomeAloneViewModel> Games { get; set; } = new List<EntryHomeAloneViewModel>();
        public List<EntryHomeAloneViewModel> Groups { get; set; } = new List<EntryHomeAloneViewModel>();
        public List<EntryHomeAloneViewModel> Roles { get; set; } = new List<EntryHomeAloneViewModel>();
        public List<EntryHomeAloneViewModel> Staffs { get; set; } = new List<EntryHomeAloneViewModel>();
    }
    public class EntryHomeAloneViewModel
    {
        public string Image { get; set; }

        public string DisPlayName { get; set; }

        /// <summary>
        /// 目前 只用于友情链接的Link
        /// </summary>
        public string DisPlayValue { get; set; }

        public int ReadCount { get; set; }

        public int CommentCount { get; set; }

        public long Id { get; set; }
    }
}
