using CnGalWebSite.UnitTest.Base;
using CnGalWebSite.UnitTest.PageTests.Videos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.UnitTest.PageTests.Peripheries
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    internal class PeripheryIndexPageTest : BasePageTest
    {
        /// <summary>
        /// 验证页面
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task VerifyPage()
        {
            var page = new PeripheryIndexPage(Page);
            await page.GotoAsync();
            await page.VerifyPage();
        }
    }
}
