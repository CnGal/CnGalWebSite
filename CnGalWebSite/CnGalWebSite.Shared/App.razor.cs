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

        [CascadingParameter]
        private Task<AuthenticationState> authenticationStateTask { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            if (NavigationManager.Uri.Contains("m.cngal.org"))
            {
                _dataCacheService.IsApp = true;
            }

            if (ToolHelper.IsMaui)
            {
                _dataCacheService.IsApp = ToolHelper.IsApp;
            }

            _dataCacheService.RefreshApp = EventCallback.Factory.Create(this, async () => await OnRefresh());
            _dataCacheService.OpenNewPage = EventCallback.Factory.Create(this, (string s) => OpenNewPage(s));
            _dataCacheService.ThemeChanged = EventCallback.Factory.Create(this, (string s) => ThemeChanged(s));
            _dataCacheService.ShareLink = EventCallback.Factory.Create(this, (ShareLinkModel s) => ShareLink(s));

            //ShowAlert();
        }


        public Task OnRefresh()
        {
            StateHasChanged();
            return Task.CompletedTask;
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
                //检查是否为移动设备
                if (NavigationManager.Uri.Contains("app.cngal.org") || NavigationManager.Uri.Contains("localhost"))
                {
                    var isApp = await IsMobile();
                    if (isApp != _dataCacheService.IsApp)
                    {
                        _dataCacheService.IsApp = isApp;
                        StateHasChanged();
                    }

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
