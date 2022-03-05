using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Theme;
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
    public sealed partial class App : IDisposable
    {
        private bool? isApp = null;

        private System.Threading.Timer mytimer;

        [CascadingParameter]
        private Task<AuthenticationState> authenticationStateTask { get; set; }

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
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (firstRender && OperatingSystem.IsBrowser())
            {
                await JS.InvokeVoidAsync("$.loading");
            }

            if (firstRender)
            {

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
                    var authState = await authenticationStateTask;
                    var user = authState.User;
                    if (user.Identity.IsAuthenticated)
                    {
                        await Http.GetFromJsonAsync<Result>(ToolHelper.WebApiPath + "api/account/MakeUserOnline");
                    }
                }

                catch
                {

                }
            });

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
