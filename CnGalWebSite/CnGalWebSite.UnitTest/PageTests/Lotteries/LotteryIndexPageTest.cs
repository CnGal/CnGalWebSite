using CnGalWebSite.UnitTest.Base;
using CnGalWebSite.UnitTest.PageTests.Peripheries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.UnitTest.PageTests.Lotteries
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class LotteryIndexPageTest : BasePageTest
    {
        /// <summary>
        /// 验证页面
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task VerifyPage()
        {
            var page = new LotteryIndexPage(Page);
            await page.GotoAsync();
            await page.VerifyPage();
        }
    }
}
