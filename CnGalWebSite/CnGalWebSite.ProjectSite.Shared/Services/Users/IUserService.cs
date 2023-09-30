using CnGalWebSite.ProjectSite.Models.ViewModels.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.ProjectSite.Shared.Services.Users
{
    public interface IUserService
    {
        UserInfoViewModel UserInfo { get; }

        event Action UserInfoChanged;

        void Login();

        void Logout();

        Task Refresh();
    }
}
