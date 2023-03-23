using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.IdentityServer.Admin.Shared.Services
{
    public class NavigationService :INavigationService
    {
        private readonly IJSRuntime JS;
        private readonly ILogger<NavigationService> _logger;

        public NavigationService(IJSRuntime js, ILogger<NavigationService> logger)
        {
            JS = js;
            _logger = logger;
        }

        public async Task OpenNewPage(string url)
        {
            try
            {
                await JS.InvokeAsync<string>("openNewPage", url);
            }
            catch
            {
                _logger.LogError("尝试通过JS打开新标签页失败");
            }
        }
    }
}
