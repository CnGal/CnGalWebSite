using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.IdentityServer.Models.ViewModels.Users
{
    public class UserEditModel:UserOverviewModel
    {
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public bool ChangePassword { get; set; }
    }
}
