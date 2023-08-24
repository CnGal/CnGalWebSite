using CnGalWebSite.UnitTest.Base;
using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.UnitTest.PageTests.Peripheries
{
    public class PeripheryIndexPage:BasePage
    {
        public PeripheryIndexPage(IPage page) : base(page, "peripheries/index/1")
        {

        }

        public async Task VerifyPage()
        {
            await Expect(_page.Locator("h1").First).ToHaveTextAsync("Visual Fan Book", new() { Timeout = 30000 });
        }
    }
}
