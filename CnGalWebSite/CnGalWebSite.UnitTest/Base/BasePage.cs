using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.UnitTest.Base
{
    public class BasePage
    {
        protected readonly string _baseUrl = "https://www.cngal.org/";
        protected readonly string _url;
        protected readonly IPage _page;

        public BasePage(IPage page,string url)
        {
            _page = page;
            _url = url;
        }

        public async Task GotoAsync()
        {
            await _page.GotoAsync(_baseUrl + _url);
            //await Task.Delay(1000);
        }
    }
}
