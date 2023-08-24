using CnGalWebSite.UnitTest.Base;
using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.UnitTest.PageTests.Tags
{
    public class TagIndexPage:BasePage
    {
        public TagIndexPage(IPage page) : base(page, "tags/index/1")
        {

        }

        public async Task VerifyPage()
        {
            await Expect(_page.Locator("h1").First).ToHaveTextAsync("游戏", new() { Timeout = 30000 });
        }
    }
}
