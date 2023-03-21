using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.IdentityServer.Models.DataModels.Account
{
    public class ApplicationRole : IdentityRole
    {
        public ICollection<ApplicationUserRole> UserRoles { get; set; }
    }
}
