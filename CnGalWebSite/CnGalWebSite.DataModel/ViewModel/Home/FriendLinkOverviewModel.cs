using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.DataModel.ViewModel.Home
{
    public class FriendLinkOverviewModel
    {
        public int Id { get; set; }

        public string Image { get; set; }

        public string Link { get; set; }

        public string Name { get; set; }

        public int Priority { get; set; }
    }

    public class FriendLinkEditModel: FriendLinkOverviewModel
    {

    }
}
