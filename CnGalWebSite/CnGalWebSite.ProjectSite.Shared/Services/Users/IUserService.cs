using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.ProjectSite.Shared.Services.Users
{
    public interface IUserService
    {
        void Login();

        void Logout();
    }
}
