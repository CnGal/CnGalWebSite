using Blazored.LocalStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.Components.Services
{
    public class MiniModeService:IMiniModeService
    {
        private readonly ILocalStorageService _localStorageService;

        public event Action MiniModeChanged;

        public bool IsMiniMode { get; set; }

        public MiniModeService(ILocalStorageService localStorageService)
        {
            _localStorageService = localStorageService;
        }

        public async Task CheckAsync()
        {
            var needRefresh = false;
            try
            {
                if (await _localStorageService.GetItemAsync<bool>("IsMiniMode"))
                {
                    if (IsMiniMode == false)
                    {
                        IsMiniMode = true;
                        needRefresh = true;
                    }
                }
                else
                {
                    if (IsMiniMode)
                    {
                        await _localStorageService.SetItemAsync<bool>("IsMiniMode", true);
                    }
                }

            }
            catch
            {

            }

            if (needRefresh)
            {
                OnMiniModeChanged();
            }
        }

        public void OnMiniModeChanged()
        {
            MiniModeChanged?.Invoke();
        }

    }
}
