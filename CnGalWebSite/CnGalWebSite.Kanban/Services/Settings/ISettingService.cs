using CnGalWebSite.Kanban.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.Kanban.Services.Settings
{
    public interface ISettingService
    {
        Task LoadAsync();

        Task SaveAsync();

        Task ResetAsync();

        SettingModel Setting { get; }

        event Action OnSettingChanged;
    }
}
