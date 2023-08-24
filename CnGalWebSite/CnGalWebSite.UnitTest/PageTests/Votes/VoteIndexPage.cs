using CnGalWebSite.UnitTest.Base;
using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.UnitTest.PageTests.Votes
{
    internal class VoteIndexPage:BasePage
    {
        public VoteIndexPage(IPage page) : base(page, "votes/index/1")
        {

        }

        public async Task VerifyPage()
        {
            await Expect(_page.Locator("h1").First).ToHaveTextAsync("【测试】关于标题下划线样式的投票", new() { Timeout = 30000 });
        }
    }
}
