using CnGalWebSite.UnitTest.Base;
using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CnGalWebSite.UnitTest.PageTests.Entries
{
    public class EntryIndexPage:BasePage
    {

        public EntryIndexPage(IPage page) : base(page, "entries/index/1")
        {

        }

        public async Task VerifyPage()
        {
            await Expect(_page.Locator("h1").First).ToHaveTextAsync("茸雪", new() { Timeout = 30000 });
        }
    }
}
