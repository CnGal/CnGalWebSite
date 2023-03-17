using CnGalWebSite.IdentityServer.Models.ViewModels.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.IdentityServer.Models.DataModels.Account
{
    public class AccountBindInfor
    {
        public List<SelectModifyAccountFieldModel> AccountFields { get; set; } = new List<SelectModifyAccountFieldModel>();
        public List<SelectModifyExternalFieldModel> ExternalFields { get; set; } = new List<SelectModifyExternalFieldModel>();

    }
}
