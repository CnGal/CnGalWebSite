using CnGalWebSite.ProjectSite.Shared.Models.Themes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.ProjectSite.Shared.Services.Themes
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
