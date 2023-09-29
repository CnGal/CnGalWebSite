using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.Components.Services
{
    public interface IMiniModeService
    {
        event Action MiniModeChanged;

        bool IsMiniMode { get; set; }

        void OnMiniModeChanged();

        Task CheckAsync();
    }
}
