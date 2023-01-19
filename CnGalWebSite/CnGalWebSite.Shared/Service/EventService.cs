using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.ViewModel.PostTools;
using CnGalWebSite.Helper.Helper;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.Shared.Service
{
    public class EventService : IEventService
    {
        public event Action RefreshApp;
        public event Action SavaTheme;

        private readonly IMauiService _mauiService;
        private readonly IJSRuntime JS;

        public EventService(IMauiService mauiService, IJSRuntime js)
        {
            _mauiService = mauiService;
            JS = js;
        }

        /// <summary>
        /// 刷新渲染框架方法
        /// </summary>
        public void OnRefreshApp()
        {
            RefreshApp?.Invoke();
        }

        /// <summary>
        /// 保存主题设置
        /// </summary>
        public void OnSavaTheme()
        {
            SavaTheme?.Invoke();
        }

        public async Task OpenNewPage(string url)
        {
            if (ToolHelper.IsMaui)
            {
                _mauiService.OpenNewPage(url);
            }
            else
            {
                await JS.InvokeAsync<string>("openNewPage", url);
            }
        }
    }
}
