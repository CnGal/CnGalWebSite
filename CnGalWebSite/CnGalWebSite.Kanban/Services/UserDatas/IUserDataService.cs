using CnGalWebSite.Kanban.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.Kanban.Services.UserDatas
{
    public interface IUserDataService
    {
        Task LoadAsync();

        Task SaveAsync();

        Task ResetAsync();

        UserDataModel UserData { get; }
    }
}
