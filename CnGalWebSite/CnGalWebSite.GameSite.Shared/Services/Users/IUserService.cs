using CnGalWebSite.DataModel.ViewModel.Space;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.GameSite.Shared.Services.Users
{
    public interface IUserService
    {
        UserInforViewModel UserInfo { get; }

        event Action UserInfoChanged;

        void Login();

        void Logout();

        Task Refresh(string id);
    }
}
