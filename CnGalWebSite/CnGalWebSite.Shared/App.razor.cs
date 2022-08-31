using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Others;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace CnGalWebSite.Shared
{
    /// <summary>
    /// 
    /// </summary>
    public partial class App : IDisposable
    {

        private System.Threading.Timer mytimer;
        CnGalWebSite.Shared.MasaComponent.Shared.Tips.CnGalRootTip cngalRootTip;

        [CascadingParameter]
        private Task<AuthenticationState> authenticationStateTask { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            I18n.SetCulture(System.Globalization.CultureInfo.GetCultureInfo("zh-CN"));//将语言切换成zh-CN

            //判断移动端
            if (NavigationManager.Uri.Contains("m.cngal.org"))
            {
                _dataCacheService.IsApp = true;
            }

            if (ToolHelper.IsMaui)
            {
                _dataCacheService.IsApp = ToolHelper.IsApp;
            }

            //判断来源
            if (NavigationManager.Uri.Contains("ref=gov"))
            {
                _dataCacheService.IsMiniMode = true;
            }


            _dataCacheService.RefreshApp = EventCallback.Factory.Create(this, async () => await OnRefresh());
            _dataCacheService.OpenNewPage = EventCallback.Factory.Create(this, (string s) => OpenNewPage(s));
            _dataCacheService.ThemeChanged = EventCallback.Factory.Create(this, (string s) => ThemeChanged(s));
            _dataCacheService.ShareLink = EventCallback.Factory.Create(this, (ShareLinkModel s) => ShareLink(s));
            _dataCacheService.Quit = EventCallback.Factory.Create(this, () => Quit());

            //ShowAlert();
        }


        public async Task OnRefresh()
        {
            _dataCacheService.OnRefreshRequsted(null);
            StateHasChanged();
            if(cngalRootTip!=null)
            {
               await cngalRootTip.ChangeDisplayMode();
            }
        }


        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (firstRender && (OperatingSystem.IsBrowser() || ToolHelper.IsMaui))
            {
                try
                {
                    await JS.InvokeVoidAsync("$.loading");
                }
                catch
                {

                }
            }

            if (firstRender)
            {
                var needRefresh = false;
                //检查是否为移动设备
                if (NavigationManager.Uri.Contains("app.cngal.org") || NavigationManager.Uri.Contains("localhost") && ToolHelper.IsMaui == false)
                {
                    var isApp = await IsMobile();
                    if (isApp != _dataCacheService.IsApp)
                    {
                        _dataCacheService.IsApp = isApp;
                        needRefresh = true;

                    }
                }
                //检查迷你模式
                try
                {
                    if (await _localStorage.GetItemAsync<bool>("IsMiniMode"))
                    {
                        if (_dataCacheService.IsMiniMode == false)
                        {
                            _dataCacheService.IsMiniMode = true;
                            needRefresh = true;
                        }
                    }
                    else
                    {
                        if (_dataCacheService.IsMiniMode)
                        {
                            await _localStorage.SetItemAsync<bool>("IsMiniMode", true);
                        }
                    }

                }
                catch
                {

                }

                if (needRefresh)
                {
                    await OnRefresh();
                }

                //需要调用一次令牌刷新接口 确保登入没有过期
                var result = await _authService.Refresh();
                if (result != null && result.Code != LoginResultCode.OK)
                {
                    StateHasChanged();
                }

                //启动定时器
                mytimer = new System.Threading.Timer(new System.Threading.TimerCallback(Send), null, 0, 1000 * 60 * 10);

            }
        }


        public async void Send(object o)
        {
            await InvokeAsync(async () =>
            {
                try
                {
                    await Http.GetFromJsonAsync<Result>(ToolHelper.WebApiPath + "api/account/MakeUserOnline");
                }
                catch
                {

                }
            });

        }

        public async Task<bool> IsMobile()
        {
            try
            {
                var re = await JS.InvokeAsync<string>("isMobile");
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

        public virtual void ShowAlert()
        {

        }
        public virtual async void OpenNewPage(string url)
        {
            await JS.InvokeAsync<string>("openNewPage", url);
        }

        public virtual void Loaded()
        {

        }

        public virtual void Quit()
        {

        }

        public virtual async void ShareLink(ShareLinkModel mode)
        {

        }

        public virtual void ThemeChanged(string theme)
        {

        }

        #region 释放实例
        public void Dispose()
        {
            if (mytimer != null)
            {
                mytimer.Dispose();
                mytimer = null;
            }
            GC.SuppressFinalize(this);
        }
        #endregion

    }
}
