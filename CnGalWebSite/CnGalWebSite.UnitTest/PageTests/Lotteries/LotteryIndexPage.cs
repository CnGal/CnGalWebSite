using CnGalWebSite.UnitTest.Base;
using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.UnitTest.PageTests.Lotteries
{
    public class LotteryIndexPage:BasePage
    {
        public LotteryIndexPage(IPage page) : base(page, "lotteries/index/3")
        {

        }

        public async Task VerifyPage()
        {
            await Expect(_page.Locator("h1").First).ToHaveTextAsync("周年庆7月30日19点直播抽奖", new() { Timeout = 30000 });
        }
    }
}
