using CnGalWebSite.GameSite.Shared.Models.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.GameSite.Shared.Services.Themes
{
    public interface ISettingService
    {
        public SettingModel Settings { get; }

        bool IsApp { get; set; }

        event Action SettingChanged;

        Task LoadAsync();

        Task SaveAsync();

        Task ResetAsync();

        void OnSettingChanged();
    }
}
