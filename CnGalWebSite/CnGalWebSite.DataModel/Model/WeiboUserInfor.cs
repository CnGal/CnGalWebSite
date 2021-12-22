using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.DataModel.Model
{
    public class WeiboUserInfor
    {
        public long Id { get; set; }

        public string WeiboName { get; set; }

        public string Image { get; set; }

        public long WeiboId { get; set; }

        public int? EntryId { get; set; }
        public Entry Entry { get; set; }
    }
}
