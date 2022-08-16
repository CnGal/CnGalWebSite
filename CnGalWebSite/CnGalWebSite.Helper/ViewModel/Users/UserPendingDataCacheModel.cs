using BlazorComponent;
using CnGalWebSite.DataModel.ViewModel.Space;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.Helper.ViewModel.Users
{
    public class UserPendingDataCacheModel
    {
        public StringNumber TabIndex { get; set; } = 1;

        public int MaxCount { get; set; } = 10;

        public int TotalPages => ((Items.Count - 1) / MaxCount) + 1;

        public int CurrentPage { get; set; } = 1;

        public List<UserPendingDataModel> Items { get; set; } = new List<UserPendingDataModel>();
    }
}
