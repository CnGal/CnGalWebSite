using CnGalWebSite.DataModel.ViewModel.Theme;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace CnGalWebSite.Shared
{
    /// <summary>
    /// 
    /// </summary>
    public sealed partial class App
    {
        /// <summary>
        /// 
        /// </summary>
        [Inject]
        private IJSRuntime JSRuntime { get; set; }

        private bool? isApp = null;

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            if (NavigationManager.Uri.Contains("app.cngal.org") || NavigationManager.Uri.Contains("localhost"))
            {
                isApp = _dataCacheService.IsApp;
            }
            else
            {
                isApp = _dataCacheService.IsApp = NavigationManager.Uri.Contains("m.cngal.org");
            }

            _dataCacheService.RefreshApp = EventCallback.Factory.Create(this, async () => await OnRefresh());
            _dataCacheService.SavaTheme = EventCallback.Factory.Create(this, async () => await SaveTheme());

        }


        public Task OnRefresh()
        {
            StateHasChanged();
            return Task.CompletedTask;
        }


        public async Task<bool> IsMobile()
        {
            try
            {
                var re = await JSRuntime.InvokeAsync<string>("isMobile");
                if (re == "true")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }

        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (firstRender && OperatingSystem.IsBrowser())
            {
                await JSRuntime.InvokeVoidAsync("$.loading");
            }

            if (firstRender)
            {
                if (isApp == null)
                {
                    _dataCacheService.IsApp = await IsMobile();
                    StateHasChanged();
                }
                //mytimer = new System.Threading.Timer(new System.Threading.TimerCallback(Send), null, 0, 10);
                try
                {
                    //读取本地主题配置
                    await LoadTheme();
                    //保存本地主题配置 更新数据结构
                    await SaveTheme();
                }
                catch
                {

                }
            }
        }

        /// <summary>
        /// 读取本地主题配置 并刷新
        /// </summary>
        /// <returns></returns>
        public async Task LoadTheme()
        {

            var theme = await _localStorage.GetItemAsync<ThemeModel>("theme");
            if (theme == null)
            {
                return;
            }
            _dataCacheService.ThemeSetting = theme;

            StateHasChanged();
        }

        /// <summary>
        /// 保存本地主题配置
        /// </summary>
        /// <returns></returns>
        public async Task SaveTheme()
        {
            await _localStorage.SetItemAsync("theme", _dataCacheService.ThemeSetting);
        }
    }
}
