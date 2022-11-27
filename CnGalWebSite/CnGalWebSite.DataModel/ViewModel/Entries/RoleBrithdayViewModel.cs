using CnGalWebSite.DataModel.ViewModel.Search;
using System;
using System.Collections.Generic;
using System.Text;

namespace CnGalWebSite.DataModel.ViewModel.Entries
{
    public class RoleBrithdayViewModel
    {
        public DateTime Brithday { get; set; }

        public EntryInforTipViewModel Infor { get; set; } = new EntryInforTipViewModel();
    }
}
