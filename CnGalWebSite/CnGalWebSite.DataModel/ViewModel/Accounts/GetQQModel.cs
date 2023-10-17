using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.DataModel.ViewModel.Accounts
{
    public class GetQQModel
    {
        public string Token { get; set; }

        public string UserId { get; set; }
    }

    public class GetQQResultModel
    {
        public long QQ { get; set; }
    }
}
